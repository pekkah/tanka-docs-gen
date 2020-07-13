## Tanka Documentation Generator

- [![master](https://dev.azure.com/tanka-ops/docs-gen/_apis/build/status/docs-gen?branchName=master)](https://dev.azure.com/tanka-ops/docs-gen/_build/latest?definitionId=2&branchName=master)

Latest

- [Documentation](https://pekkah.github.io/tanka-docs-gen)

## Install

1. Install the global tool

```bash
dotnet tool install --global Tanka.DocsGen
```

2. Add tanka-docs.yml configuration file to root of your repo
3. Add some sections with markdown docs and tanka-docs-section.yml
4. Run the tool in the directory with tanka-docs.yml

```bash
tanka-docs
```

4. Output will be generated to output (example: `gh-pages`)

## Serve

1. Install dotnet-serve https://github.com/natemcmaster/dotnet-serve
2. Run dotnet serve in the output folder

```bash
dotnet serve
```
