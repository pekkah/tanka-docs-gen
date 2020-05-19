using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var currentPath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Working on {currentPath}");
            Console.WriteLine();
            var configuration = new ConfigurationBuilder()
                .AddYamlFile(Path.Combine(currentPath, "tanka-docs.yaml"), true)
                .AddCommandLine(args)
                .Build();

            var site = new SiteDefinition();
            configuration.Bind(site);

            var executor = new Executor(site, currentPath);
            await executor.Execute();
        }
    }
}