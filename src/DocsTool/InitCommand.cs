using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Tanka.DocsTool.Init;

namespace Tanka.DocsTool;

/// <summary>
/// Command to initialize a new Tanka Docs project.
/// </summary>
public class InitCommand : AsyncCommand<InitCommandSettings>
{
    private readonly IAnsiConsole _console;

    public InitCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, InitCommandSettings settings)
    {
        try
        {
            _console.MarkupLine("[bold green]Initializing Tanka Docs project...[/]");

            // Resolve output directory
            var outputDir = string.IsNullOrEmpty(settings.OutputDir) 
                ? Directory.GetCurrentDirectory() 
                : Path.GetFullPath(settings.OutputDir);

            _console.MarkupLine($"[dim]Output directory: {outputDir}[/]");

            // Change to output directory for git operations
            var originalDir = Directory.GetCurrentDirectory();
            if (outputDir != originalDir)
            {
                Directory.SetCurrentDirectory(outputDir);
            }

            try
            {
                // Validate git repository (unless only extracting UI bundle)
                if (!settings.UiBundleOnly)
                {
                    if (!GitValidator.IsGitRepository())
                    {
                        _console.MarkupLine("[red]Error:[/] Current directory is not a Git repository.");
                        _console.MarkupLine("[dim]Run 'git init' to initialize a Git repository.[/]");
                        return -1;
                    }
                }

                // Detect or use specified branch
                string defaultBranch = "main";
                if (!settings.UiBundleOnly)
                {
                    if (!string.IsNullOrEmpty(settings.Branch))
                    {
                        defaultBranch = settings.Branch;
                        _console.MarkupLine($"[dim]Using specified branch: {defaultBranch}[/]");
                    }
                    else
                    {
                        var detectedBranch = GitValidator.GetCurrentBranch();
                        if (!string.IsNullOrEmpty(detectedBranch))
                        {
                            defaultBranch = detectedBranch;
                            _console.MarkupLine($"[dim]Detected branch: {defaultBranch}[/]");
                        }
                        else
                        {
                            _console.MarkupLine($"[yellow]Warning:[/] Could not detect branch, using default: {defaultBranch}");
                        }
                    }
                }

                // Derive or use specified project name
                string projectName = "My Docs";
                if (!string.IsNullOrEmpty(settings.ProjectName))
                {
                    projectName = settings.ProjectName;
                }
                else
                {
                    var derivedName = TemplateProcessor.DeriveProjectNameFromDirectory();
                    if (!string.IsNullOrEmpty(derivedName))
                    {
                        projectName = derivedName;
                    }
                }
                _console.MarkupLine($"[dim]Project name: {projectName}[/]");

                var createdFiles = new List<string>();

                // Extract UI bundle
                if (!settings.ConfigOnly)
                {
                    _console.MarkupLine("[bold]Extracting UI bundle...[/]");
                    
                    var uiBundlePath = Path.Combine(outputDir, "ui-bundle");
                    
                    try
                    {
                        ZipExtractor.ExtractUiBundleToDirectory(uiBundlePath, settings.Force);
                        _console.MarkupLine("[green]✓[/] UI bundle extracted to: ui-bundle/");
                        createdFiles.Add("ui-bundle/");
                    }
                    catch (InvalidOperationException ex)
                    {
                        _console.MarkupLine($"[red]Error:[/] {ex.Message}");
                        return -1;
                    }
                }

                // Create configuration files
                if (!settings.UiBundleOnly)
                {
                    _console.MarkupLine("[bold]Creating configuration files...[/]");

                    // Create main configuration
                    var mainConfigPath = Path.Combine(outputDir, "tanka-docs.yml");
                    if (File.Exists(mainConfigPath) && !settings.Force)
                    {
                        _console.MarkupLine("[yellow]⚠[/] Skipped existing: tanka-docs.yml");
                    }
                    else
                    {
                        var mainConfigContent = TemplateProcessor.ProcessDefaultConfig(projectName, defaultBranch);
                        await File.WriteAllTextAsync(mainConfigPath, mainConfigContent);
                        _console.MarkupLine("[green]✓[/] Created: tanka-docs.yml");
                        createdFiles.Add("tanka-docs.yml");
                    }

                    // Create WIP configuration (unless --no-wip specified)
                    if (!settings.NoWip)
                    {
                        var wipConfigPath = Path.Combine(outputDir, "tanka-docs-wip.yml");
                        if (File.Exists(wipConfigPath) && !settings.Force)
                        {
                            _console.MarkupLine("[yellow]⚠[/] Skipped existing: tanka-docs-wip.yml");
                        }
                        else
                        {
                            var wipConfigContent = TemplateProcessor.ProcessDefaultWipConfig(projectName);
                            await File.WriteAllTextAsync(wipConfigPath, wipConfigContent);
                            _console.MarkupLine("[green]✓[/] Created: tanka-docs-wip.yml");
                            createdFiles.Add("tanka-docs-wip.yml");
                        }
                    }
                }

                // Success summary
                _console.MarkupLine("[bold green]✓ Initialization completed![/]");
                
                if (createdFiles.Any())
                {
                    _console.MarkupLine($"[dim]Created {createdFiles.Count} files/directories[/]");
                }

                // Post-initialization guidance (unless --quiet)
                if (!settings.Quiet)
                {
                    _console.WriteLine();
                    _console.MarkupLine("[bold]Next steps:[/]");
                    
                    if (!settings.UiBundleOnly)
                    {
                        _console.MarkupLine("1. Review and customize your configuration:");
                        _console.MarkupLine("   [cyan]tanka-docs.yml[/] - Production build configuration");
                        if (!settings.NoWip)
                        {
                            _console.MarkupLine("   [cyan]tanka-docs-wip.yml[/] - Development/WIP configuration");
                        }
                    }

                    if (!settings.ConfigOnly)
                    {
                        _console.MarkupLine("2. Customize your site appearance:");
                        _console.MarkupLine("   [cyan]ui-bundle/[/] - Templates and styling");
                    }

                    _console.MarkupLine("3. Start building your documentation:");
                    _console.MarkupLine("   [green]tanka-docs build[/] - Build your site");
                    _console.MarkupLine("   [green]tanka-docs dev[/] - Start development server");
                }

                return 0;
            }
            finally
            {
                // Restore original directory
                if (outputDir != originalDir)
                {
                    Directory.SetCurrentDirectory(originalDir);
                }
            }
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            return -1;
        }
    }
} 