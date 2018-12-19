﻿using System.Threading.Tasks;
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

            var pipeline = new Pipeline(configuration)
            {
                Steps =
                {
                    EnumerateFiles(configuration),
                    TransformInputFilesToHtmlOutputFiles(configuration),
                    WriteFiles(configuration)
                }
            };

            await pipeline.Execute();
        }
    }
}