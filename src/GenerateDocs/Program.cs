using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using static Fugu.GenerateDocs.Generator;

namespace Fugu.GenerateDocs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddYamlFile("fugu-docs.yaml", true)
                .AddCommandLine(args)
                .Build();

            var options = configuration.Get<GeneratorOptions>();
            options.Configuration = configuration;

            var pipeline = new Pipeline(options)
            {
                Steps =
                {
                    AnalyzeSolution(options),
                    EnumerateFiles(options),
                    TransformInputFilesToHtmlOutputFiles(options),
                    GenerateToc(options),
                    AddHtmlLayout(options),
                    Assets(".js", ".css"),
                    WriteFiles(options)
                }
            };

            await pipeline.Execute();
        }
    }
}