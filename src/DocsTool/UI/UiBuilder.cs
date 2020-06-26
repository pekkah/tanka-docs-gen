using System.Threading;
using System.Threading.Tasks;
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

        public async Task BuildSite(Site site, IUiBundle uiBundle)
        {
            await uiBundle.Initialize(CancellationToken.None);

            foreach (var version in site.Versions)
                // compose doc sections
            foreach (var section in site.GetSectionsByVersion(version))
            {
                var composer = new SectionComposer(site, _cache, _output, uiBundle);
                await composer.ComposeSection(section);
            }
        }
    }
}