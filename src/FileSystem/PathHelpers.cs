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

            int start = 0;

            // if path starts with / or . remove it
            if (original.Length > 0 && original[start] == '.' || original[start] == '/')
                start++;

            // if path starts with /
            if (original.Length > start && original[start] == '/')
                start++;

            Span<char> target = stackalloc char[original.Length-start];

            for (int i = start; i < original.Length-start; i++)
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

            return target.ToString();
        }

        public static string Combine(string left, string right)
        {
            return System.IO.Path.Combine(left, right);
        }
    }
}