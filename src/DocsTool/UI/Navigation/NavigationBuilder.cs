using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Syntax;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.UI.Navigation
{
    public class NavigationBuilder
    {
        private readonly List<NavigationItem> _items = new List<NavigationItem>();
        private readonly DocsMarkdownService _markdownService;

        public NavigationBuilder(DocsMarkdownService markdownService, DocsSiteRouter router)
        {
            _markdownService = markdownService;
        }

        public IReadOnlyCollection<NavigationItem> Build()
        {
            return _items;
        }

        public NavigationBuilder Add(MarkdownDocument document)
        {
            foreach (var block in document)
                switch (block)
                {
                    case ListBlock listBlock:
                        _items.AddRange(ParseListBlock(listBlock));
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Document contains invalid navigation document syntax at {block.ToPositionText()}. Expected list block.");
                }

            return this;
        }

        public NavigationBuilder Add(string[] menu)
        {
            var documents = menu.Select(_markdownService.Parse)
                .ToList();

            return Add(documents);
        }

        public NavigationBuilder Add(IEnumerable<MarkdownDocument> documents)
        {
            foreach (var document in documents)
                Add(document);

            return this;
        }

        private IEnumerable<NavigationItem> ParseListBlock(ListBlock listBlock)
        {
            foreach (var listItemBlock in listBlock.OfType<ListItemBlock>())
            {
                var item = ParseListItemBlock(listItemBlock);

                if (item.Equals(default(NavigationItem)) == false)
                    yield return item;
            }
        }

        private NavigationItem ParseListItemBlock(ListItemBlock listItemBlock)
        {
            DisplayLink rootLink = default;
            List<NavigationItem> children = new List<NavigationItem>();
            foreach (var listItemChild in listItemBlock)
                switch (listItemChild)
                {
                    case ParagraphBlock paragraphBlock:
                        rootLink = ParseParagraph(paragraphBlock);
                        break;
                    case ListBlock listBlock:
                        children.AddRange(ParseListBlock(listBlock));
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Document contains invalid navigation document syntax at {listItemBlock.ToPositionText()}. Expected paragraph.");
                }

            if (rootLink.Equals(default(DisplayLink)))
                return default;

            return new NavigationItem(rootLink, children);
        }

        private DisplayLink ParseParagraph(ParagraphBlock paragraphBlock)
        {
            // CS8602: paragraphBlock.Inline could be null
            if (paragraphBlock.Inline?.FirstChild is DisplayLinkInline inlineLink)
            {
                return ParseInlineLink(inlineLink);
            }
            
            throw new InvalidOperationException(
                $"Document contains invalid navigation document syntax at {paragraphBlock.ToPositionText()}. Expected inline link.");
        }

        private DisplayLink ParseInlineLink(DisplayLinkInline inlineLink)
        {
            return inlineLink.DisplayLink;
        }
    }
}