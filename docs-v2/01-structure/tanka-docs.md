## tanka-docs.yml

When `tanka-docs` command is executed if looks by convention for site configuration
file called `tanka-docs.yml` from the current directory.

Example

```yaml
base_path: "/"
title: "Site title"
index_page: xref://root@master:index.md
output_path: "gh-pages"
build_path: "_build"
branches:
  master:
    input_path:
      - ui-bundle
      - docs
      - src
tags:
  1.*:
    input_path:
      - ui-bundle
      - docs
      - src
```

| Property    | Description                            |
| :---------- | :------------------------------------- |
| base_path   | HTML meta base tag value               |
| title       | Title of the site                      |
| index_page  | Main index page of the site            |
| output_path | Generated output, HTML pages           |
| build_path  | Temporary working folder               |
| branches    | Git branches used to look for sections |
| tags        | Git tags used to look for sections     |
