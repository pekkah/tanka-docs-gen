using System.ComponentModel;
using Spectre.Console.Cli;

namespace Tanka.DocsTool;

/// <summary>
/// Settings for the init command.
/// </summary>
public class InitCommandSettings : CommandSettings
{
    [CommandOption("-f|--force")]
    [Description("Overwrite existing files without prompting.")]
    public bool Force { get; set; }

    [CommandOption("--ui-bundle-only")]
    [Description("Only extract UI bundle, skip configuration file creation.")]
    public bool UiBundleOnly { get; set; }

    [CommandOption("--config-only")]
    [Description("Only create configuration files, skip UI bundle extraction.")]
    public bool ConfigOnly { get; set; }

    [CommandOption("--no-wip")]
    [Description("Skip creating tanka-docs-wip.yml (create only main configuration).")]
    public bool NoWip { get; set; }

    [CommandOption("--branch <BRANCH>")]
    [Description("Specify default branch name (default: auto-detect from git).")]
    public string? Branch { get; set; }

    [CommandOption("--output-dir <PATH>")]
    [Description("Specify output directory (default: current directory).")]
    public string? OutputDir { get; set; }

    [CommandOption("-q|--quiet")]
    [Description("Skip post-creation configuration guidance (for automation).")]
    public bool Quiet { get; set; }

    [CommandOption("--project-name <NAME>")]
    [Description("Specify project name (default: derive from directory name).")]
    public string? ProjectName { get; set; }
}