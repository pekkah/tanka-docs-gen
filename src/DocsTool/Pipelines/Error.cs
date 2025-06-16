using Tanka.DocsTool.Catalogs;

namespace Tanka.DocsTool.Pipelines;

public record Error
{
    public Error(string message, ContentItem? contentItem = null)
    {
        Message = message;
        ContentItem = contentItem;
    }

    public string Message { get; }
    public ContentItem? ContentItem { get; }
}; 