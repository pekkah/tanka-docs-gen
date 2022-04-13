using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tanka.FileSystem.Memory
{
    public class InMemoryFileSystem : IFileSystem, IDisposable
    {
        private readonly ConcurrentDictionary<FileSystemPath, InMemoryDirectory> _directories =
            new ConcurrentDictionary<FileSystemPath, InMemoryDirectory>();

        private readonly ConcurrentDictionary<FileSystemPath, InMemoryFile> _files =
            new ConcurrentDictionary<FileSystemPath, InMemoryFile>();


        public InMemoryFileSystem()
        {
            _directories[""] = new InMemoryDirectory(this, "");
        }

        public void Dispose()
        {
            foreach (var file in _files) file.Value?.Dispose();
        }

        public ValueTask<IReadOnlyFile?> GetFile(FileSystemPath path)
        {
            if (_files.TryGetValue(path, out var file)) return new ValueTask<IReadOnlyFile?>(file);

            return new ValueTask<IReadOnlyFile?>(default(IReadOnlyFile?));
        }

        public ValueTask<IReadOnlyDirectory?> GetDirectory(FileSystemPath path)
        {
            if (_directories.TryGetValue(path, out var dir)) return new ValueTask<IReadOnlyDirectory?>(dir);

            return new ValueTask<IReadOnlyDirectory?>(default(IReadOnlyDirectory?));
        }

        public async IAsyncEnumerable<IFileSystemNode> Enumerate(FileSystemPath path)
        {
            await Task.Delay(0);
            foreach (var file in _files)
            {
                var filepath = file.Key;
                var filename = file.Key.GetFileName();

                if (filepath - path == filename)
                    yield return file.Value;
            }


            foreach (var directory in _directories)
            {
                // do not enumerate root path again
                if (directory.Key == "")
                    continue;

                var directorypath = directory.Key;
                var directoryName = directory.Key.Segments()[^1];

                if (directorypath - path == directoryName)
                    yield return directory.Value;
            }
        }

        public ValueTask<IFile> GetOrCreateFile(FileSystemPath path)
        {
            var file = _files.GetOrAdd(path, p => new InMemoryFile(this, p));
            return new ValueTask<IFile>(file);
        }

        public ValueTask<IDirectory> GetOrCreateDirectory(FileSystemPath path)
        {
            var dir = _directories.GetOrAdd(path, p => new InMemoryDirectory(this, p));
            return new ValueTask<IDirectory>(dir);
        }

        public ValueTask<IFileSystem> Mount(FileSystemPath path)
        {
            return new ValueTask<IFileSystem>(new InMemoryFileSystem());
        }

        public Task DeleteDir(FileSystemPath path)
        {
            throw new NotImplementedException();
        }

        public Task CleanDirectory(FileSystemPath path)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryDirectory : IDirectory
    {
        private readonly InMemoryFileSystem _fileSystem;

        public InMemoryDirectory(InMemoryFileSystem fileSystem, in FileSystemPath path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public FileSystemPath Path { get; }


        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.Enumerate(Path);
        }
    }

    public class InMemoryFile : IFile, IDisposable
    {
        private readonly InMemoryFileSystem _fileSystem;

        private InMemoryFileStream _stream;

        public InMemoryFile(InMemoryFileSystem fileSystem, FileSystemPath path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public void Dispose()
        {
            _stream?.DisposeInternal();
        }

        public FileSystemPath Path { get; }

        public ValueTask<Stream> OpenRead()
        {
            _stream ??= new InMemoryFileStream();
            _stream.Position = 0;
            return new ValueTask<Stream>(_stream);
        }

        public ValueTask<Stream> OpenWrite()
        {
            if (_stream != null)
            {
                _stream.DisposeInternal();
                _stream = new InMemoryFileStream();
            }
            else if (_stream == null)
            {
                _stream = new InMemoryFileStream();
            }

            _stream.Position = 0;
            return new ValueTask<Stream>(_stream);
        }
    }

    internal class InMemoryFileStream : Stream, IDisposable
    {
        private readonly MemoryStream _actualStream;

        public InMemoryFileStream()
        {
            _actualStream = new MemoryStream();
        }

        public override void Flush()
        {
            _actualStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _actualStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _actualStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _actualStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _actualStream.Write(buffer, offset, count);
        }

        public override bool CanRead => _actualStream.CanRead;
        public override bool CanSeek => _actualStream.CanSeek;
        public override bool CanWrite => _actualStream.CanWrite;
        public override long Length => _actualStream.Length;

        public override long Position
        {
            get => _actualStream.Position;
            set => _actualStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
        }

        internal void DisposeInternal()
        {
            _actualStream?.Dispose();
        }
    }
}