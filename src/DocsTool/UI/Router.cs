using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;

namespace Tanka.DocsTool.UI
{
    public class Router
    {
        public Path GetOutputPath(SectionModel section, Xref xref, string extension)
        {
            var path = new Path(xref.Path).ChangeExtension(extension);

            if (xref.SectionId == null || xref.Version == null)
                return $"{section.Version}/{section.Definition.Id}/{path}";

            return $"{xref.Version}/{xref.SectionId}/{path}";
        }
    }
}