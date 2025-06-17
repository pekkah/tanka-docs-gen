# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Tanka Documentation Generator is a .NET 9.0 console application that generates static HTML documentation sites from Markdown files. It's distributed as a .NET Global Tool (`Tanka.DocsGen`) that can be installed via `dotnet tool install --global Tanka.DocsGen`. It provides advanced features like live C# code integration via Roslyn and multi-source content aggregation.

## Common Development Commands

### Building and Testing
```bash
# Build the project
dotnet build

# Full build with tests and packaging
./build.ps1

# Run all tests
dotnet test

# Run tests with coverage in artifacts directory
dotnet test --logger trx --results-directory ./artifacts
```

### Development Workflow (Dogfooding)
```bash
# Development mode with live reload for documentation changes
dotnet run --project ./src/DocsTool/ -- dev -f ./tanka-docs-wip.yml

# Build documentation site (prefer this when testing changes)
dotnet run --project ./src/DocsTool/ -- build -f ./tanka-docs-wip.yml
```

### Tool Installation and Testing
```bash
# Install from NuGet (primary method)
dotnet tool install --global Tanka.DocsGen

# Or build and pack the global tool locally for development
dotnet pack -c Release -o ./artifacts
dotnet tool install --global --add-source ./artifacts Tanka.DocsGen

# Test the installed tool
tanka-docs build -f ./tanka-docs.yml
```

## Architecture Overview

### Core Components

**Pipeline Architecture**: The system uses a modular pipeline approach for processing documentation, with configurable stages for content aggregation, Markdown processing, and static site generation.

**Multi-Layer File System**: 
- Physical file system (`/src/FileSystem/PhysicalFileSystem.cs`)
- Git repository file system (`/src/FileSystem/GitFileSystem.cs`) 
- In-memory file system for testing

**Content Processing Chain**:
1. **Content Aggregation** - Collect from multiple sources (local files, Git repos)
2. **Markdown Processing** - Enhanced with custom extensions via Markdig
3. **Code Integration** - Live C# code inclusion using Roslyn analysis
4. **Template Rendering** - Handlebars-based UI generation
5. **Static Site Output** - Final HTML generation

### Key Projects Structure

- **`/src/DocsTool/`** - Main CLI application with `build` and `dev` commands
- **`/src/FileSystem/`** - File system abstraction layer
- **`/tests/`** - xUnit tests with NSubstitute mocking

### Advanced Features

**Live Code Integration**: Uses `#include::xref://` syntax to include C# code snippets with Roslyn-powered symbol extraction from actual source files.

**Cross-Reference System**: Sophisticated linking with `xref://` syntax for navigation between documentation components.

**Multi-Source Content**: Supports aggregating content from multiple Git repositories and file system locations with version-aware processing.

## Configuration Files

- **`tanka-docs.yml`** - Production documentation configuration
- **`tanka-docs-wip.yml`** - Development/WIP documentation configuration  
- **`Directory.Build.props`** - Shared MSBuild properties across all projects
- **`GitVersion.yml`** - Semantic versioning configuration

## Development Server

The `dev` command provides live reload functionality:
- File watching with automatic rebuilds
- WebSocket-based browser communication (`/src/DocsTool/WebSocketService.cs`)
- Concurrent build protection via semaphore locks
- Serves documentation on local development server

## Testing Strategy

Uses xUnit with NSubstitute for mocking. Key test areas:
- Pipeline integration tests
- File system abstraction tests  
- Markdown processing tests
- Template rendering tests

Run specific test projects:
```bash
dotnet test tests/DocsTool.Tests/
dotnet test tests/FileSystem.Tests/
```