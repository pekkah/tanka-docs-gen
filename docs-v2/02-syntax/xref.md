## Cross-references

Cross-references (`xref`) are a special syntax used to create links between documents within your documentation site. Using `xref` links allows you to reference other documents without using relative file paths. This makes it easier to reorganize your documentation structure without breaking links, as Tanka Docs will validate these references during generation.

There are three ways to use `xref` links:

### 1. Reference a document in the current section

This is the simplest form of `xref`. It's used when you want to link to another document that resides within the same documentation section.

**Syntax:**
```markdown
[Link Text](xref://document-name.md)
```

**Example:**
If you are in `section-a/page-one.md` and want to link to `section-a/page-two.md`, you would use:
```markdown
[Go to Page Two](xref://page-two.md)
```

### 2. Reference a document in a different section

When you need to link to a document in a different section, you must prefix the document name with the ID of the target section.

**Syntax:**
```markdown
[Link Text](xref://sectionId:document-name.md)
```

**Example:**
If you are in `section-a/page-one.md` and want to link to `section-b/introduction.md` (assuming `section-b` has an ID of `b-docs`), you would use:
```markdown
[Read Section B Intro](xref://b-docs:introduction.md)
```

### 3. Reference a document in a different section and version

For projects with multiple versions, you can link to a document in a specific version of a section. This is done by adding the version number after the section ID, separated by an `@` symbol.

**Syntax:**
```markdown
[Link Text](xref://sectionId@version:document-name.md)
```

**Example:**
If you are in `section-a/page-one.md` (version `2.0.0`) and want to link to `section-b/release-notes.md` from version `1.0.0` (assuming `section-b` has an ID of `b-docs`), you would use:
```markdown
[View v1.0.0 Release Notes](xref://b-docs@1.0.0:release-notes.md)
```

This comprehensive format ensures that all variations are documented with clear explanations and examples.
