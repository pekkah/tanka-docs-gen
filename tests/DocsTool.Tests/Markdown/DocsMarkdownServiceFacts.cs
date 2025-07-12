using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tanka.DocsTool.Markdown;
using Xunit;

namespace Tanka.DocsTool.Tests.Markdown
{
    public class DocsMarkdownServiceFacts
    {
        [Fact]
        public async Task RenderPage_WithMemoryStream_ShouldNotThrowClosedStreamException()
        {
            // Given - Create a DocsMarkdownService
            var service = new DocsMarkdownService(new Markdig.MarkdownPipelineBuilder());
            
            // Create markdown content with emojis (reproducing the original issue)
            var markdownContent = @"---
title: Test Page
---

# Test Page 🚀

This is a test page with emoji content that caused the original issue.

Content includes:
- Emoji characters: 🎉 ✨ 📝
- Regular text
- More content to make it substantial
";

            // Create input stream (simulating processedStream in PageComposer)
            await using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdownContent));
            await using var outputStream = new MemoryStream();
            
            // When - Call RenderPage (this should reproduce the "Cannot access a closed Stream" error)
            var exception = await Record.ExceptionAsync(async () =>
            {
                await service.RenderPage(inputStream, outputStream);
            });
            
            // Then - Should not throw any exception
            Assert.Null(exception);
            
            // Verify output was generated
            Assert.True(outputStream.Length > 0);
            
            // Verify we can still read from the input stream after RenderPage
            inputStream.Position = 0;
            using var reader = new StreamReader(inputStream, Encoding.UTF8);
            var content = await reader.ReadToEndAsync();
            Assert.Contains("Test Page", content);
        }
        
        [Fact]
        public async Task RenderPage_SimulatePageComposerScenario_ShouldHandleStreamLifetime()
        {
            // Given - Simulate the exact scenario from PageComposer.ComposePartialHtmlPage
            var service = new DocsMarkdownService(new Markdig.MarkdownPipelineBuilder());
            
            var markdownContent = @"---
title: Architecture Overview
---

# Architecture Overview 🏗️

System design includes:
- Pipeline architecture
- Multi-layer file system  
- Content processing chain
";

            Exception? caughtException = null;
            string contentPreview = "";
            
            // When - Simulate the exact flow from PageComposer
            await using var processedStream = new MemoryStream(Encoding.UTF8.GetBytes(markdownContent));
            processedStream.Position = 0;
            
            await using var outputStream = new MemoryStream();
            
            try
            {
                var frontmatter = await service.RenderPage(processedStream, outputStream);
                // This should succeed without throwing
                Assert.NotNull(frontmatter);
                Assert.Equal("Architecture Overview", frontmatter.Title);
            }
            catch (Exception e)
            {
                caughtException = e;
                
                // Simulate the error handling code from PageComposer that tries to read the stream
                try
                {
                    processedStream.Position = 0;
                    using var debugReader = new StreamReader(processedStream, Encoding.UTF8, leaveOpen: true);
                    var buffer = new char[100];
                    var charsRead = await debugReader.ReadAsync(buffer, 0, buffer.Length);
                    contentPreview = new string(buffer, 0, charsRead);
                    processedStream.Position = 0;
                }
                catch (Exception debugEx)
                {
                    // This is where we might see "Cannot access a closed Stream"
                    contentPreview = $"[Debug failed: {debugEx.Message}]";
                }
            }
            
            // Then - Should not have any exception
            if (caughtException != null)
            {
                Assert.True(false, $"Unexpected exception: {caughtException.Message}. Content preview: {contentPreview}");
            }
        }
    }
}