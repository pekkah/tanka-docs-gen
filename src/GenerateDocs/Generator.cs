using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Fugu.GenerateDocs.Markdig;
using HandlebarsDotNet;
using Markdig;
using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public static class Generator
    {
        public static PipelineStep AnalyzeSolution(IConfiguration configuration)
        {
            if (configuration["solution"] == null)
                throw new InvalidOperationException("Failed to get 'solution' from configuration");

            var solution = configuration["solution"];
            AnalyzerManager analyzerManager = new AnalyzerManager(solution);

            return context =>
            {
                context.Solution = new SolutionContext(analyzerManager);
                return Task.CompletedTask;
            };
        }

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

            return async context =>
            {
                var markdownPipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Use(new CodeExtension(context))
                    .Build();

                foreach (var inputFile in context.InputFiles)
                {
                    var markdownContent = await File.ReadAllTextAsync(inputFile.FullName);
                    var htmlContent = Markdown.ToHtml(markdownContent, markdownPipeline);

                    context.OutputFiles.Add((Path.GetRelativePath(input.FullName, inputFile.FullName), htmlContent));
                }
            };
        }

        public static PipelineStep GenerateToc(IConfiguration configuration)
        {
            return context => Task.CompletedTask;
        }

        public static PipelineStep AddLayout(IConfiguration configuration)
        {
            if (configuration["input"] == null)
                throw new InvalidOperationException("Failed to get 'input' from configuration");

            var input = Directory.CreateDirectory(configuration["input"]);
            var templateFileName = configuration["template"] ?? "_template.html";

            // check if template file exists
            var templateFilePath = Path.Combine(input.FullName, templateFileName);
            if (!File.Exists(templateFilePath))
            {
                return context => Task.CompletedTask;
            }

            var template = File.ReadAllText(templateFilePath);
            var renderTemplate = Handlebars.Compile(template);

            return context =>
            {
                foreach (var outputFile in context.OutputFiles.ToList())
                {
                    context.OutputFiles.Remove(outputFile);
                    
                    var content = renderTemplate(
                        new
                        {
                            Context = context,
                            Path = outputFile.path,
                            Content = outputFile.content
                        });

                    context.OutputFiles.Add((outputFile.path, content));
                }

                return Task.CompletedTask;
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
                    var fullName = Path.GetFileNameWithoutExtension(path);
                    var fullPath = Path.Combine(output.FullName, $"{fullName}.html");
                    await File.WriteAllTextAsync(fullPath, content);
                }
            };
        }
    }
}