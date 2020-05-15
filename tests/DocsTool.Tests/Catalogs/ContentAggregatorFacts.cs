using System.Threading.Tasks;
using NSubstitute;
using Tanka.DocsTool.Catalogs;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests.Catalogs
{
    public class ContentAggregatorFacts
    {
        public ContentAggregatorFacts()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _classifier = Substitute.For<IContentClassifier>();
            _sut = new ContentAggregator(
                _classifier,
                _fileSystem);
        }

        private readonly IFileSystem _fileSystem;
        private readonly ContentAggregator _sut;
        private readonly IContentClassifier _classifier;

        [Fact]
        public async Task Classify_ContentItems()
        {
            /* Given */
            var directory = new Directory(_fileSystem, "");
            var file = new File(_fileSystem, "file.md");
            _fileSystem.GetDirectory(".").Returns(directory);
            _fileSystem.EnumerateDirectory(directory)
                .Returns(Async.From(new[]
                {
                    file
                }));

            _classifier.Classify(directory, file).Returns("text/markdown");

            /* When */
            var actual = await Async.FromAsync(_sut.Enumerate());

            /* Then */
            var contentItem = Assert.Single(actual);
            Assert.NotNull(contentItem!.Type);
            Assert.Equal("text/markdown", contentItem.Type);
        }
    }
}