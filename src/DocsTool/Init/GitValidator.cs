using System.Diagnostics;

namespace Tanka.DocsTool.Init;

/// <summary>
/// Utilities for Git repository validation and branch detection.
/// </summary>
public static class GitValidator
{
    /// <summary>
    /// Validates that the current directory is a Git repository.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when not in a Git repository</exception>
    public static void ValidateGitRepository()
    {
        if (!IsGitRepository())
        {
            throw new InvalidOperationException(
                "Current directory is not a Git repository.\n\n" +
                "Tanka Docs requires a Git repository to function properly.\n\n" +
                "To initialize a Git repository:\n" +
                "  git init\n" +
                "  git add .\n" +
                "  git commit -m \"Initial commit\"\n\n" +
                "Then run 'tanka-docs init' again.");
        }
    }
    
    /// <summary>
    /// Checks if the current directory is a Git repository.
    /// </summary>
    /// <returns>True if in a Git repository, false otherwise</returns>
    public static bool IsGitRepository()
    {
        // First check for .git directory (most common case)
        if (Directory.Exists(".git"))
            return true;
            
        // Then check via git command (handles worktrees and other cases)
        try
        {
            var result = RunGitCommand("rev-parse --git-dir");
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets the current Git branch name.
    /// </summary>
    /// <returns>The current branch name, or null if unable to determine</returns>
    public static string? GetCurrentBranch()
    {
        try
        {
            // Try git symbolic-ref first (works for normal branches)
            var result = RunGitCommand("symbolic-ref --short HEAD");
            if (!string.IsNullOrWhiteSpace(result))
                return result.Trim();
                
            // Fallback to rev-parse (works for detached HEAD)
            result = RunGitCommand("rev-parse --abbrev-ref HEAD");
            if (!string.IsNullOrWhiteSpace(result) && result.Trim() != "HEAD")
                return result.Trim();
                
            return null; // Detached HEAD or other edge case
        }
        catch
        {
            return null; // Git not available or not a git repository
        }
    }
    
    /// <summary>
    /// Gets a sensible default branch name, preferring current branch or falling back to common defaults.
    /// </summary>
    /// <returns>A branch name to use as default</returns>
    public static string GetDefaultBranchName()
    {
        // Try to detect current branch
        var currentBranch = GetCurrentBranch();
        if (!string.IsNullOrEmpty(currentBranch))
            return currentBranch;
            
        // Fallback to modern default
        return "main";
    }
    
    /// <summary>
    /// Checks if Git is available on the system.
    /// </summary>
    /// <returns>True if Git command is available</returns>
    public static bool IsGitAvailable()
    {
        try
        {
            RunGitCommand("--version");
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Runs a Git command and returns its output.
    /// </summary>
    /// <param name="arguments">Git command arguments</param>
    /// <returns>Command output, or empty string on failure</returns>
    /// <exception cref="InvalidOperationException">Thrown when Git command fails</exception>
    private static string RunGitCommand(string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git command failed: git {arguments}\n{error}");
        }
        
        return output;
    }
} 