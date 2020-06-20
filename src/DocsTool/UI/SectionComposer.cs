using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI.Navigation;
using Tanka.FileSystem;
using Path = Tanka.FileSystem.Path;

namespace Tanka.DocsTool.UI
{
    public class SectionComposer
    {
        private readonly Site _site;
        private readonly IFileSystem _cache;
        private readonly IFileSystem _output;
        private readonly IUiBundle _uiBundle;

        public SectionComposer(Site site, IFileSystem cache, IFileSystem output, IUiBundle uiBundle)
        {
            _site = site;
            _cache = cache;
            _output = output;
            _uiBundle = uiBundle;
        }

        public async Task ComposeSection(Section section)
        {
            var menu = await ComposeMenu(section);
            var router = new DocsSiteRouter(_site, section);
            await ComposeAssets(section, router);
            await ComposePages(section, menu, router);
            //ComposeIndexPages(section, menu);
        }

        private async Task ComposeAssets(Section section, DocsSiteRouter router)
        {
            foreach (var (relativePath, assetItem) in section.ContentItems.Where(ci => IsAsset(ci.Key, ci.Value)))
            {
                // open file streams
                await using var inputStream = await assetItem.File.OpenRead();

                // create output dir for page
                Path outputPath = router.GenerateRoute(new Xref(assetItem.Version, section.Id, relativePath))
                    ?? throw new InvalidOperationException($"Could not generate output path for '{outputPath}'.");

                await _output.GetOrCreateDirectory(outputPath.GetDirectoryPath());

                // create output file
                var outputFile = await _output.GetOrCreateFile(outputPath);
                await using var outputStream = await outputFile.OpenWrite();

                await inputStream.CopyToAsync(outputStream);
            }
        }

        private bool IsAsset(Path relativePath, ContentItem contentItem)
        {
            if (IsPage(relativePath, contentItem))
                return false;

            var extension = relativePath.GetExtension().ToString();

            return new []
            {
                ".js",
                ".css"
            }.Contains(extension);
        }

        private async Task ComposePages(Section section, IReadOnlyCollection<IReadOnlyCollection<NavigationItem>> menu, DocsSiteRouter router)
        {
            var pageComposer = new PageComposer(_site, section, _cache, _output, _uiBundle);

            foreach (var pageItem in section.ContentItems.Where(ci => IsPage(ci.Key, ci.Value)))
            {
                await pageComposer.ComposePage(pageItem.Key, pageItem.Value, menu, router);
            }
        }

        private bool IsPage(Path relativePath, ContentItem contentItem)
        {
            return relativePath.GetExtension() == ".md" && relativePath.GetFileName() != "nav.md";
        }

        private async Task<IReadOnlyCollection<IReadOnlyCollection<NavigationItem>>> ComposeMenu(Section section)
        {
            var items = new List<List<NavigationItem>>();

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
                var fileItems = builder
                    .Add(new []{ text })
                    .Build()
                    .ToList();
                
                
                items.Add(fileItems);
            }

            return items;
        }
    }
}