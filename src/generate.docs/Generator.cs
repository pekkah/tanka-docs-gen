﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using HandlebarsDotNet;
using Markdig;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;
using Microsoft.Extensions.Configuration;
using tanka.generate.docs.Markdig;

namespace tanka.generate.docs
{
    /// <summary>
    ///     Pipeline steps
    /// </summary>
    public static class Generator
    {
        public static (string Name, PipelineStep Step) WriteConfigurationToConsole()
        {
            return (nameof(WriteConfigurationToConsole), context =>
            {
                var configuration = context.Options.Configuration;

                Console.WriteLine("Configuration");

                foreach (var section in configuration.AsEnumerable())
                {
                    Console.Write(section.Key);
                    Console.Write("=");
                    Console.WriteLine(section.Value);
                }

                return Task.CompletedTask;
            });
        }

        public static (string Name, PipelineStep Step) CleanOutput(GeneratorOptions options)
        {
            var output = Directory.CreateDirectory(options.Output);
            
            return (nameof(CleanOutput), context =>
            {
                Console.WriteLine($"Clean output folder {output}");
                foreach (var directory in output.EnumerateDirectories())
                {
                    if  (directory.Name.StartsWith("."))
                        continue;

                    directory.Delete(true);
                }

                foreach (var file in output.EnumerateFiles())
                {
                    if (file.Name.StartsWith("."))
                        continue;

                    file.Delete();
                }

                return Task.CompletedTask;
            });
        }

        public static (string Name, PipelineStep Step) AnalyzeSolution(GeneratorOptions options)
        {
            if (string.IsNullOrEmpty(options.Solution))
                return (nameof(AnalyzeSolution), context => Task.CompletedTask);

            var analyzerManager = new AnalyzerManager(options.Solution);

            return (nameof(AnalyzeSolution), async context =>
            {
                Console.WriteLine($"Analyzing solution {options.Solution}");
                if (!File.Exists(options.Solution))
                    throw new FileNotFoundException("Could not find solution file", options.Solution);

                context.Solution = await new SolutionContext(analyzerManager)
                    .Initialize();
            });
        }

        public static (string Name, PipelineStep Step) EnumerateFiles(GeneratorOptions options)
        {
            var input = Directory.CreateDirectory(options.Input);

            return (nameof(EnumerateFiles), context =>
            {
                Console.WriteLine($"Enumerating input files from {input}");
                var inputFiles = input.EnumerateFiles("*.*", new EnumerationOptions
                {
                    RecurseSubdirectories = true
                });

                context.InputFiles.AddRange(inputFiles);
                Console.WriteLine($"Found {context.InputFiles.Count} input files");
                return Task.CompletedTask;
            });
        }

        public static (string Name, PipelineStep Step) TransformInputFilesToHtmlOutputFiles(GeneratorOptions options)
        {
            var input = Directory.CreateDirectory(options.Input);

            return (nameof(TransformInputFilesToHtmlOutputFiles), async context =>
            {
                Console.WriteLine($"Transforming md to html");
                var dfmContext = new MarkdownContext();
                var markdownPipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Use(new CodeExtension(context))
                    .UseDocfxExtensions(dfmContext)
                    .Build();

                foreach (var inputFile in context.InputFiles
                    .Where(filename => filename.Extension == ".md" && !filename.FullName.Contains("assets")))
                {
                    var filename = Path.GetRelativePath(
                        input.FullName,
                        inputFile.FullName);

                    var markdownContent = await File.ReadAllTextAsync(inputFile.FullName);

                    using (InclusionContext.PushFile(filename))
                    {
                        var htmlContent = Markdown.ToHtml(markdownContent, markdownPipeline);

                        filename = Path.ChangeExtension(filename, null);

                        context.OutputFiles.Add((
                            $"{filename}.html",
                            htmlContent));
                    }
                }
            });
        }

        public static (string Name, PipelineStep Step) Assets(params string[] extensions)
        {
            return (nameof(Assets), async context =>
            {
                var input = Directory.CreateDirectory(context.Options.Input);

                Console.WriteLine($"Enumerating asset files ({string.Join(',', extensions)}) from {input}");

                var assetFiles = context.InputFiles
                    .Where(filename => extensions.Contains(filename.Extension))
                    .ToList();

                Console.WriteLine($"Found {assetFiles.Count} asset files");
                foreach (var inputFile in assetFiles)
                {
                    var content = await File.ReadAllTextAsync(inputFile.FullName);

                    var filename = Path.GetRelativePath(
                        input.FullName,
                        inputFile.FullName);

                    context.OutputFiles.Add((
                        filename,
                        content));
                }

            });
        }

        public static (string Name, PipelineStep Step) AddHtmlLayout(GeneratorOptions options)
        {
            var input = Directory.CreateDirectory(options.Input);
            var templateFileName = options.Template ?? "_template.html";
            var basePath = options.BasePath;

            // check if template file exists
            var templateFilePath = Path.Combine(input.FullName, templateFileName);
            if (!File.Exists(templateFilePath)) 
                return (nameof(AddHtmlLayout), context => Task.CompletedTask);

            var template = File.ReadAllText(templateFilePath);
            var renderTemplate = Handlebars.Compile(template);

            return (nameof(AddHtmlLayout), context =>
            {
                Console.WriteLine($"Applying html layout");
                var htmlFiles = context.OutputFiles
                    .Where(filename => filename.path.EndsWith(".html"))
                    .ToList();

                foreach (var outputFile in htmlFiles)
                {
                    context.OutputFiles.Remove(outputFile);

                    var current = new PageInfo(outputFile.path);
                    var toc = CreateToc(htmlFiles, current);

                    var content = renderTemplate(
                        new
                        {
                            Context = context,
                            Path = outputFile.path,
                            Content = outputFile.content,
                            Toc = toc,
                            Current = current,
                            BasePath = basePath
                        });

                    context.OutputFiles.Add((outputFile.path, content));
                }

                return Task.CompletedTask;
            });
        }

        public static (string Name, PipelineStep Step) WriteFiles(GeneratorOptions options)
        {
            var output = Directory.CreateDirectory(options.Output);

            return (nameof(WriteFiles), async context =>
            {
                Console.WriteLine("Writing output files");
                foreach (var (path, content) in context.OutputFiles)
                {
                    var fullPath = Path.Combine(output.FullName, path);

                    var folder = Path.GetDirectoryName(fullPath);

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    await File.WriteAllTextAsync(fullPath, content);
                    Console.WriteLine($"Out: {fullPath}");
                }
            });
        }

        private static IEnumerable<PageCategory> CreateToc(List<(string path, string content)> htmlFiles,
            PageInfo current)
        {
            var toc = htmlFiles.GroupBy(file => Path.GetDirectoryName(file.path))
                .Select(g => new PageCategory(g.Key, g.Select(page =>
                {
                    var pageInfo = new PageInfo(page.path);

                    if (pageInfo == current)
                        return new PageInfo(page.path, true);

                    return pageInfo;
                })))
                .ToList();

            return toc;
        }
    }
}