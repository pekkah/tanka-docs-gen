using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Testing;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests.Pipelines
{
    public class AugmentFilesSectionsFacts
    {
        private readonly TestConsole _testConsole;
        private readonly IAnsiConsole _console;
        private readonly IFileSystem _fileSystem;
        private readonly AugmentFilesSections _middleware;

        public AugmentFilesSectionsFacts()
        {
            _testConsole = new TestConsole();
            _console = _testConsole;
            _fileSystem = Substitute.For<IFileSystem>();
            _middleware = new AugmentFilesSections(_console);
        }

        [Fact]
        public async Task Invoke_WhenContextHasErrors_SkipsProcessingAndCallsNext()
        {
            // Given
            var context = CreateBuildContext();
            context.Add(new Error("Test error"));
            var nextCalled = false;
            Task Next(BuildContext ctx)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            // When
            await _middleware.Invoke(Next, context);

            // Then
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WhenNoFilesSections_SkipsProcessingAndCallsNext()
        {
            // Given
            var context = CreateBuildContext();
            var regularSection = CreateSection("regular-section", "doc");
            context.Sections = new[] { regularSection };
            var nextCalled = false;
            Task Next(BuildContext ctx)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            // When
            await _middleware.Invoke(Next, context);

            // Then
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WhenFilesSectionExists_ProcessesSection()
        {
            // Given
            var context = CreateBuildContext();
            var filesSection = CreateSection("files-section", "files");
            context.Sections = new[] { filesSection };
            context.FileSystem = _fileSystem;

            var sectionDirectory = Substitute.For<IDirectory>();
            _fileSystem.GetDirectory(Arg.Any<FileSystemPath>()).Returns(Task.FromResult<IDirectory?>(sectionDirectory));

            var mockFile = Substitute.For<IReadOnlyFile>();
            mockFile.Path.Returns(new FileSystemPath("test.md"));
            sectionDirectory.Enumerate().Returns(CreateAsyncEnumerable(mockFile));

            var nextCalled = false;
            Task Next(BuildContext ctx)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            // When
            await _middleware.Invoke(Next, context);

            // Then
            Assert.True(nextCalled);
            await _fileSystem.Received().GetDirectory(Arg.Any<FileSystemPath>());
        }

        [Fact]
        public async Task Invoke_WhenMultipleFilesSections_ProcessesAllSections()
        {
            // Given
            var context = CreateBuildContext();
            var filesSection1 = CreateSection("files-section-1", "files");
            var filesSection2 = CreateSection("files-section-2", "FILES"); // Test case insensitive
            var regularSection = CreateSection("regular-section", "doc");
            context.Sections = new[] { filesSection1, filesSection2, regularSection };
            context.FileSystem = _fileSystem;

            var sectionDirectory = Substitute.For<IDirectory>();
            _fileSystem.GetDirectory(Arg.Any<FileSystemPath>()).Returns(Task.FromResult<IDirectory?>(sectionDirectory));
            sectionDirectory.Enumerate().Returns(CreateEmptyAsyncEnumerable());

            var nextCalled = false;
            Task Next(BuildContext ctx)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            // When
            await _middleware.Invoke(Next, context);

            // Then
            Assert.True(nextCalled);
            await _fileSystem.Received(2).GetDirectory(Arg.Any<FileSystemPath>());
        }

        private BuildContext CreateBuildContext()
        {
            var siteDefinition = new SiteDefinition
            {
                Title = "Test Site",
                OutputPath = "output",
                BuildPath = "_build"
            };
            return new BuildContext(siteDefinition, new FileSystemPath("/test"));
        }

        private Section CreateSection(string id, string type)
        {
            var sectionDefinition = new SectionDefinition
            {
                Id = id,
                Type = type,
                Title = $"Test {id}"
            };

            var contentSource = Substitute.For<IContentSource>();
            contentSource.Version.Returns("HEAD");
            contentSource.Path.Returns(new FileSystemPath($"/test/{id}"));

            var file = Substitute.For<IReadOnlyFile>();
            file.Path.Returns(new FileSystemPath($"/test/{id}/tanka-docs-section.yml"));

            var contentItem = new ContentItem(contentSource, ContentItem.SectionDefinitionType, file);
            var contentItems = new Dictionary<FileSystemPath, ContentItem>();

            return new Section(contentItem, sectionDefinition, contentItems);
        }

        private static async IAsyncEnumerable<IFileSystemNode> CreateAsyncEnumerable(params IFileSystemNode[] nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
            }
            await Task.CompletedTask;
        }

        private static async IAsyncEnumerable<IFileSystemNode> CreateEmptyAsyncEnumerable()
        {
            yield break;
        }
    }

    public class FilesSectionAugmenterFacts
    {
        private readonly TestConsole _testConsole;
        private readonly IAnsiConsole _console;

        public FilesSectionAugmenterFacts()
        {
            _testConsole = new TestConsole();
            _console = _testConsole;
        }

        [Fact]
        public async Task AugmentCatalog_WithNonExistentDirectory_LogsWarning()
        {
            // Given
            var fileSystem = Substitute.For<IFileSystem>();
            var catalog = new Catalog();
            var section = CreateSection("test-section", "files");
            var sections = new[] { section };

            fileSystem.GetDirectory(Arg.Any<FileSystemPath>()).Returns(Task.FromResult<IDirectory?>(null));
            var augmenter = new FilesSectionAugmenter(fileSystem, _console);

            // When
            await _console.Progress()
                .StartAsync(async progress =>
                {
                    await augmenter.AugmentCatalog(catalog, sections, progress);
                });

            // Then
            var output = _testConsole.Output;
            Assert.Contains("not found in working directory", output);
        }

        [Fact]
        public async Task AugmentCatalog_WithException_LogsErrorAndContinues()
        {
            // Given
            var fileSystem = Substitute.For<IFileSystem>();
            var catalog = new Catalog();
            var section = CreateSection("test-section", "files");
            var sections = new[] { section };

            fileSystem.GetDirectory(Arg.Any<FileSystemPath>()).Returns<Task<IDirectory?>>(x => throw new InvalidOperationException("Test exception"));
            var augmenter = new FilesSectionAugmenter(fileSystem, _console);

            // When
            await _console.Progress()
                .StartAsync(async progress =>
                {
                    await augmenter.AugmentCatalog(catalog, sections, progress);
                });

            // Then
            var output = _testConsole.Output;
            Assert.Contains("Failed to augment files section", output);
        }

        private Section CreateSection(string id, string type)
        {
            var sectionDefinition = new SectionDefinition
            {
                Id = id,
                Type = type,
                Title = $"Test {id}"
            };

            var contentSource = Substitute.For<IContentSource>();
            contentSource.Version.Returns("HEAD");
            contentSource.Path.Returns(new FileSystemPath($"/test/{id}"));

            var file = Substitute.For<IReadOnlyFile>();
            file.Path.Returns(new FileSystemPath($"/test/{id}/tanka-docs-section.yml"));

            var contentItem = new ContentItem(contentSource, ContentItem.SectionDefinitionType, file);
            var contentItems = new Dictionary<FileSystemPath, ContentItem>();

            return new Section(contentItem, sectionDefinition, contentItems);
        }

        private static async IAsyncEnumerable<IFileSystemNode> CreateAsyncEnumerable(params IFileSystemNode[] nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
            }
            await Task.CompletedTask;
        }

        private static async IAsyncEnumerable<IFileSystemNode> CreateEmptyAsyncEnumerable()
        {
            yield break;
        }
    }
}