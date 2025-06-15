using Tanka.FileSystem.Git;

public class InitializeFileSystems : IMiddleware
{
    private readonly IAnsiConsole _console;

    public string Name => "Initialize file systems";

    public InitializeFileSystems(IAnsiConsole console)
    {
        _console = console;
    }

    public async Task Invoke(PipelineStep next, BuildContext context)
    {
        await _console.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Creating file systems", async status =>
            {
                string gitDiscoveryPath = Path.GetFullPath(Directory.GetCurrentDirectory()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                _console.LogInformation($"Using path for Git discovery: '{gitDiscoveryPath}'. (Derived from Directory.GetCurrentDirectory())");

                context.GitRoot = GitFileSystemRoot.Discover(gitDiscoveryPath);
                _console.LogInformation($"Initialized git file system '{context.GitRoot.Path}'");

                context.FileSystem = new PhysicalFileSystem(context.WorkPath);
                _console.LogInformation($"Initialized main file system '{context.WorkPath}'");

                context.CacheFileSystem = CreateFileSystem(
                    context.WorkPath,
                    context.SiteDefinition.BuildPath);

                await context.CacheFileSystem.DeleteDir("content");
                await context.CacheFileSystem.GetOrCreateDirectory("content");

                await context.CacheFileSystem.DeleteDir("content-html");
                await context.CacheFileSystem.GetOrCreateDirectory("content-html");

                _console.LogInformation($"Initialized cache '{GetRootedPath(context.WorkPath, context.SiteDefinition.BuildPath)}'");

                context.RawCache = await context.CacheFileSystem.Mount("content");
                context.PageCache = await context.CacheFileSystem.Mount("content-html");

                var outputPath = GetRootedPath(context.WorkPath, context.SiteDefinition.OutputPath);
                await context.FileSystem.CleanDirectory(outputPath);
                await context.FileSystem.GetOrCreateDirectory(outputPath);
                context.OutputFs = await context.FileSystem.Mount(outputPath);
                _console.LogInformation($"Initialized output file system '{outputPath}'");
            });

        await next(context);
    }

    private static IFileSystem CreateFileSystem(string rootPath, string? inputPath)
    {
        rootPath = GetRootedPath(rootPath, inputPath);
        return new PhysicalFileSystem(rootPath);
    }

    private static FileSystemPath GetRootedPath(string rootPath, string? inputPath)
    {
    Console.WriteLine($"[GetRootedPath] Initial rootPath: '{rootPath}', inputPath: '{inputPath ?? "null"}'");

    // Ensure rootPath is absolute before using it as a base for other paths,
    // or as a return value if inputPath is null/empty.
    if (!Path.IsPathRooted(rootPath))
    {
        Console.WriteLine($"[GetRootedPath] Initial rootPath ('{rootPath}') is not rooted. Making it absolute using Path.GetFullPath(rootPath).");
        // This resolves rootPath against the current working directory.
        rootPath = Path.GetFullPath(rootPath);
        Console.WriteLine($"[GetRootedPath] Absolutified rootPath: '{rootPath}'");
    }

        if (!string.IsNullOrEmpty(inputPath))
        {
            if (Path.IsPathRooted(inputPath))
        {
            Console.WriteLine($"[GetRootedPath] inputPath ('{inputPath}') is rooted. Using it as new rootPath.");
            rootPath = inputPath; // inputPath is already absolute and becomes the new root.
        }
            else
        {
            // Now, rootPath is guaranteed to be absolute here.
            Console.WriteLine($"[GetRootedPath] inputPath ('{inputPath}') is relative. Calling Path.GetFullPath(\"{inputPath}\", \"{rootPath}\").");
                rootPath = Path.GetFullPath(inputPath, rootPath);
            Console.WriteLine($"[GetRootedPath] Path.GetFullPath resolved to: '{rootPath}'");
        }
    }
    else
    {
        Console.WriteLine($"[GetRootedPath] inputPath is null or empty. Using (potentially absolutified) rootPath: '{rootPath}'");
        }

    Console.WriteLine($"[GetRootedPath] Returning final path: '{rootPath}'");
    return rootPath; // Implicit conversion from string to FileSystemPath
    }
}