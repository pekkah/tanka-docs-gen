using System;

namespace Tanka.FileSystem
{
    internal static class PathHelpers
    {
        public static string Normalize(string path)
        {
            var original = path.AsSpan();
            Span<char> target = stackalloc char[original.Length];

            for(int i= 0; i < original.Length; i++)
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