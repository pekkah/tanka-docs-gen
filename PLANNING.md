# Tanka Docs - Development Roadmap

This document outlines the planned features and improvements for Tanka Docs.

## Roadmap Status Legend
- 🟢 **Completed** - Feature is implemented and released
- 🟡 **In Progress** - Feature is currently being developed
- 🔵 **Planned** - Feature is planned for future development
- 🔴 **On Hold** - Feature is temporarily paused

---

## Features

### 🔵 Init Command - Project Initialization & UI Bundle Management
**Status:** Planned  
**Priority:** High  
**Target:** v0.10.0

#### Overview
Implement a comprehensive project initialization system with embedded UI bundle support. This will dramatically improve the first-time user experience and provide a foundation for UI customization.

#### UI Bundle Embedding & Init Command Plan

##### Architecture Overview

**Embedded Resource Strategy**
- **Bundle Format**: Zip file containing entire `ui-bundle/` directory
- **Embedding Location**: Embedded resource in `DocsTool.csproj`
- **Runtime Extraction**: On-demand extraction via `init` command
- **Fallback Usage**: Tool can use embedded bundle directly without extraction

##### Implementation Plan

**Phase 1: Bundle Embedding**

*1.1 Build Process Integration*
- **MSBuild Target**: Create custom target to zip `ui-bundle/` during build
- **Zip Creation**: 
  - Source: `./ui-bundle/**/*` 
  - Output: `ui-bundle-embedded.zip`
  - Include in project as embedded resource
- **Versioning**: Include bundle version metadata in zip

*1.2 Resource Management*
- **Resource Naming**: `Tanka.DocsTool.Resources.ui-bundle.zip`
- **Access Pattern**: Use `Assembly.GetManifestResourceStream()`
- **Memory Efficiency**: Stream-based extraction, no full memory load

**Phase 2: Init Command Implementation**

*2.1 Command Structure*
```bash
tanka-docs init [options]
```

*2.2 Command Options*
- `--force` / `-f`: Overwrite existing files
- `--ui-bundle-only`: Only extract UI bundle, skip config creation
- `--config-only`: Only create config files, skip UI bundle
- `--no-wip`: Skip creating `tanka-docs-wip.yml` (create only main config)
- `--branch <name>`: Specify default branch name (default: auto-detect from git)
- `--output-dir <path>`: Specify output directory (default: current directory)
- `--quiet` / `-q`: Skip post-creation configuration guidance (for automation)

*2.3 Command Behavior*

**Default Behavior (`tanka-docs init`)**:
1. Validate current directory is a Git repository (required)
2. Check if `tanka-docs.yml` exists
3. Check if `tanka-docs-wip.yml` exists  
4. Check if `ui-bundle/` directory exists
5. Create missing components with user confirmation

**Conflict Resolution**:
- Existing `tanka-docs.yml`: Prompt user or skip (unless `--force`)
- Existing `tanka-docs-wip.yml`: Prompt user or skip (unless `--force`)
- Existing `ui-bundle/`: Prompt user or skip (unless `--force`)
- Use interactive prompts for better UX

**Phase 3: Default Configuration Template**

*3.1 Embedded Configuration Templates*
- **Template Files**: Both `default-tanka-docs.yml` and `default-tanka-docs-wip.yml` as embedded resources
- **Placeholder Support**: Template variables for customization
- **Sensible Defaults**: Common project structure assumptions
- **Dual Configuration**: Production and development configurations

*3.2 Template Content Strategy*

**Main Configuration (`tanka-docs.yml`)**:
```yaml
# Template with placeholders
title: "{{PROJECT_NAME}} Documentation"
output_path: "output"
build_path: "_build"
index_page: xref://docs@{{DEFAULT_BRANCH}}:index.md

ui_bundle: "./ui-bundle"  # Point to extracted bundle

branches:
  {{DEFAULT_BRANCH}}:
    input_path:
      - docs
      - _partials
```

**Development Configuration (`tanka-docs-wip.yml`)**:
```yaml
# Template for development/WIP configuration
base_path: "/"
title: "{{PROJECT_NAME}} Documentation - WIP"
index_page: xref://docs@HEAD:index.md
output_path: "gh-pages"  # Common for GitHub Pages
build_path: "_build"

ui_bundle: "./ui-bundle"

branches:
  HEAD:
    input_path:
      - ui-bundle      # Include UI bundle for customization
      - docs
      - _partials
      - src            # Include source for code examples
```

*3.3 Template Variable Resolution*
- `{{PROJECT_NAME}}`: Derive from directory name or prompt user
- `{{DEFAULT_BRANCH}}`: Detect current git branch or prompt user
- `{{OUTPUT_PATH}}`: Default to "output" with option to customize  
- Interactive prompts for key values during init
- **Dual File Creation**: Both production and WIP configurations generated

**Phase 4: Integration with Build Process**

*4.1 Bundle Resolution Logic (Updated)*
```
Priority order:
1. Explicit --ui-bundle flag
2. ui_bundle setting in tanka-docs.yml  
3. Local ./ui-bundle/ directory
4. Embedded bundle (direct usage, no extraction)
```

*4.2 Embedded Bundle Usage*
- **Direct Usage**: Tool can render using embedded templates without extraction
- **Performance**: Stream templates directly from embedded zip
- **No File System Dependency**: Tool works without extracted bundle

**Phase 5: User Experience Design**

*5.1 Init Command Flow*

**First Time Setup**:
```bash
$ tanka-docs init
✓ Tanka Docs Project Initialization

Checking Git repository... ✓
Current directory: /path/to/my-project
Project name [my-project]: My Documentation Site
Default branch [main]: ⏎

The following will be created:
  • tanka-docs.yml (main configuration)
  • tanka-docs-wip.yml (development configuration)  
  • ui-bundle/ (default UI templates)

Continue? (y/N): y

✓ Created tanka-docs.yml
✓ Created tanka-docs-wip.yml
✓ Extracted UI bundle to ./ui-bundle/
✓ Project initialized successfully!

📝 Configuration Review:

The following files have been created:

┌─ tanka-docs.yml ──────────────────────────────────────┐
│ title: "My Documentation Site Documentation"          │
│ output_path: "output"                                 │
│ build_path: "_build"                                  │
│ index_page: xref://docs@main:index.md                 │
│                                                       │
│ ui_bundle: "./ui-bundle"                              │
│                                                       │
│ branches:                                             │
│   main:                                               │
│     input_path:                                       │
│       - docs        ← Your documentation files       │
│       - _partials   ← Shared content (optional)      │
└───────────────────────────────────────────────────────┘

┌─ tanka-docs-wip.yml ──────────────────────────────────┐
│ base_path: "/"                                        │
│ title: "My Documentation Site Documentation - WIP"    │
│ index_page: xref://docs@HEAD:index.md                 │
│ output_path: "gh-pages"                               │
│ build_path: "_build"                                  │
│                                                       │
│ ui_bundle: "./ui-bundle"                              │
│                                                       │
│ branches:                                             │
│   HEAD:                                               │
│     input_path:                                       │
│       - ui-bundle   ← UI customization               │
│       - docs        ← Your documentation files       │
│       - src         ← Source code for includes       │
└───────────────────────────────────────────────────────┘

🔧 Customize Your Configuration:
Edit these files to match your project structure:
• Update input_path directories to match your layout
• Add or remove paths as needed for your content
• The paths are relative to your project root

Next steps:
  1. Review and customize input_path settings above
  2. Create your documentation directories  
  3. Add your first documentation section
  4. Run 'tanka-docs build' to generate your site
  5. Use 'tanka-docs build -f tanka-docs-wip.yml' for development builds
```

**Existing Project**:
```bash
$ tanka-docs init
✓ Existing files detected

Found: tanka-docs.yml
Missing: tanka-docs-wip.yml, ui-bundle/

Create missing files? (y/N): y
✓ Created tanka-docs-wip.yml
✓ Extracted UI bundle to ./ui-bundle/
✓ Initialization complete!
```

*5.2 Error Handling & User Guidance*
- **Git Repository Validation**: Ensure current directory is a Git repository
- **Clear Error Messages**: Specific guidance for common issues
- **Permission Errors**: Suggest solutions for write permissions
- **Existing Files**: Clear options for handling conflicts
- **Rollback Support**: Ability to undo partial initialization

*5.3 Git Repository Error Example*
```bash
$ tanka-docs init
✗ Error: Current directory is not a Git repository

Tanka Docs requires a Git repository to function properly.

To initialize a Git repository:
  git init
  git add .
  git commit -m "Initial commit"

Then run 'tanka-docs init' again.
```

**Phase 6: Technical Implementation Details**

*6.1 Zip Extraction Strategy*
- **Selective Extraction**: Only extract changed files when using `--force`
- **Directory Structure**: Preserve exact directory structure from source
- **File Permissions**: Maintain appropriate file permissions on Unix systems
- **Atomic Operations**: Ensure clean state on interruption

*6.2 Configuration Template Processing*
- **Simple Substitution**: Use basic `{{VARIABLE}}` replacement for both config files
- **Validation**: Validate generated YAML syntax for both configurations
- **Backup Creation**: Create `.bak` files when overwriting existing configs
- **Dual File Generation**: Process both production and WIP templates simultaneously
- **Post-Creation Guidance**: Read and display actual created configuration files with input_path customization instructions

*6.3 Bundle Validation*
- **Structure Validation**: Ensure extracted bundle has required files
- **Template Validation**: Basic syntax check for Handlebars templates
- **Integrity Check**: Verify zip extraction completed successfully

**Phase 7: Testing Strategy**

*7.1 Unit Tests*
- Resource loading and extraction
- Template variable substitution
- Configuration file generation
- Error handling scenarios

*7.2 Integration Tests*
- Full init command workflow
- Bundle extraction verification
- Generated project building successfully
- Conflict resolution scenarios

*7.3 User Acceptance Testing*
- New project initialization flow
- Existing project enhancement
- Error recovery scenarios
- Cross-platform behavior

##### Migration & Compatibility

**Backward Compatibility**
- Existing projects continue working unchanged
- No breaking changes to existing commands
- Optional adoption of new `init` command

**Documentation Updates**
- Update getting started guide to use `init` command
- Provide migration guide for existing projects
- Document customization workflow

##### Future Considerations

**Bundle Updates**
- Future: `tanka-docs update-bundle` command
- Version tracking of embedded bundles
- Migration assistance for bundle updates

**Custom Bundle Support**
- Foundation for future custom bundle support
- Plugin architecture consideration
- Bundle marketplace preparation

---

## Future Feature Ideas

*This section is for collecting ideas that haven't been fully planned yet.*

- **Plugin System**: Extensible architecture for custom processors and renderers
- **Theme Marketplace**: Official repository of community themes
- **Live Editing**: In-browser editing capabilities during dev mode
- **Performance Optimizations**: Incremental builds and caching improvements
- **Multi-language Support**: Internationalization features
- **Advanced Git Integration**: Better branch/tag handling and conflict resolution 