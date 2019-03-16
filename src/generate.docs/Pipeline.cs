using System;
using System.Diagnostics;
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

        public OrderedList<(string Name, PipelineStep Step)> Steps { get; } = new OrderedList<(string Name, PipelineStep Step)>();

        public async Task Execute()
        {
            var context = new PipelineContext(_options);

            var timer = Stopwatch.StartNew();
            foreach (var (name, pipelineStep) in Steps)
            {
                Console.WriteLine($"{name}");
                Console.WriteLine("-----------------------------------------");
                var stepStart = timer.Elapsed;
                await pipelineStep(context);
                Console.WriteLine($"End: {(timer.Elapsed - stepStart).TotalSeconds}s Elapsed: {timer.Elapsed.TotalSeconds}s");
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.WriteLine($"Total: {timer.Elapsed.TotalSeconds}s");
        }
    }
}