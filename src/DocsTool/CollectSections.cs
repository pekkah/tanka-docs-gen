﻿public class CollectSections : IMiddleware
{
    private readonly IAnsiConsole _console;

    public string Name => "Collect sections";

    public CollectSections(IAnsiConsole console)
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
                var collector = new SectionCollector(_console);
                await collector.Collect(context.Catalog, progress, context);
                context.Sections = collector.Sections;
            });

        await next(context);
    }
}