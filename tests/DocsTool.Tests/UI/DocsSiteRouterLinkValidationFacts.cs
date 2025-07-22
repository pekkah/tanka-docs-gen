using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests.UI;

public class DocsSiteRouterLinkValidationFacts
{
    private readonly DocsSiteRouter _router;
    private readonly BuildContext _strictBuildContext;
    private readonly BuildContext _relaxedBuildContext;
    private readonly Section _section;

    public DocsSiteRouterLinkValidationFacts()
    {
        // Create mock content source
        var source = Substitute.For<IContentSource>();
        source.Version.Returns("TEST");
        source.Path.Returns(new FileSystemPath(""));

        // Create content items for the section
        var contentItems = new Dictionary<FileSystemPath, ContentItem>
        {
            ["existing-file.md"] = new ContentItem(source, "text/markdown", Substitute.For<IReadOnlyFile>()),
            ["subfolder/another-file.md"] = new ContentItem(source, "text/markdown", Substitute.For<IReadOnlyFile>())
        };

        // Create section with content items
        _section = new Section(
            new ContentItem(source, "tanka/section", Substitute.For<IReadOnlyFile>()),
            new SectionDefinition { Id = "test-section" },
            contentItems
        );

        // Create site with the section
        var versionSections = new Dictionary<string, Section> { { "test-section", _section } };
        var allSections = new Dictionary<string, Dictionary<string, Section>> { { "TEST", versionSections } };
        var site = new Site(new SiteDefinition(), allSections);

        _router = new DocsSiteRouter(site, _section);

        // Create build contexts for different validation modes
        _strictBuildContext = new BuildContext(new SiteDefinition(), "/test")
        {
            LinkValidation = LinkValidation.Strict
        };

        _relaxedBuildContext = new BuildContext(new SiteDefinition(), "/test")
        {
            LinkValidation = LinkValidation.Relaxed
        };
    }

    [Fact]
    public void FullyQualify_ExistingFile_ShouldReturnXref()
    {
        // Given
        var xref = new Xref(null, null, "existing-file.md");

        // When
        var result = _router.FullyQualify(xref, _strictBuildContext);

        // Then
        Assert.NotNull(result);
        Assert.Equal("test-section", result.Value.SectionId);
        Assert.Equal("TEST", result.Value.Version);
        Assert.Equal("existing-file.md", result.Value.Path);
        Assert.False(_strictBuildContext.HasErrors);
        Assert.Empty(_strictBuildContext.Warnings);
    }

    [Fact]
    public void FullyQualify_MissingSection_StrictMode_ShouldAddError()
    {
        // Given
        var xref = new Xref(null, "non-existent-section", "some-file.md");

        // When
        var result = _router.FullyQualify(xref, _strictBuildContext);

        // Then
        Assert.Null(result);
        Assert.True(_strictBuildContext.HasErrors);
        Assert.Single(_strictBuildContext.Errors);
        Assert.Contains("section not found", _strictBuildContext.Errors.First().Message);
        Assert.Empty(_strictBuildContext.Warnings);
    }

    [Fact]
    public void FullyQualify_MissingSection_RelaxedMode_ShouldAddWarning()
    {
        // Given
        var xref = new Xref(null, "non-existent-section", "some-file.md");

        // When
        var result = _router.FullyQualify(xref, _relaxedBuildContext);

        // Then
        Assert.Null(result);
        Assert.False(_relaxedBuildContext.HasErrors);
        Assert.NotEmpty(_relaxedBuildContext.Warnings);
        Assert.Single(_relaxedBuildContext.Warnings);
        Assert.Contains("section not found", _relaxedBuildContext.Warnings.First().Message);
    }

    [Fact]
    public void FullyQualify_MissingContentItem_StrictMode_ShouldAddError()
    {
        // Given
        var xref = new Xref(null, null, "non-existent-file.md");

        // When
        var result = _router.FullyQualify(xref, _strictBuildContext);

        // Then
        Assert.Null(result);
        Assert.True(_strictBuildContext.HasErrors);
        Assert.Single(_strictBuildContext.Errors);
        Assert.Contains("content item not found", _strictBuildContext.Errors.First().Message);
        Assert.Empty(_strictBuildContext.Warnings);
    }

    [Fact]
    public void FullyQualify_MissingContentItem_RelaxedMode_ShouldAddWarning()
    {
        // Given
        var xref = new Xref(null, null, "non-existent-file.md");

        // When
        var result = _router.FullyQualify(xref, _relaxedBuildContext);

        // Then
        Assert.Null(result);
        Assert.False(_relaxedBuildContext.HasErrors);
        Assert.NotEmpty(_relaxedBuildContext.Warnings);
        Assert.Single(_relaxedBuildContext.Warnings);
        Assert.Contains("content item not found", _relaxedBuildContext.Warnings.First().Message);
    }

    [Fact]
    public void GenerateRoute_ExistingFile_ShouldReturnUrl()
    {
        // Given
        var xref = new Xref(null, null, "existing-file.md");

        // When
        var result = _router.GenerateRoute(xref, _strictBuildContext);

        // Then
        Assert.NotNull(result);
        Assert.Equal("TEST/existing-file.html", result);
        Assert.False(_strictBuildContext.HasErrors);
        Assert.Empty(_strictBuildContext.Warnings);
    }

    [Fact]
    public void GenerateRoute_MissingSection_ShouldReturnPlaceholder()
    {
        // Given
        var xref = new Xref(null, "non-existent-section", "some-file.md");

        // When - Test both strict and relaxed modes
        var strictResult = _router.GenerateRoute(xref, _strictBuildContext);
        var relaxedResult = _router.GenerateRoute(xref, _relaxedBuildContext);

        // Then - Both should return placeholders
        Assert.NotNull(strictResult);
        Assert.NotNull(relaxedResult);
        Assert.StartsWith("#broken-xref-", strictResult);
        Assert.StartsWith("#broken-xref-", relaxedResult);
        Assert.Equal(strictResult, relaxedResult);

        // Strict mode should add error
        Assert.True(_strictBuildContext.HasErrors);
        Assert.Contains("section not found", _strictBuildContext.Errors.First().Message);

        // Relaxed mode should add warning
        Assert.NotEmpty(_relaxedBuildContext.Warnings);
        Assert.Contains("section not found", _relaxedBuildContext.Warnings.First().Message);
    }

    [Fact]
    public void GenerateRoute_MissingContentItem_ShouldReturnPlaceholder()
    {
        // Given
        var xref = new Xref(null, null, "non-existent-file.md");

        // When - Test both strict and relaxed modes
        var strictResult = _router.GenerateRoute(xref, _strictBuildContext);
        var relaxedResult = _router.GenerateRoute(xref, _relaxedBuildContext);

        // Then - Both should return placeholders
        Assert.NotNull(strictResult);
        Assert.NotNull(relaxedResult);
        Assert.StartsWith("#broken-xref-", strictResult);
        Assert.StartsWith("#broken-xref-", relaxedResult);
        Assert.Equal(strictResult, relaxedResult);

        // Strict mode should add error
        Assert.True(_strictBuildContext.HasErrors);
        Assert.Contains("content item not found", _strictBuildContext.Errors.First().Message);

        // Relaxed mode should add warning
        Assert.NotEmpty(_relaxedBuildContext.Warnings);
        Assert.Contains("content item not found", _relaxedBuildContext.Warnings.First().Message);
    }

    [Fact]
    public void GenerateRoute_PlaceholderSanitization_ShouldReplaceSpecialCharacters()
    {
        // Given - Xref with special characters
        var xref = new Xref("v1.0", "section", "path/with:special@chars.md");

        // When
        var result = _router.GenerateRoute(xref, _relaxedBuildContext);

        // Then - Special characters should be replaced with hyphens  
        Assert.NotNull(result);
        Assert.Equal("#broken-xref-xref---section-v1.0-path-with-special-chars.md", result);
    }

    [Fact]
    public void FullyQualify_HeadVersion_ShouldAddError()
    {
        // Given
        var xref = new Xref("HEAD", "test-section", "existing-file.md");

        // When
        var result = _router.FullyQualify(xref, _strictBuildContext);

        // Then
        Assert.Null(result);
        Assert.True(_strictBuildContext.HasErrors);
        Assert.Single(_strictBuildContext.Errors);
        Assert.Contains("HEAD version is not allowed", _strictBuildContext.Errors.First().Message);
    }

    [Fact]
    public void GenerateRoute_HeadVersion_ShouldReturnPlaceholder()
    {
        // Given
        var xrefUpperCase = new Xref("HEAD", "test-section", "existing-file.md");
        var xrefLowerCase = new Xref("head", "test-section", "existing-file.md");

        // When - Test both upper and lower case
        var resultUpper = _router.GenerateRoute(xrefUpperCase, _strictBuildContext);
        var resultLower = _router.GenerateRoute(xrefLowerCase, _relaxedBuildContext);

        // Then - Both should return placeholders and add errors
        Assert.NotNull(resultUpper);
        Assert.NotNull(resultLower);
        Assert.StartsWith("#broken-xref-", resultUpper);
        Assert.StartsWith("#broken-xref-", resultLower);

        // Both contexts should have errors (HEAD is always an error, not warning)
        Assert.True(_strictBuildContext.HasErrors);
        Assert.Contains("HEAD version is not allowed", _strictBuildContext.Errors.First().Message);
        
        Assert.True(_relaxedBuildContext.HasErrors);
        Assert.Contains("HEAD version is not allowed", _relaxedBuildContext.Errors.First().Message);
    }
}