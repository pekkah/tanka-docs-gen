using System.Threading.Tasks;
using Markdig;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.Extensions
{
    public abstract class Extension
    {
        public virtual Task ConfigureMarkdown(DocsMarkdownRenderingContext context, MarkdownPipelineBuilder builder)
        {
            return Task.CompletedTask;
        }

        public virtual Task ConfigurePreProcessors(Site site, Section section, PreProcessorPipelineBuilder builder)
        {
            return Task.CompletedTask;
        }
    }
}