using System.Threading.Tasks;
using Buildalyzer;
using Markdig;
using Tanka.DocsTool.Markdown;

namespace Tanka.DocsTool.Extensions.Roslyn
{
    public class RoslynExtension: Extension
    {
        public override async Task ConfigureMarkdown(DocsMarkdownRenderingContext context, MarkdownPipelineBuilder builder)
        {
            if (context.Section.Definition.Extensions.TryGetValue("roslyn", out var options))
            {
                if (options.TryGetValue("solution", out var solution))
                {
                    var solutionContext = new SolutionContext(new AnalyzerManager(solution.ToString()));
                    await solutionContext.Initialize();
                    builder.Use<CodeExtension>(new CodeExtension(solutionContext));
                }
            }
        }
    }
}