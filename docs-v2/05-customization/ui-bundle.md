## Customizing the UI with UI Bundles and Handlebars

Tanka Docs uses a UI bundle system, leveraging Handlebars.js templates, to generate the final HTML pages for your documentation. This system allows for extensive customization of the look and feel of your documentation site.

### What is Handlebars.js?

Handlebars.js is a popular templating engine. It uses a template-based approach, where you create HTML structures with embedded Handlebars expressions. These expressions, enclosed in `{{ }}` (double curly braces), act as placeholders where data is inserted during the rendering process. For example, `{{page.title}}` might be replaced with the actual title of a documentation page. Handlebars also supports helpers for more complex logic like loops and conditional statements.

### What is a UI Bundle?

In Tanka Docs, a UI bundle is a collection of files that define the presentation layer of your documentation site. It typically includes:

-   **Handlebars Templates (`.hbs` files):** These define the structure of different parts of your site, such as the overall page layout, article rendering, navigation menus, headers, and footers.
-   **CSS Files:** For styling the HTML content.
-   **JavaScript Files:** For adding custom client-side behavior or interactions.
-   **Static Assets:** Such as images, fonts, or other resources.

### How Tanka Docs Uses UI Bundles

Tanka Docs processes your Markdown content and then uses the Handlebars templates from the UI bundle to render the final HTML. It passes data (like page content, site navigation, configuration options) to these templates, which Handlebars then uses to generate the complete web pages.

### Customizing the UI

While Tanka Docs likely provides a default UI bundle to get you started, you can customize nearly every aspect of your site's appearance.

**1. Overriding Default Templates:**

The most common way to customize is by overriding specific templates from the default UI bundle. Tanka Docs will typically have a defined directory structure for UI bundle templates (e.g., `templates/partials/`, `templates/layouts/`). To override a default template:

-   Create a file with the **same name and relative path** as the default template within your custom UI bundle directory.
-   Tanka Docs will then use your version of the template instead of the default one.

For example, to change how an article's content is displayed, you might override an `article.hbs` template.

**2. Adding Custom CSS:**

-   You can add your own CSS files to your UI bundle (e.g., in a `css/` subdirectory).
-   Link these CSS files in your layout templates (e.g., `main-layout.hbs` or `head.hbs`) to have them included in the generated HTML.
-   Alternatively, you might modify existing CSS files if the default bundle structure allows for it.

**3. Adding Custom JavaScript:**

-   Similarly, custom JavaScript files can be added to your UI bundle (e.g., in a `js/` subdirectory).
-   Include these scripts in your templates, typically at the end of the body or in the head, depending on the script's requirements.

**4. Configuring Your UI Bundle:**

The `tanka-docs.yml` site configuration file is likely where you would specify the path to your custom UI bundle. This tells Tanka Docs where to find your templates and assets.

```yaml
# Example snippet for tanka-docs.yml
# (Exact configuration key might vary)
ui:
  bundle_path: "path/to/your/custom-ui-bundle"
  # Or it might be implicitly looked for in a conventional location like 'ui-bundle'
```
If no custom UI bundle path is specified, Tanka Docs will use its default UI bundle.

### Simple Handlebars Example

Imagine a template for rendering a page title:

```handlebars
{{! In a template like article.hbs }}
<header>
  <h1>{{page.title}}</h1>
  {{#if page.version}}
    <span class="version-badge">Version: {{page.version}}</span>
  {{/if}}
</header>
```

In this example:
-   `{{page.title}}` is a Handlebars expression that will be replaced by the title of the current page.
-   `{{#if page.version}} ... {{/if}}` is a conditional helper. The content inside will only be rendered if `page.version` has a value.
-   `{{page.version}}` displays the page's version if it exists.

### Getting Started with Customization

1.  **Locate the Default UI Bundle:** If Tanka Docs provides one, exploring its structure is the best way to understand which templates and assets you can override. (The documentation for Tanka Docs should specify where this is or how to obtain it).
2.  **Identify What to Change:** Start with small changes, like modifying a color in CSS or adjusting the layout of a specific template.
3.  **Consult Handlebars.js Documentation:** For more complex template logic, refer to the official Handlebars.js documentation.
4.  **Iterate:** Build your documentation locally with Tanka Docs to see the effects of your changes as you make them.

By customizing the UI bundle, you can create a unique and branded experience for your Tanka Docs-generated documentation site.
```
