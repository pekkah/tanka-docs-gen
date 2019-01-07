using System.Collections.Generic;
using System.Linq;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Fugu.GenerateDocs
{
    public class SolutionContext
    {
        private readonly AnalyzerManager _analyzerManager;

        private readonly List<Compilation> _compilations = new List<Compilation>();

        public SolutionContext(AnalyzerManager analyzerManager)
        {
            _analyzerManager = analyzerManager;

            foreach (var (file, project) in _analyzerManager.Projects)
            {
                var workspace = project.GetWorkspace();

                foreach (var solutionProject in workspace.CurrentSolution.Projects)
                {
                    var compilation = solutionProject.GetCompilationAsync().Result;       
                    _compilations.Add(compilation);
                }
            }
        }

        public SourceCode GetSourceText(string displayName)
        {
            foreach (var compilation in _compilations)
            {
                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var model = compilation.GetSemanticModel(syntaxTree);
                    var nodes = syntaxTree.GetRoot().DescendantNodes()
                        .Where(n =>
                        {
                            var symbol = model.GetDeclaredSymbol(n);

                            if (symbol == null)
                                return false;

                            var name = model.GetDeclaredSymbol(n)
                                .ToDisplayString(NameMatchFormat);

                            return name == displayName;
                        })
                        .ToList();

                    if (nodes.Count > 0)
                    {
                        var first = nodes.First();
                        var position = first.GetLocation().GetLineSpan();
                        return new SourceCode()
                        {
                            Text = first.ToFullString(),
                            FileName = syntaxTree.FilePath,
                            Span = position.Span.ToString()
                        };
                    }
                }
            }

            return new SourceCode()
            {
                Text = displayName,
                FileName = string.Empty
            };
        }

        /// <summary>
        /// Formats the names of all types and namespaces in a fully qualified style (including the global alias).
        /// </summary>
        private static SymbolDisplayFormat NameMatchFormat { get; } =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
    }
}