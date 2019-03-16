using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using static tanka.generate.docs.Generator;

namespace tanka.generate.docs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var currentPath = Directory.GetCurrentDirectory();
                Console.WriteLine($"Working on {currentPath}");
                Console.WriteLine();
                var configuration = new ConfigurationBuilder()
                    .AddYamlFile(Path.Combine(currentPath, "tanka-docs.yaml"), true)
                    .AddCommandLine(args)
                    .Build();

                var options = configuration.Get<GeneratorOptions>();
                options.Configuration = configuration;

                var pipeline = new Pipeline(options)
                {
                    Steps =
                    {
                        WriteConfigurationToConsole(),
                        CleanOutput(options),
                        AnalyzeSolution(options),
                        EnumerateFiles(options),
                        TransformInputFilesToHtmlOutputFiles(options),
                        AddHtmlLayout(options),
                        Assets(".js", ".css"),
                        WriteFiles(options)
                    }
                };

                await pipeline.Execute();
                Console.WriteLine("Completed");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
            }
        }
    }
}