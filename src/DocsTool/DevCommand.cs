﻿using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Tanka.DocsTool.Definitions;

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
        WebSocketService? webSocketService = null;

        // Create a cancellation token source that we control
        using var cts = new CancellationTokenSource();

        // Handle Ctrl+C properly
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Prevent immediate termination
            _console.MarkupLine("[yellow]Shutting down...[/]");
            cts.Cancel();
        };

        try
        {
            // 1. Get site configuration from tanka-docs.yml
            _console.MarkupLine("Initializing...");
            var (currentPath, configFilePath) = PathResolver.ResolvePaths(settings.ConfigFile);
            _console.MarkupLine($"[bold]CurrentPath[/]: {currentPath}");
            _console.MarkupLine($"[bold]ConfigFilePath[/]: {configFilePath}");

            if (!File.Exists(configFilePath))
            {
                _console.WriteError($"Could not load configuration: '{configFilePath}'");
                return -1;
            }

            var siteDefinitionResult = (await File.ReadAllTextAsync(configFilePath, cts.Token))
                .TryParseYaml<SiteDefinition>();

            if (siteDefinitionResult.IsFailure)
            {
                _console.WriteError($"Could not load configuration '{configFilePath}': {siteDefinitionResult.Error}");
                return -1;
            }

            var site = siteDefinitionResult.Value;

            // 2. Perform an initial full build
            _console.MarkupLine("Performing initial site build...");
            var buildSettings = new BuildSiteCommand.Settings
            {
                LinkValidation = settings.LinkValidation
            };
            var builder = new PipelineBuilder(_services).UseDefault();
            var executor = new PipelineExecutor(buildSettings);
            var buildContext = await executor.Execute(builder, site, currentPath);
            _console.MarkupLine("Initial build complete.");

            if (!ReportErrors(_console, buildContext))
                return -1;


            // 4. Set up file watcher
            webSocketService = new WebSocketService();
            _console.MarkupLine("Setting up file watcher...");
            var pathsToWatch = GetPathsToWatch(site, configFilePath, currentPath);
            var outputPath = Path.GetFullPath(Path.Combine(currentPath, site.OutputPath));

            watcher.Start(pathsToWatch, async (change) =>
            {
                // Check if cancellation was requested
                if (cts.Token.IsCancellationRequested)
                    return;

                if (change.FullPath.StartsWith(outputPath, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!await _buildLock.WaitAsync(0, cts.Token))
                {
                    _console.MarkupLine("[grey]Build already in progress. Skipping change.[/]");
                    return;
                }

                try
                {
                    _console.MarkupLine($"[yellow]Change detected: {change.ChangeType} - {change.FullPath}[/]");
                    _console.MarkupLine("[yellow]Rebuilding site...[/]");

                    // It's important to re-read the configuration on every change
                    var updatedSiteResult = (await File.ReadAllTextAsync(configFilePath, cts.Token))
                        .TryParseYaml<SiteDefinition>();

                    if (updatedSiteResult.IsFailure)
                    {
                        _console.WriteError($"Could not reload configuration '{configFilePath}': {updatedSiteResult.Error}");
                        return;
                    }

                    var updatedSite = updatedSiteResult.Value;

                    // Use relaxed validation for file watcher rebuilds to avoid interrupting development
                    var rebuildSettings = new BuildSiteCommand.Settings
                    {
                        LinkValidation = LinkValidation.Relaxed
                    };
                    var rebuildExecutor = new PipelineExecutor(rebuildSettings);
                    var rebuildContext = await rebuildExecutor.Execute(builder, updatedSite, currentPath);

                    if (ReportErrors(_console, rebuildContext))
                    {
                        _console.MarkupLine("[green]Rebuild complete.[/]");
                        // Notify browser to reload
                        await webSocketService.SendReload(cts.Token);
                    }
                    else
                    {
                        _console.WriteError("Rebuild failed.");
                    }
                }
                catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    // Expected during shutdown
                }
                catch (Exception ex)
                {
                    _console.WriteError("Error during rebuild:");
                    _console.WriteException(ex);
                }
                finally
                {
                    _buildLock.Release();
                }
            }, cts.Token);

            // 5. Set up and run the web server
            var builderOptions = new WebApplicationOptions()
            {
                ContentRootPath = currentPath,
                WebRootPath = site.OutputPath,
                Args = context.Remaining.Raw.ToArray()
            };

            var webAppBuilder = WebApplication.CreateBuilder(builderOptions);
            webAppBuilder.Services.AddSingleton(webSocketService);
            webAppBuilder.Logging.ClearProviders(); // prevent default console logging

            var app = webAppBuilder.Build();
            app.UseWebSockets();
            app.Map("/ws", wsApp =>
            {
                wsApp.Run(async wsContext =>
                {
                    if (wsContext.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await wsContext.WebSockets.AcceptWebSocketAsync();
                        await webSocketService.Handle(webSocket, cts.Token);
                    }
                    else
                    {
                        wsContext.Response.StatusCode = 400;
                    }
                });
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            var url = $"http://localhost:{settings.Port}";
            app.Urls.Add(url);

            _console.MarkupLine($"Starting web server at [link]{url}[/]");
            _console.MarkupLine("Use [bold]CTRL+C[/] to stop the server.");
            _console.MarkupLine($"Serving files from: [green]{Path.GetFullPath(site.OutputPath)}[/]");

            // Use our own cancellation token instead of the application lifetime
            await app.RunAsync(cts.Token);

            return 0;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _console.MarkupLine("[green]Shutdown complete.[/]");
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
            webSocketService?.Dispose();
        }
    }

    private static bool ReportErrors(IAnsiConsole console, BuildContext context)
    {
        // report warnings
        foreach (var warning in context.Warnings)
        {
            if (warning.ContentItem != null)
            {
                console.WriteWarning(warning.Message, warning.ContentItem.File.Path.ToString());
            }
            else
            {
                console.WriteWarning(warning.Message);
            }
        }

        // report errors
        if (context.HasErrors)
        {
            console.WriteBuildFailure();
            foreach (var error in context.Errors)
            {
                if (error.ContentItem != null)
                {
                    console.MarkupLine($"- In {Markup.Escape(error.ContentItem.File.Path.ToString())}: {Markup.Escape(error.Message)}");
                }
                else
                {
                    console.MarkupLine($"- {Markup.Escape(error.Message)}");
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