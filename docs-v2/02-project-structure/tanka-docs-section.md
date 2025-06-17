---
title: tanka-docs-section.yml Configuration
---

## The `tanka-docs-section.yml` File

Documentation is structured into sections by adding a `tanka-docs-section.yml` file to a directory. Tanka Docs discovers these files by searching the `input_path` directories defined in your main `tanka-docs.yml`.

All files within the same directory as the `tanka-docs-section.yml` file (and its subdirectories) are considered part of that section. File paths within the section configuration are relative to the directory containing the `tanka-docs-section.yml` file.

### Configuration Fields

#### `id`

-   **Purpose:** A unique identifier for the section. This ID is used when creating cross-references (`xref`) to content within this section from other parts of your documentation.
-   **Type:** `String`
-   **Required:** Yes
-   **Example:** `getting-started`

#### `title`

-   **Purpose:** The display title for the section. This is often used in navigation menus and page headers.
-   **Type:** `String`
-   **Required:** Yes
-   **Example:** `Getting Started Guide`

#### `index_page`

-   **Purpose:** Specifies the main entry page for the section. This is typically the first page a user sees when they navigate to the section.
-   **Type:** `xref`
-   **Required:** No
-   **Default:** `xref://index.md`
-   **Example:** `xref://introduction.md`

#### `nav`

-   **Purpose:** A list of `xref` links to Markdown files that define the navigation structure for the section. These files contain lists of links that will be rendered as the section's navigation menu.
-   **Type:** `List of xrefs`
-   **Required:** No
-   **Example:**
    ```yaml
    nav:
      - xref://nav.md
      - xref://advanced-nav.md
    ```

#### `type`

-   **Purpose:** The type of the section, which can influence how it's processed.
-   **Type:** `String`
-   **Required:** No
-   **Default:** `doc`
-   **Example:** `doc`

#### `includes`

-   **Purpose:** A list of glob patterns to include files in the section.
-   **Type:** `List of Strings`
-   **Required:** No
-   **Example:**
    ```yaml
    includes:
      - "**/*.cs"
    ```

#### `extensions`

-   **Purpose:** Configures extensions specific to this section. The structure is dependent on the extension being used.
-   **Type:** `Object`
-   **Required:** No

### Example `tanka-docs-section.yml`

```yaml
id: examples
title: "Examples of Usage"
index_page: xref://basics.md
nav:
  - xref://nav.md
includes:
  - "**/*.puml"
```
