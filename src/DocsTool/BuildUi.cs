using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;

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
        try
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
                    await ui.BuildSite(context.Site ?? throw new InvalidOperationException(), progress, context);
                });
        }
        catch (Exception ex)
        {
            context.Add(new Error($"UI build failed: {ex.Message}"));
            _console.MarkupLine($"[red]UI build error:[/] {Markup.Escape(ex.Message)}");
        }

        await next(context);
    }
}