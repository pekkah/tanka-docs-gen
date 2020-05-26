using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public interface IContentClassifier
    {
        string Classify(IReadOnlyFile file);
    }
}