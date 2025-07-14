using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests
{
    public class XrefLongFilenameIntegrationFacts
    {
        [Fact]
        public void DocsSiteRouter_ShouldResolveLongFilename()
        {
            /* Given - Create a site with a section containing a long filename */
            var longFilename = "Chibi.Ui.Benchmarks.Graphics.Basic.BasicDrawingBenchmarks-report-github.md";

            // Create source and content items
            var source = CreateMockContentSource();
            var contentItems = new Dictionary<FileSystemPath, ContentItem>
            {
                { new FileSystemPath(longFilename), new ContentItem(source, "text/markdown", Substitute.For<IReadOnlyFile>()) }
            };

            // Create a section with the content item
            var section = new Section(
                new ContentItem(source, "tanka/section", Substitute.For<IReadOnlyFile>()),
                new SectionDefinition() { Id = "default" },
                contentItems
            );

            // Create a site with the section
            var versionSections = new Dictionary<string, Section> { { "default", section } };
            var allSections = new Dictionary<string, Dictionary<string, Section>> { { "TEST", versionSections } };
            var site = new Site(new SiteDefinition(), allSections);

            // Create the router
            var router = new DocsSiteRouter(site, section);

            /* When - Try to resolve the XREF link */
            var xref = new Tanka.DocsTool.Navigation.Xref(
                version: null,
                sectionId: null,
                path: longFilename
            );

            /* Then - Should not throw exception */
            var exception = Record.Exception(() => router.FullyQualify(xref));
            Assert.Null(exception);
        }

        [Fact]
        public void DocsSiteRouter_ShouldThrowNotFoundForMissingLongFilename()
        {
            /* Given - Create a site without the missing filename */
            var longFilename = "Chibi.Ui.Benchmarks.Graphics.Basic.BasicDrawingBenchmarks-report-github.md";
            var missingFilename = "Different.Long.Filename.That.Does.Not.Exist.md";

            // Create source and content items (only the existing file)
            var source = CreateMockContentSource();
            var contentItems = new Dictionary<FileSystemPath, ContentItem>
            {
                { new FileSystemPath(longFilename), new ContentItem(source, "text/markdown", Substitute.For<IReadOnlyFile>()) }
            };

            // Create a section with only the existing content item
            var section = new Section(
                new ContentItem(source, "tanka/section", Substitute.For<IReadOnlyFile>()),
                new SectionDefinition() { Id = "default" },
                contentItems
            );

            // Create a site with the section
            var versionSections = new Dictionary<string, Section> { { "default", section } };
            var allSections = new Dictionary<string, Dictionary<string, Section>> { { "TEST", versionSections } };
            var site = new Site(new SiteDefinition(), allSections);

            // Create the router
            var router = new DocsSiteRouter(site, section);

            /* When - Try to resolve a missing XREF link */
            var xref = new Tanka.DocsTool.Navigation.Xref(
                version: null,
                sectionId: null,
                path: missingFilename
            );

            /* Then - Should throw NotFound exception */
            var exception = Assert.Throws<InvalidOperationException>(() =>
                router.FullyQualify(xref));

            Assert.Contains("NotFound", exception.Message);
            Assert.Contains(missingFilename, exception.Message);
        }

        [Theory]
        [InlineData("Very.Long.Namespace.Class.Method.Benchmark-Results-Report.md")]
        [InlineData("Another.Really.Long.ClassName.WithManySegments.AndNumbers123.AndMore-final-report.md")]
        [InlineData("Extremely.Long.File.Name.With.Multiple.Dots.And.Dashes.And.Numbers.123.456.789.Final.md")]
        [InlineData("Short.md")]
        public void DocsSiteRouter_ShouldHandleVaryingFilenameLengths(string filename)
        {
            /* Given - Create a site with the specific filename */
            var source = CreateMockContentSource();
            var contentItems = new Dictionary<FileSystemPath, ContentItem>
            {
                { new FileSystemPath(filename), new ContentItem(source, "text/markdown", Substitute.For<IReadOnlyFile>()) }
            };

            var section = new Section(
                new ContentItem(source, "tanka/section", Substitute.For<IReadOnlyFile>()),
                new SectionDefinition() { Id = "default" },
                contentItems
            );

            var versionSections = new Dictionary<string, Section> { { "default", section } };
            var allSections = new Dictionary<string, Dictionary<string, Section>> { { "TEST", versionSections } };
            var site = new Site(new SiteDefinition(), allSections);
            var router = new DocsSiteRouter(site, section);

            /* When - Try to resolve the XREF link */
            var xref = new Tanka.DocsTool.Navigation.Xref(
                version: null,
                sectionId: null,
                path: filename
            );

            /* Then - Should not throw exception */
            var exception = Record.Exception(() => router.FullyQualify(xref));
            Assert.Null(exception);
        }

        [Fact]
        public void FileSystemPath_ShouldHandleLongFilenames()
        {
            /* Given */
            var longFilename = "Chibi.Ui.Benchmarks.Graphics.Basic.BasicDrawingBenchmarks-report-github.md";

            /* When */
            var path = new FileSystemPath(longFilename);

            /* Then */
            Assert.Equal(longFilename, path.ToString());
            Assert.Equal(longFilename, (string)path);
        }

        [Fact]
        public void FileSystemPath_EqualityWithLongFilenames()
        {
            /* Given */
            var longFilename = "Chibi.Ui.Benchmarks.Graphics.Basic.BasicDrawingBenchmarks-report-github.md";

            /* When */
            var path1 = new FileSystemPath(longFilename);
            var path2 = new FileSystemPath(longFilename);
            var path3 = new FileSystemPath("different.md");

            /* Then */
            Assert.Equal(path1, path2);
            Assert.NotEqual(path1, path3);

            // Test dictionary key behavior
            var dict = new Dictionary<FileSystemPath, string>
            {
                { path1, "test" }
            };

            Assert.True(dict.ContainsKey(path2));
            Assert.False(dict.ContainsKey(path3));
        }

        private static IContentSource CreateMockContentSource()
        {
            var source = Substitute.For<IContentSource>();
            source.Version.Returns("TEST");
            source.Path.Returns(new FileSystemPath(""));
            return source;
        }
    }
}