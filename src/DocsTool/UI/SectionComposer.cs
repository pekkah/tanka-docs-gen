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

        public async Task ComposeSection(Section section)
        {
            var preprocessorPipe = BuildPreProcessors(section);
            var router = new DocsSiteRouter(_site, section);
            var renderer = await BuildMarkdownService(section, router);

            var menu = await ComposeMenu(section);
            
            await ComposeAssets(section, router);
            await ComposePages(section, menu, router, renderer, preprocessorPipe);
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

        private async Task ComposeAssets(Section section, DocsSiteRouter router)
        {
            // compose assets from sections
            foreach (var (relativePath, assetItem) in section.ContentItems.Where(ci => IsAsset(ci.Key, ci.Value)))
            {
                // open file streams
                await using var inputStream = await assetItem.File.OpenRead();

                // create output dir for page
                FileSystemPath outputPath = router.GenerateRoute(new Xref(assetItem.Version, section.Id, relativePath))
                    ?? throw new InvalidOperationException($"Could not generate output path for '{outputPath}'.");

                await _output.GetOrCreateDirectory(outputPath.GetDirectoryPath());

                // create output file
                var outputFile = await _output.GetOrCreateFile(outputPath);
                await using var outputStream = await outputFile.OpenWrite();

                await inputStream.CopyToAsync(outputStream);
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
            Func<FileSystemPath, PipeReader, Task<PipeReader>> preprocessorPipe)
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
                        throw new InvalidOperationException($"Failed to compose page '{pageItem.Key}'.", e);
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
                        throw new InvalidOperationException($"Failed to compose redirect page 'index.html'.", e);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        private bool IsPage(FileSystemPath relativePath, ContentItem contentItem)
        {
            return relativePath.GetExtension() == ".md" && relativePath.GetFileName() != "nav.md";
        }

        private async Task<IReadOnlyCollection<NavigationItem>> ComposeMenu(Section section)
        {
            var items = new List<NavigationItem>();

            foreach (var naviFileLink in section.Definition.Nav)
            {
                if (naviFileLink.Xref == null)
                    throw new NotSupportedException("External navigation file links are not supported");

                var xref = naviFileLink.Xref.Value;

                var targetSection = _site.GetSectionByXref(xref, section);

                if (targetSection == null)
                    throw new InvalidOperationException($"Invalid navigation file link {naviFileLink}. Section not found.");

                var navigationFileItem = targetSection.GetContentItem(xref.Path);

                if (navigationFileItem == null)
                    throw new InvalidOperationException($"Invalid navigation file link {naviFileLink}. Path not found.");

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

            return items;
        }
    }
}