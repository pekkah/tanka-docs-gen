---
title: Command Line Interface Reference
---

# Command Line Interface Reference

Tanka Docs provides a command-line interface (CLI) that allows you to build and manage your documentation projects. The tool can be run directly from source or installed as a local .NET Global Tool.

## Installation

Install Tanka Docs as a .NET global tool:

```bash
# Install from NuGet (recommended)
dotnet tool install --global Tanka.DocsGen
```

**Alternative installation from source (for development):**

```bash
# Clone and build from source
git clone https://github.com/pekkah/tanka-docs-gen.git
cd tanka-docs-gen
dotnet pack -c Release -o ./artifacts
dotnet tool install --global --add-source ./artifacts Tanka.DocsGen
```

To update to the latest version:

```bash
# Update from NuGet
dotnet tool update --global Tanka.DocsGen

# Or update from source (if installed from source)
git pull
dotnet pack -c Release -o ./artifacts
dotnet tool update --global --add-source ./artifacts Tanka.DocsGen
```

## Commands

### `tanka-docs init`

Initialize a new Tanka Docs project with default configuration and UI templates.

**Syntax:**
```bash
tanka-docs init [options]
```

**Options:**

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--force` | `-f` | Overwrite existing files without prompting | `false` |
| `--ui-bundle-only` | | Only extract UI bundle, skip configuration file creation | `false` |
| `--config-only` | | Only create configuration files, skip UI bundle extraction | `false` |
| `--no-wip` | | Skip creating `tanka-docs-wip.yml` (create only main configuration) | `false` |
| `--branch <BRANCH>` | | Specify default branch name (default: auto-detect from git) | Auto-detected |
| `--output-dir <PATH>` | | Specify output directory (default: current directory) | Current directory |
| `--quiet` | `-q` | Skip post-creation configuration guidance (for automation) | `false` |
| `--project-name <NAME>` | | Specify project name (default: derive from directory name) | Auto-derived |

**Prerequisites:**
- Must be run in a Git repository (use `git init` first if needed)
- Requires .NET 9+ and the Tanka Docs global tool to be installed

**Examples:**

```bash
# Basic initialization in current directory
tanka-docs init

# Initialize with custom project name
tanka-docs init --project-name "My Documentation Site"

# Initialize with specific branch
tanka-docs init --branch main

# Only create configuration files (useful if you have custom UI)
tanka-docs init --config-only

# Only extract UI bundle (useful for UI customization)
tanka-docs init --ui-bundle-only

# Force overwrite existing files
tanka-docs init --force

# Quiet mode for automation scripts
tanka-docs init --quiet --project-name "Auto-Generated Docs"

# Initialize in a specific directory
tanka-docs init --output-dir ./my-docs-project
```

**Generated Files:**

The init command creates the following structure:

```
./
├── tanka-docs.yml          # Production configuration
├── tanka-docs-wip.yml      # Development/WIP configuration
└── ui-bundle/              # Customizable UI templates
    ├── article.hbs         # Main page template
    ├── tanka-docs-section.yml
    └── partials/
        ├── Navigation.hbs
        └── NavigationItem.hbs
```

**Configuration Templates:**

- **`tanka-docs.yml`**: Production configuration using the detected Git branch for stable builds
- **`tanka-docs-wip.yml`**: Development configuration using `HEAD` for work-in-progress content

### `tanka-docs build`

The primary command for generating static documentation from your source files.

**Syntax:**
```bash
tanka-docs build [options]
```

**Options:**

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--debug` | | Enable verbose output for troubleshooting | `false` |
| `--file <FILE>` | `-f` | Path to the `tanka-docs.yml` configuration file | `./tanka-docs.yml` |
| `--output <OUTPUT>` | `-o` | Output directory for generated site | From config file |
| `--build <BUILD>` | `-b` | Build/cache directory for intermediate files | From config file |
| `--base <BASE>` | | Base href for generated HTML pages (useful for subdirectory deployments) | From config file |

**Examples:**

```bash
# Basic build using default configuration
tanka-docs build

# Build with custom configuration file
tanka-docs build -f ./custom-config.yml

# Build with custom output directory
tanka-docs build -o ./custom-output

# Build with debug output
tanka-docs build --debug

# Build for deployment to subdirectory
tanka-docs build --base "/my-docs/"

# Combine multiple options
tanka-docs build -f ./config.yml -o ./dist --debug
```

### `tanka-docs dev`

Development server mode for live preview during documentation writing.

**Syntax:**
```bash
tanka-docs dev [options]
```

**Options:**

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--file <FILE>` | `-f` | Path to the `tanka-docs.yml` configuration file | `./tanka-docs.yml` |
| `--port <PORT>` | `-p` | Port to run the development server on | `5000` |

**Examples:**

```bash
# Run dev server with default settings
tanka-docs dev

# Run dev server on a custom port
tanka-docs dev --port 8080

# Use a custom configuration file
tanka-docs dev -f ./custom-config.yml
```

**Features:**
- **Live reload**: WebSocket-based automatic browser refresh when files change
- **File watching**: Monitors all input directories and configuration files
- **Development server**: Built-in HTTP server for preview
- **Concurrent build protection**: Prevents multiple builds from running simultaneously
- **Real-time feedback**: Shows build status and errors in the console

## Configuration File Resolution

When you run commands like `tanka-docs build` or `tanka-docs dev`, the tool looks for configuration files in the following order:

1. **Explicit path**: If you specify `-f` or `--file`, it uses that exact path
2. **Current directory**: Looks for `tanka-docs.yml` in the current working directory
3. **Error**: If no configuration file is found, the command fails with an error

**Note**: Use `tanka-docs init` to create default configuration files if you don't have them yet.

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `-1` | General error (configuration not found, build failed, etc.) |
| `1` | Command-specific error |

## Common Usage Patterns

### New Project Setup

```bash
# Create a new project from scratch
mkdir my-docs && cd my-docs
git init
tanka-docs init

# Create documentation content
mkdir docs
echo "# Welcome" > docs/index.md
git add . && git commit -m "Initial docs"

# Start development
tanka-docs dev
```

### Local Development

```bash
# Build and output to default location
tanka-docs build

# Build with debug output to see detailed logs
tanka-docs build --debug

# Use WIP configuration for development
tanka-docs dev -f tanka-docs-wip.yml
```

### CI/CD Integration

```bash
# Build for production deployment
tanka-docs build --output ./dist --base "/docs/"

# Build with custom configuration for different environments
tanka-docs build -f ./config/production.yml -o ./dist
```

### Multiple Configurations

```bash
# Development build
tanka-docs build -f ./config/dev.yml -o ./dev-output

# Production build
tanka-docs build -f ./config/prod.yml -o ./prod-output --base "/api-docs/"
```

## Working Directory

The tool uses the directory containing the configuration file as the working directory for resolving relative paths. This means:

- If you specify `-f ./config/tanka-docs.yml`, the working directory becomes `./config/`
- All relative paths in the configuration are resolved relative to this directory
- Source paths, output paths, and other file references are resolved from this base

## Troubleshooting

### Common Issues

**Configuration file not found:**
```bash
Could not load configuration: 'tanka-docs.yml'
```
- Ensure the configuration file exists
- Use `tanka-docs init` to create default configuration files
- Check the file path if using `-f` option
- Verify you're in the correct directory

**Build failures:**
- Use `--debug` flag to get detailed error information
- Check that all referenced files and directories exist
- Verify the syntax of your configuration files

**Permission errors:**
- Ensure you have write permissions to the output directory
- Check that the build directory is writable

### Getting Help

```bash
# Show available commands and options
tanka-docs --help

# Show help for specific command
tanka-docs init --help
tanka-docs build --help
tanka-docs dev --help
```

## Environment Variables

Currently, Tanka Docs does not use environment variables for configuration. All settings must be specified through command-line options or configuration files.

## Performance Tips

- Use `--debug` sparingly in production builds as it generates more output
- Consider using a dedicated build directory (separate from output) for better performance
- Ensure your Git repository is clean and has proper tags/branches for version detection 