using System.Collections.Generic;
using System.IO;

namespace tanka.generate.docs
{
    public class PipelineContext
    {
        public PipelineContext(GeneratorOptions options)
        {
            Options = options;
        }

        public GeneratorOptions Options { get; }

        public List<FileInfo> InputFiles { get; } = new List<FileInfo>();

        public List<(string path, string content)> OutputFiles { get; } = new List<(string path, string content)>();

        public SolutionContext Solution { get; set; }
    }
}