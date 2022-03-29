using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Markdig.Helpers;

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
            /* xref://section-id@releases/1.0.0:path/to/file.md */
            var indexOfClose = Unread.Length;

            var span = Unread.Slice(0, indexOfClose);

            // xref | http | https | etc
            string uriOrPath;
            string? sectionId = null;
            string? version = null;
            var scheme = ParseScheme();

            if (IsXref(scheme))
            {
                ReadOnlySpan<char> uriOrPathSpan;
                var maybeSectionIdAndPath = Unread;

                // has sectionId?
                var indexOfSectionIdSeparator = maybeSectionIdAndPath
                    .IndexOf(':');

                if (indexOfSectionIdSeparator != -1)
                {
                    var sectionSpan = maybeSectionIdAndPath.Slice(0, indexOfSectionIdSeparator);

                    var versionSeparator = sectionSpan.IndexOf('@');

                    if (versionSeparator != -1)
                    {
                        version = sectionSpan.Slice(versionSeparator)
                            .TrimStart('@')
                            .ToString();
                        
                        sectionId = sectionSpan.Slice(0, versionSeparator)
                            .ToString();
                    }
                    else
                    {
                        sectionId = sectionSpan.ToString();
                    }

                    uriOrPathSpan = maybeSectionIdAndPath.Slice(sectionSpan.Length + 1);
                }
                else
                {
                    uriOrPathSpan = maybeSectionIdAndPath;
                }

                var indexOfQueryStart = uriOrPathSpan.IndexOf('?');

                if (indexOfQueryStart != -1)
                {
                    var querySpan = uriOrPathSpan.Slice(indexOfQueryStart);
                    var query = ParseQuery(querySpan);

                    return new Link(new Xref(version, sectionId, uriOrPathSpan.Slice(0, indexOfQueryStart).ToString(), query));
                }

                return new Link(new Xref(version, sectionId, uriOrPathSpan.ToString()));
            }

            uriOrPath = span.ToString();

            return new Link(uriOrPath);
        }

        private IReadOnlyDictionary<string, string> ParseQuery(in ReadOnlySpan<char> querySpan)
        {
            ReadOnlySpan<char> unread = querySpan;
            if (querySpan[0] == '?')
                unread = unread.Slice(1);
                
            var result = new Dictionary<string ,string>();
            while (!unread.IsEmpty)
            {
                var indexOfAndOrEnd = unread.IndexOf('&');
                if (indexOfAndOrEnd == -1)
                    indexOfAndOrEnd = unread.Length;

                var kvSpan = unread.Slice(0, indexOfAndOrEnd);
                var indexOfEquals = kvSpan.IndexOf('=');
                var key = kvSpan.Slice(0, indexOfEquals)
                    .ToString();
                var value = kvSpan.Slice(indexOfEquals + 1)
                    .ToString();

                result[key] = value;

                unread = unread.Slice(indexOfAndOrEnd).TrimStart('&');
            }

            return result;
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

        public static Link Parse(in ReadOnlySpan<byte> link)
        {
            var size = Encoding.UTF8.GetCharCount(link);
            Span<char> chars = new char[size];
            Encoding.UTF8.GetChars(link, chars);
            var parser = new LinkParser(chars);
            return parser.Parse();
        }
    }
}