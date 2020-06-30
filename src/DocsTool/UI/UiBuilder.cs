using System;
using System.Threading;
using System.Threading.Tasks;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;

namespace Tanka.DocsTool.UI
{
    public class UiBuilder
    {
        private readonly IFileSystem _cache;
        private readonly IFileSystem _output;

        public UiBuilder(IFileSystem cache, IFileSystem output)
        {
            _cache = cache;
            _output = output;
        }

        public async Task BuildSite(Site site)
        {
            foreach (var version in site.Versions)
            {
                // compose doc sections
                foreach (var section in site.GetSectionsByVersion(version))
                {
                    var uiBundleRef = LinkParser.Parse("xref://ui-bundle:tanka-docs-section.yml").Xref!.Value;
                    var uiContent = site.GetSectionByXref(uiBundleRef, section);
                    
                    if (uiContent == null)
                        throw new InvalidOperationException($"Could not resolve ui-bundle. Xref '{uiBundleRef}' could not be resolved.'");

                    var uiBundle = new HandlebarsUiBundle(site, uiContent, _output);
                    await uiBundle.Initialize(CancellationToken.None);

                    var composer = new SectionComposer(site, _cache, _output, uiBundle);
                    await composer.ComposeSection(section);
                }
            }
        }
    }
}