using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Definitions
{
    public class SectionDefinition
    {
        public string Id { get; set; }
        
        public string Title { get; set; }
        
        public Link? IndexPage { get; set; }

        public Link?[] Nav { get; set; }
    }
}
