using System;

namespace Tanka.DocsTool.Navigation
{
    public ref struct LinkParser
    {
        private static readonly char[] SchemeDelimiter = {':', '/', '/'};
        private static readonly char[] Http = {'h', 't', 't', 'p'};
        private static readonly char[] Https = {'h', 't', 't', 'p', 's'};
        private static readonly char[] Xref = {'x', 'r', 'e', 'f'};

        private readonly ReadOnlySpan<char> _link;
        private int _position;

        public LinkParser(in ReadOnlySpan<char> link)
        {
            _link = link;
            _position = 0;
        }

        public Link Parse()
        {
            /* https://uri.invalid */
            /* xref://page.md */
            /* xref://section:page.md */
            var indexOfClose = Unread.Length;

            var span = Unread.Slice(0, indexOfClose);

            // xref | http | https | etc
            string uriOrPath;
            string? sectionId = null;
            var scheme = ParseScheme();

            if (IsXref(scheme))
            {
                var maybeSectionIdAndPath = Unread;

                // has sectionId?
                var indexOfSectionIdSeparator = maybeSectionIdAndPath
                    .IndexOf(':');

                if (indexOfSectionIdSeparator != -1)
                {
                    sectionId = maybeSectionIdAndPath.Slice(0, indexOfSectionIdSeparator)
                        .ToString();

                    uriOrPath = maybeSectionIdAndPath.Slice(sectionId.Length + 1)
                        .ToString();
                }
                else
                {
                    uriOrPath = maybeSectionIdAndPath.ToString();
                }

                return new Link(new Xref(sectionId, uriOrPath));
            }

            uriOrPath = span.ToString();

            return new Link(uriOrPath);
        }

        private ReadOnlySpan<char> Unread
        {
            get
            {
                if (_position == _link.Length - 1)
                    return ReadOnlySpan<char>.Empty;

                return _link.Slice(_position);
            }
        }

        private ReadOnlySpan<char> Read
        {
            get
            {
                if (_position < 0)
                    return ReadOnlySpan<char>.Empty;

                return _link.Slice(0, _position + 1);
            }
        }

        private char Current => _link[_position];

        private bool Advance()
        {
            if (_position == _link.Length - 1)
                return false;

            _position++;
            return true;
        }

        private bool Advance(int count)
        {
            if (_position + count == _link.Length)
                return false;

            _position += count;
            return true;
        }

        private void Skip(char expected)
        {
            if (Current != expected)
                throw new InvalidOperationException(
                    $"Expected to skip '{expected}' but current '{Current}");

            Advance();
        }

        private bool IsXref(in ReadOnlySpan<char> scheme)
        {
            if (scheme.SequenceEqual(Xref))
                return true;

            return false;
        }

        private ReadOnlySpan<char> ParseScheme()
        {
            var indexOfClose = Unread.IndexOf(SchemeDelimiter);

            if (indexOfClose == -1)
                throw TokenNotFound(':');

            var scheme = Unread.Slice(0, indexOfClose);
            Advance(scheme.Length);

            Skip(':');
            Skip('/');
            Skip('/');
            return scheme;
        }

        private Exception TokenNotFound(char c)
        {
            return new InvalidOperationException(
                $"Could not find '{c}'. Start position: {_position}");
        }

        public static Link Parse(string link)
        {
            return Parse(link.AsSpan());
        }

        public static Link Parse(in ReadOnlySpan<char> link)
        {
            var parser = new LinkParser(in link);
            return parser.Parse();
        }
    }
}