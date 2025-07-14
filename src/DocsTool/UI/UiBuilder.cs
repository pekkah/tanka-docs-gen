using Tanka.DocsTool.Navigation;
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

        public async Task BuildSite(Site site, ProgressContext progress, BuildContext buildContext)
        {
            var tasks = site.Versions
                .OrderBy(v => v)
                .ToDictionary(v => v, v => progress.AddTask($"Version: {v}", maxValue: 0));

            foreach (var version in site.Versions)
            {
                var sections = site.GetSectionsByVersion(version).ToList();
                var task = tasks[version];
                task.MaxValue = sections.Count;

                // compose doc sections
                foreach (var section in sections)
                {
                    try
                    {
                        _console.LogInformation($"Building: {section}");
                        var uiBundleRef = LinkParser.Parse("xref://ui-bundle:tanka-docs-section.yml").Xref!.Value;
                        var uiContent = site.GetSectionByXref(uiBundleRef, section);

                        if (uiContent == null)
                        {
                            buildContext.Add(new Error($"Could not resolve ui-bundle. Xref '{uiBundleRef}' could not be resolved.'"));
                            task.Increment(1);
                            continue;
                        }

                        var uiBundle = new HandlebarsUiBundle(site, uiContent, _output);
                        await uiBundle.Initialize(CancellationToken.None);

                        var composer = new SectionComposer(site, _cache, _output, uiBundle);
                        await composer.ComposeSection(section, buildContext);

                        _console.LogInformation($"Built: {section}");
                    }
                    catch (Exception ex)
                    {
                        buildContext.Add(new Error($"Failed to build section '{section}': {ex.Message}"));
                        _console.LogError($"Error building section '{section}': {ex.Message}");
                    }

                    task.Increment(1);
                }

                task.StopTask();
            }

            await ComposeIndexPage(site, buildContext);
        }

        private async Task ComposeIndexPage(Site site, BuildContext buildContext)
        {
            try
            {
                var redirectoToPage = site.Definition.IndexPage;

                string? target;
                if (redirectoToPage.IsXref)
                {
                    var xref = redirectoToPage.Xref.Value;
                    var targetSection = site.GetSectionByXref(xref);

                    if (targetSection == null)
                    {
                        buildContext.Add(new Error(
                            $"Cannot generate site index page. " +
                            $"Target section of '{redirectoToPage}' not found."));
                        return;
                    }

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
                {
                    buildContext.Add(new Error(
                        $"Cannot generate a index.html redirect page '{targetFilePath}'. " +
                        $"Redirect would point to same file as the generated file and would" +
                        "end in an endless loop"));
                    return;
                }

                await _output.GetOrCreateDirectory(targetFilePath.GetDirectoryPath());

                // create output file
                var outputFile = await _output.GetOrCreateFile(targetFilePath);
                await using var outputStream = await outputFile.OpenWrite();
                await using var writer = new StreamWriter(outputStream);
                await writer.WriteAsync(generatedHtml);
            }
            catch (Exception ex)
            {
                buildContext.Add(new Error($"Failed to compose index page: {ex.Message}"));
            }
        }
    }
}