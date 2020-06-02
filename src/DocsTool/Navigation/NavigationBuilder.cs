using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.Navigation
{
    public class NavigationBuilder
    {
        private List<NavigationItem> _items = new List<NavigationItem>();

        public NavigationBuilder()
        {
            
        }

        public static IReadOnlyCollection<NavigationItem> Build(MarkdownDocument[] documents)
        {
            var builder = new NavigationBuilder();

            foreach (var document in documents)
            {
                builder.Add(document);
            }

            return builder.Build();
        }

        public IReadOnlyCollection<NavigationItem> Build()
        {
            return _items;
        }

        public NavigationBuilder Add(MarkdownDocument document)
        {
            foreach (var block in document)
            {
                switch (block)
                {
                    case ListBlock listBlock:
                        _items.AddRange(ParseListBlock(listBlock));
                        break;
                    default:
                        throw new InvalidOperationException($"Document contains invalid navigation document syntax at {block.ToPositionText()}. Expected list block.");
                }
            }

            return this;
        }

        private IEnumerable<NavigationItem> ParseListBlock(ListBlock listBlock)
        {
            foreach (var listItemBlock in listBlock.OfType<ListItemBlock>())
            {
                var item =  ParseListItemBlock(listItemBlock);

                if (item.Equals(default(NavigationItem)) == false)
                    yield return item;
            }
        }

        private NavigationItem ParseListItemBlock(ListItemBlock listItemBlock)
        {
            DisplayLink rootLink = default;
            List<NavigationItem> children = new List<NavigationItem>();
            foreach (var listItemChild in listItemBlock)
            {
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
            }

            if (rootLink.Equals(default(DisplayLink)))
                return default;

            return new NavigationItem(rootLink, children);
        }

        private DisplayLink ParseParagraph(ParagraphBlock paragraphBlock)
        {
            switch (paragraphBlock.Inline.FirstChild)
            {
                case DisplayLinkInline inlineLink:
                    return ParseInlineLink(inlineLink);
                default:
                    throw new InvalidOperationException($"Document contains invalid navigation document syntax at {paragraphBlock.Inline.FirstChild.ToPositionText()}. Expected inline link.");
            }
        }

        private DisplayLink ParseInlineLink(DisplayLinkInline inlineLink)
        {
            return inlineLink.DisplayLink;
        }
    }
}