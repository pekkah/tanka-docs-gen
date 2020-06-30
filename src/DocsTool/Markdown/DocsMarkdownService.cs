using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;

namespace Tanka.DocsTool.Markdown
{
    public class DocsMarkdownService
    {
        private readonly MarkdownPipeline _pipeline;

        public DocsMarkdownService(DocsMarkdownRenderingContext section)
        {
            var builder = new MarkdownPipelineBuilder();
            builder.UseYamlFrontMatter();
            builder.Use(new DisplayLinkExtension(section));

            _pipeline = builder.Build();
        }

        public MarkdownDocument Parse(string text)
        {
            var document = MarkdownParser.Parse(text, _pipeline);
            return document;
        }

        public async Task<PageFrontmatter?> RenderPage(Stream input, Stream output)
        {
            using var reader = new StreamReader(input);
            await using var writer = new StreamWriter(output);

            var text = await reader.ReadToEndAsync();
            var markdown = Parse(text);

            var frontmatterBlock = markdown
                .OfType<YamlFrontMatterBlock>()
                .SingleOrDefault();

            var renderer = new HtmlRenderer(writer);
            _pipeline.Setup(renderer);

            renderer.Render(markdown);

            var yaml = frontmatterBlock?.Lines.ToString();
            return yaml?.ParseYaml<PageFrontmatter>();
        }

        public async Task<(MarkdownDocument Document, PageFrontmatter? Page)> ParsePage(Stream input)
        {
            using var reader = new StreamReader(input);

            var text = await reader.ReadToEndAsync();
            var markdown = Parse(text);

            var frontmatterBlock = markdown
                .OfType<YamlFrontMatterBlock>()
                .SingleOrDefault();


            var yaml = frontmatterBlock?.Lines.ToString();
            var page = yaml?.ParseYaml<PageFrontmatter>();

            return (markdown, page);
        }

        public async Task<(string Html, PageFrontmatter? Page)> RenderPage(Stream input)
        {
            using var reader = new StreamReader(input);

            var text = await reader.ReadToEndAsync();
            var markdown = Parse(text);

            var frontmatterBlock = markdown
                .OfType<YamlFrontMatterBlock>()
                .SingleOrDefault();

            var stringWriter = new StringWriter();
            var renderer = new HtmlRenderer(stringWriter);
            _pipeline.Setup(renderer);

            renderer.Render(markdown);

            var yaml = frontmatterBlock?.Lines.ToString();
            var page = yaml?.ParseYaml<PageFrontmatter>();

            return (stringWriter.ToString(), page);
        }
    }
}