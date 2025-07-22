using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;

namespace Tanka.DocsTool.UI
{
    /// <summary>
    /// Unified asset processing service for handling all types of assets
    /// </summary>
    public class AssetProcessor
    {
        private readonly Site _site;
        private readonly IFileSystem _output;
        private readonly ILogger<AssetProcessor> _logger;
        private readonly ConcurrentDictionary<string, bool> _copiedAssets = new();

        public AssetProcessor(Site site, IFileSystem output)
        {
            _site = site;
            _output = output;
            _logger = Infra.LoggerFactory.CreateLogger<AssetProcessor>();
        }

        /// <summary>
        /// Process tracked cross-section xref assets (thread-safe)
        /// </summary>
        public async Task ProcessTrackedXrefAssets(BuildContext buildContext)
        {
            var tasks = buildContext.GetTrackedAssets()
                .Where(NeedsCopying)
                .Select(async assetRef =>
                {
                    try
                    {
                        var outputPath = GenerateOutputPath(assetRef);
                        if (outputPath == null) return;
                        
                        // Thread-safe deduplication
                        if (!_copiedAssets.TryAdd(outputPath, true)) return;
                        
                        await CopyAsset(assetRef.TargetItem, outputPath, buildContext);
                        
                        _logger.LogDebug("Copied xref asset {Xref} to {OutputPath}", assetRef.Xref, outputPath);
                    }
                    catch (Exception ex)
                    {
                        buildContext.Add(new Error($"Failed to copy xref asset '{assetRef.Xref}': {ex.Message}", assetRef.TargetItem));
                    }
                });
                
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Process assets from a section (regular section assets)
        /// </summary>
        public async Task ProcessSectionAssets(Section section, DocsSiteRouter router, BuildContext buildContext)
        {
            var tasks = section.ContentItems
                .Where(ci => IsAsset(ci.Key, ci.Value, section))
                .Select(async item =>
                {
                    try
                    {
                        var outputPath = router.GenerateAssetRoute(new Xref(section.Version, section.Id, item.Key));
                        if (outputPath == null)
                        {
                            buildContext.Add(new Error($"Could not generate output path for asset '{item.Key}'.", item.Value));
                            return;
                        }

                        // Thread-safe deduplication
                        if (!_copiedAssets.TryAdd(outputPath, true)) return;

                        await CopyAsset(item.Value, outputPath, buildContext);
                    }
                    catch (Exception ex)
                    {
                        buildContext.Add(new Error($"Failed to copy section asset '{item.Key}': {ex.Message}", item.Value));
                    }
                });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Process assets from UI bundle (theme assets)
        /// </summary>
        public async Task ProcessUiBundleAssets(Section uiBundle, DocsSiteRouter router, BuildContext buildContext)
        {
            var tasks = uiBundle.ContentItems
                .Where(ci => IsAsset(ci.Key, ci.Value, uiBundle))
                .Select(async item =>
                {
                    try
                    {
                        var outputPath = router.GenerateAssetRoute(new Xref(uiBundle.Version, uiBundle.Id, item.Key));
                        if (outputPath == null)
                        {
                            buildContext.Add(new Error($"Could not generate output path for UI bundle asset '{item.Key}'.", item.Value));
                            return;
                        }

                        // Thread-safe deduplication
                        if (!_copiedAssets.TryAdd(outputPath, true)) return;

                        await CopyAsset(item.Value, outputPath, buildContext);
                        
                        _logger.LogDebug("Copied UI bundle asset {Path} to {OutputPath}", item.Key, outputPath);
                    }
                    catch (Exception ex)
                    {
                        buildContext.Add(new Error($"Failed to copy UI bundle asset '{item.Key}': {ex.Message}", item.Value));
                    }
                });

            await Task.WhenAll(tasks);
        }

        private bool NeedsCopying(XrefAssetReference assetRef)
        {
            // Skip if asset is in same section (handled by regular asset copying)
            return assetRef.SourceSection.Id != assetRef.TargetSection.Id || 
                   assetRef.SourceSection.Version != assetRef.TargetSection.Version;
        }

        private string? GenerateOutputPath(XrefAssetReference assetRef)
        {
            var router = new DocsSiteRouter(_site, assetRef.SourceSection);
            return router.GenerateAssetRoute(assetRef.Xref);
        }

        private bool IsAsset(FileSystemPath relativePath, ContentItem contentItem, Section section)
        {
            if (IsPage(relativePath, contentItem))
                return false;

            return AssetConfiguration.IsAsset(relativePath.ToString(), section.Definition);
        }

        private bool IsPage(FileSystemPath relativePath, ContentItem contentItem)
        {
            if (contentItem.Type != "text/markdown")
                return false;

            var extension = relativePath.GetExtension().ToString().ToLowerInvariant();
            if (extension != ".md")
                return false;

            // nav.md files are not pages
            if (relativePath.GetFileName().ToString().ToLowerInvariant() == "nav.md")
                return false;

            return true;
        }

        private async Task CopyAsset(ContentItem sourceItem, string outputPath, BuildContext buildContext)
        {
            await using var inputStream = await sourceItem.File.OpenRead();
            
            await _output.GetOrCreateDirectory(Path.GetDirectoryName(outputPath));
            var outputFile = await _output.GetOrCreateFile(outputPath);
            await using var outputStream = await outputFile.OpenWrite();
            
            await inputStream.CopyToAsync(outputStream);
        }
    }
}