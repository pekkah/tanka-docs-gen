using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public class PipelineContext
    {
        public PipelineContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public List<FileInfo> InputFiles { get; } = new List<FileInfo>();

        public List<(string path, string content)> OutputFiles { get; } = new List<(string path, string content)>();
    }
}