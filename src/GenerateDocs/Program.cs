using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddYamlFile("fugu-docs.yaml", true)
                .AddCommandLine(args)
                .Build();
            
            var pipeline = new Pipeline(configuration);
            await pipeline.Execute();
        }

        private static async Task RecursiveRenderMarkdownFiles(DirectoryContext context)
        {
            var input = context.Input;
            var output = context.Output;

            var files = input.GetFiles("*.md");

            foreach (var fileInfo in files)
            {
                var targetFileName = $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.html";
                var targetFile = Path.Combine(output.FullName, targetFileName);
                var content = await File.ReadAllTextAsync(fileInfo.FullName);
                var htmlContent = context.Transform(fileInfo.FullName, targetFile, content);
                await File.WriteAllTextAsync(targetFile, htmlContent);
            }
        }
    }

    public static class Steps
    {
        public static PipelineStep ReadFiles(IConfiguration configuration)
        {
            if (configuration["input"] == null)
            {
                throw new InvalidOperationException($"Failed to get 'input' from configuration");
            }

            var input = Directory.CreateDirectory(configuration["input"]);

            return context =>
            {
                var inputFiles = input.EnumerateFiles("*.md", new EnumerationOptions()
                {
                    RecurseSubdirectories = true,           
                });

                context.InputFiles.AddRange(inputFiles);
                return Task.CompletedTask;
            };
        }

        public static PipelineStep WriteFiles(IConfiguration configuration)
        {
            if (configuration["output"] == null)
            {
                throw new InvalidOperationException($"Failed to get 'output' from configuration");
            }

            var output = Directory.CreateDirectory(configuration["output"]);

            return async context =>
            {
                foreach (var VARIABLE in context.Output)
                {
                    
                }
            };
        }
    }
}