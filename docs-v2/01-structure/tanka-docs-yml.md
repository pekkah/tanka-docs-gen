---
title: tanka-docs.yml Configuration
---

## The `tanka-docs.yml` File

The `tanka-docs.yml` file is the main configuration file for your Tanka Docs site. It resides at the root of your Git repository and tells Tanka Docs how to find your documentation sources, which versions to build, how to theme your site, and where to output the generated HTML.

This file uses YAML syntax.

### Overall Structure

The `tanka-docs.yml` file is a YAML mapping with several top-level keys that control different aspects of the documentation generation process.

### Top-Level Configuration Fields

Here are some of the common configuration fields you can use:

#### `site_title`

-   **Purpose:** Defines the overall title for your documentation site. This title is often used in page headers, browser tabs, and metadata.
-   **Type:** `String`
-   **Required:** Yes
-   **Example:** `My Project Documentation`

#### `output_dir`

-   **Purpose:** Specifies the directory where Tanka Docs will place the generated HTML site.
-   **Type:** `String`
-   **Required:** No (Tanka Docs might have a default like `_site` or `public`)
-   **Example:** `dist/docs`

#### `sources`

-   **Purpose:** A crucial section that defines where Tanka Docs should find your documentation content. It allows specifying different Git branches, tags, and explicit paths.
-   **Type:** `Object`
-   **Required:** Yes

This object can contain the following nested fields:

##### `sources.branches`

-   **Purpose:** An object where each key is a Git branch name (e.g., `main`, `develop`). The value for each branch specifies the input paths for documentation within that branch. Tanka Docs will check out these branches to find documentation sections.
-   **Type:** `Object`
-   **Required:** No (but either `branches` or `tags` or both are usually needed)
-   **Structure:**
    ```yaml
    branches:
      main: # Branch name
        input_path: # Paths within this branch
          - docs/
          - modules/module-a/docs/
      develop:
        input_path:
          - docs/
    ```
    -   `input_path`: (List of Strings) Specifies an array of directory paths within the branch where `tanka-docs-section.yml` files can be found.

##### `sources.tags`

-   **Purpose:** An object where each key is a Git tag pattern (e.g., `v1.*`, `release-*`). The value for each tag pattern specifies the input paths for documentation within tags matching that pattern.
-   **Type:** `Object`
-   **Required:** No
-   **Structure:**
    ```yaml
    tags:
      'v2.*': # Tag pattern (quotes needed if it starts with special chars)
        input_path:
          - documentation/
      'v1.0.0': # Specific tag
        input_path:
          - docs/
    ```
    -   `input_path`: (List of Strings) Specifies an array of directory paths within the tags where `tanka-docs-section.yml` files can be found.

##### `sources.include`

-   **Purpose:** A list of glob patterns or direct paths to explicitly include documentation files or directories. This can be useful if your documentation isn't strictly organized under `input_path` directories defined in `branches` or `tags`, or if you want to pull in content from outside those specific versioned paths (though this is less common for versioned content).
-   **Type:** `List of Strings`
-   **Required:** No
-   **Example:**
    ```yaml
    include:
      - "shared_content/**/*.md"
      - "legal/LICENSE.md"
    ```

##### `sources.exclude`

-   **Purpose:** A list of glob patterns or direct paths to exclude from processing, even if they are matched by other rules. Useful for ignoring drafts, temporary files, or specific subdirectories.
-   **Type:** `List of Strings`
-   **Required:** No
-   **Example:**
    ```yaml
    exclude:
      - "**/_drafts/*"
      - "**/obsolete_docs/"
      - "docs/README.md" # If you only want it on GitHub, not in the site
    ```

#### `ui`

-   **Purpose:** Configures the User Interface (UI) bundle for your documentation site.
-   **Type:** `Object`
-   **Required:** No (Tanka Docs will likely use a default UI bundle if this is omitted)

##### `ui.bundle_path`

-   **Purpose:** Specifies the path to your custom UI bundle directory. This directory should contain Handlebars templates, CSS, JavaScript, and other assets.
-   **Type:** `String`
-   **Required:** No
-   **Example:** `my-custom-ui-bundle/`

##### `ui.default_theme`

-   **Purpose:** If your UI bundle supports multiple themes (e.g., light, dark), this field could specify which theme to use by default. (This is a plausible field, actual support depends on Tanka Docs implementation).
-   **Type:** `String`
-   **Required:** No
-   **Example:** `dark`

#### `extensions`

-   **Purpose:** Configures any extensions or plugins that Tanka Docs might support. The structure under this key would be specific to each extension.
-   **Type:** `Object`
-   **Required:** No
-   **Example:**
    ```yaml
    extensions:
      my_custom_search:
        api_key: "YOUR_API_KEY"
      mermaid_diagrams:
        version: "8.10.2"
    ```

### Example `tanka-docs.yml`

```yaml
site_title: "My Awesome Project Docs"
output_dir: "gh-pages" # Output generated site to the 'gh-pages' directory

sources:
  branches:
    main:
      input_path:
        - docs/ # Main documentation for the current stable release
        - ui-bundle/ # Include UI bundle specific to main branch if needed
    develop:
      input_path:
        - docs/ # Documentation for the upcoming release
        - ui-bundle/develop-ui/ # Potentially a different UI for develop
  tags:
    'v[0-9].*': # Process all tags starting with 'v' followed by a digit (e.g., v1.0, v2.1.3)
      input_path:
        - docs/
    'archive/v0.*': # Example: Older archived versions from a specific path pattern
      input_path:
        - old_docs/
  include:
    - "shared/snippets/**/*.cs" # Include shared code snippets
  exclude:
    - "**/README.md" # Exclude all README.md files from being processed as pages
    - "docs/drafts/" # Exclude draft documents

ui:
  bundle_path: "custom-ui/" # Path to the custom UI bundle
  default_theme: "light"    # Specify a default theme if supported

extensions:
  # Example: Configuration for a hypothetical syntax highlighting extension
  code_highlighting:
    theme: "dracula"
    show_line_numbers: true
  # Example: Configuration for a search plugin
  search:
    provider: "lunr" # Use Lunr.js for client-side search
```

This example showcases how to define the site title, output directory, various sources including branches and tags with specific input paths, global includes/excludes, a custom UI bundle, and placeholder extension configurations. Remember that the exact fields and their behavior might vary based on the specific version and capabilities of Tanka Docs.
```
