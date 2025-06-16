using Tanka.DocsTool.Catalogs;

public class AggregateContent : IMiddleware
{
    private readonly IAnsiConsole _console;

    public string Name => "Aggregate content";

    public AggregateContent(IAnsiConsole console)
    {
        _console = console;
    }

    public async Task Invoke(PipelineStep next, BuildContext context)
    {
        if (context.HasErrors)
        {
            _console.LogInformation($"Skipping {Name} because of previous errors.");
            await next(context);
            return;
        }

        var aggregator = new ContentAggregator(
                    context.SiteDefinition,
                    context.GitRoot,
                    context.FileSystem,
                    new MimeDbClassifier(),
                    _console);

        await _console.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new ItemCountColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn(), 
                new SpinnerColumn()
            )
            .StartAsync(async progress =>
            {
                await context.Catalog.Add(aggregator.Aggregate(context, progress, CancellationToken.None), CancellationToken.None);
            });

        await next(context);
    }
}