using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
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
        private ILogger<SectionComposer> _logger;

        public SectionComposer(Site site, IFileSystem cache, IFileSystem output, IUiBundle uiBundle)
        {
            _site = site;
            _cache = cache;
            _output = output;
            _uiBundle = uiBundle;
            _logger = Infra.LoggerFactory.CreateLogger<SectionComposer>();
        }

        public async Task ComposeSection(Section section, BuildContext buildContext)
        {
            try
            {
                var preprocessorPipe = BuildPreProcessors(section);
                var router = new DocsSiteRouter(_site, section);
                var renderer = await BuildMarkdownService(section, router);

                var menu = await ComposeMenu(section, buildContext);
                
                await ComposeAssets(section, router, buildContext);
                await ComposePages(section, menu, router, renderer, preprocessorPipe, buildContext);
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

        private Task<DocsMarkdownService> BuildMarkdownService(Section section, DocsSiteRouter router)
        {
            var context = new DocsMarkdownRenderingContext(_site, section, router);
            var builder = new MarkdownPipelineBuilder();
            builder.Use(new DisplayLinkExtension(context));

            return Task.FromResult(new DocsMarkdownService(builder));
        }

        private async Task ComposeAssets(Section section, DocsSiteRouter router, BuildContext buildContext)
        {
            // compose assets from sections
            foreach (var (relativePath, assetItem) in section.ContentItems.Where(ci => IsAsset(ci.Key, ci.Value)))
            {
                try
                {
                    // open file streams
                    await using var inputStream = await assetItem.File.OpenRead();

                    // create output dir for page
                    var outputPath = router.GenerateRoute(new Xref(assetItem.Version, section.Id, relativePath));
                    if (outputPath == null)
                    {
                        buildContext.Add(new Error($"Could not generate output path for asset '{relativePath}'.", assetItem));
                        continue;
                    }

                    await _output.GetOrCreateDirectory(Path.GetDirectoryName(outputPath));

                    // create output file
                    var outputFile = await _output.GetOrCreateFile(outputPath);
                    await using var outputStream = await outputFile.OpenWrite();

                    await inputStream.CopyToAsync(outputStream);
                }
                catch (Exception ex)
                {
                    buildContext.Add(new Error($"Failed to compose asset '{relativePath}': {ex.Message}", assetItem));
                }
            }
        }

        private bool IsAsset(FileSystemPath relativePath, ContentItem contentItem)
        {
            if (IsPage(relativePath, contentItem))
                return false;

            var extension = relativePath.GetExtension()
                .ToString()
                .ToLowerInvariant();

            //todo: better management of assets
            return new []
            {
                ".js",
                ".css",
                ".png",
                ".jpg",
                ".gif",
                ".zip"
            }.Contains(extension);
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
                    using var reader = new StreamReader(fileStream);
                    var text = await reader.ReadToEndAsync();

                    // override context so each navigation file is rendered in the context of the owning section
                    var router = new DocsSiteRouter(_site, targetSection);
                    var renderer = new DocsMarkdownService(new DocsMarkdownRenderingContext(_site, targetSection, router));
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