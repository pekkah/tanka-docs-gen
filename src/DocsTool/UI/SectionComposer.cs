﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Extensions;
using Tanka.DocsTool.Extensions.Roslyn;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI.Navigation;
using Tanka.FileSystem;
using FileSystemPath = Tanka.FileSystem.FileSystemPath;

namespace Tanka.DocsTool.UI
{
    public class SectionComposer
    {
        private readonly Site _site;
        private readonly IFileSystem _cache;
        private readonly IFileSystem _output;
        private readonly IUiBundle _uiBundle;
        private readonly AssetProcessor _assetProcessor;
        private ILogger<SectionComposer> _logger;

        public SectionComposer(Site site, IFileSystem cache, IFileSystem output, IUiBundle uiBundle, AssetProcessor assetProcessor)
        {
            _site = site;
            _cache = cache;
            _output = output;
            _uiBundle = uiBundle;
            _assetProcessor = assetProcessor;
            _logger = Infra.LoggerFactory.CreateLogger<SectionComposer>();
        }

        public async Task ComposeSection(Section section, BuildContext buildContext)
        {
            try
            {
                var preprocessorPipe = BuildPreProcessors(section);
                var router = new DocsSiteRouter(_site, section);
                var renderer = await BuildMarkdownService(section, router, buildContext);

                var menu = await ComposeMenu(section, buildContext);

                // Process pages first to track all xref asset references
                await ComposePages(section, menu, router, renderer, preprocessorPipe, buildContext);
                
                // Process regular section assets using unified processor
                await _assetProcessor.ProcessSectionAssets(section, router, buildContext);
            }
            catch (Exception ex)
            {
                buildContext.Add(new Error($"Failed to compose section '{section}': {ex.Message}"));
            }
        }

        private Func<FileSystemPath, PipeReader, Task<PipeReader>> BuildPreProcessors(Section section)
        {
            var builder = new PreProcessorPipelineBuilder();
            new RoslynExtension().ConfigurePreProcessors(_site, section, builder);

            return builder.Build();
        }

        private Task<DocsMarkdownService> BuildMarkdownService(Section section, DocsSiteRouter router, BuildContext buildContext)
        {
            var context = new DocsMarkdownRenderingContext(_site, section, router, buildContext);
            var builder = new MarkdownPipelineBuilder();
            builder.Use(new DisplayLinkExtension(context));

            return Task.FromResult(new DocsMarkdownService(builder));
        }


        private async Task ComposePages(
            Section section,
            IReadOnlyCollection<NavigationItem> menu,
            DocsSiteRouter router,
            DocsMarkdownService renderer,
            Func<FileSystemPath, PipeReader, Task<PipeReader>> preprocessorPipe,
            BuildContext buildContext)
        {
            var pageComposer = new PageComposer(_site, section, _cache, _output, _uiBundle, renderer);

            var tasks = new List<Task>();
            foreach (var pageItem in section.ContentItems.Where(ci => IsPage(ci.Key, ci.Value)))
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await pageComposer.ComposePage(pageItem.Key, pageItem.Value, menu, router, preprocessorPipe);
                    }
                    catch (Exception e)
                    {
                        lock (buildContext)
                        {
                            buildContext.Add(new Error($"Failed to compose page '{pageItem.Key}': {e.Message}", pageItem.Value));
                        }
                    }
                }));
            }

            // If index.md is one of the content items, we don't need a redirect,
            // as it will be compiled to index.html.
            var hasIndexMd = section.ContentItems.ContainsKey("index.md");

            // create redirect from section root to index page
            if (!hasIndexMd && (section.IndexPage.Xref != null || section.IndexPage.Uri != null))
            {
                // we need a redirect target
                var redirectToPage = section.IndexPage;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await pageComposer.ComposeRedirectPage("index.html", redirectToPage);
                    }
                    catch (Exception e)
                    {
                        lock (buildContext)
                        {
                            buildContext.Add(new Error($"Failed to compose redirect page 'index.html': {e.Message}"));
                        }
                    }
                }));
            }

            // Wait for all tasks to complete, regardless of whether some fail
            await Task.WhenAll(tasks);
        }

        private bool IsPage(FileSystemPath relativePath, ContentItem contentItem)
        {
            return relativePath.GetExtension() == ".md" && relativePath.GetFileName() != "nav.md";
        }

        private async Task<IReadOnlyCollection<NavigationItem>> ComposeMenu(Section section, BuildContext buildContext)
        {
            var items = new List<NavigationItem>();

            foreach (var naviFileLink in section.Definition.Nav)
            {
                try
                {
                    if (naviFileLink.Xref == null)
                    {
                        buildContext.Add(new Error("External navigation file links are not supported"));
                        continue;
                    }

                    var xref = naviFileLink.Xref.Value;

                    var targetSection = _site.GetSectionByXref(xref, section);

                    if (targetSection == null)
                    {
                        buildContext.Add(new Error($"Invalid navigation file link {naviFileLink}. Section not found."));
                        continue;
                    }

                    var navigationFileItem = targetSection.GetContentItem(xref.Path);

                    if (navigationFileItem == null)
                    {
                        buildContext.Add(new Error($"Invalid navigation file link {naviFileLink}. Path not found.", navigationFileItem));
                        continue;
                    }

                    await using var fileStream = await navigationFileItem.File.OpenRead();
                    using var reader = new StreamReader(fileStream, Encoding.UTF8);
                    var text = await reader.ReadToEndAsync();

                    // override context so each navigation file is rendered in the context of the owning section
                    var router = new DocsSiteRouter(_site, targetSection);
                    var renderer = new DocsMarkdownService(new DocsMarkdownRenderingContext(_site, targetSection, router, buildContext));
                    var builder = new NavigationBuilder(renderer, router);
                    var fileItems = builder.Add(new string[]
                        {
                            text
                        })
                        .Build();

                    items.AddRange(fileItems);
                }
                catch (Exception ex)
                {
                    buildContext.Add(new Error($"Failed to compose navigation for '{naviFileLink}': {ex.Message}"));
                }
            }

            return items;
        }
    }
}