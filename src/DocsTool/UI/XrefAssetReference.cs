using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.UI
{
    /// <summary>
    /// Represents an asset referenced via xref:// that needs to be copied from another section
    /// </summary>
    public record XrefAssetReference(
        Xref Xref,
        Section SourceSection,     // Section making the reference  
        Section TargetSection,     // Section containing the asset
        ContentItem TargetItem     // The actual asset file
    );
}