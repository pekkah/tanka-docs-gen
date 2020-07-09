using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool
{
    public class Options
    {
        [Option("debug", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Debug { get; set; }

        [Option('f', "file", Required = false, HelpText = "tanka-docs.yml file")]
        public string? ConfigFile { get; set; }
    }

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(HandleParseError)
                .WithParsedAsync(Execute);
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            foreach (var error in obj)
            {
                Infra.Logger.LogError(error.ToString());
            }
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

                logger.LogInformationJson("Site", site);

                var executor = new Executor(site, currentPath);
                await executor.Execute();
                logger.LogInformation("Done!");
            }
            catch (Exception x)
            {
                logger.LogError(x, "Execution failed :(");
            }
        }
    }
}