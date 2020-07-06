using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Extensions.Includes;
using Tanka.FileSystem.Memory;
using Xunit;
using Path = Tanka.FileSystem.Path;

namespace Tanka.DocsTool.Tests.Extensions
{
    public class IncludeProcessorFacts
    {
        public IncludeProcessorFacts()
        {
            _fileSystem = new InMemoryFileSystem();
        }

        private readonly InMemoryFileSystem _fileSystem;

        private async Task<ContentItem> CreateContentItem(Path filePath, string content)
        {
            var file = await _fileSystem.GetOrCreateFile(filePath);
            await using var stream = await file.OpenWrite();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);

            return new ContentItem(
                new FileSystemContentSource(_fileSystem, "master", ""), "text/markdown",
                file);
        }


        [Fact]
        public async Task Output_after()
        {
            /* Given */
            var md = @"
#include::xref://section:file.md
after
";
            var reader = new Pipe();
            var writer = new Pipe();
            var contentItem = await CreateContentItem("file.md", "included");
            var sut = new IncludeProcessor(xref => contentItem);

            reader.Writer.Write(Encoding.UTF8.GetBytes(md));
            await reader.Writer.FlushAsync();
            await reader.Writer.CompleteAsync();

            /* When */
            await sut.Process(new IncludeProcessorContext(reader.Reader, writer.Writer));

            /* Then */
            await using var stream = new MemoryStream();
            await writer.Reader.CopyToAsync(stream);

            stream.Position = 0;
            using var streamWriter = new StreamReader(stream);
            var actual = await streamWriter.ReadToEndAsync();

            Assert.Equal(@"
included
after
", actual);
        }

        [Fact]
        public async Task Output_before()
        {
            /* Given */
            var md = @"
before
#include::xref://section:file.md
";
            var reader = new Pipe();
            var writer = new Pipe();
            var contentItem = await CreateContentItem("file.md", "included");
            var sut = new IncludeProcessor(xref => contentItem);

            reader.Writer.Write(Encoding.UTF8.GetBytes(md));
            await reader.Writer.FlushAsync();
            await reader.Writer.CompleteAsync();

            /* When */
            await sut.Process(new IncludeProcessorContext(reader.Reader, writer.Writer));

            /* Then */
            await using var stream = new MemoryStream();
            await writer.Reader.CopyToAsync(stream);

            stream.Position = 0;
            using var streamWriter = new StreamReader(stream);
            var actual = await streamWriter.ReadToEndAsync();

            Assert.Equal(@"
before
included
", actual);
        }

        [Fact]
        public async Task Output_both()
        {
            /* Given */
            var md = @"
before
#include::xref://section:file.md
after
";
            var reader = new Pipe();
            var writer = new Pipe();
            var contentItem = await CreateContentItem("file.md", "included");
            var sut = new IncludeProcessor(xref => contentItem);

            reader.Writer.Write(Encoding.UTF8.GetBytes(md));
            await reader.Writer.FlushAsync();
            await reader.Writer.CompleteAsync();

            /* When */
            await sut.Process(new IncludeProcessorContext(reader.Reader, writer.Writer));

            /* Then */
            await using var stream = new MemoryStream();
            await writer.Reader.CopyToAsync(stream);

            stream.Position = 0;
            using var streamWriter = new StreamReader(stream);
            var actual = await streamWriter.ReadToEndAsync();

            Assert.Equal(@"
before
included
after
", actual);
        }

        [Fact]
        public async Task Process()
        {
            /* Given */
            var md = @"#include::xref://section:file.md";
            var reader = new Pipe();
            var writer = new Pipe();
            var contentItem = await CreateContentItem("file.md", "included");
            var sut = new IncludeProcessor(xref => contentItem);

            reader.Writer.Write(Encoding.UTF8.GetBytes(md));
            await reader.Writer.FlushAsync();
            await reader.Writer.CompleteAsync();

            /* When */
            await sut.Process(new IncludeProcessorContext(reader.Reader, writer.Writer));

            /* Then */
            await using var stream = new MemoryStream();
            await writer.Reader.CopyToAsync(stream);

            stream.Position = 0;
            using var streamWriter = new StreamReader(stream);
            var actual = await streamWriter.ReadToEndAsync();

            Assert.Equal("included", actual);
        }
    }
}