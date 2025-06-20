# Tanka Documentation Generator

[![master](https://dev.azure.com/tanka-ops/docs-gen/_apis/build/status/docs-gen?branchName=master)](https://dev.azure.com/tanka-ops/docs-gen/_build/latest?definitionId=2&branchName=master)

Tanka Docs is a powerful technical documentation generator designed for .NET projects, inspired by the Antora project. It transforms your Markdown files and code examples into beautiful, versioned documentation websites.

## 🚀 Key Features

- **Versioned Documentation**: Generate documentation from Git repositories with support for versioning using tags and branches
- **Modular Structure**: Organize documentation using sections for better maintainability  
- **Live Code Integration**: Include C# code snippets/files using `#include` syntax with Roslyn integration
- **Dynamic Content Support**: Include generated files with `type: files` sections for build artifacts and reports
- **Cross-References**: Link between documents using `xref://` syntax for maintainable internal links
- **Customizable UI**: Use Handlebars templates for flexible UI customization
- **Git Integration**: Built-in support for Git repositories and version management
- **Static Site Generation**: Generates static HTML sites that can be hosted anywhere

## 📦 Quick Start

### 1. Install

Install Tanka Docs as a .NET global tool:

```bash
# Install from NuGet (recommended)
dotnet tool install --global Tanka.DocsGen

# Or install from local source (for development)
git clone https://github.com/pekkah/tanka-docs-gen.git
cd tanka-docs-gen
dotnet pack -c Release -o ./artifacts
dotnet tool install --global --add-source ./artifacts Tanka.DocsGen
```

### 2. Initialize Your Project

Navigate to your project directory and initialize Tanka Docs:

```bash
# Navigate to your project (must be a Git repository)
cd my-project

# Initialize Tanka Docs project
tanka-docs init
```

This creates:
- `tanka-docs.yml` - Production configuration
- `tanka-docs-wip.yml` - Development configuration  
- `ui-bundle/` - Customizable UI templates

### 3. Create Documentation Content

Create your documentation directory and files:

```bash
mkdir docs
echo "# Welcome to My Documentation" > docs/index.md
```

### 4. Build

```bash
# If installed as global tool:
tanka-docs build

# Or run directly from source:
dotnet run --project ./src/DocsTool/ -- build
```

### 5. Serve Locally

```bash
dotnet tool install --global dotnet-serve
cd output
dotnet serve
```

## 📚 Documentation

Comprehensive documentation is available at: **[https://pekkah.github.io/tanka-docs-gen](https://pekkah.github.io/tanka-docs-gen)**

### Quick Links

- **[Getting Started Guide](https://pekkah.github.io/tanka-docs-gen/01-getting-started/)** - Step-by-step setup instructions
- **[CLI Reference](https://pekkah.github.io/tanka-docs-gen/04-advanced/cli-reference.html)** - Command line options and usage
- **[Include Directives](https://pekkah.github.io/tanka-docs-gen/03-writing-content/include.html)** - Embedding code and content
- **[Cross-References](https://pekkah.github.io/tanka-docs-gen/03-writing-content/xref.html)** - Internal linking system
- **[Troubleshooting](https://pekkah.github.io/tanka-docs-gen/04-advanced/troubleshooting.html)** - Common issues and solutions

## 🛠️ Command Line Usage

### Initialize New Project

```bash
# Initialize in current directory
tanka-docs init

# Initialize with custom project name
tanka-docs init --project-name "My Amazing Docs"

# Initialize with custom branch
tanka-docs init --branch main

# Only create configuration files (skip UI bundle)
tanka-docs init --config-only

# Only extract UI bundle (skip configuration)
tanka-docs init --ui-bundle-only

# Force overwrite existing files
tanka-docs init --force

# Quiet mode (skip guidance output)
tanka-docs init --quiet

# Skip WIP configuration
tanka-docs init --no-wip

# Initialize in specific directory
tanka-docs init --output-dir /path/to/project
```

### Build Documentation

```bash
# Basic build
tanka-docs build

# With custom configuration
tanka-docs build -f custom-config.yml

# With custom output directory  
tanka-docs build -o ./dist

# With debug output
tanka-docs build --debug

# For subdirectory deployment
tanka-docs build --base "/my-docs/"
```

### Development Mode

```bash
# Development server with live reload
tanka-docs dev

# Custom port and configuration
tanka-docs dev --port 8080 -f custom-config.yml
```

## 📁 Project Structure

```
my-project/
├── tanka-docs.yml              # Main configuration
├── docs/                       # Documentation section
│   ├── tanka-docs-section.yml  # Section configuration
│   ├── index.md               # Documentation files
│   └── getting-started.md
├── reports/                    # Generated content section
│   ├── tanka-docs-section.yml  # type: files for dynamic content
│   ├── benchmark.md           # Generated at build time
│   └── coverage-report.html   # Generated reports
├── _partials/                  # Shared content
│   ├── tanka-docs-section.yml
│   └── common-notice.md
└── src/                        # Source code (for includes)
    └── Program.cs
```

## ✨ Advanced Features

### Include Code Snippets

Include entire files or specific symbols:

```markdown
\```csharp
# Include entire file
\#include::xref://src:Program.cs

# Include specific method
\#include::xref://src:MyClass.cs?s=MyNamespace.MyClass.MyMethod

# Include from another section
\#include::xref://examples:sample.cs
\```
```

### Dynamic Content Sections

Include dynamically generated content that exists in your working directory but may not be committed to version control:

```yaml
# reports/tanka-docs-section.yml
id: performance-reports
type: files  # <-- Special section type for dynamic content
title: "Performance Reports"
includes:
  - "**/*.md"
  - "*.html"
  - "*.json"
```

Perfect for:
- Build artifacts and generated reports
- Benchmark results created during CI/CD
- API documentation generated from code analysis
- Coverage reports and metrics

### Cross-Reference Links

Create maintainable internal links:

```markdown
[Getting Started](xref://getting-started.md)
[API Reference](xref://api:overview.md)
[Version 1.0 Docs](xref://docs@1.0.0:index.md)
[Latest Benchmarks](xref://performance-reports:benchmark.md)
```

### Versioning with Git

In Tanka Docs, versioning is managed directly through branches and tags in your `tanka-docs.yml`.

```yaml
# tanka-docs.yml
title: "My Project"
output_path: "output"

# Build docs from the main branch
branches:
  HEAD:
    input_path:
      - docs

# Build docs from all v1.* tags
tags:
  'v1.*':
    input_path:
      - docs
```

## 🎨 Customization

### Custom UI Bundle

Create custom templates using Handlebars:

```html
<!-- ui-bundle/article.hbs -->
<!DOCTYPE html>
<html>
<head>
    <title>{{page.title}} | {{site.title}}</title>
</head>
<body>
    <header>{{site.title}}</header>
    <main>{{{content}}}</main>
</body>
</html>
```

### Custom Styling

```css
/* ui-bundle/css/custom.css */
:root {
  --primary-color: #007acc;
  --background-color: #f8f9fa;
}
```

## 🔧 Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `title` | Site title | - |
| `description` | Site description | - |
| `output_path` | Output directory | `gh-pages` |
| `build_path` | Build cache directory | System temp directory |
| `base_path` | Base URL path | `/` |
| `ui_bundle` | UI template bundle | `default` |

## 📋 Requirements

- **.NET 9+** - Required for running the tool
- **Git** - Optional, for version control and Git sources
- **Text Editor** - Any editor that supports Markdown

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: [https://pekkah.github.io/tanka-docs-gen](https://pekkah.github.io/tanka-docs-gen)
- **Issues**: [GitHub Issues](https://github.com/pekkah/tanka-docs-gen/issues)
- **Discussions**: [GitHub Discussions](https://github.com/pekkah/tanka-docs-gen/discussions)
