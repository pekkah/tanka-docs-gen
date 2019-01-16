using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using HandlebarsDotNet;
using Markdig;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;
using tanka.generate.docs.Markdig;

namespace tanka.generate.docs
{
    /// <summary>
    ///     Pipeline steps
    /// </summary>
    public static class Generator
    {
        public static PipelineStep CleanOutput(GeneratorOptions options)
        {
            var output = Directory.CreateDirectory(options.Output);

            return context =>
            {
                foreach (var directory in output.EnumerateDirectories())
                {
                    directory.Delete(true);
                }

                foreach (var file in output.EnumerateFiles())
                {
                    file.Delete();
                }

                return Task.CompletedTask;
            };
        }

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
                var dfmContext = new MarkdownContext();
                var markdownPipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Use(new CodeExtension(context))
                    .UseDocfxExtensions(dfmContext)
                    .Build();

                foreach (var inputFile in context.InputFiles
                    .Where(filename => filename.Extension == ".md"))
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

                foreach (var outputFile in htmlFiles)
                {
                    context.OutputFiles.Remove(outputFile);

                    var content = renderTemplate(
                        new
                        {
                            Context = context,
                            Path = outputFile.path,
                            Content = outputFile.content,
                            Toc = CreateToc(htmlFiles)
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

        private static IEnumerable<PageCategory> CreateToc(List<(string path, string content)> htmlFiles)
        {
            var toc = htmlFiles.GroupBy(file => Path.GetDirectoryName(file.path))
                .Select(g => new PageCategory(g.Key, g.Select(page => new PageInfo(page.path))))
                .ToList();

            return toc;
        }
    }

    internal class PageCategory
    {
        public PageCategory(string category, IEnumerable<PageInfo> pages)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Pages = pages ?? throw new ArgumentNullException(nameof(pages));
        }

        public string Category { get; }

        public IEnumerable<PageInfo> Pages { get; }

        public string DisplayName
        {
            get
            {
                var displayName = Category.Replace('-', ' ');

                if (string.IsNullOrEmpty(displayName))
                    return string.Empty;

                displayName = displayName.Substring(0, 1).ToUpperInvariant() + displayName.Substring(1);
                return displayName;
            }
        }
    }

    internal class PageInfo
    {
        public PageInfo(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path { get; }

        public string Href
        {
            get
            {
                var href = Path.Replace('\\', '/');
                return href;
            }
        }

        public string DisplayName
        {
            get
            {
                var displayName = Path.Replace('-', ' ');

                if (string.IsNullOrEmpty(displayName))
                    return string.Empty;

                displayName = System.IO.Path.GetFileNameWithoutExtension(displayName);
                displayName = displayName.Substring(0, 1).ToUpperInvariant() + displayName.Substring(1);
                return displayName;
            }
        }
    }
}