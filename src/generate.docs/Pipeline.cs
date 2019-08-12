using System;
using System.Diagnostics;
using System.Linq;
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
                Console.WriteLine($"Total: {(timer.Elapsed - stepStart).TotalSeconds}s Step: {timer.Elapsed.TotalSeconds}s");
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.WriteLine($"Total: {timer.Elapsed.TotalSeconds}s");
            Console.WriteLine();

            if (context.Warnings.Any())
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warnings");
                Console.WriteLine("-----------------------------------------");
                foreach (var warning in context.Warnings)
                {
                    Console.WriteLine(warning);
                }

                Console.ForegroundColor = color;
            }
            Console.WriteLine();
        }
    }
}