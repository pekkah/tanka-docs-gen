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
}
