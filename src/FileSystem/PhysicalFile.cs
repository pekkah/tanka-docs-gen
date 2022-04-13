using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public class Metadata : Dictionary<string, string>
    {
        public string Author
        {
            get => this[nameof(Author)];
            set => this[nameof(Author)] = value;
        }

        public DateTimeOffset ModifiedOn
        {
            get => DateTimeOffset.ParseExact(this[nameof(ModifiedOn)], "O", DateTimeFormatInfo.InvariantInfo);
            set => this[nameof(ModifiedOn)] = value.ToString("O");
        }
    }

    internal class PhysicalFile : IFile
    {
        private readonly string _fullPath;

        public PhysicalFile(FileSystemPath path, string fullPath)
        {
            Path = path;
            _fullPath = fullPath;
            Metadata = new Metadata()
            {
                ModifiedOn = File.GetLastWriteTimeUtc(fullPath),
                Author = string.Empty
            };
        }

        public FileSystemPath Path { get; }

        public IReadOnlyDictionary<string, string> Metadata { get; }

        public ValueTask<Stream> OpenRead()
        {
            var stream = File.OpenRead(_fullPath);
            return new ValueTask<Stream>(stream);
        }

        public ValueTask<Stream> OpenWrite()
        {
            var stream = new FileStream(_fullPath, FileMode.Create);
            return new ValueTask<Stream>(stream);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}