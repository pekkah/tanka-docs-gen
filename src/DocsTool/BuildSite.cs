internal class BuildSite : IMiddleware
{
    private readonly IAnsiConsole _console;

    public string Name => string.Empty;

    public BuildSite(IAnsiConsole console)
    {
        _console = console;
    }

    public async Task Invoke(PipelineStep next, BuildContext context)
    {
        await _console.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Create site", status =>
            {
                context.SiteBuilder.Add(context.Sections);
                context.Site = context.SiteBuilder.Build();

                return Task.CompletedTask;
            });

        await next(context);
    }
}