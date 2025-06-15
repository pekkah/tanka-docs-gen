## Includes

The `#include` syntax is a powerful preprocessor directive that allows you to embed content from other files directly into your documentation. This embedding happens before the Markdown is parsed, meaning that included Markdown content will be processed as if it were originally part of the parent document.

This feature is useful for reusing content, maintaining single sources of truth for code examples, or breaking down large documents into smaller, manageable parts.

### General Usage

The basic syntax for an include is:

```markdown
#include::xref://[sectionId@version:]path/to/your/file.ext
```

- The `xref` part works similarly to how cross-references for links work, allowing you to target files in the current section, different sections, or different versions of sections.
- The content of the specified file will be inserted in place of the `#include` directive.

**Important:** For the included content to be correctly syntax-highlighted or rendered (e.g., code examples), it's often best to place the `#include` directive within a fenced code block with the appropriate language identifier.

### Including Entire Files

You can include any type of text-based file. If you include another Markdown file (`.md`), its content will be processed as Markdown. If you include a code file (e.g., `.cs`, `.js`, `.yaml`), the raw content of that file will be inserted.

**Example: Including a Markdown file**

If you have a reusable notice in `_partials/important-notice.md`:
```markdown
<!-- _partials/important-notice.md -->
**Note:** This is an important notice that applies to multiple pages.
```

You can include it in another Markdown file like this:
```markdown
This is some introductory text.

#include::xref://_partials:important-notice.md

And here is some text after the included notice.
```
This will result in the content of `important-notice.md` being placed between the two paragraphs.

**Example: Including a Code File**

To include an entire C# file and have it displayed with C# syntax highlighting:

```markdown
```csharp
#include::xref://src:DocsTool/Program.cs
```
```
This will render the full content of `Program.cs` inside a C# code block.

### Including C# Code Snippets (Symbol Inclusion)

Tanka Docs offers a special integration with Roslyn (the .NET Compiler Platform) to include specific C# code elements (like namespaces, classes, methods, or properties) directly from your `.cs` files. This is particularly useful for keeping your documentation examples synchronized with your actual codebase.

To use this feature:
1. The target file in the `xref` **must be a C# file** (ending in `.cs`).
2. You must append the `s` (symbol) query parameter to the `xref`, specifying the fully qualified name of the C# symbol you want to include.

**Syntax:**
```markdown
#include::xref://path/to/your/file.cs?s=Your.Namespace.ClassName.MethodName
```
Or for a property:
```markdown
#include::xref://path/to/your/file.cs?s=Your.Namespace.ClassName.PropertyName
```
Or for an entire class:
```markdown
#include::xref://path/to/your/file.cs?s=Your.Namespace.ClassName
```

**Example: Including a Specific Method**

To include the `ConfigurePreProcessors` method from the `RoslynExtension.cs` file:

```markdown
```csharp
#include::xref://src:DocsTool/Extensions/Roslyn/RoslynExtension.cs?s=Tanka.DocsTool.Extensions.Roslyn.RoslynExtension.ConfigurePreProcessors
```
```
Tanka Docs will use Roslyn to find the `ConfigurePreProcessors` method within the specified file and insert its source code. This ensures your documentation always reflects the latest version of the code.

**What can be included using the `s` parameter?**
- Namespaces
- Classes (including structs, interfaces, enums)
- Methods
- Properties
- Fields
- Events

When including larger elements like classes or namespaces, their entire definition, including all members, will be inserted.
```
