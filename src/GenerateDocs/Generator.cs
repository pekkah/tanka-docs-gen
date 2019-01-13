using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using tanka.generate.docs.Markdig;
using HandlebarsDotNet;
using Markdig;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace tanka.generate.docs
{
    /// <summary>
    ///     Pipeline steps
    /// </summary>
    public static class Generator
    {
        public static PipelineStep AnalyzeSolution(GeneratorOptions options)
        {
            var analyzerManager = new AnalyzerManager(options.Solution);

            return context =>
            {
                context.Solution = new SolutionContext(analyzerManager);
                return Task.CompletedTask;
            };
        }

        public static PipelineStep EnumerateFiles(GeneratorOptions options)
        {
            var input = Directory.CreateDirectory(options.Input);

            return context =>
            {
                var inputFiles = input.EnumerateFiles("*.*", new EnumerationOptions
                {
                    RecurseSubdirectories = true
                });

                context.InputFiles.AddRange(inputFiles);
                return Task.CompletedTask;
            };
        }

        public static PipelineStep TransformInputFilesToHtmlOutputFiles(GeneratorOptions options)
        {
            var input = Directory.CreateDirectory(options.Input);

            return async context =>
            {
                var markdownPipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Use(new CodeExtension(context))
                    .Build();

                foreach (var inputFile in context.InputFiles
                    .Where(filename => filename.Extension == ".md"))
                {
                    var markdownContent = await File.ReadAllTextAsync(inputFile.FullName);
                    var htmlContent = Markdown.ToHtml(markdownContent, markdownPipeline);
                   
                    var filename = Path.GetRelativePath(
                        input.FullName, 
                        inputFile.FullName);

                    filename = Path.ChangeExtension(filename, null);

                    context.OutputFiles.Add((
                        $"{filename}.html", 
                            htmlContent));
                }
            };
        }

        public static PipelineStep Assets(params string[] extensions)
        {
            return async context =>
            {
                var input = Directory.CreateDirectory(context.Options.Input);

                foreach (var inputFile in context.InputFiles
                    .Where(filename => extensions.Contains(filename.Extension)))
                {
                    var content = await File.ReadAllTextAsync(inputFile.FullName);

                    var filename = Path.GetRelativePath(
                        input.FullName, 
                        inputFile.FullName);

                    context.OutputFiles.Add((
                        filename, 
                        content));
                }
            };
        }

        public static PipelineStep GenerateToc(GeneratorOptions options)
        {
            return context =>
            {
                return Task.CompletedTask;
            };
        }

        public static PipelineStep AddHtmlLayout(GeneratorOptions options)
        {
            var input = Directory.CreateDirectory(options.Input);
            var templateFileName = options.Template ?? "_template.html";

            // check if template file exists
            var templateFilePath = Path.Combine(input.FullName, templateFileName);
            if (!File.Exists(templateFilePath)) return context => Task.CompletedTask;

            var template = File.ReadAllText(templateFilePath);
            var renderTemplate = Handlebars.Compile(template);

            return context =>
            {
                var htmlFiles = context.OutputFiles
                    .Where(filename => filename.path.EndsWith(".html"))
                    .ToList();

                var groupedFiles = htmlFiles.GroupBy(file => Path.GetDirectoryName(file.path))
                    .Select(g => new
                    {
                        Key = g.Key,
                        Values = g.Select(v => v.path).ToList()
                    })
                    .ToList();

                foreach (var outputFile in htmlFiles)
                {
                    context.OutputFiles.Remove(outputFile);

                    var content = renderTemplate(
                        new
                        {
                            Context = context,
                            Path = outputFile.path,
                            Content = outputFile.content,
                            Toc = groupedFiles
                        });

                    context.OutputFiles.Add((outputFile.path, content));
                }

                return Task.CompletedTask;
            };
        }

        public static PipelineStep WriteFiles(GeneratorOptions options)
        {
            var output = Directory.CreateDirectory(options.Output);

            return async context =>
            {
                foreach (var (path, content) in context.OutputFiles)
                {
                    var fullPath = Path.Combine(output.FullName, path);

                    var folder = Path.GetDirectoryName(fullPath);

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    await File.WriteAllTextAsync(fullPath, content);
                }
            };
        }
    }
}