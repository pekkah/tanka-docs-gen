## tanka-docs-section.yml

Documentation is structured into sections by putting a `tanka-docs-section.ym`
file into a directory. Tanka Docs will look for these files from the input paths
specified in the branches and tags sections of the `tanka-docs.yml` configuration file.

All files found from the folder with the section definition file and under it are considered
owned by the section. All file paths used during the processing are relative to the root path of the
section (The directory where the `tanka-docs-section.yml` is located).

Example

```yaml
id: examples
title: "Examples of usage"
index_page: xref://examples:basics.md
nav:
  - xref://nav.md
```

| Property   | Description                                                       |
| :--------- | :---------------------------------------------------------------- |
| id         | Id of the section. Uses as section id when using cross-references |
| title      | Title of the section                                              |
| index_page | Root index page of te section                                     |
| nav        | List of markdown files with navigation for this section           |
