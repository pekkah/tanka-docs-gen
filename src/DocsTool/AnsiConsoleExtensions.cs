using Spectre.Console;

namespace Tanka.DocsTool;

internal static class AnsiConsoleExtensions
{
    public static void LogInformation(this IAnsiConsole console, FormattableString message)
    {
        console.MarkupLineInterpolated($"[[{DateTimeOffset.Now:T}]] [green bold]INFO[/]: {message}");
    }

    public static void LogWarning(this IAnsiConsole console, FormattableString message)
    {
        console.MarkupLineInterpolated($"[[{DateTimeOffset.Now:T}]] [yellow bold]INFO[/]: {message}");
    }

    public static void LogError(this IAnsiConsole console, FormattableString message)
    {
        console.MarkupLineInterpolated($"[[{DateTimeOffset.Now:T}]] [red bold]INFO[/]: {message}");
    }

    public static void LogDebug(this IAnsiConsole console, FormattableString message)
    {
        console.MarkupLineInterpolated($"[grey][[{DateTimeOffset.Now:T}]] [bold]DEBUG[/]: {message}[/]");
    }

    public static void LogError(this IAnsiConsole console, Exception exception, FormattableString message)
    {
        console.MarkupLineInterpolated($"[[{DateTimeOffset.Now:T}]] [red bold]INFO[/]: {message}");
        console.WriteException(exception);
    }

    // Validation message extension methods for consistent styling
    public static void WriteError(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
    }

    public static void WriteError(this IAnsiConsole console, string message, string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
            console.MarkupLine($"[red]Error:[/] In {Markup.Escape(filePath)}: {Markup.Escape(message)}");
        else
            console.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
    }

    public static void WriteWarning(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[yellow]Warning:[/] {Markup.Escape(message)}");
    }

    public static void WriteWarning(this IAnsiConsole console, string message, string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
            console.MarkupLine($"[yellow]Warning:[/] In {Markup.Escape(filePath)}: {Markup.Escape(message)}");
        else
            console.MarkupLine($"[yellow]Warning:[/] {Markup.Escape(message)}");
    }

    public static void WriteBuildFailure(this IAnsiConsole console)
    {
        console.MarkupLine("[red]Build failed with errors:[/]");
    }
}
