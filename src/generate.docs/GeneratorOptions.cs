using Microsoft.Extensions.Configuration;

namespace tanka.generate.docs
{
    public class GeneratorOptions
    {
        public string Input { get; set; }

        public string Output { get;set; }

        public string Solution { get; set; }

        public string BasePath { get; set; } = "/";

        public IConfiguration Configuration { get; set; }

        public string Template { get; set; }

        public string CategoryTemplate { get; set; }
    }
}