using System.Threading;
using System.Threading.Tasks;

namespace Tanka.DocsTool.UI
{
    public interface IUiBundle
    {
        string DefaultTemplate { get; }

        Task Initialize(CancellationToken cancellationToken);

        IPageRenderer GetPageRenderer(string template, DocsSiteRouter router);
    }
}