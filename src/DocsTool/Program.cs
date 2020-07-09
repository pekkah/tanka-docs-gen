using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Infra.Initialize(args);
            var logger = Infra.Logger;

            try
            {
                var currentPath = Directory.GetCurrentDirectory();
                logger.LogInformation($"Current path: '{currentPath}'");

                var configFilePath = Path.Combine(currentPath, "tanka-docs.yml");

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