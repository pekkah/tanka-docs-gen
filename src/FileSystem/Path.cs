namespace FileSystem
{
    public readonly struct Path
    {
        private readonly string _path;

        public Path(string path)
        {
            _path = PathHelpers.Normalize(path);
        }

        public Path Combine(Path path)
        {
            return new Path(PathHelpers.Combine(_path, path._path));
        }
        
        public static Path operator /(Path left, Path right)
        {
            return left.Combine(right);
        }
        
        public static implicit operator string(Path path)
        {
            return path._path;
        }

        public static implicit operator Path(string path)
        {
            return new Path(path);
        }
    }
}