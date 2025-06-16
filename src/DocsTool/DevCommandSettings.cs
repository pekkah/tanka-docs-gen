using System.ComponentModel;
using Spectre.Console.Cli;

namespace Tanka.DocsTool;

public class DevCommandSettings : CommandSettings
{
    [CommandOption("-f|--file <FILE>")]
    [Description("tanka-docs.yml file path.")]
    public string? ConfigFile { get; set; }

    [CommandOption("-p|--port <PORT>")]
    [Description("Port to run the dev server on.")]
    [DefaultValue(5000)]
    public int Port { get; set; }
} 