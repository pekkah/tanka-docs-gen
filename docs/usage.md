Usage
=============================================================

1. Install the global tool

```
dotnet tool install --global Fugu.GenerateDocs
```

2. Add fugu-docs.yaml configuration file to root of your repo

```yaml
output: docs-html
```

3. Run the tool

```
dotnet docs-gen
```

4. Output will be generated to `docs-html`
