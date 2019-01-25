using System.Threading.Tasks;

namespace tanka.generate.docs
{
    public delegate Task PipelineStep(PipelineContext context);
}