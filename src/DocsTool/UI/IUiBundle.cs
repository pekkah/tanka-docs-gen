using System.Threading;
using System.Threading.Tasks;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.UI
{
    public interface IUiBundle
    {
        string DefaultTemplate { get; }

        Task Initialize(CancellationToken cancellationToken);

        IPageRenderer GetRenderer(string template);
    }

    public interface IPageRenderer
    {
        string Render(PageRenderingContext context);
    }
}