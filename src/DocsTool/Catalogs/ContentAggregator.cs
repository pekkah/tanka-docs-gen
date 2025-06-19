using System.Runtime.CompilerServices;
using System.Text.Json;
using DotNet.Globbing;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Catalogs;

public class ContentAggregator
{
    private readonly IContentClassifier _classifier;
    private readonly IAnsiConsole _console;
    private readonly GitFileSystemRoot _git;
    private readonly SiteDefinition _site;
    private readonly IFileSystem _workFileSystem;

    public ContentAggregator(
        SiteDefinition site,
        GitFileSystemRoot git,
        IFileSystem workFileSystem,
        IContentClassifier classifier,
        IAnsiConsole console)
    {
        _site = site;
        _git = git;
        _workFileSystem = workFileSystem;
        _classifier = classifier;
        _console = console;
    }

    public async IAsyncEnumerable<ContentItem> Aggregate(
        BuildContext context,
        ProgressContext progress,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var stack = new Stack<IFileSystemNode>();
        await foreach (var source in BuildSources(context, cancellationToken))
        {
            _console.LogInformation($"Loading: '{source.Path}@{source.Version}'");
            
            var task = progress.AddTask(GetTaskDescription(source), maxValue: 0);
           
            await foreach (var node in source.Enumerate(cancellationToken))
                stack.Push(node);

            task.MaxValue = stack.Count;
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                task.Increment(1);
               
                switch (node)
                {
                    case IReadOnlyFile file:
                        yield return CreateContentItem(source, file);
                        break;
                    case IReadOnlyDirectory dir:
                        await foreach (var subNode in dir.Enumerate()
                            .WithCancellation(cancellationToken))
                        {
                            stack.Push(subNode);
                            task.MaxValue++;
                        }
                        break;
                }
            }

            task.StopTask();
        }
    }

    private string GetTaskDescription(IContentSource source)
    {
        return source switch
        {
            _ => $"{source.Path}@{source.Version}"
        };
    }

    private ContentItem CreateContentItem(IContentSource source, IReadOnlyFile file)
    {
        var type = _classifier.Classify(file);
        var ci = new ContentItem(source, type, file);

        return ci;
    }

    private async Task<bool> HasFilesSections(string inputPath, CancellationToken cancellationToken)
    {
        try
        {
            var inputDirectory = await _workFileSystem.GetDirectory(inputPath);
            if (inputDirectory == null)
                return false;

            // Find all tanka-docs-section.yml files recursively
            var sectionFiles = new List<IReadOnlyFile>();
            var stack = new Stack<IReadOnlyDirectory>();
            stack.Push(inputDirectory);

            while (stack.Count > 0)
            {
                var directory = stack.Pop();
                await foreach (var node in directory.Enumerate().WithCancellation(cancellationToken))
                {
                    switch (node)
                    {
                        case IReadOnlyFile file when file.Name == "tanka-docs-section.yml":
                            sectionFiles.Add(file);
                            break;
                        case IReadOnlyDirectory subDirectory:
                            stack.Push(subDirectory);
                            break;
                    }
                }
            }

            // Check each section file for type: files
            foreach (var sectionFile in sectionFiles)
            {
                try
                {
                    using var stream = await sectionFile.OpenRead();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    
                    // Simple YAML parsing to check for type: files
                    // This avoids adding complex YAML parsing dependency here
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("type:"))
                        {
                            var typeValue = trimmed.Substring(5).Trim();
                            if (typeValue == "files" || typeValue == "\"files\"" || typeValue == "'files'")
                            {
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    // If we can't parse a section file, continue checking others
                    continue;
                }
            }

            return false;
        }
        catch
        {
            // If there's any error scanning, default to false (use Git file system)
            return false;
        }
    }

    private async IAsyncEnumerable<IContentSource> BuildSources(
        BuildContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var branches = _site.Branches;
       
        foreach (var (branch, definition) in branches)
        {
            var commonInputPaths = definition.InputPath;
            foreach (var commonInputPath in commonInputPaths)
            {
                if (branch == "HEAD")
                {
                    //var status = _git.Repo.RetrieveStatus();

                    //// If we have changes then we actually use the current folder
                    //// as the source of the truth
                    //if (status.IsDirty)
                    //{
                        var inputDirectory = await _workFileSystem.GetDirectory(commonInputPath);
                        if (inputDirectory != null)
                        {
                            yield return new FileSystemContentSource(
                                _workFileSystem,
                                "HEAD",
                                inputDirectory.Path); // we're mounting the input path
                        }
                    //}
                    //else
                    //{
                    //    var head = _git.Head();
                    //    if (await head.GetDirectory(commonInputPath) != null)
                    //        yield return new GitBranchContentSource(head, commonInputPath);
                    //}
                }
                else
                {
                    // Check if this path contains sections with type: files
                    var hasFilesSections = await HasFilesSections(commonInputPath, cancellationToken);
                    
                    if (hasFilesSections)
                    {
                        // Force PhysicalFileSystem for paths with type: files sections
                        var inputDirectory = await _workFileSystem.GetDirectory(commonInputPath);
                        if (inputDirectory != null)
                        {
                            yield return new FileSystemContentSource(
                                _workFileSystem,
                                branch, // Use branch name as version
                                inputDirectory.Path);
                        }
                    }
                    else
                    {
                        // Use Git file system as before
                        var glob = Glob.Parse(branch);
                        var matched = false;
                        foreach (var repoBranch in _git.Repo.Branches)
                        {
                            if (glob.IsMatch(repoBranch.FriendlyName) || glob.IsMatch(repoBranch.CanonicalName))
                            {
                                matched = true;
                                var matchingBranch = _git.Branch(repoBranch.CanonicalName);
                                if (await matchingBranch.GetDirectory(commonInputPath) != null)
                                {
                                    
                                    yield return new GitBranchContentSource(
                                        matchingBranch,
                                        commonInputPath);
                                }
                            }
                        }

                        if (!matched)
                            context.Add(new Error($"Branch pattern '{branch}' did not match any branches."), isWarning: true);
                    }
                }
            }
        }

        var tags = _site.Tags;

        foreach (var (tag, definition) in tags)
        {
            var inputPaths = definition.InputPath;

            foreach (var inputPath in inputPaths)
            {
                // Check if this path contains sections with type: files
                var hasFilesSections = await HasFilesSections(inputPath, cancellationToken);
                
                if (hasFilesSections)
                {
                    // Force PhysicalFileSystem for paths with type: files sections
                    var inputDirectory = await _workFileSystem.GetDirectory(inputPath);
                    if (inputDirectory != null)
                    {
                        yield return new FileSystemContentSource(
                            _workFileSystem,
                            tag, // Use tag name as version
                            inputDirectory.Path);
                    }
                }
                else
                {
                    // Use Git file system as before
                    var glob = Glob.Parse(tag);
                    var matched = false;
                    foreach (var repoTag in _git.Repo.Tags)
                    {
                        if (glob.IsMatch(repoTag.FriendlyName) || glob.IsMatch(repoTag.CanonicalName))
                        {
                            matched = true;
                            var matchingCommit = _git.Tag(repoTag);
                            if (await matchingCommit.GetDirectory(inputPath) != null)
                            {
                                yield return new GitCommitContentSource(
                                    matchingCommit,
                                    repoTag.FriendlyName,
                                    inputPath);
                            }
                        }
                    }
                    if (!matched)
                        context.Add(new Error($"Tag pattern '{tag}' did not match any tags."), isWarning: true);
                }
            }
        }
    }
}
