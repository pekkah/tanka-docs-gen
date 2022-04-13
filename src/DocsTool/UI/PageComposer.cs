using System.IO.Pipelines;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using FileSystemPath = Tanka.FileSystem.FileSystemPath;

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

        public PageComposer(
            Site site,
            Section section,
            IFileSystem cache,
            IFileSystem output,
            IUiBundle uiBundle,
            DocsMarkdownService renderer)
        {
            _site = site;
            _section = section;
            _cache = cache;
            _output = output;
            _uiBundle = uiBundle;
            _router = new DocsSiteRouter(site, section);
            _docsMarkdownService = renderer;
        }


        public async Task<ContentItem> ComposePage(
            FileSystemPath relativePath,
            ContentItem page,
            IReadOnlyCollection<NavigationItem> menu,
            DocsSiteRouter router,
            Func<FileSystemPath, PipeReader, Task<PipeReader>> preprocessorPipe)
        {
            var (partialHtmlPage, frontmatter) = await ComposePartialHtmlPage(
                relativePath,
                page,
                router,
                preprocessorPipe);

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
            IReadOnlyCollection<NavigationItem> menu)
        {
            // open file streams
            await using var inputStream = await partialHtmlPage.File.OpenRead();
            using var reader = new StreamReader(inputStream);
            var partialPageHtml = await reader.ReadToEndAsync();

            // create output dir for page
            await _output.GetOrCreateDirectory(partialHtmlPage.File.Path.GetDirectoryPath());

            // create output file
            var outputFile = await _output.GetOrCreateFile(partialHtmlPage.File.Path);

            if (string.IsNullOrEmpty(frontmatter.Title))
                frontmatter.Title = outputFile.Path.GetFileName().ChangeExtension(null);

            var fullPageHtml = _uiBundle.GetPageRenderer(frontmatter.Template, _router)
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
            await writer.FlushAsync();

            return partialHtmlPage.WithFile(outputFile);
        }

        private async Task<(ContentItem Page, PageFrontmatter? Frontmatter)> ComposePartialHtmlPage(
            FileSystemPath relativePath,
            ContentItem page,
            DocsSiteRouter router,
            Func<FileSystemPath, PipeReader, Task<PipeReader>> preprocessorPipe)
        {
            FileSystemPath outputPath = router.GenerateRoute(new Xref(page.Version, _section.Id, relativePath))
                ?? throw new InvalidOperationException($"Could not generate output path for '{page}'");

            // create output dir for page
            await _cache.GetOrCreateDirectory(outputPath.GetDirectoryPath());

            // create output file
            var outputFile = await _cache.GetOrCreateFile(outputPath);

            // open file streams
            await using var inputStream = await page.File.OpenRead();

            // stream for preprocessed content
            await using var processedStream = new MemoryStream();

            // process preprocessor directives
            var reader = await preprocessorPipe(relativePath, PipeReader.Create(inputStream));
            var writer = PipeWriter.Create(processedStream, new StreamPipeWriterOptions(leaveOpen: true));
            await reader.CopyToAsync(writer);
            await reader.CompleteAsync();
            await writer.CompleteAsync();
            processedStream.Position = 0;

            // render markdown
            await using var outputStream = await outputFile.OpenWrite();
            var frontmatter = await _docsMarkdownService.RenderPage(processedStream, outputStream);

            return (page.WithFile(outputFile, "text/html"), frontmatter);
        }

        public async Task ComposeRedirectPage(FileSystemPath relativePath, Link redirectToPage)
        {
            var target = redirectToPage.IsXref
                ? _router.GenerateRoute(redirectToPage.Xref.Value)
                : redirectToPage.Uri;

            if (target == null)
                throw new InvalidOperationException(
                    $"Cannot generate redirect target from '{redirectToPage}'.");

            var generatedHtml = string.Format(RedirectPageHtml, _site.BasePath, target);

            // create output dir for page
            FileSystemPath targetFilePath = _router.GenerateRoute(
                new Xref(_section.Version, _section.Id, relativePath));

            if (targetFilePath == new FileSystemPath(target))
                throw new InvalidOperationException(
                    $"Cannot generate a index.html redirect page '{targetFilePath}'. " +
                    $"Redirect would point to same file as the generated file and would" +
                    "end in a endless loop");

            await _output.GetOrCreateDirectory(targetFilePath.GetDirectoryPath());

            // create output file
            var outputFile = await _output.GetOrCreateFile(targetFilePath);
            await using var outputStream = await outputFile.OpenWrite();
            await using var writer = new StreamWriter(outputStream);
            await writer.WriteAsync(generatedHtml);
        }

        internal static string RedirectPageHtml = @"<!DOCTYPE html>
<html>
    <head>
        <base href=""{0}"">
      <script>
        window.location = ""{1}"";
      </script>
    </head>
    <body>
    </body>
</html>";
    }
}