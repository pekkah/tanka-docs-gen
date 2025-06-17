---
title: Command Line Interface Reference
---

# Command Line Interface Reference

Tanka Docs provides a command-line interface (CLI) that allows you to build and manage your documentation projects. The tool can be run directly from source or installed as a local .NET Global Tool.

## Installation

**Note:** Tanka.DocsGen is currently distributed as source code. To install:

```bash
# Clone the repository
git clone https://github.com/pekkah/tanka-docs-gen.git
cd tanka-docs-gen

# Build the tool
dotnet build

# Install as global tool (optional)
dotnet pack -c Release -o ./artifacts
dotnet tool install --global --add-source ./artifacts Tanka.DocsGen
```

To update to the latest version:

```bash
# Pull latest changes and rebuild
git pull
dotnet build

# Update global tool (if installed)
dotnet tool update --global --add-source ./artifacts Tanka.DocsGen
```

## Commands

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

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `-1` | General error (configuration not found, build failed, etc.) |
| `1` | Command-specific error |

## Common Usage Patterns

### Local Development

```bash
# Build and output to default location
tanka-docs build

# Build with debug output to see detailed logs
tanka-docs build --debug
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
tanka-docs build --help
```

## Environment Variables

Currently, Tanka Docs does not use environment variables for configuration. All settings must be specified through command-line options or configuration files.

## Performance Tips

- Use `--debug` sparingly in production builds as it generates more output
- Consider using a dedicated build directory (separate from output) for better performance
- Ensure your Git repository is clean and has proper tags/branches for version detection 