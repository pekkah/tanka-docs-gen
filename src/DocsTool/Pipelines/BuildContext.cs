using Tanka.DocsTool.Catalogs;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Pipelines;

public record BuildContext(SiteDefinition SiteDefinition, FileSystemPath WorkPath)
{
    public IFileSystem? FileSystem { get; set; } // CS8618

    public IFileSystem? CacheFileSystem { get; set; } // CS8618

    public GitFileSystemRoot? GitRoot { get; set; } // CS8618

    public IFileSystem? RawCache { get; set; } // CS8618

    public IFileSystem? PageCache { get; set; } // CS8618

    public IFileSystem? OutputFs { get; set; } // CS8618

    public Catalog Catalog { get; } = new Catalog();

    public SiteBuilder SiteBuilder { get; } = new SiteBuilder(SiteDefinition);

    public IReadOnlyCollection<Section>? Sections { get; set; } // CS8618

    public Site? Site { get; set; }
}