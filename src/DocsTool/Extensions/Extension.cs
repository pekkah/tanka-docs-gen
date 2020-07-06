using System.Threading.Tasks;
using Markdig;
using Tanka.DocsTool.Markdown;

namespace Tanka.DocsTool.Extensions.Roslyn
{
    public abstract class Extension
    {
        public virtual Task ConfigureMarkdown(DocsMarkdownRenderingContext context, MarkdownPipelineBuilder builder)
        {
            return Task.CompletedTask;
        }
    }
}