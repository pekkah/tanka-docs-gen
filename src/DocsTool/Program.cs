using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool
{
    [Verb("gen", true, HelpText = "Generate documentation")]
    public class Options
    {
        [Option("debug", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Debug { get; set; }

        [Option('f', "file", Required = false, HelpText = "tanka-docs.yml file path")]
        public string? ConfigFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output directory")]
        public string? OutputPath { get; set; }

        [Option('b', "build", Required = false, HelpText = "Build directory")]
        public string? BuildPath { get; set; }

        [Option("base", HelpText = "Set the base href meta for the generated html pages")]
        public string? Base { get; set; }
    }

    internal class Program
    {
        private static int statusCode;

        public static async Task<int> Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(HandleParseError)
                .WithParsedAsync(Execute);

            return statusCode;
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            foreach (var error in obj) Infra.Logger.LogError(error.ToString());

            statusCode = 1;
        }

        private static async Task Execute(Options options)
        {
            Infra.Initialize(options);
            var logger = Infra.Logger;

            try
            {
                var currentPath = Directory.GetCurrentDirectory();
                var configFilePath = Path.Combine(currentPath, "tanka-docs.yml");
                configFilePath = Path.GetFullPath(configFilePath);

                if (!string.IsNullOrEmpty(options.ConfigFile))
                {
                    configFilePath = currentPath = Path.GetFullPath(options.ConfigFile);
                    currentPath = Path.GetDirectoryName(currentPath);
                }

                logger.LogInformation($"Current path: '{currentPath}'");
                logger.LogInformation($"Config: '{configFilePath}'");

                if (!File.Exists(configFilePath))
                {
                    logger.LogError(
                        "Could not load configuration: '{path}'",
                        configFilePath);

                    return;
                }

                var site = (await File.ReadAllTextAsync(configFilePath))
                    .ParseYaml<SiteDefinition>();

                // override output path if set
                if (!string.IsNullOrEmpty(options.OutputPath))
                {
                    site.OutputPath = options.OutputPath;
                }

                // override build path if set
                if (!string.IsNullOrEmpty(options.BuildPath))
                {
                    site.BuildPath = options.BuildPath;
                }

                // override html meta basepath if set
                if (!string.IsNullOrEmpty(options.Base))
                {
                    site.BasePath = options.Base;
                }

                logger.LogInformationJson("Site", site);

                var executor = new Executor(site, currentPath);
                await executor.Execute();
                logger.LogInformation("Done!");

                statusCode = 0;
            }
            catch (Exception x)
            {
                logger.LogError(x, "Execution failed :(");
                statusCode = 2;
            }
        }
    }
}