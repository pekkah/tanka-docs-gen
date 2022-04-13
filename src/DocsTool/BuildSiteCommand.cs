using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

public class BuildSiteCommand : AsyncCommand<BuildSiteCommand.Settings>
{
    private readonly IServiceProvider _services;
    private readonly IAnsiConsole _console;

    public BuildSiteCommand(IServiceProvider services, IAnsiConsole console)
    {
        _services = services;
        _console = console;
    }

    public async override Task<int> ExecuteAsync(
        [NotNull] CommandContext context,
        [NotNull] Settings settings)
    {
        try
        {
            _console.LogInformation($"Initializing...");

            var (currentPath, configFilePath) = _console.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("Initializing...", status =>
                {
                    status.Status("Resolving paths..");

                    var currentPath = Directory.GetCurrentDirectory();
                    var configFilePath = Path.Combine(currentPath, "tanka-docs.yml");
                    configFilePath = Path.GetFullPath(configFilePath);

                    if (!string.IsNullOrEmpty(settings.ConfigFile))
                    {
                        configFilePath = currentPath = Path.GetFullPath(settings.ConfigFile);
                        currentPath = Path.GetDirectoryName(currentPath) ?? "";
                    }

                    status.Status("Resolved.");
                    return (currentPath, configFilePath);
                });

            _console.LogInformation($"[bold]CurrentPath[/]: {currentPath}");
            _console.LogInformation($"[bold]ConfigFilePath[/]: {configFilePath}");

            if (!File.Exists(configFilePath))
            {
                _console.MarkupLine($"Could not load configuration: '{configFilePath}'");
                return -1;
            }

            var site = (await File.ReadAllTextAsync(configFilePath))
                .ParseYaml<SiteDefinition>();

            // override output path if set
            if (!string.IsNullOrEmpty(settings.OutputPath))
            {
                site.OutputPath = settings.OutputPath;
            }

            // override build path if set
            if (!string.IsNullOrEmpty(settings.BuildPath))
            {
                site.BuildPath = settings.BuildPath;
            }

            // override html meta basepath if set
            if (!string.IsNullOrEmpty(settings.Base))
            {
                site.BasePath = settings.Base;
            }

            _console.Write(new Table()
                .AddColumn("Type")
                .AddColumn("Resolved Path")
                .AddRow(new Text("Work directory"), new TextPath(currentPath))
                .AddRow(new Text("Configuration file"), new TextPath(configFilePath))
                .AddRow(new Text("OutputPath"), new TextPath(site.OutputPath))
                .AddRow(new Text("BuildPath"), new TextPath(site.BuildPath))
                .AddRow(new Text("Base"), new TextPath(site.BasePath))
                .Border(TableBorder.Minimal)
                );

            if (settings.Debug)
                _console.WriteLine($"Site: {site.ToJson()}");

            var builder = new PipelineBuilder(_services)
                .UseDefault();

            var executor = new PipelineExecutor(settings);
            await executor.Execute(builder, site, currentPath);

            return 0;
        }
        catch (Exception x)
        {
            _console.WriteException(x);
            return -1;
        }
    }

    public class Settings : CommandSettings
    {
        [CommandOption("--debug")]
        [Description("Set output to verbose messages.")]
        public bool Debug { get; set; }

        [CommandOption("-f|--file")]
        [Description("tanka-docs.yml file path.")]
        public string? ConfigFile { get; set; }

        [CommandOption("-o|--output")]
        [Description("Output directory.")]
        public string? OutputPath { get; set; }

        [CommandOption("-b|--build")]
        [Description("Build directory.")]
        public string? BuildPath { get; set; }

        [CommandOption("--base")]
        [Description("Set the base href meta for the generated html pages.")]
        public string? Base { get; set; }
    }
}
