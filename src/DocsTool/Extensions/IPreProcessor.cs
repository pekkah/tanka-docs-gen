using System.Threading.Tasks;

namespace Tanka.DocsTool.Extensions
{
    public interface IPreProcessor
    {
        Task Process(IncludeProcessorContext context);
    }
}