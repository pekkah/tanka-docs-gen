namespace Tanka.DocsTool.Pipelines;

public class PipelineExecutor
{
    public PipelineExecutor(BuildSiteCommand.Settings options)
    {
        Options = options;
    }

    public BuildSiteCommand.Settings Options { get; }

    public Task Execute(
        PipelineBuilder pipelineBuilder,
        SiteDefinition siteDefinition,
        FileSystemPath workPath)
    {
        var context = new BuildContext(siteDefinition, workPath);
        var pipeline = pipelineBuilder.Build();
        return pipeline(context);
    }
}
