using System.Threading.Tasks;
using Markdig.Helpers;
using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public class Pipeline
    {
        private readonly IConfiguration _configuration;

        public OrderedList<PipelineStep> Steps { get; } = new OrderedList<PipelineStep>();

        public Pipeline(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Execute()
        {
            var context = new PipelineContext(_configuration);

            foreach (var pipelineStep in Steps)
            {
                await pipelineStep(context);
            }
        }
    }
}