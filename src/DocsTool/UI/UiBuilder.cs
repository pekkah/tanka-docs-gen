using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;
using Path = Tanka.FileSystem.Path;

namespace Tanka.DocsTool.UI
{
    public class UiBuilder
    {
        private readonly IFileSystem _cache;
        private readonly IFileSystem _output;

        public UiBuilder(IFileSystem cache, IFileSystem output)
        {
            _cache = cache;
            _output = output;
        }

        public async Task BuildSite(Site site)
        {
            foreach (var version in site.Versions)
            {
                // compose doc sections
                foreach (var section in site.GetSectionsByVersion(version))
                {
                    var uiBundleRef = LinkParser.Parse("xref://ui-bundle:tanka-docs-section.yml").Xref!.Value;
                    var uiContent = site.GetSectionByXref(uiBundleRef, section);
                    
                    if (uiContent == null)
                        throw new InvalidOperationException($"Could not resolve ui-bundle. Xref '{uiBundleRef}' could not be resolved.'");

                    var uiBundle = new HandlebarsUiBundle(site, uiContent, _output);
                    await uiBundle.Initialize(CancellationToken.None);

                    var composer = new SectionComposer(site, _cache, _output, uiBundle);
                    await composer.ComposeSection(section);
                }
            }

            await ComposeIndexPage(site);
        }

        private async Task ComposeIndexPage(Site site)
        {
            var redirectoToPage = site.Definition.IndexPage;

            string target = string.Empty;
            if (redirectoToPage.IsXref)
            {
                var xref = redirectoToPage.Xref.Value;
                var targetSection = site.GetSectionByXref(xref);

                if (targetSection == null)
                    throw new InvalidOperationException(
                        $"Cannot generate site index page. " +
                        $"Target section of '{redirectoToPage}' not found.");

                var router = new DocsSiteRouter(site, targetSection);
                target = router.GenerateRoute(xref) ?? string.Empty;
            }
            else
            {
                target = redirectoToPage.Uri ?? string.Empty;
            }

            
            var generatedHtml = string.Format(
                PageComposer.RedirectPageHtml, 
                target);

            // create output dir for page
            Path targetFilePath = "index.html";

            if (targetFilePath == new Path(target))
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
    }
}