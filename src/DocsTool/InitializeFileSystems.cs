﻿using LibGit2Sharp;
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

                if (!Repository.IsValid(gitDiscoveryPath))
                {
                    context.Add(new Error($"Could not find git repository from path '{gitDiscoveryPath}'. Tanka-docs-gen uses git to version the documentation."));
                    return;
                }

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
        if (string.IsNullOrEmpty(inputPath))
        {
            return rootPath;
        }

        if (Path.IsPathRooted(inputPath))
        {
            return inputPath;
        }

        return Path.GetFullPath(inputPath, rootPath);
    }
}