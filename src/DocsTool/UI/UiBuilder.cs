﻿using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using FileSystemPath = Tanka.FileSystem.FileSystemPath;

namespace Tanka.DocsTool.UI
{
    public class UiBuilder
    {
        private readonly IFileSystem _cache;
        private readonly IFileSystem _output;
        private readonly IAnsiConsole _console;

        public UiBuilder(IFileSystem cache, IFileSystem output, IAnsiConsole console)
        {
            _cache = cache;
            _output = output;
            _console = console;
        }

        public async Task BuildSite(Site site, ProgressContext progress)
        {
            var tasks = site.Versions
                .ToDictionary(v => v, v => progress.AddTask($"Version: {v}", maxValue: 0));

            foreach (var version in site.Versions)
            {
                var sections = site.GetSectionsByVersion(version).ToList();
                var task = tasks[version];
                task.MaxValue = sections.Count;

                // compose doc sections
                foreach (var section in sections)
                {
                    _console.LogInformation($"Building: {section}");
                    var uiBundleRef = LinkParser.Parse("xref://ui-bundle:tanka-docs-section.yml").Xref!.Value;
                    var uiContent = site.GetSectionByXref(uiBundleRef, section);

                    if (uiContent == null)
                        throw new InvalidOperationException($"Could not resolve ui-bundle. Xref '{uiBundleRef}' could not be resolved.'");

                    var uiBundle = new HandlebarsUiBundle(site, uiContent, _output);
                    await uiBundle.Initialize(CancellationToken.None);

                    var composer = new SectionComposer(site, _cache, _output, uiBundle);
                    await composer.ComposeSection(section);

                    _console.LogInformation($"Built: {section}");
                    task.Increment(1);
                }

                task.StopTask();
            }

            await ComposeIndexPage(site);
        }

        private async Task ComposeIndexPage(Site site)
        {
            var redirectoToPage = site.Definition.IndexPage;

            string? target;
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
                site.BasePath,
                target);

            // create output dir for page
            FileSystemPath targetFilePath = "index.html";

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
    }
}