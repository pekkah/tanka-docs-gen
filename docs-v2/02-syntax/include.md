## Includes

Include syntax allows including contents of another file into the document. Content
is included before the Markdown parsing as an preprocessor step. This allows including
other Markdown files that will get parsed as they would've been part of the original
document.

Include syntax also allows including C# code snippets from C# files (\*.cs).

### Include files

```markdown
\#include::xref://src:DocsTool/Program.cs
```

```csharp
#include::xref://src:DocsTool/Program.cs
```

### Include csharp code snippets or files

To use the Roslyn extension to find the given symbol you must target a `.cs` file
with your include and provide a `s`(symbol) query string. Tanka Docs will load the
given file using Roslyn and look for the symbol using the fully qualified symbol name.
This allows including namespaces, classes, methods, properties.

> Wrap includes into fenced code blocks to enable syntax highlighting.

Include `ConfigurePreProcessors` -method from RoslynExtension class.

```markdown
\#include::xref://src:DocsTool/Extensions/Roslyn/RoslynExtension.cs?s=Tanka.DocsTool.Extensions.Roslyn.RoslynExtension.ConfigurePreProcessors
```

```csharp
#include::xref://src:DocsTool/Extensions/Roslyn/RoslynExtension.cs?s=Tanka.DocsTool.Extensions.Roslyn.RoslynExtension.ConfigurePreProcessors
```
