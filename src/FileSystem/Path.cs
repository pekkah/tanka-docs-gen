using System;

namespace Tanka.FileSystem
{
    public readonly struct Path
    {
        private readonly string _path;

        public Path(string path)
        {
            _path = PathHelpers.Normalize(path);
        }

        public Path Combine(in Path path)
        {
            return new Path(PathHelpers.Combine(_path, path._path));
        }
        
        public static Path operator /(in Path left, in Path right)
        {
            return left.Combine(right);
        }

        public static Path operator -(in Path left, in Path right)
        {
            var leftSegments = left.Segments();
            var rightSegments = right.Segments();

            int i = 0;
            for (; i < leftSegments.Length; i++)
            {
                if (i >= rightSegments.Length)
                {
                    break;
                }

                var leftSegment = leftSegments[i];
                var rightSegment = rightSegments[i];

                if (leftSegment != rightSegment)
                    break;
            }

            var resultSegments = leftSegments.Slice(i);
            var resultPath = string.Join('/', resultSegments.ToArray());
            return resultPath;
        }

        public ReadOnlySpan<string> Segments()
        {
            return _path.Split('/');
        }

        public static implicit operator string(in Path path)
        {
            return path._path;
        }

        public static implicit operator Path(string path)
        {
            return new Path(path);
        }

        public Path GetRelative(in Path relativeTo)
        {
            return System.IO.Path.GetRelativePath(relativeTo, _path);
        }

        public override string ToString()
        {
            return _path;
        }

        public bool IsSubPathOf(in Path path)
        {
            var parentPath = path.GetDirectoryPath().ToString();
            var directoryPath =  GetDirectoryPath().ToString();

            return directoryPath.StartsWith(parentPath);
        }

        public Path GetDirectoryPath()
        {
            var path = System.IO.Path.GetDirectoryName(_path);

            if (string.IsNullOrEmpty(path))
                return "";

            return path;
        }

        public Path GetFileName()
        {
            return System.IO.Path.GetFileName(_path);
        }

        public Path GetExtension()
        {
            return System.IO.Path.GetExtension(_path);
        }
    }
}