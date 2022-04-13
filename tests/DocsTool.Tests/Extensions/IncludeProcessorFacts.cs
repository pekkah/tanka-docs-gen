using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Extensions;
using Tanka.FileSystem.Memory;
using Xunit;
using FileSystemPath = Tanka.FileSystem.FileSystemPath;

namespace Tanka.DocsTool.Tests.Extensions
{
    public class IncludeProcessorFacts: IDisposable
    {
        public IncludeProcessorFacts()
        {
            _fileSystem = new InMemoryFileSystem();
        }

        private readonly InMemoryFileSystem _fileSystem;

        private async Task<PipeReader> CreateContentItem(FileSystemPath filePath, string content)
        {
            var file = await _fileSystem.GetOrCreateFile(filePath);
            await using var stream = await file.OpenWrite();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();

            var readStream = await file.OpenRead();
            return PipeReader.Create(readStream);
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
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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

        [Fact]
        public async Task Process_with_params()
        {
            /* Given */
            var md = @"#include::xref://section:file.md?c=Program&f=Main";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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

        [Fact]
        public async Task Process1()
        {
            /* Given */
            var md = @"```csharp
#include::xref://src:DocsTool/Program.cs?f=Main
```";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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

            Assert.Equal(@"```csharp
included
```", actual);
        }

        [Fact]
        public async Task Escape()
        {
            /* Given */
            var md = @"```csharp
\#include::xref://src:DocsTool/Program.cs?f=Main
```";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));
            
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

            Assert.Equal(@"```csharp
\#include::xref://src:DocsTool/Program.cs?f=Main
```", actual);
        }

        [Fact]
        public async Task Escape_and_include()
        {
            /* Given */
            var md = @"```csharp
\#include::xref://src:DocsTool/Program.cs?f=Main

#include::xref://src:DocsTool/Program.cs?f=Main
```";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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

            Assert.Equal(@"```csharp
\#include::xref://src:DocsTool/Program.cs?f=Main

included
```", actual);
        }

        [Fact]
        public async Task Mixed_content_with_headers()
        {
            /* Given */
            var md = @"
# header
#include::xref://src:DocsTool/Program.cs?f=Main
";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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
# header
included
", actual);
        }

        [Fact]
        public async Task Process_full_example()
        {
            /* Given */
            var md = @"## Syntax

### Include csharp code snippets

#### Include file

```markdown
\#include::xref://src:DocsTool/Program.cs
```

```csharp
#include::xref://src:DocsTool/Program.cs
```

#### Include function

```markdown
\#include::xref://src:DocsTool/Program.cs?f=Main
```

```csharp
#include::xref://src:DocsTool/Program.cs?f=Main
```
";
            var reader = new Pipe();
            var writer = new Pipe();
            var sut = new IncludeProcessor(xref => CreateContentItem("file.md", "included"));

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

            Assert.Equal(@"## Syntax

### Include csharp code snippets

#### Include file

```markdown
\#include::xref://src:DocsTool/Program.cs
```

```csharp
included
```

#### Include function

```markdown
\#include::xref://src:DocsTool/Program.cs?f=Main
```

```csharp
included
```
", actual);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }
    }
}