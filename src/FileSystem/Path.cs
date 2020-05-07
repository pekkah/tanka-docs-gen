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
    }
}