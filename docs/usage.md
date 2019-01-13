Usage
=============================================================

1. Install the global tool

```
dotnet tool install --global tanka.generate.docs
```

2. Add tanka-docs.yaml configuration file to root of your repo

```yaml
input: 'docs'
output: 'docs-html'
solution: 'your-solution.sln'
```

3. Run the tool

```
dotnet generate-docs
```

4. Output will be generated to output (example: `docs-html`)


## How to include code from solution

### Include class

[{tanka.generate.docs.Program}]

### Include function

[{tanka.generate.docs.Program.Main}]
