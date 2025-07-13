using NSubstitute;
using Tanka.DocsTool.Security;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests.Security;

public class FileSecurityFilterFacts
{
    [Theory]
    [InlineData(".env")]
    [InlineData(".env.local")]
    [InlineData(".env.production")]
    [InlineData("secrets.txt")]
    [InlineData("password.txt")]
    [InlineData("credentials.json")]
    [InlineData("config.production")]
    [InlineData("private.key")]
    [InlineData("certificate.pem")]
    [InlineData("keystore.jks")]
    [InlineData("backup.bak")]
    [InlineData("temp.tmp")]
    [InlineData(".DS_Store")]
    [InlineData("Thumbs.db")]
    public void ShouldExclude_SensitiveFiles_ReturnsTrue(string filename)
    {
        /* Given */
        var filter = new ConfigurableFileSecurityFilter();
        var file = CreateMockFile(filename);

        /* When */
        var result = filter.ShouldExclude(file);

        /* Then */
        Assert.True(result, $"File '{filename}' should be excluded for security reasons");
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("documentation.pdf")]
    [InlineData("image.png")]
    [InlineData("data.json")]
    [InlineData("script.js")]
    [InlineData("style.css")]
    [InlineData("source.cs")]
    [InlineData("test.txt")]
    public void ShouldExclude_NormalFiles_ReturnsFalse(string filename)
    {
        /* Given */
        var filter = new ConfigurableFileSecurityFilter();
        var file = CreateMockFile(filename);

        /* When */
        var result = filter.ShouldExclude(file);

        /* Then */
        Assert.False(result, $"File '{filename}' should not be excluded");
    }

    [Theory]
    [InlineData(".git")]
    [InlineData(".vscode")]
    [InlineData(".idea")]
    [InlineData("node_modules")]
    [InlineData("bin")]
    [InlineData("obj")]
    [InlineData("target")]
    [InlineData("build")]
    [InlineData("dist")]
    [InlineData(".nuget")]
    public void ShouldExclude_SensitiveDirectories_ReturnsTrue(string dirname)
    {
        /* Given */
        var filter = new ConfigurableFileSecurityFilter();
        var directory = CreateMockDirectory(dirname);

        /* When */
        var result = filter.ShouldExclude(directory);

        /* Then */
        Assert.True(result, $"Directory '{dirname}' should be excluded for security reasons");
    }

    [Theory]
    [InlineData("docs")]
    [InlineData("src")]
    [InlineData("tests")]
    [InlineData("examples")]
    [InlineData("assets")]
    [InlineData("images")]
    public void ShouldExclude_NormalDirectories_ReturnsFalse(string dirname)
    {
        /* Given */
        var filter = new ConfigurableFileSecurityFilter();
        var directory = CreateMockDirectory(dirname);

        /* When */
        var result = filter.ShouldExclude(directory);

        /* Then */
        Assert.False(result, $"Directory '{dirname}' should not be excluded");
    }

    [Fact]
    public void ConfigurableFilter_WithCustomExcludePattern_ExcludesMatchingFiles()
    {
        /* Given */
        var config = new FileFilterConfiguration();
        config.ExcludePatterns.Add(@".*\.secret$");
        
        var filter = new ConfigurableFileSecurityFilter(config);
        var secretFile = CreateMockFile("data.secret");
        var normalFile = CreateMockFile("data.txt");

        /* When */
        var secretResult = filter.ShouldExclude(secretFile);
        var normalResult = filter.ShouldExclude(normalFile);

        /* Then */
        Assert.True(secretResult, "File matching custom exclude pattern should be excluded");
        Assert.False(normalResult, "File not matching custom exclude pattern should not be excluded");
    }

    [Fact]
    public void ConfigurableFilter_WithIncludePattern_OverridesExclusion()
    {
        /* Given */
        var config = new FileFilterConfiguration();
        config.IncludePatterns.Add(@"allowed\.env$");
        
        var filter = new ConfigurableFileSecurityFilter(config);
        var allowedEnvFile = CreateMockFile("allowed.env");
        var normalEnvFile = CreateMockFile(".env");

        /* When */
        var allowedResult = filter.ShouldExclude(allowedEnvFile);
        var normalResult = filter.ShouldExclude(normalEnvFile);

        /* Then */
        Assert.False(allowedResult, "File matching include pattern should not be excluded");
        Assert.True(normalResult, "Normal .env file should still be excluded");
    }

    [Fact]
    public void ConfigurableFilter_WithSecurityDisabled_AllowsAllFiles()
    {
        /* Given */
        var config = new FileFilterConfiguration 
        { 
            EnableSecurityFiltering = false,
            IncludeHiddenFiles = true // Need to also allow hidden files to test .env
        };
        var filter = new ConfigurableFileSecurityFilter(config);
        var sensitiveFile = CreateMockFile(".env");

        /* When */
        var result = filter.ShouldExclude(sensitiveFile);

        /* Then */
        Assert.False(result, "When security filtering is disabled, sensitive files should not be excluded");
    }

    [Fact]
    public void ConfigurableFilter_WithHiddenFilesDisabled_ExcludesHiddenFiles()
    {
        /* Given */
        var config = new FileFilterConfiguration { IncludeHiddenFiles = false };
        var filter = new ConfigurableFileSecurityFilter(config);
        var hiddenFile = CreateMockFile(".hidden");

        /* When */
        var result = filter.ShouldExclude(hiddenFile);

        /* Then */
        Assert.True(result, "Hidden files should be excluded when IncludeHiddenFiles is false");
    }

    [Fact]
    public void ConfigurableFilter_WithHiddenFilesEnabled_AllowsHiddenFiles()
    {
        /* Given */
        var config = new FileFilterConfiguration 
        { 
            IncludeHiddenFiles = true,
            EnableSecurityFiltering = false // Disable security to focus on hidden file behavior
        };
        var filter = new ConfigurableFileSecurityFilter(config);
        var hiddenFile = CreateMockFile(".hidden");

        /* When */
        var result = filter.ShouldExclude(hiddenFile);

        /* Then */
        Assert.False(result, "Hidden files should be allowed when IncludeHiddenFiles is true");
    }

    [Fact]
    public void ConfigurableFilter_WithCustomExtensions_ExcludesMatchingFiles()
    {
        /* Given */
        var config = new FileFilterConfiguration();
        config.ExcludeExtensions.Add("custom");
        config.ExcludeExtensions.Add(".special");
        
        var filter = new ConfigurableFileSecurityFilter(config);
        var customFile = CreateMockFile("file.custom");
        var specialFile = CreateMockFile("file.special");
        var normalFile = CreateMockFile("file.txt");

        /* When */
        var customResult = filter.ShouldExclude(customFile);
        var specialResult = filter.ShouldExclude(specialFile);
        var normalResult = filter.ShouldExclude(normalFile);

        /* Then */
        Assert.True(customResult, "File with custom excluded extension should be excluded");
        Assert.True(specialResult, "File with custom excluded extension should be excluded");
        Assert.False(normalResult, "File with normal extension should not be excluded");
    }

    [Fact]
    public void ConfigurableFilter_WithCustomDirectories_ExcludesMatchingDirectories()
    {
        /* Given */
        var config = new FileFilterConfiguration();
        config.ExcludeDirectories.Add("custom-build");
        config.IncludeDirectories.Add("important-secrets"); // Should override exclusion
        
        var filter = new ConfigurableFileSecurityFilter(config);
        var customBuildDir = CreateMockDirectory("custom-build");
        var importantSecretsDir = CreateMockDirectory("important-secrets");
        var normalDir = CreateMockDirectory("docs");

        /* When */
        var customResult = filter.ShouldExclude(customBuildDir);
        var importantResult = filter.ShouldExclude(importantSecretsDir);
        var normalResult = filter.ShouldExclude(normalDir);

        /* Then */
        Assert.True(customResult, "Custom excluded directory should be excluded");
        Assert.False(importantResult, "Directory in include list should not be excluded");
        Assert.False(normalResult, "Normal directory should not be excluded");
    }

    private static IReadOnlyFile CreateMockFile(string filename)
    {
        var file = Substitute.For<IReadOnlyFile>();
        var path = new FileSystemPath(filename);
        file.Path.Returns(path);
        return file;
    }

    private static IReadOnlyDirectory CreateMockDirectory(string dirname)
    {
        var directory = Substitute.For<IReadOnlyDirectory>();
        var path = new FileSystemPath(dirname);
        directory.Path.Returns(path);
        return directory;
    }
}