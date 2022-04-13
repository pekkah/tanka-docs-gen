using Spectre.Console;
using Spectre.Console.Cli;

public class DevCommand : AsyncCommand<DevCommand.Settings>
{
    private readonly IAnsiConsole _console;

    public DevCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await _console.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading configuration..", async status =>
            {
                await Task.Delay(1000);
                status.Status("Loaded");
            });

        _console.MarkupLineInterpolated($"[green]Configuration loaded[/]");
        await _console.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading content sources..", async status =>
            {
                await Task.Delay(1000);
            });

        _console.MarkupLineInterpolated($"[green]Configuration loaded[/]");
        await _console.Progress()
            .StartAsync(async progress =>
            {
                var tasks = new[]
                {
                    progress.AddTask("Branch: master"),
                    progress.AddTask("Tag: 1.0")
                };

                foreach (var task in tasks)
                {
                    while (!task.IsFinished)
                    {
                        task.Increment(1);
                        await Task.Delay(10);
                    }
                }
            });

        _console.MarkupLineInterpolated($"[green]Configuration loaded[/]");
        await _console.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading content..", async status =>
            {
                await Task.Delay(1000);
            });

        _console.MarkupLineInterpolated($"[green]COMPLETE[/]");
        return 1;
    }

    public class Settings : CommandSettings
    {

    }
}