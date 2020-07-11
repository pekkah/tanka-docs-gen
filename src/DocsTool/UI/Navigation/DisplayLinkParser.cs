using System;

namespace Tanka.DocsTool.Navigation
{
    public ref struct DisplayLinkParser
    {
        private static readonly char[] SchemeDelimiter = {':', '/', '/'};
        private static readonly char[] Http = {'h', 't', 't', 'p'};
        private static readonly char[] Https = {'h', 't', 't', 'p', 's'};
        private static readonly char[] Xref = {'x', 'r', 'e', 'f'};

        private readonly ReadOnlySpan<char> _link;
        private int _position;

        private DisplayLinkParser(in ReadOnlySpan<char> link)
        {
            _link = link;
            _position = 0;
        }

        public DisplayLink Parse()
        {
            var title = ParseTitle();

            /* (https://uri.invalid) */
            Skip('(');
            var indexOfClose = Unread.IndexOf(')');

            if (indexOfClose == -1)
                throw TokenNotFound(')');

            var span = Unread.Slice(0, indexOfClose);
            var linkParser = new LinkParser(in span);
            var link = linkParser.Parse();

            return new DisplayLink(title, link);
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

        private ReadOnlySpan<char> Unread
        {
            get
            {
                if (_position == _link.Length - 1)
                    return ReadOnlySpan<char>.Empty;

                return _link.Slice(_position);
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

        private string ParseTitle()
        {
            Skip('[');
            var indexOfClose = Unread.IndexOf(']');

            if (indexOfClose == -1)
                throw TokenNotFound(']');

            var title = Unread.Slice(0, indexOfClose);
            Advance(title.Length);
            Skip(']');

            return title.ToString();
        }

        private Exception TokenNotFound(char c)
        {
            return new InvalidOperationException(
                $"Could not find '{c}'. Start position: {_position}");
        }

        public static DisplayLink Parse(string link)
        {
            return Parse(link.AsSpan());
        }

        public static DisplayLink Parse(in ReadOnlySpan<char> link)
        {
            var parser = new DisplayLinkParser(in link);
            return parser.Parse();
        }
    }
}