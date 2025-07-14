using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool;

public class BuildSiteCommand : AsyncCommand<BuildSiteCommand.Settings>
{
    private readonly System.IServiceProvider _services;
    private readonly IAnsiConsole _console;
    private readonly Settings _settings;

    public BuildSiteCommand(System.IServiceProvider services, IAnsiConsole console)
    {
        _services = services;
        _console = console;
        _settings = new Settings();
    }

    public BuildSiteCommand(System.IServiceProvider services, IAnsiConsole console, Settings settings)
    {
        _services = services;
        _console = console;
        _settings = settings;
    }

    public async override Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings)
    {
        try
        {
            _console.MarkupLine("Initializing...");

            var (currentPath, configFilePath) = PathResolver.ResolvePaths(settings.ConfigFile);

            _console.MarkupLine($"[bold]CurrentPath[/]: {currentPath}");
            _console.MarkupLine($"[bold]ConfigFilePath[/]: {configFilePath}");

            currentPath = Path.GetFullPath(currentPath);
            _console.MarkupLine($"[bold]Ensured Absolute CurrentPath[/]: {currentPath}");

            if (!File.Exists(configFilePath))
            {
                _console.MarkupLine($"Could not load configuration: '{configFilePath}'");
                return -1;
            }

            var siteDefinitionResult = (await File.ReadAllTextAsync(configFilePath))
                .TryParseYaml<SiteDefinition>();

            if (siteDefinitionResult.IsFailure)
            {
                _console.WriteError($"Could not load configuration '{configFilePath}': {siteDefinitionResult.Error}");
                return -1;
            }

            var site = siteDefinitionResult.Value;

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
            var buildContext = await executor.Execute(builder, site, currentPath);

            // report warnings
            foreach (var warning in buildContext.Warnings)
            {
                if (warning.ContentItem != null)
                {
                    _console.WriteWarning(warning.Message, warning.ContentItem.File.Path.ToString());
                }
                else
                {
                    _console.WriteWarning(warning.Message);
                }
            }

            // report errors
            if (buildContext.HasErrors)
            {
                _console.WriteBuildFailure();
                foreach (var error in buildContext.Errors)
                {
                    if (error.ContentItem != null)
                    {
                        _console.MarkupLine($"- In {Markup.Escape(error.ContentItem.File.Path.ToString())}: {Markup.Escape(error.Message)}");
                    }
                    else
                    {
                        _console.MarkupLine($"- {Markup.Escape(error.Message)}");
                    }
                }
                return -1;
            }

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

        [CommandOption("-f|--file <FILE>")]
        [Description("tanka-docs.yml file path.")]
        public string? ConfigFile { get; set; }

        [CommandOption("-o|--output <OUTPUT>")]
        [Description("Output directory.")]
        public string? OutputPath { get; set; }

        [CommandOption("-b|--build <BUILD>")]
        [Description("Build directory.")]
        public string? BuildPath { get; set; }

        [CommandOption("--base <BASE>")]
        [Description("Set the base href meta for the generated html pages.")]
        public string? Base { get; set; }

        [CommandOption("--link-validation <MODE>")]
        [Description("Link validation mode: strict or relaxed")]
        public LinkValidation LinkValidation { get; set; } = LinkValidation.Strict;
    }
}
