using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.Extensions.Roslyn
{
    public class RoslynExtension : Extension
    {
        public override Task ConfigurePreProcessors(Site site, Section section, PreProcessorPipelineBuilder builder)
        {
            builder.Add("**/*.md", new IncludeProcessor(xref => Resolver(site, section, xref)));
            return Task.CompletedTask;
        }

        private async Task<PipeReader> Resolver(Site site, Section section, Xref xref)
        {
            var targetSection = site.GetSectionByXref(xref, section);

            if (targetSection == null)
                throw new InvalidOperationException($"Could not resolve include. Could not resolve section '{xref}'.");

            var targetItem = targetSection
                .GetContentItem(xref.Path);

            if (targetItem == null)
                throw new InvalidOperationException(
                    $"Could not resolve include. Could not resolve content item '{xref}'.");

            var stream = await targetItem.File.OpenRead();

            if (xref.Query.Any())
            {
                if (xref.Query.TryGetValue("s", out var symbolName))
                {
                    using var reader = new StreamReader(stream);
                    var text = await reader.ReadToEndAsync();

                    var syntaxTree = CSharpSyntaxTree.ParseText(text);
                    var compilation = syntaxTree.GetCompilationUnitRoot();
                    var sourceCode = GetSourceText(compilation, symbolName);

                    var pipe = new Pipe();
                    pipe.Writer.Write(Encoding.UTF8.GetBytes(sourceCode.Text));
                    await pipe.Writer.FlushAsync();
                    await pipe.Writer.CompleteAsync();

                    return pipe.Reader;
                }
            }

            return PipeReader.Create(stream);
        }

        private SourceCode GetSourceText(CompilationUnitSyntax compilationUnit, string symbolName)
        {
            var compilation = CSharpCompilation.Create("Tanka.DocsTool.CodeAnalysis")
                .AddSyntaxTrees(compilationUnit.SyntaxTree);

            var syntaxTree = compilationUnit.SyntaxTree;
            var model = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.GetRoot().DescendantNodes()
                .FirstOrDefault(n =>
                {
                    var symbol = model.GetDeclaredSymbol(n);

                    if (symbol == null)
                        return false;

                    // CS8602: Use the 'symbol' variable that's already checked for null.
                    var name = symbol.ToDisplayString(NameMatchFormat);

                    return name == symbolName;
                });

            if (node != null)
            {
                var position = node.GetLocation().GetLineSpan();
                return new SourceCode
                {
                    Text = node.ToFullString(),
                    FileName = syntaxTree.FilePath,
                    Span = position.Span.ToString()
                };
            }

            return new SourceCode
            {
                Text = symbolName,
                FileName = string.Empty,
                NotFound = true
            };
        }

        /// <summary>
        ///     Formats the names of all types and namespaces in a fully qualified style (including the global alias).
        /// </summary>
        private static SymbolDisplayFormat NameMatchFormat { get; } =
            new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                SymbolDisplayMemberOptions.IncludeContainingType,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
    }
}