using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public class PipelineContext
    {
        public IConfiguration Configuration { get; }

        public List<string> InputFiles { get; } = new List<string>();

        public List<IFile> OutputFiles { get; } = new List<IFile>();

        public PipelineContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }

    public interface IDocument
    {
        string FullName { get; }
    }

    public interface IFileSystem
    {
        Task<IDocument> Read(string path);

        Task Write(string path, IDocument document);
    }

    public class FileSystem : IFileSystem
    {
        public IDocument OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public IDocument OpenWrite(string path)
        {
            return File.OpenWrite(path);
        }
    }
}