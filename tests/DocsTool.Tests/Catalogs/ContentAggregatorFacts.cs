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

            _directory = new Directory(_fileSystem, "");
            _file = new File(_fileSystem, "file.md");
            _fileSystem.GetDirectory("").Returns(_directory);
            _fileSystem.EnumerateDirectory(_directory)
                .Returns(Async.From(new[]
                {
                    _file
                }));

            _sut = new ContentAggregator(
                _classifier,
                _directory);
        }

        private readonly IFileSystem _fileSystem;
        private readonly ContentAggregator _sut;
        private readonly IContentClassifier _classifier;
        private Directory _directory;
        private File _file;

        [Fact]
        public async Task Classify_ContentItems()
        {
            /* Given */
            _classifier.Classify(_directory, _file).Returns("text/markdown");

            /* When */
            var actual = await Async.FromAsync(_sut.Enumerate());

            /* Then */
            var contentItem = Assert.Single(actual);
            Assert.NotNull(contentItem!.Type);
            Assert.Equal("text/markdown", contentItem.Type);
        }
    }
}