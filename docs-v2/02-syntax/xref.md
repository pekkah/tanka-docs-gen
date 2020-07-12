## Cross-references

To generate link to a document in another file you can use `xref` links. This avoids
using a fully path to the document providing an easier way of reorganizing your documentation
without breaking the links (or at least getting an error if you break one).

Reference document in current section

```markdown
[title](xref://link.md)
```

Reference document in another section

```markdown
[title](xref://sectionId:link.md)
```

Reference document in another section and version

```markdown
[title](xref://sectionId@1.0.0:link.md)
```
