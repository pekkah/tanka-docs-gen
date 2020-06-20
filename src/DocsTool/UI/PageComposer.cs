using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;
using Path = Tanka.FileSystem.Path;

namespace Tanka.DocsTool.UI
{
    public class PageComposer
    {
        private readonly Site _site;
        private readonly Section _section;
        private readonly IFileSystem _cache;
        private readonly DocsMarkdownService _docsMarkdownService;
        private readonly IFileSystem _output;
        private readonly DocsSiteRouter _router;
        private readonly IUiBundle _uiBundle;

        public PageComposer(Site site, Section section, IFileSystem cache, IFileSystem output, IUiBundle uiBundle)
        {
            _site = site;
            _section = section;
            _cache = cache;
            _output = output;
            _uiBundle = uiBundle;
            _router = new DocsSiteRouter(site, section);
            _docsMarkdownService = new DocsMarkdownService(new DocsMarkdownRenderingContext(site, section, _router));
        }


        public async Task<ContentItem> ComposePage(Path relativePath, ContentItem page, IReadOnlyCollection<IReadOnlyCollection<NavigationItem>> menu, DocsSiteRouter router)
        {
            var (partialHtmlPage, frontmatter) = await ComposePartialHtmlPage(relativePath, page, router);

            if (frontmatter == null)
                frontmatter = new PageFrontmatter
                {
                    Template = _uiBundle.DefaultTemplate
                };

            var fullHtmlPage = await ComposeFullHtmlPage(partialHtmlPage, frontmatter, menu);

            return fullHtmlPage;
        }

        private async Task<ContentItem> ComposeFullHtmlPage(
            ContentItem partialHtmlPage,
            PageFrontmatter frontmatter, 
            IReadOnlyCollection<IReadOnlyCollection<NavigationItem>> menu)
        {
            // open file streams
            await using var inputStream = await partialHtmlPage.File.OpenRead();
            using var reader = new StreamReader(inputStream);
            var partialPageHtml = await reader.ReadToEndAsync();

            // create output dir for page
            await _output.GetOrCreateDirectory(partialHtmlPage.File.Path.GetDirectoryPath());

            // create output file
            var outputFile = await _output.GetOrCreateFile(partialHtmlPage.File.Path);
            var fullPageHtml = _uiBundle.GetRenderer(frontmatter.Template, _router)
                .Render(new PageRenderingContext(
                    _site,
                    _section,
                    menu,
                    frontmatter,
                    partialPageHtml
                ));

            await using var outputStream = await outputFile.OpenWrite();
            await using var writer = new StreamWriter(outputStream);
            await writer.WriteAsync(fullPageHtml);

            return partialHtmlPage.WithFile(outputFile);
        }

        private async Task<(ContentItem Page, PageFrontmatter? Frontmatter)> ComposePartialHtmlPage(
            Path relativePath,
            ContentItem page,
            DocsSiteRouter router)
        {
            Path outputPath = router.GenerateRoute(new Xref(page.Version, _section.Id, relativePath))
                ?? throw new InvalidOperationException($"Could not generate output path for '{page}'");

            // create output dir for page
            await _cache.GetOrCreateDirectory(outputPath.GetDirectoryPath());

            // create output file
            var outputFile = await _cache.GetOrCreateFile(outputPath);

            // open file streams
            await using var inputStream = await page.File.OpenRead();
            await using var outputStream = await outputFile.OpenWrite();

            var frontmatter = await _docsMarkdownService.Render(inputStream, outputStream);

            return (page.WithFile(outputFile, "text/html"), frontmatter);
        }
    }
}