using System.IO.Pipelines;
using DotNet.Globbing;

namespace Tanka.DocsTool.Extensions
{
    public class PreProcessorPipelineBuilder
    {
        private readonly Dictionary<string, List<IPreProcessor>> _preProcessors =
            new Dictionary<string, List<IPreProcessor>>();

        public PreProcessorPipelineBuilder Add(string pattern, IPreProcessor processor)
        {
            if (!_preProcessors.TryGetValue(pattern, out var preProcessorsByPattern))
                _preProcessors[pattern] = new List<IPreProcessor>();

            _preProcessors[pattern].Add(processor);

            return this;
        }

        public Func<FileSystemPath, PipeReader, Task<PipeReader>> Build()
        {
            var dictionary = BuildDictionary();

            return async (FileSystemPath path, PipeReader reader) =>
            {
                var matchingProcessors = dictionary
                    .Where(kv => kv.Key.IsMatch(path))
                    .SelectMany(kv => kv.Value);

                var readPipe = new Pipe();
                await reader.CopyToAsync(readPipe.Writer);
                await readPipe.Writer.FlushAsync();
                await readPipe.Writer.CompleteAsync();

                foreach (var preProcessor in matchingProcessors)
                {
                    var writePipe = new Pipe();
                    await preProcessor.Process(new IncludeProcessorContext(readPipe.Reader, writePipe.Writer));
                    readPipe = writePipe;
                }

                return readPipe.Reader;
            };
        }

        private IReadOnlyDictionary<Glob, List<IPreProcessor>> BuildDictionary() => _preProcessors
                .ToDictionary(
                    kv => Glob.Parse(kv.Key),
                    kv => kv.Value
                );
    }
}