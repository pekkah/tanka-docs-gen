using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Tanka.DocsTool.Extensions.Includes;
using Xunit;

namespace Tanka.DocsTool.Tests.Extensions
{
    public class IncludeProcessorFacts
    {
        [Fact]
        public async Task Process()
        {
            /* Given */
            var md = @"#include::xref://section:file.md";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor();

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

            Assert.Equal("todo: include", actual);
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
            var sut = new IncludeProcessor();

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
todo: include
", actual);
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
            var sut = new IncludeProcessor();

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
todo: include
after
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
            var sut = new IncludeProcessor();

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
todo: include
after
", actual);
        }
    }
}