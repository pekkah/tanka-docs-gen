using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;
using Tanka.FileSystem;
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
                Assert.Fail($"Unexpected exception: {caughtException.Message}. Content preview: {contentPreview}");
            }
        }

        [Fact]
        public async Task RenderPage_XrefImage_ShouldResolveToActualPath()
        {
            // Given - Set up real site and sections for xref resolution (based on NavigationBuilderFacts pattern)
            var source = Substitute.For<IContentSource>();
            source.Version.Returns("HEAD");
            source.Path.Returns(new FileSystemPath("visual-tests"));

            var currentSectionFile = Substitute.For<IReadOnlyFile>();
            currentSectionFile.Path.Returns((FileSystemPath)"visual-tests/tanka-docs-section.yml");

            var currentSection = new Section(new ContentItem(
                    source, "tanka/section", currentSectionFile),
                new SectionDefinition()
                {
                    Id = "current"
                }, new Dictionary<FileSystemPath, ContentItem>());

            var targetSectionFile = Substitute.For<IReadOnlyFile>();
            targetSectionFile.Path.Returns((FileSystemPath)"visual-tests/tanka-docs-section.yml");

            var imageFile = Substitute.For<IReadOnlyFile>();
            imageFile.Path.Returns((FileSystemPath)"visual-tests/Snapshots/Graphics/PorterDuffVisual.PorterDuff_BasicModes_Demonstration.verified.png");

            var targetSection = new Section(new ContentItem(
                    source, "tanka/section", targetSectionFile),
                new SectionDefinition()
                {
                    Id = "visual-tests"
                }, new Dictionary<FileSystemPath, ContentItem>()
                {
                    ["Snapshots/Graphics/PorterDuffVisual.PorterDuff_BasicModes_Demonstration.verified.png"] = 
                        new ContentItem(source, "image/png", imageFile)
                });

            var site = new Site(
                new SiteDefinition(),
                new Dictionary<string, Dictionary<string, Section>>()
                {
                    ["HEAD"] = new Dictionary<string, Section>()
                    {
                        ["current"] = currentSection,
                        ["visual-tests"] = targetSection
                    }
                });

            var router = new DocsSiteRouter(site, currentSection);
            var buildContext = new BuildContext(new SiteDefinition(), "/test");
            var context = new DocsMarkdownRenderingContext(site, currentSection, router, buildContext);
            
            var service = new DocsMarkdownService(context);

            var markdownContent = @"# Test Page

![Basic Porter-Duff Modes](xref://visual-tests:Snapshots/Graphics/PorterDuffVisual.PorterDuff_BasicModes_Demonstration.verified.png)

This should render with resolved image path.
";

            // When - Render the markdown
            await using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdownContent));
            
            var (html, frontmatter) = await service.RenderPage(inputStream);

            // Then - The xref:// URL should be resolved to actual path
            // Based on DocsSiteRouter.GenerateRoute: targetSection.Version + "/" + targetSection.Path / path
            // Should be: HEAD/[empty_dir]/Snapshots/Graphics/...png 
            Assert.Contains("Snapshots/Graphics/PorterDuffVisual.PorterDuff_BasicModes_Demonstration.verified.png", html);
            Assert.DoesNotContain("xref://visual-tests:", html);
            Assert.Contains("<img src=\"", html);
            Assert.Contains("class=\"img-fluid\"", html);
            Assert.Contains("alt=\"Basic Porter-Duff Modes\"", html);
        }

        [Fact]
        public async Task RenderPage_XrefImageBroken_ShouldHandleGracefullyInRelaxedMode()
        {
            // Given - Set up site with no target section (broken xref scenario)
            var source = Substitute.For<IContentSource>();
            source.Version.Returns("HEAD");
            source.Path.Returns(new FileSystemPath("current"));

            var currentSectionFile2 = Substitute.For<IReadOnlyFile>();
            currentSectionFile2.Path.Returns((FileSystemPath)"current/tanka-docs-section.yml");
            
            var currentSection = new Section(new ContentItem(
                    source, "tanka/section", currentSectionFile2),
                new SectionDefinition()
                {
                    Id = "current"
                }, new Dictionary<FileSystemPath, ContentItem>());

            // Empty site - no visual-tests section exists
            var site = new Site(
                new SiteDefinition(),
                new Dictionary<string, Dictionary<string, Section>>()
                {
                    ["HEAD"] = new Dictionary<string, Section>()
                    {
                        ["current"] = currentSection
                        // Note: no "non-existent" section = broken xref
                    }
                });

            var router = new DocsSiteRouter(site, currentSection);
            var buildContext = new BuildContext(new SiteDefinition(), "/test")
            {
                LinkValidation = LinkValidation.Relaxed
            };
            var context = new DocsMarkdownRenderingContext(site, currentSection, router, buildContext);
            
            var service = new DocsMarkdownService(context);

            var markdownContent = @"# Test Page

![Broken Image](xref://non-existent:broken.png)

This should render with broken xref placeholder.
";

            // When - Render the markdown
            await using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdownContent));
            
            var (html, frontmatter) = await service.RenderPage(inputStream);

            // Then - Should generate broken xref placeholder
            Assert.Contains("#broken-xref-", html);
            Assert.Contains("class=\"broken-xref-image\"", html);
            Assert.Contains("data-original-xref=", html);
            Assert.Contains("data-original-xref=\"xref://non-existent:broken.png\"", html);
            // xref URL should NOT appear in the main src attribute (only in data-original-xref)
            Assert.DoesNotContain("src=\"xref://non-existent:", html);
        }
    }
}