## Extensions to Markdig

### C# Code inclusion support using Roslyn

#### Include class

```markdown
[{tanka.generate.docs.Program}]
```

[{tanka.generate.docs.Program}]

#### Include function

```markdown
[{tanka.generate.docs.Program.Main}]
```

[{tanka.generate.docs.Program.Main}]


### Include code snippet using DFM extension

```markdown
[!code-yaml[tanka-docs](tanka-docs.yaml)]
```

[!code-yaml[tanka-docs](tanka-docs.yaml)]

> More about DFM https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a