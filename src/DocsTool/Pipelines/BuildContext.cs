using Tanka.DocsTool.Catalogs;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Pipelines;

public record BuildContext(SiteDefinition SiteDefinition, FileSystemPath WorkPath)
{
    private readonly List<Error> _errors = new();
    private readonly List<Error> _warnings = new();

    public IReadOnlyCollection<Error> Errors => _errors.AsReadOnly();
    
    public IReadOnlyCollection<Error> Warnings => _warnings.AsReadOnly();

    public bool HasErrors => _errors.Count > 0;

    public void Add(Error error, bool isWarning = false)
    {
        if (isWarning)
            _warnings.Add(error);
        else
            _errors.Add(error);
    }

    public IFileSystem FileSystem { get; set; }

    public IFileSystem CacheFileSystem { get; set; }

    public GitFileSystemRoot GitRoot { get; set; }

    public IFileSystem RawCache { get; set; }

    public IFileSystem PageCache { get; set; }

    public IFileSystem OutputFs { get; set; }

    public Catalog Catalog { get; } = new Catalog();

    public SiteBuilder SiteBuilder { get; } = new SiteBuilder(SiteDefinition);

    public IReadOnlyCollection<Section> Sections { get; set; }

    public Site? Site { get; set; }
}