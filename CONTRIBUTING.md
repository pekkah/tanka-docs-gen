# Contributing to Tanka Docs Gen

First off, thank you for considering contributing! This project is a tool for creating technical documentation, and your help is greatly appreciated.

This document serves as a guide for developers, including LLMs, who are working on this project.

## How to Contribute

The best way to contribute is by creating pull requests. Whether you're fixing a bug, adding a new feature, or improving documentation, we welcome your contributions.

- **For bugs**: Please provide a detailed description of the issue and steps to reproduce it.
- **For features**: Explain the feature and why it would be a valuable addition.

## Setting Up Your Environment

To build and run this project, you will need the [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

## Repository Structure

The repository is organized into the following key directories:

- **`/src/DocsTool/`**: The main command-line application source code.
- **`/src/FileSystem/`**: File system abstractions used by the tool.
- **`/tests/`**: Unit and integration tests for the projects.
- **`/docs-v2/`**: The source Markdown files for the project's own documentation.
- **`/ui-bundle/`**: Handlebars templates and static assets that define the UI of the generated site.
- **`tanka-docs.yml`**: The production configuration for building this project's own documentation.
- **`tanka-docs-wip.yml`**: The development/work-in-progress configuration for dogfooding.

## Building the Project

To compile the entire solution, run the following command from the repository root:

```bash
dotnet build
```

## Running Tests

To run the complete test suite, use the following command:

```bash
dotnet test
```

## Dogfooding (Building the Docs)

"Dogfooding" is the practice of using your own product. In this case, we use Tanka Docs Gen to build its own documentation.

### Development Mode

For live preview and hot-reloading while editing documentation, use the `dev` command. This is the recommended workflow for documentation changes.

```bash
dotnet run --project .\src\DocsTool\ -- dev -f .\tanka-docs-wip.yml
```

### Production Build

To perform a full, clean build of the documentation site, use the `build` command:

```bash
dotnet run --project .\src\DocsTool\ -- build -f .\tanka-docs-wip.yml
```

The output will be generated in the `/gh-pages/` directory, as configured in the YAML file.

## Coding Conventions

- **Follow existing style**: The project follows standard .NET coding conventions. Please maintain the existing style for consistency.
- **Async Suffix**: Asynchronous methods should be suffixed with `Async`.
- **Use abstractions**: Prefer using the abstractions provided in the `Tanka.FileSystem` library over direct file system access where possible.

## Submitting Changes

1. **Fork** the repository.
2. Create a new **branch** for your feature or bug fix.
3. Make your changes and **commit** them with a clear, descriptive message.
4. **Push** your changes to your fork.
5. Create a **Pull Request** to the `main` branch of the original repository.

Thank you for contributing! 