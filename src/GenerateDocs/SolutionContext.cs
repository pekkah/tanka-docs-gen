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

        public SolutionContext(AnalyzerManager analyzerManager)
        {
            _analyzerManager = analyzerManager;
        }

        public string GetSourceText(string symbolName)
        {
            foreach (var (file, project) in _analyzerManager.Projects)
            {
                var workspace = project.GetWorkspace();
                var compilation = workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result;

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

                            return name == symbolName;
                        })
                        .ToList();

                    if (nodes.Count > 0)
                    {
                        var first = nodes.First();
                        return first.ToFullString();
                    }
                }
            }

            return symbolName;
        }

        /// <summary>
        /// Formats the names of all types and namespaces in a fully qualified style (including the global alias).
        /// </summary>
        private static SymbolDisplayFormat NameMatchFormat { get; } =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
    }
}