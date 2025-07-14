using System;

namespace Tanka.FileSystem
{
    internal static class PathHelpers
    {
        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // Step 1: Replace all backslashes with forward slashes.
            string normalizedPath = path.Replace('\\', '/');

            // Step 2: Determine if it's intended to be a Unix-style absolute path.
            bool isUnixAbsolute = normalizedPath.StartsWith("/");

            // Step 3: Handle "./" at the beginning of path segments.
            if (isUnixAbsolute && normalizedPath.Length >= 2 && normalizedPath[1] == '.')
            {
                if (normalizedPath.Length == 2) // path is "/."
                {
                    // Path.GetFullPath("/.") on Linux results in "/"
                    // Let's ensure it becomes "/"
                    normalizedPath = "/";
                }
                else if (normalizedPath[2] == '/') // path is "/./<something>"
                {
                    normalizedPath = "/" + normalizedPath.Substring(3);
                }
                // else path is "/.filename", which is a valid filename starting with "." in root.
            }
            else if (!isUnixAbsolute && normalizedPath.StartsWith("./"))
            {
                if (normalizedPath.Length == 2) // path is "./"
                {
                    normalizedPath = "";
                }
                else
                {
                    normalizedPath = normalizedPath.Substring(2);
                }
            }

            // Step 4: Handle leading slashes.
            if (isUnixAbsolute)
            {
                int firstNonSlash = 0;
                while (firstNonSlash < normalizedPath.Length && normalizedPath[firstNonSlash] == '/')
                {
                    firstNonSlash++;
                }

                if (firstNonSlash == normalizedPath.Length && firstNonSlash > 0) // All slashes e.g. "///"
                {
                    normalizedPath = "/";
                }
                else if (firstNonSlash > 0) // Starts with multiple slashes e.g. "//foo"
                {
                    normalizedPath = "/" + normalizedPath.Substring(firstNonSlash);
                }
                // If firstNonSlash is 0, it means it didn't start with a slash,
                // which contradicts isUnixAbsolute. This case should ideally not be hit
                // if isUnixAbsolute is true. If path was just "/", firstNonSlash is 1, Substring(1) is empty, result is "/".
            }
            else
            {
                normalizedPath = normalizedPath.TrimStart('/');
            }
            return normalizedPath;
        }

        public static string Combine(string left, string right)
        {
            if (string.IsNullOrEmpty(left)) return Normalize(right);
            if (string.IsNullOrEmpty(right)) return Normalize(left);

            string normLeft = Normalize(left);
            string normRight = Normalize(right);

            if (normRight.StartsWith("/"))
                return normRight;

            if (normLeft.EndsWith("/"))
                return normLeft + normRight;

            // Handle case where normLeft might be empty after normalization (e.g. if it was "./")
            if (string.IsNullOrEmpty(normLeft))
                return normRight;

            return normLeft + "/" + normRight;
        }
    }
}