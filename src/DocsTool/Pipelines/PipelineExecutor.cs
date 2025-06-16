namespace Tanka.DocsTool.Pipelines;

public class PipelineExecutor
{
    public PipelineExecutor(BuildSiteCommand.Settings options)
    {
        Options = options;
    }

    public BuildSiteCommand.Settings Options { get; }

    public async Task<BuildContext> Execute(
        PipelineBuilder pipelineBuilder,
        SiteDefinition siteDefinition,
        FileSystemPath workPath)
    {
        var context = new BuildContext(siteDefinition, workPath);
        var pipeline = pipelineBuilder.Build();
        await pipeline(context);
        return context;
    }
}
