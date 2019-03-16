using System.Collections.Generic;
using System.IO;

namespace tanka.generate.docs
{
    public class PipelineContext
    {
        private readonly List<string> _warnings = new List<string>();

        public PipelineContext(GeneratorOptions options)
        {
            Options = options;
        }

        public GeneratorOptions Options { get; }

        public List<FileInfo> InputFiles { get; } = new List<FileInfo>();

        public List<(string path, string content)> OutputFiles { get; } = new List<(string path, string content)>();

        public SolutionContext Solution { get; set; }

        public IEnumerable<string> Warnings => _warnings;

        public void Warning(string message)
        {
            _warnings.Add(message);
        }
    }
}