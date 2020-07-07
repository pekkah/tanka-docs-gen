using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.UI.Navigation;

namespace Tanka.DocsTool.Extensions
{
    public class IncludeProcessor : IPreProcessor
    {
        public static byte[] IncludeDirective =
        {
            (byte) '#',
            (byte) 'i',
            (byte) 'n',
            (byte) 'c',
            (byte) 'l',
            (byte) 'u',
            (byte) 'd',
            (byte) 'e',
            (byte) ':',
            (byte) ':'
        };

        private readonly Func<Xref, Task<PipeReader>> _resolver;

        public IncludeProcessor(Func<Xref, Task<PipeReader>> resolver)
        {
            _resolver = resolver;
        }

        public async Task Process(IncludeProcessorContext context)
        {
            var reader = context.Reader;
            var writer = context.Writer;

            try
            {
                while (true)
                {
                    var readResult = await reader.ReadAsync();

                    if (readResult.IsCanceled)
                        break;

                    var buffer = readResult.Buffer;

                    if (readResult.IsCompleted && buffer.IsEmpty)
                        break;

                    if (TryReadInclude(readResult, out var start, out var end, out var include))
                    {
                        var beforePosition = buffer.Slice(0, start);

                        if (beforePosition.Length > 0)
                        {
                            //todo: fix
                            var memory = writer.GetMemory((int) beforePosition.Length);
                            beforePosition.CopyTo(memory.Span);
                            writer.Advance((int) beforePosition.Length);
                            await writer.FlushAsync();
                        }

                        //todo: lookup the included file and copy to writer
                        await WriteInclude(writer, include);
                        await writer.FlushAsync();

                        // advance reader
                        reader.AdvanceTo(end, end);
                    }
                    else
                    {
                        if (buffer.IsSingleSegment)
                            await writer.WriteAsync(buffer.First);
                        else
                            foreach (var memory in buffer)
                                await writer.WriteAsync(memory);

                        await writer.FlushAsync();
                        reader.AdvanceTo(buffer.End);
                    }
                }
            }
            catch (Exception x)
            {
                await writer.CompleteAsync(x);
            }
            finally
            {
                await writer.CompleteAsync();
            }
        }

        private async Task WriteInclude(PipeWriter writer, IncludeDirective include)
        {
            var includedContentReader = await _resolver(include.Xref);
            await includedContentReader.CopyToAsync(writer);
            await includedContentReader.CompleteAsync();
        }

        private bool TryReadInclude(ReadResult readResult, out SequencePosition start, out SequencePosition end,
            out IncludeDirective? include)
        {
            var reader = new SequenceReader<byte>(readResult.Buffer);

            while (reader.TryReadTo(out ReadOnlySpan<byte> _, (byte) '#', (byte) '\\', false))
            {
                start = reader.Position;

                if (!reader.IsNext(IncludeDirective, true))
                {
                    reader.Advance(1);
                    continue;
                }

                ReadOnlySpan<byte> bytes;

                // includes are one liners so we read until end of line
                if (reader.TryReadToAny(
                    out ReadOnlySpan<byte> definitionBytes,
                    ParserConstants.ReturnAndNewLine.Span,
                    false))
                {
                    bytes = definitionBytes.ToArray();
                }
                else
                {
                    // there's no more data coming so must be end of the file or stream
                    bytes = reader.Sequence.Slice(reader.Position).ToArray();
                    reader.Advance(bytes.Length);
                }

                end = reader.Position;

                // parse include
                include = ParseInclude(bytes);
                return true;
            }

            start = reader.Position;
            end = reader.Position;
            include = null;
            return false;
        }

        private IncludeDirective ParseInclude(in ReadOnlySpan<byte> bytes)
        {
            var xrefSpan = bytes;

            //todo: fix link parser so that it takes bytes as input
            var temp = Encoding.UTF8.GetString(xrefSpan);

            var xref = LinkParser.Parse(temp).Xref.Value;
            return new IncludeDirective(xref);
        }
    }

    public class IncludeDirective
    {
        public IncludeDirective(Xref xref)
        {
            Xref = xref;
        }

        public Xref Xref { get; set; }
    }

    public class IncludeProcessorContext
    {
        public IncludeProcessorContext(PipeReader reader, PipeWriter writer)
        {
            Reader = reader;
            Writer = writer;
        }

        public PipeReader Reader { get; }

        public PipeWriter Writer { get; }
    }
}