## Tanka Documentation Generator

Beta
* [Beta docs](https://pekkah.github.io/tanka-docs-gen/beta/)
* [![develop](https://dev.azure.com/tanka-ops/docs-gen/_apis/build/status/docs-gen?branchName=develop)](https://dev.azure.com/tanka-ops/docs-gen/_build/latest?definitionId=2&branchName=develop)

Release
* [Release docs](https://pekkah.github.io/tanka-docs-gen/)
* [![master](https://dev.azure.com/tanka-ops/docs-gen/_apis/build/status/docs-gen?branchName=master)](https://dev.azure.com/tanka-ops/docs-gen/_build/latest?definitionId=2&branchName=master)


## Install

1. Install the global tool

```bash
dotnet tool install --global tanka.generate.docs
```

2. Add tanka-docs.yaml configuration file to root of your repo

```yaml
input: 'docs'
output: 'gh-pages'
solution: 'tanka-docs-gen.sln'
basepath: '/'
```

3. Run the tool

```bash
generate-docs
```

4. Output will be generated to output (example: `gh-pages`)


## Serve

1. Install dotnet-serve https://github.com/natemcmaster/dotnet-serve
2. Run dotnet serve in the output folder

```bash
dotnet serve
```
