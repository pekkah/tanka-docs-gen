using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using Tanka.DocsTool.Definitions;
using System.Threading;

namespace Tanka.DocsTool;

public class DevCommand : AsyncCommand<DevCommandSettings>
{
    private readonly IAnsiConsole _console;
    private readonly IServiceProvider _services;
    private readonly SemaphoreSlim _buildLock = new(1, 1);

    public DevCommand(IAnsiConsole console, IServiceProvider services)
    {
        _console = console;
        _services = services;
    }

    public override async Task<int> ExecuteAsync(
        [NotNull] CommandContext context,
        [NotNull] DevCommandSettings settings)
    {
        var watcher = new FileWatcher();
        try
        {
            // 1. Get site configuration from tanka-docs.yml
            _console.MarkupLine("Initializing...");
            var (currentPath, configFilePath) = PathResolver.ResolvePaths(settings.ConfigFile);
            _console.MarkupLine($"[bold]CurrentPath[/]: {currentPath}");
            _console.MarkupLine($"[bold]ConfigFilePath[/]: {configFilePath}");

            if (!File.Exists(configFilePath))
            {
                _console.MarkupLine($"[red]Error:[/] Could not load configuration: '{configFilePath}'");
                return -1;
            }

            var siteDefinitionResult = (await File.ReadAllTextAsync(configFilePath))
                .TryParseYaml<SiteDefinition>();

            if (siteDefinitionResult.IsFailure)
            {
                _console.MarkupLine($"[red]Error:[/] Could not load configuration '{configFilePath}': {siteDefinitionResult.Error}");
                return -1;
            }

            var site = siteDefinitionResult.Value;

            // 2. Perform an initial full build
            _console.MarkupLine("Performing initial site build...");
            var buildSettings = new BuildSiteCommand.Settings(); // Use default build settings
            var builder = new PipelineBuilder(_services).UseDefault();
            var executor = new PipelineExecutor(buildSettings);
            var buildContext = await executor.Execute(builder, site, currentPath);
            _console.MarkupLine("Initial build complete.");
            
            if (!ReportErrors(_console, buildContext))
                return -1;


            // 4. Set up file watcher
            var webSocketService = new WebSocketService();
            _console.MarkupLine("Setting up file watcher...");
            var pathsToWatch = GetPathsToWatch(site, configFilePath, currentPath);
            var outputPath = Path.GetFullPath(Path.Combine(currentPath, site.OutputPath));

            watcher.Start(pathsToWatch, async (change) =>
            {
                if (change.FullPath.StartsWith(outputPath, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!await _buildLock.WaitAsync(0))
                {
                    _console.MarkupLine("[grey]Build already in progress. Skipping change.[/]");
                    return;
                }
                
                try
                {
                    _console.MarkupLine($"[yellow]Change detected: {change.ChangeType} - {change.FullPath}[/]");
                    _console.MarkupLine("[yellow]Rebuilding site...[/]");
                    
                    // It's important to re-read the configuration on every change
                    var updatedSiteResult = (await File.ReadAllTextAsync(configFilePath))
                        .TryParseYaml<SiteDefinition>();

                    if (updatedSiteResult.IsFailure)
                    {
                        _console.MarkupLine(
                            $"[red]Error:[/] Could not reload configuration '{configFilePath}': {updatedSiteResult.Error}");
                        return;
                    }

                    var updatedSite = updatedSiteResult.Value;
                    var rebuildContext = await executor.Execute(builder, updatedSite, currentPath);
                    
                    if (ReportErrors(_console, rebuildContext))
                    {
                        _console.MarkupLine("[green]Rebuild complete.[/]");
                        // Notify browser to reload
                        await webSocketService.SendReload();
                    }
                    else
                    {
                        _console.MarkupLine("[red]Rebuild failed.[/]");
                    }
                }
                catch (Exception ex)
                {
                    _console.MarkupLine("[red]Error during rebuild:[/]");
                    _console.WriteException(ex);
                }
                finally
                {
                    _buildLock.Release();
                }
            });

            // 5. Set up and run the web server
            var builderOptions = new WebApplicationOptions()
            {
                ContentRootPath = currentPath, WebRootPath = site.OutputPath, Args = context.Remaining.Raw.ToArray()
            };

            var webAppBuilder = WebApplication.CreateBuilder(builderOptions);
            webAppBuilder.Services.AddSingleton(webSocketService);
            webAppBuilder.Logging.ClearProviders(); // prevent default console logging

            var app = webAppBuilder.Build();
            app.UseWebSockets();
            app.Map("/ws", async wsContext =>
            {
                if (wsContext.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await wsContext.WebSockets.AcceptWebSocketAsync();
                    await webSocketService.Handle(webSocket);
                }
                else
                {
                    wsContext.Response.StatusCode = 400;
                }
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            var url = $"http://localhost:{settings.Port}";
            app.Urls.Add(url);

            _console.MarkupLine($"Starting web server at [link]{url}[/]");
            _console.MarkupLine("Use [bold]CTRL+C[/] to stop the server.");
            _console.MarkupLine($"Serving files from: [green]{Path.GetFullPath(site.OutputPath)}[/]");

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            var cancellationToken = lifetime.ApplicationStopping;
            cancellationToken.Register(() => watcher.Stop());

            await app.RunAsync(cancellationToken);

            return 0;
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            return -1;
        }
        finally
        {
            watcher.Stop();
        }
    }

    private static bool ReportErrors(IAnsiConsole console, BuildContext context)
    {
        // report warnings
        foreach (var warning in context.Warnings)
        {
            if (warning.ContentItem != null)
            {
                console.MarkupLine(
                    $"[yellow]Warning:[/] In {warning.ContentItem.File.Path}: {warning.Message}");
            }
            else
            {
                console.MarkupLine($"[yellow]Warning:[/] {warning.Message}");
            }
        }

        // report errors
        if (context.HasErrors)
        {
            console.MarkupLine("[red]Build failed with errors:[/]");
            foreach (var error in context.Errors)
            {
                if (error.ContentItem != null)
                {
                    console.MarkupLine($"- In {error.ContentItem.File.Path}: {error.Message}");
                }
                else
                {
                    console.MarkupLine($"- {error.Message}");
                }
            }

            return false;
        }

        return true;
    }

    public static IEnumerable<string> GetPathsToWatch(SiteDefinition site, string configFilePath, string currentPath)
    {
        var pathsToWatch = new List<string> { configFilePath };
        if (site.Branches.TryGetValue("HEAD", out var headDefinition))
        {
            pathsToWatch.AddRange(headDefinition.InputPath.Select(p => Path.GetFullPath(p, currentPath)));
        }

        return pathsToWatch;
    }
}