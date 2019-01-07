Usage
=============================================================

1. Install the global tool

```
dotnet tool install --global Fugu.GenerateDocs
```

2. Add fugu-docs.yaml configuration file to root of your repo

```yaml
input: 'docs'
output: 'docs-html'
solution: 'your-solution.sln'
```

3. Run the tool

```
dotnet docs-gen
```

4. Output will be generated to `docs-html`


## How to include code from solution

### Include class

[{Fugu.GenerateDocs.Program}]

### Include function

[{Fugu.GenerateDocs.Program.Main}]
