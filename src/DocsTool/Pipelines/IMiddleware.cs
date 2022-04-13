namespace Tanka.DocsTool.Pipelines;

public interface IMiddleware
{
    string Name { get; }

    Task Invoke(PipelineStep next, BuildContext context);
}