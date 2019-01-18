## Install

1. Install the global tool

```bash
dotnet tool install --global tanka.generate.docs
```

2. Add tanka-docs.yaml configuration file to root of your repo

```yaml
input: 'docs'
output: 'docs-html'
solution: 'your-solution.sln'
```

3. Run the tool

```bash
generate-docs
```

4. Output will be generated to output (example: `docs-html`)


## Serve

1. Install dotnet-serve https://github.com/natemcmaster/dotnet-serve
2. Run dotnet serve in the output folder

```bash
dotnet serve
```