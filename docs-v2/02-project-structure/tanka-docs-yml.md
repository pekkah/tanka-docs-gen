---
title: tanka-docs.yml Configuration
---

## The `tanka-docs.yml` File

The `tanka-docs.yml` file is the main configuration file for your Tanka Docs site. It resides at the root of your Git repository and tells Tanka Docs how to find your documentation sources, which versions to build, and where to output the generated HTML.

This file uses YAML syntax.

### Top-Level Configuration Fields

Here are the supported top-level keys:

#### `title`

-   **Purpose:** Defines the overall title for your documentation site. This title is often used in page headers, browser tabs, and metadata.
-   **Type:** `String`
-   **Required:** No
-   **Default:** `Tanka Docs`
-   **Example:** `My Project Documentation`

#### `index_page`

-   **Purpose:** Defines the root index page of the whole site.
-   **Type:** `xref`
-   **Required:** No
-   **Default:** `xref://root@HEAD:index.md`
-   **Example:** `xref://root@master:index.md`

#### `output_path`

-   **Purpose:** Specifies the directory where Tanka Docs will place the generated HTML site.
-   **Type:** `String`
-   **Required:** No
-   **Default:** `output`
-   **Example:** `gh-pages`

#### `build_path`

-   **Purpose:** Specifies a temporary directory used for building the site.
-   **Type:** `String`
-   **Required:** No
-   **Default:** `Path.GetTempPath()`
-   **Example:** `_build`

#### `base_path`

-   **Purpose:** A base path to prepend to all generated links.
-   **Type:** `String`
-   **Required:** No
-   **Default:** ``
-   **Example:** `/`

#### `branches`

-   **Purpose:** An object where each key is a Git branch name (e.g., `master`, `develop`). The value for each branch specifies the input paths for documentation within that branch. Tanka Docs will check out these branches to find documentation sections.
-   **Type:** `Object`
-   **Required:** No
-   **Structure:**
    ```yaml
    branches:
      master: # Branch name
        input_path: # Paths within this branch
          - docs-v2
          - ui-bundle
      develop:
        input_path:
          - docs-v2
    ```
    -   `input_path`: (List of Strings) Specifies an array of directory paths within the branch where `tanka-docs-section.yml` files can be found.

#### `tags`

-   **Purpose:** An object where each key is a Git tag pattern (e.g., `v1.*`, `release-*`). The value for each tag pattern specifies the input paths for documentation within tags matching that pattern.
-   **Type:** `Object`
-   **Required:** No
-   **Structure:**
    ```yaml
    tags:
      '0.8.*': # Tag pattern (quotes needed if it starts with special chars)
        input_path:
          - docs-v2
      '1.*': # Specific tag
        input_path:
          - docs-v2
    ```
    -   `input_path`: (List of Strings) Specifies an array of directory paths within the tags where `tanka-docs-section.yml` files can be found.

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
base_path: "/"
title: "Tanka Docs Generator"
index_page: xref://root@master:index.md
output_path: "gh-pages"
build_path: "_build"
branches:
  master:
    input_path:
      - ui-bundle
      - docs-v2
      - src
tags:
  '1.*':
    input_path:
      - ui-bundle
      - docs-v2
      - src
```

This example showcases how to define the site title, output directory, various sources including branches and tags with specific input paths and placeholder extension configurations. Remember that the exact fields and their behavior might vary based on the specific version and capabilities of Tanka Docs.
```