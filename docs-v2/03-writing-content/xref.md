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

## Cross-reference Images

Cross-references also work with images! You can reference images stored in other sections using the same `xref://` syntax. This is particularly useful for sharing logos, diagrams, or other assets across multiple sections.

**Syntax:**
```markdown
![Alt Text](xref://image-file.png)
![Alt Text](xref://sectionId:image-file.png)
![Alt Text](xref://sectionId@version:image-file.png)
```

**Examples:**
```markdown
<!-- Reference image in current section -->
![Company Logo](xref://logo.svg)

<!-- Reference image from a different section -->
![Architecture Diagram](xref://architecture:system-overview.png)

<!-- Reference image from specific version -->
![Legacy UI](xref://ui-docs@1.0.0:old-interface.jpg)
```

### Asset Processing

When you reference an image using `xref://` syntax:

1. **Resolution**: The xref URL is resolved to the actual file path during build
2. **Copying**: The referenced image is automatically copied to the output directory
3. **Path Generation**: The image URL follows the same pattern as HTML pages for consistency
4. **Cross-Section Support**: Images from other sections are tracked and copied correctly

**Supported Image Formats:**
- `.png`, `.jpg`, `.jpeg`, `.gif`, `.svg`, `.webp`, `.bmp`, `.ico`, `.tiff`

This comprehensive format ensures that all variations are documented with clear explanations and examples.
