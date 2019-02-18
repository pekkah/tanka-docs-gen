using System.Threading.Tasks;
using Markdig.Helpers;

namespace tanka.generate.docs
{
    public class Pipeline
    {
        private readonly GeneratorOptions _options;

        public Pipeline(GeneratorOptions options)
        {
            _options = options;
        }

        public OrderedList<PipelineStep> Steps { get; } = new OrderedList<PipelineStep>();

        public async Task Execute()
        {
            var context = new PipelineContext(_options);

            foreach (var pipelineStep in Steps) await pipelineStep(context);
        }
    }
}