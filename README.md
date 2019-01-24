## Tanka Documentation Generator

* [Release docs](https://pekkah.github.io/tanka-docs-gen/)
* [Beta docs](https://pekkah.github.io/tanka-docs-gen/beta/)


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
