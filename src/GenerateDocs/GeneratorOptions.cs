using Microsoft.Extensions.Configuration;

namespace Fugu.GenerateDocs
{
    public class GeneratorOptions
    {
        public string Input { get; set; }

        public string Output { get;set; }

        public string Solution { get; set; }

        public IConfiguration Configuration { get; set; }

        public string Template { get; set; }
    }
}