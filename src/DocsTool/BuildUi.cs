﻿using Tanka.DocsTool.UI;

internal class BuildUi : IMiddleware
{
    private readonly IAnsiConsole _console;

    public string Name => "Build UI";

    public BuildUi(IAnsiConsole console)
    {
        _console = console;
    }

    public async Task Invoke(PipelineStep next, BuildContext context)
    {
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
                var ui = new UiBuilder(context.PageCache, context.OutputFs, _console);
                await ui.BuildSite(context.Site ?? throw new InvalidOperationException(), progress);
            });
         
        await next(context);
    }
}