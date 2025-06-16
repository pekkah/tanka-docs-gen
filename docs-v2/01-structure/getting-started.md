---
title: Getting Started Guide
---

# Getting Started with Tanka Docs

This guide will walk you through setting up your first Tanka Docs project from scratch. By the end, you'll have a working documentation site that you can build upon.

## Prerequisites

- **.NET 6+** installed on your system
- **Git**
- **Text editor** or IDE of your choice

## Step 1: Install Tanka Docs

Install Tanka Docs as a global .NET tool:

```bash
dotnet tool install --global Tanka.DocsGen
```

Verify the installation:

```bash
tanka-docs --help
```

You should see the help output showing available commands.

## Step 2: Create Your Project Structure

Create a new directory for your documentation project and initialize a Git repository:

```bash
mkdir my-docs-project
cd my-docs-project
git init -b main
```

Set up the basic structure:

```bash
# Create main documentation section
mkdir docs

# Create a partials section for shared content
mkdir _partials

# Create a source directory (optional, for code includes)
mkdir src
```

Your project structure should look like this:

```
my-docs-project/
├── .git/
├── docs/
├── _partials/
└── src/
```

## Step 3: Create the Main Configuration File

Create `tanka-docs.yml` in your project root:

```yaml
title: "My Documentation Site"
output_path: "gh-pages"
build_path: "_build"
base_path: "/"
index_page: xref://docs@HEAD:index.md

# Content sources
# In Tanka Docs, content is versioned and sourced directly from Git.
# The `branches` key tells the tool to look for documentation
# in the `docs`, `_partials`, and `src` directories in the `HEAD` revision.
branches:
  HEAD:
    input_path:
      - docs
      - _partials
      - src
```

## Step 4: Configure Your Documentation Section

Create `docs/tanka-docs-section.yml`:

```yaml
id: "docs"
title: "Documentation"
index_page: "xref://index.md"
nav:
  - xref://nav.md
includes:
  - "**/*.md"
```

## Step 5: Create Your Documentation Content

Create your main documentation files:

### `docs/nav.md`

Create a navigation file for the `docs` section.

```markdown
*   Getting Started
    *   [Introduction](xref://index.md)
    *   [Installation](xref://installation.md)
    *   [Configuration](xref://configuration.md)
*   Advanced Topics
    *   [Customization](xref://ui:customize/article.md)
```

### `docs/index.md`

```markdown
---
title: Welcome to My Documentation
---

# Welcome to My Documentation

This is the home page of your documentation site. Here you can provide an overview of your project, key features, and guide users to the most important sections.

## Quick Navigation

- [Installation Guide](xref://installation.md) - Get up and running quickly
- [Configuration](xref://configuration.md) - Learn how to configure the system
- [Advanced Topics](xref://ui:customize/article.md) - Deep dive into advanced features

## What's New

- Version 2.0 released with new features
- Updated installation process
- Improved configuration options

## Getting Help

If you need help, check out our [troubleshooting guide](xref://troubleshooting.md) or contact support.
```

### `docs/installation.md`

```markdown
---
title: Installation Guide
---

# Installation Guide

This guide covers the installation process for our software.

## System Requirements

- Operating System: Windows 10+, macOS 10.15+, or Linux
- Memory: 4GB RAM minimum, 8GB recommended
- Disk Space: 500MB available space

## Installation Steps

### Option 1: Package Manager (Recommended)

```bash
# Install using package manager
npm install -g my-package
```

### Option 2: Manual Installation

1.  Download the latest release from our [releases page](https://github.com/myuser/myproject/releases)
2.  Extract the archive to your desired location
3.  Add the installation directory to your PATH

## Verification

After installation, verify everything is working:

```bash
my-command --version
```

You should see the version number displayed.

## Next Steps

- [Configure your installation](xref://configuration.md)
- [Learn about advanced features](xref://ui:customize/article.md)
```

### `docs/configuration.md`

```markdown
---
title: Configuration Guide
---

# Configuration Guide

Learn how to configure the system to meet your needs.

## Configuration File

The main configuration is stored in `config.yml`:

```yaml
# Example configuration
app:
  name: "My Application"
  version: "1.0.0"

database:
  host: "localhost"
  port: 5432
  name: "myapp"

logging:
  level: "info"
  file: "app.log"
```

## Configuration Options

### Application Settings

| Setting     | Description         | Default    |
| ----------- | ------------------- | ---------- |
| `app.name`  | Application name    | "My App"   |
| `app.version` | Application version | "1.0.0"    |
| `app.debug` | Enable debug mode   | `false`    |

### Database Settings

Configure your database connection:

-   **host**: Database server hostname
-   **port**: Database server port
-   **name**: Database name
-   **username**: Database username (optional)
-   **password**: Database password (optional)

## Environment Variables

You can override configuration using environment variables:

```bash
export APP_NAME="My Custom App"
export DATABASE_HOST="production-db.example.com"
```

## Validation

Validate your configuration:

```bash
my-command config validate
```

```

### Create Advanced Section

Create the advanced directory and content:

```bash
mkdir docs/advanced
```

Create `docs/advanced/customization.md`:

```markdown
---
title: Customization Guide
---

# Customization Guide

Learn how to customize and extend the system.

## Custom Themes

You can create custom themes by modifying the CSS and templates.

### CSS Customization

Create a custom CSS file:

```css
/* custom.css */
:root {
  --primary-color: #007acc;
  --secondary-color: #ff6b35;
}

.header {
  background-color: var(--primary-color);
}
```

### Template Customization

Override default templates by creating files in the `templates/` directory:

```html
<!-- templates/page.html -->
<!DOCTYPE html>
<html>
<head>
    <title>{{title}}</title>
</head>
<body>
    <header>{{site.title}}</header>
    <main>{{content}}</main>
</body>
</html>
```

## Plugins

Extend functionality with plugins:

```javascript
// plugins/my-plugin.js
module.exports = {
  name: 'my-plugin',

  init: function(app) {
    // Plugin initialization
  }
};
```

## API Extensions

Create custom API endpoints:

```javascript
// api/custom-endpoint.js
app.get('/api/custom', (req, res) => {
  res.json({ message: 'Custom endpoint' });
});
```

## Step 6: Configure Partials Section

Create `_partials/tanka-docs-section.yml`:

```yaml
id: "partials"
title: "Shared Content"
type: "partials"
```

Create `_partials/common-notice.md`:

```markdown
> **Important:** This is a shared notice that can be included in multiple pages using the include syntax.
```

## Step 7: Commit and Build Your Documentation

Commit your changes to Git so Tanka Docs can source the content from `HEAD`.

```bash
git add .
git commit -m "Initial documentation setup"
```

Now build your documentation site:

```bash
tanka-docs build
```

If everything is configured correctly, you should see output similar to:

```
Tanka Docs
Initializing...
CurrentPath: /path/to/my-docs-project
ConfigFilePath: /path/to/my-docs-project/tanka-docs.yml
OutputPath: gh-pages
BuildPath: _build
```

## Step 8: View Your Documentation

Your generated documentation will be in the `gh-pages` directory. To view it locally:

### Option 1: Using dotnet-serve

```bash
# Install dotnet-serve if you haven't already
dotnet tool install --global dotnet-serve

# Serve your documentation
cd gh-pages
dotnet serve
```

### Option 2: Using Python

```bash
cd gh-pages
python -m http.server 8000
```

### Option 3: Using Node.js

```bash
cd gh-pages
npx http-server
```

Open your browser and navigate to `http://localhost:8080` (or the port shown in the output).

## Step 9: Add a `.gitignore`

Add a `.gitignore` file to keep your repository clean:

```
# Tanka Docs build artifacts
_build/
gh-pages/

# OS files
.DS_Store
Thumbs.db
```

## What's Next?

Now that you have a basic documentation site running, you can:

1.  **Add more content** - Create additional pages and sections
2.  **Use includes** - Include code snippets from your source files using `\#include::` syntax
3.  **Customize the UI** - Create a custom UI bundle for your site's appearance
4.  **Set up versioning** - Configure Git branches and tags for documentation versions
5.  **Deploy your site** - Set up continuous deployment to host your documentation

## Troubleshooting

### Common Issues

**Build fails with "Configuration not found":**
- Ensure `tanka-docs.yml` exists in your project root
- Check YAML syntax is valid

**Pages not appearing in navigation:**
- Verify file names in `tanka-docs-section.yml` match actual files
- Check that referenced files exist

**Includes not working:**
- Ensure `\#include::` syntax is correct
- Verify referenced files exist
- Use `--debug` flag to see detailed processing

### Getting Help

- Check the [File System Structure](xref://file-system.md) guide
- Review the [Configuration Reference](xref://tanka-docs-yml.md)
- Use `tanka-docs build --debug` for detailed error information

## Example Project

You can find a complete example project in the [Tanka Docs repository](https://github.com/pekkah/tanka-docs-gen) that demonstrates these concepts in action. 