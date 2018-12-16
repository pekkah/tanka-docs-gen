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

            if (configuration["input"] == null)
            {
                Console.WriteLine("Input folder missing");
                return;
            }

            if (configuration["output"] == null)
            {
                Console.WriteLine("Output folder missing");
                return;
            }

            var output = GetDirectory(configuration["output"]);
            var input = GetDirectory(configuration["input"]);

            var markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var context = new DirectoryContext(input, output, markdownPipeline);
            await RecursiveRenderMarkdownFiles(context);
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

        private static DirectoryInfo GetDirectory(string folderPath)
        {
            return Directory.CreateDirectory(folderPath);
        }
    }

    internal class DirectoryContext
    {
        private readonly MarkdownPipeline _markdownPipeline;

        public DirectoryInfo Input { get; }

        public DirectoryInfo Output { get; }

        public DirectoryContext(DirectoryInfo input, DirectoryInfo output, MarkdownPipeline markdownPipeline)
        {
            _markdownPipeline = markdownPipeline;
            Input = input;
            Output = output;
        }

        public string Transform(string sourceFilePath, string targetFilePath, string content)
        {
            return Markdown.ToHtml(content, _markdownPipeline);
        }
    }
}