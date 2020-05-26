using System;

namespace Tanka.FileSystem
{
    internal static class PathHelpers
    {
        public static string Normalize(string path)
        {
            var original = path.AsSpan();

            if (original.IsEmpty)
                return string.Empty;

            Span<char> target = original.Length < 256 
                ? stackalloc char[original.Length]
                : new char[original.Length];

            // replace '\' with '/'
            for (int i=0; i < original.Length; i++)
            {
                var code = original[i];

                // replace \ with /
                if (code == '\\')
                {
                    target[i] = '/';
                }
                else
                {
                    target[i] = original[i];
                }
            }

            
            // trim starting '/' and "./"
            var trimSequence = new [] {'.', '/'};
            if (target.StartsWith(trimSequence))
            {
                target = target.Slice(2);
            }

            target = target.TrimStart('/');

            return target.ToString();
        }

        public static string Combine(string left, string right)
        {
            return System.IO.Path.Combine(left, right);
        }
    }
}