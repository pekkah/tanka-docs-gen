using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.Security;
using Tanka.FileSystem;

namespace Tanka.DocsTool;

public class AugmentFilesSections : IMiddleware
{
    private readonly IAnsiConsole _console;
    private readonly ILogger<AugmentFilesSections> _logger;

    public string Name => "Augment files sections";

    public AugmentFilesSections(IAnsiConsole console)
    {
        _console = console;
        _logger = Infra.LoggerFactory.CreateLogger<AugmentFilesSections>();
    }

    public async Task Invoke(PipelineStep next, BuildContext context)
    {
        if (context.HasErrors)
        {
            _console.LogInformation($"Skipping {Name} because of previous errors.");
            await next(context);
            return;
        }

        // Find sections with type: files and augment catalog with working directory content
        var filesSections = context.Sections?.Where(s => s.Definition.Type?.ToLowerInvariant() == "files").ToList();
        
        if (filesSections?.Any() == true)
        {
            _logger.LogInformation($"Found {filesSections.Count} files sections, augmenting with working directory content");
            
            await _console.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new ItemCountColumn(),
                    new ElapsedTimeColumn(),
                    new SpinnerColumn()
                )
                .StartAsync(async progress =>
                {
                    var augmenter = new FilesSectionAugmenter(context.FileSystem, _console);
                    await augmenter.AugmentCatalog(context.Catalog, filesSections, progress);
                });
        }
        else
        {
            _logger.LogDebug("No files sections found, skipping augmentation");
        }

        await next(context);
    }
}

public class FilesSectionAugmenter
{
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;
    private readonly IContentClassifier _classifier;
    private readonly ConfigurableFileSecurityFilter _securityFilter;
    private readonly ILogger<FilesSectionAugmenter> _logger;

    public FilesSectionAugmenter(IFileSystem fileSystem, IAnsiConsole console, ILogger<FilesSectionAugmenter>? logger = null)
    {
        _fileSystem = fileSystem;
        _console = console;
        _classifier = new MimeDbClassifier();
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<FilesSectionAugmenter>.Instance;
        _securityFilter = new ConfigurableFileSecurityFilter(logger: Microsoft.Extensions.Logging.Abstractions.NullLogger<ConfigurableFileSecurityFilter>.Instance);
    }

    public async Task AugmentCatalog(Catalog catalog, IEnumerable<Section> filesSections, ProgressContext progress)
    {
        foreach (var section in filesSections)
        {
            var task = progress.AddTask($"Files section: {section.Id}@{section.Version}", maxValue: 0);
            
            try
            {
                await AugmentSectionWithWorkingDirectoryContent(catalog, section, task);
            }
            catch (Exception ex)
            {
                _console.LogError($"Failed to augment files section '{section.Id}@{section.Version}': {ex.Message}");
            }
            finally
            {
                task.StopTask();
            }
        }
    }

    private async Task AugmentSectionWithWorkingDirectoryContent(Catalog catalog, Section section, ProgressTask task)
    {
        // Get the section's directory in the working file system
        var sectionDirectory = await _fileSystem.GetDirectory(section.Path);
        if (sectionDirectory == null)
        {
            _console.LogWarning($"Section directory '{section.Path}' not found in working directory for files section '{section.Id}'");
            return;
        }

        _console.LogInformation($"Augmenting files section '{section.Id}@{section.Version}' from working directory: {section.Path}");

        // Stream content items directly from working directory
        var contentItems = CollectWorkingDirectoryContent(sectionDirectory, section, task);
        await catalog.Add(contentItems);
        
        _console.LogInformation($"Augmented files section '{section.Id}' with working directory content");
    }

    private async IAsyncEnumerable<ContentItem> CollectWorkingDirectoryContent(
        IReadOnlyDirectory directory, 
        Section section, 
        ProgressTask task)
    {
        await foreach (var node in directory.Enumerate())
        {
            switch (node)
            {
                case IReadOnlyFile file:
                    // Apply security filtering to prevent sensitive file exposure
                    if (_securityFilter.ShouldExclude(file))
                    {
                        _logger.LogDebug("Excluding file due to security filter: {FilePath}", file.Path);
                        break;
                    }

                    // Create a FileSystemContentSource for this specific file
                    var contentSource = new FileSystemContentSource(_fileSystem, section.Version, section.Path);
                    var contentType = _classifier.Classify(file);
                    var contentItem = new ContentItem(contentSource, contentType, file);
                    
                    task.Increment(1);
                    yield return contentItem;
                    break;
                    
                case IReadOnlyDirectory subDirectory:
                    // Apply security filtering to directories
                    if (_securityFilter.ShouldExclude(subDirectory))
                    {
                        _logger.LogDebug("Excluding directory due to security filter: {DirectoryPath}", subDirectory.Path);
                        break;
                    }

                    // Recursively yield from subdirectories
                    await foreach (var subItem in CollectWorkingDirectoryContent(subDirectory, section, task))
                    {
                        yield return subItem;
                    }
                    break;
            }
        }
    }
}