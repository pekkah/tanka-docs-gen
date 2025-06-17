---
title: Customizing the Page Template (article.hbs)
---

## Customizing the Page Template (`article.hbs`)

The `article.hbs` file in the UI bundle is the main Handlebars template that defines the entire HTML structure for every generated documentation page. By modifying this file, you can control the overall layout, including the header, navigation, content area, styling, and footer.

### Template Structure

The default `article.hbs` is a complete HTML document that uses Bootstrap for layout. It is composed of several key parts:

#### 1. The `<head>` Section

The head contains metadata, stylesheets, and the base path for all relative links.

```handlebars
<head>
    <base href="{{Site.BasePath}}">
    <title>{{ Page.Title }} - {{ Site.Title }}</title>
    ...
    {{#xref "xref://ui-bundle:prism.css"}}<link rel="stylesheet" href="{{url}}" />{{/xref}}
    ...
</head>
```

- `{{Site.BasePath}}`: Sets the base URL for the site, which is important for correct routing when hosted in a subdirectory.
- `{{Page.Title}}` and `{{Site.Title}}`: Dynamically create the page title.
- The template also links to Bootstrap and Prism.js CSS for styling and code highlighting.

#### 2. Top Navigation Bar

The top navigation bar provides site-wide navigation and version selection.

```handlebars
<nav class="navbar ...">
    <a class="navbar-brand" href="#">{{Site.Title}}</a>
    ...
    <ul class="navbar-nav mr-auto">
        {{#sections Section.Version}}
            <li class="nav-item">
                <a class="nav-link" href="{{#xref IndexPage}}{{url}}{{/xref}}">{{Title}}</a>
            </li>
        {{/sections}}
    </ul>
    <ul class="navbar-nav ml-auto">
        ...
        <li class="nav-item dropdown ...">
            <a class="nav-link dropdown-toggle" ...>
                {{Section.Version}}
            </a>
            <div class="dropdown-menu" ...>
                {{#versions}}
                    <a class="dropdown-item" href="{{../Site.BasePath}}{{Version}}">{{Version}}</a>
                {{/versions}}
            </div>
        </li>
    </ul>
</nav>
```

- `{{#sections Section.Version}}`: Loops through all documentation sections available for the current version to build the main navigation links.
- `{{#versions}}`: Loops through all available documentation versions to build the version selector dropdown.

#### 3. Sidebar and Main Content

The body is split into a sidebar for section-specific navigation and a main area for the page content.

```handlebars
<div class="row flex-xl-nowrap">
    <nav class="col-12 col-md-3 col-xl-2 sidebar">
        ...
        {{>partials/Navigation.hbs}}
        ...
    </nav>
    <main role="main" class="col-12 col-md-8 col-xl-6 ... content">
        {{{PageHtml}}}                    
    </main>
</div>
```

- `{{>partials/Navigation.hbs}}`: This is a Handlebars partial include. It renders the navigation menu for the current section using the `Navigation.hbs` template.
- `{{{PageHtml}}}`: This is the most important placeholder. It's where the HTML content, which has been converted from your Markdown file, is injected. The triple curly braces `{{{...}}}` are used to ensure the HTML is not escaped.

### Customization Example

To customize the template, you can create your own `article.hbs` file in your UI bundle. For example, you could add a conditional header that only displays if the `Page.Title` is set in the front matter.

```handlebars
<main role="main" class="col-12 col-md-8 col-xl-6 ... content">
    {{#if Page.Title}}
        <h1>{{Page.Title}}</h1>
    {{/if}}

    {{{PageHtml}}}                    
</main>
```

This ensures that a top-level `<h1>` is only rendered when a title is provided, giving you more control over the page structure.
