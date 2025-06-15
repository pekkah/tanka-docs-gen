---
title: Introduction
---

Tanka Docs is a technical documentation generator inspired by the Antora project.

Its main target is to provide easy way of writing versioned documentation for
.NET Core projects but it's not limited to just that.

Features

- Modular documentation structure using sections
- Generate documentation from Git repository with support for versioning using tags
  and branches
- Distributed as dotnet global tool
- Documents are written in Markdown syntax
- Supports including C# code snippets/files using `#include`-syntax (Uses Roslyn internally
  to find the requested symbol when a symbol is requested)
- Handlebars templates are used as ui-bundle to wrap the HTML generated from Markdown documents

## Quickstart

1. Install `dotnet tool install --global Tanka.DocsGen`
2. Add a `tanka-docs.yml` configuration file to the root of your repository. This file tells Tanka Docs where to find your documentation content and how to build your site. See [tanka-docs.yml Configuration](xref://structure:tanka-docs-yml.md) for detailed information and examples.
3. Write your documentation
4. Add `tanka-docs-section.yml` to root of your docs folder
5. Run "tanka-docs" command to generate the documentation
