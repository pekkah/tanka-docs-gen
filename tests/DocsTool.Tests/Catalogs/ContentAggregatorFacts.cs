using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NSubstitute;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;
using Tanka.FileSystem.Memory;
using Xunit;

namespace Tanka.DocsTool.Tests.Catalogs
{
    public class ContentAggregatorFacts: IDisposable
    {
        public ContentAggregatorFacts()
        {
            _classifier = Substitute.For<IContentClassifier>();

            _workingFileSystem = new InMemoryFileSystem();
            _buildFileSystem = new InMemoryFileSystem();

            _sut = new ContentAggregator(
                _classifier,
                _workingFileSystem,
                _buildFileSystem,
                GitFileSystemRoot.Discover(Environment.CurrentDirectory),
                new SiteDefinition()
                {
                    Branches = new Dictionary<string, string>()
                    {
                        ["__WIP__"] = "work"
                    }
                });
        }

        private readonly ContentAggregator _sut;
        private readonly IContentClassifier _classifier;
        private InMemoryFileSystem _workingFileSystem;
        private InMemoryFileSystem _buildFileSystem;

        [Fact]
        public async Task Classify_ContentItems()
        {
            /* Given */
            var file = await _workingFileSystem.GetOrCreateFile("file.md");
            _classifier.Classify(file).Returns("text/markdown");

            /* When */
            var actual = await Async.FromAsync(_sut.Enumerate());

            /* Then */
            var contentItem = Assert.Single(actual);
            Assert.NotNull(contentItem!.Type);
            Assert.Equal("text/markdown", contentItem.Type);
        }

        public void Dispose()
        {
            _workingFileSystem.Dispose();
            _buildFileSystem.Dispose();
        }
    }
}