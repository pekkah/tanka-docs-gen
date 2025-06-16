using Spectre.Console.Testing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.UI;
using Xunit;

namespace Tanka.DocsTool.Tests;

public class DevCommandFacts
{
    [Fact(Skip = "Refactor to use mocks and not run the full command")]
    public void Test1()
    {
        // Arrange

        // Act

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void GetPathsToWatch_should_return_correct_paths()
    {
        // Arrange
        var site = new SiteDefinition
        {
            Branches = new Dictionary<string, BranchDefinition>
            {
                ["HEAD"] = new()
                {
                    InputPath = new[] { "docs/1.0", "shared" }
                }
            }
        };
        var currentPath = Path.GetTempPath();
        var configFilePath = Path.Combine(currentPath, "tanka-docs.yml");

        // Act
        var result = DevCommand.GetPathsToWatch(site, configFilePath, currentPath).ToList();

        // Assert
        Assert.Contains(configFilePath, result);
        Assert.Contains(Path.Combine(currentPath, "docs", "1.0"), result);
        Assert.Contains(Path.Combine(currentPath, "shared"), result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task FileWatcherService_should_detect_file_change()
    {
        // Arrange
        var console = new TestConsole();
        var tempFile = Path.GetTempFileName();
        var changeDetected = new TaskCompletionSource<bool>();
        var watcher = new FileWatcher();
        watcher.Start(new[] { tempFile }, async (change) =>
        {
            changeDetected.TrySetResult(true);
        });

        try
        {
            // Act
            await File.AppendAllTextAsync(tempFile, "test content");

            // Assert
            var timeout = Task.Delay(5000);
            var completedTask = await Task.WhenAny(changeDetected.Task, timeout);
            Assert.Equal(changeDetected.Task, completedTask);
        }
        finally
        {
            watcher.Stop();
            File.Delete(tempFile);
        }
    }
} 