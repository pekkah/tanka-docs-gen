using System.Threading.Tasks;

namespace Fugu.GenerateDocs
{
    public delegate Task PipelineStep(PipelineContext context);
}