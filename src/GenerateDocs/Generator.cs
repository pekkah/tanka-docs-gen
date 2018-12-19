using System;
using System.IO;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public static class Generator
    {
        public static PipelineStep EnumerateFiles(IConfiguration configuration)
        {
            if (configuration["input"] == null)
                throw new InvalidOperationException("Failed to get 'input' from configuration");

            var input = Directory.CreateDirectory(configuration["input"]);

            return context =>
            {
                var inputFiles = input.EnumerateFiles("*.md", new EnumerationOptions
                {
                    RecurseSubdirectories = true
                });

                context.InputFiles.AddRange(inputFiles);
                return Task.CompletedTask;
            };
        }

        public static PipelineStep TransformInputFilesToHtmlOutputFiles(IConfiguration configuration)
        {
            if (configuration["input"] == null)
                throw new InvalidOperationException("Failed to get 'input' from configuration");

            var input = Directory.CreateDirectory(configuration["input"]);

            var markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            return async context =>
            {
                foreach (var inputFile in context.InputFiles)
                {
                    var markdownContent = await File.ReadAllTextAsync(inputFile.FullName);
                    var htmlContent = Markdown.ToHtml(markdownContent, markdownPipeline);

                    context.OutputFiles.Add((Path.GetRelativePath(input.FullName, inputFile.FullName), htmlContent));
                }
            };
        }

        public static PipelineStep WriteFiles(IConfiguration configuration)
        {
            if (configuration["output"] == null)
                throw new InvalidOperationException("Failed to get 'output' from configuration");

            var output = Directory.CreateDirectory(configuration["output"]);

            return async context =>
            {
                foreach (var (path, content) in context.OutputFiles)
                {
                    var fullPath = Path.Combine(output.FullName, path);
                    await File.WriteAllTextAsync(fullPath, content);
                }
            };
        }
    }
}