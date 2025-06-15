---
title: Markdown Syntax Guide
---

# Markdown Syntax Guide

Tanka Docs uses [Markdig](https://github.com/xoofx/markdig) as its Markdown processor, which provides standard Markdown syntax plus many useful extensions. This guide covers the syntax you can use in your documentation.

## Standard Markdown

### Headers

```markdown
# H1 Header
## H2 Header  
### H3 Header
#### H4 Header
##### H5 Header
###### H6 Header
```

### Text Formatting

```markdown
**Bold text**
*Italic text*
***Bold and italic***
~~Strikethrough~~
`Inline code`
```

**Examples:**
- **Bold text**
- *Italic text* 
- ***Bold and italic***
- ~~Strikethrough~~
- `Inline code`

### Lists

**Unordered Lists:**
```markdown
- Item 1
- Item 2
  - Nested item
  - Another nested item
- Item 3
```

**Ordered Lists:**
```markdown
1. First item
2. Second item
   1. Nested item
   2. Another nested item
3. Third item
```

### Links and Images

```markdown
[Link text](https://example.com)
[Link with title](https://example.com "Link title")
![Image alt text](path/to/image.png)
![Image with title](path/to/image.png "Image title")
```

### Code Blocks

**Fenced code blocks:**
````markdown
```javascript
function hello() {
    console.log("Hello, world!");
}
```
````

**With line numbers:**
````markdown
```csharp {.line-numbers}
public class Example
{
    public void Method() { }
}
```
````

## Tanka Docs Extensions

### Include Directives

Include content from other files using the `\#include::` syntax:

```markdown
\#include::xref://src:Program.cs
\#include::xref://src:Program.cs?s=MyNamespace.MyClass.MyMethod
\#include::xref://_partials:common-notice.md
```

See the [Include Directives](xref://include.md) guide for detailed information.

### Cross-References

Link to other pages using the `xref://` syntax:

```markdown
[Link to same section](xref://other-page.md)
[Link to different section](xref://section-id:page.md)
[Link to specific version](xref://section-id@1.0.0:page.md)
```

See the [Cross-References](xref://xref.md) guide for detailed information.

## Markdig Extensions

### Tables

```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Data 1   | Data 2   | Data 3   |
| Data 4   | Data 5   | Data 6   |
```

**With alignment:**
```markdown
| Left | Center | Right |
|:-----|:------:|------:|
| L1   | C1     | R1    |
| L2   | C2     | R2    |
```

### Task Lists

```markdown
- [x] Completed task
- [ ] Incomplete task
- [x] Another completed task
```

**Result:**
- [x] Completed task
- [ ] Incomplete task  
- [x] Another completed task

### Admonitions/Callouts

```markdown
!!! note "Optional Title"
    This is a note admonition.

!!! warning
    This is a warning without a custom title.

!!! tip "Pro Tip"
    This is a tip with a custom title.
```

**Available types:**
- `note` - General information
- `tip` - Helpful tips  
- `warning` - Important warnings
- `danger` - Critical warnings
- `info` - Informational content

### Definition Lists

```markdown
Term 1
:   Definition 1

Term 2  
:   Definition 2a
:   Definition 2b
```

### Abbreviations

```markdown
The HTML specification is maintained by the W3C.

*[HTML]: HyperText Markup Language
*[W3C]: World Wide Web Consortium
```

### Footnotes

```markdown
Here's a sentence with a footnote[^1].

[^1]: This is the footnote content.
```

### Math (LaTeX)

**Inline math:**
```markdown
The formula is $E = mc^2$.
```

**Block math:**
```markdown
$$
\int_{-\infty}^{\infty} e^{-x^2} dx = \sqrt{\pi}
$$
```

### Emojis

```markdown
:smile: :heart: :thumbsup: :warning:
```

You can use GitHub-style emoji codes in your text.

### Highlighting

```markdown
==highlighted text==
```

### Keyboard Keys

```markdown
Press ++ctrl+alt+del++ to restart.
```

### Smart Typography

Automatic conversions:
- `--` becomes –
- `---` becomes —
- `(c)` becomes ©
- `(tm)` becomes ™
- `"quotes"` become "smart quotes"

## Front Matter

Add YAML front matter to your pages for metadata:

```markdown
---
title: "Page Title"
description: "Page description for SEO"
author: "Author Name"
date: "2024-01-15"
tags: ["tag1", "tag2"]
---

# Your content starts here
```

## Code Syntax Highlighting

Tanka Docs supports syntax highlighting for many languages:

### C# Examples

````markdown
```csharp
public class Example
{
    public string Name { get; set; }
    
    public void DoSomething()
    {
        Console.WriteLine($"Hello, {Name}!");
    }
}
```
````

### JavaScript Examples

````markdown
```javascript
function fetchData(url) {
    return fetch(url)
        .then(response => response.json())
        .catch(error => console.error('Error:', error));
}
```
````

### YAML Examples

````markdown
```yaml
title: "My Site"
description: "A documentation site"
sources:
  - name: "local"
    type: "local"
    path: "."
```
````

### JSON Examples

````markdown
```json
{
  "name": "my-project",
  "version": "1.0.0",
  "dependencies": {
    "example": "^1.0.0"
  }
}
```
````

## Best Practices

### Document Structure

1. **Use clear headings** - Structure your content hierarchically
2. **Add table of contents** - For longer documents
3. **Use consistent formatting** - Be consistent with code blocks, links, etc.
4. **Include examples** - Show don't just tell

### Code Documentation

1. **Syntax highlighting** - Always specify language for code blocks
2. **Include context** - Explain what the code does
3. **Use includes** - Reference actual source files when possible
4. **Line numbers** - Use for longer code examples

### Links and References

1. **Use xref links** - For internal documentation links
2. **Descriptive link text** - Avoid "click here" or generic text
3. **Check broken links** - Validate links during build

### Accessibility

1. **Alt text for images** - Always provide meaningful alt text
2. **Clear link text** - Links should be descriptive
3. **Proper heading hierarchy** - Don't skip heading levels
4. **Table headers** - Use proper table markup

## Markdown Validation

To ensure your Markdown is valid:

1. **Preview locally** - Use your editor's preview feature
2. **Build and check** - Use `tanka-docs build` to catch errors
3. **Use debug mode** - Add `--debug` flag for detailed error information
4. **Validate syntax** - Use online Markdown validators

## Common Pitfalls

### Escaping Special Characters

When you need to show literal Markdown syntax:

```markdown
To show a literal asterisk: \*not italic\*
To show a literal backtick: \`not code\`
To show a literal hash: \#not a header
```

### Mixed Content Types

Be careful when mixing HTML with Markdown:

```markdown
<!-- This works -->
<div class="custom-class">
**This Markdown** will be processed.
</div>

<!-- This doesn't work as expected -->
<div>**This might not be processed**</div>
```

### Code Block Languages

Use correct language identifiers:

```markdown
<!-- Good -->
```csharp
// C# code
```

<!-- Bad -->
```c#
// Wrong identifier
```
```

## Editor Support

Most modern editors support Markdown with extensions:

- **VS Code**: Markdown Preview Enhanced, Markdig extensions
- **JetBrains IDEs**: Built-in Markdown support
- **Typora**: WYSIWYG Markdown editor
- **Mark Text**: Real-time preview editor

Configure your editor to use Markdig-compatible extensions for the best preview experience. 