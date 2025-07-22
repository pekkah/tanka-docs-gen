# Asset Handling

Tanka Docs Generator provides comprehensive asset handling capabilities, automatically processing and copying various types of files to the output directory during documentation generation.

## Asset Types

Assets are files that are not processed as markdown content but are copied to the output directory to be served alongside your documentation. Common asset types include:

### Images
- `.png`, `.jpg`, `.jpeg`, `.gif`, `.svg`, `.webp`, `.bmp`, `.ico`, `.tiff`

### Fonts
- `.woff`, `.woff2`, `.ttf`, `.eot`

### Documents
- `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.txt`

### Web Assets
- `.js`, `.css`

### Other
- `.zip`, `.tar`, `.gz`, `.json`, `.xml`, `.csv`

## Default Configuration

Tanka Docs Generator comes with sensible defaults for asset file extensions. These defaults cover the most common use cases without requiring any configuration.

## Per-Section Configuration

You can customize which file extensions are treated as assets on a per-section basis in your `tanka-docs-section.yml` file:

```yaml
id: my-section
title: "My Section"
index_page: xref://index.md
nav:
  - xref://nav.md

# Custom asset extensions for this section
asset_extensions:
  - ".png"
  - ".jpg" 
  - ".svg"
  - ".pdf"
  - ".custom"
```

When `asset_extensions` is specified, it **overrides** the default extensions for that section. If you want to extend the defaults rather than replace them, you should include the default extensions you want to keep.

## Asset Processing

### Three-Phase Processing

Asset processing happens in three phases during documentation generation:

1. **UI Bundle Assets**: Theme and template assets (CSS, JS, fonts) are processed first
2. **Section Assets**: Regular assets within each documentation section
3. **Cross-Section xref Assets**: Images and other assets referenced via `xref://` from other sections

### Cross-Section Asset Handling

When you reference an asset from another section using `xref://` syntax:

```markdown
![Logo](xref://branding:company-logo.svg)
```

The system:
1. **Tracks** the cross-section reference during page rendering
2. **Resolves** the file location from the target section
3. **Copies** the asset to the correct output location
4. **Updates** the URL to point to the copied asset

### Thread-Safe Operations

Asset processing is designed to handle parallel section processing safely:
- Uses `ConcurrentBag` for tracking cross-section asset references
- Uses `ConcurrentDictionary` for deduplication to prevent copying the same asset multiple times
- Atomic file operations ensure consistency during parallel builds

## Output Structure

Assets are copied to the output directory following the same path structure as your documentation:

```
output/
├── HEAD/
│   ├── section-a/
│   │   ├── page.html
│   │   └── diagram.png      # Section asset
│   ├── section-b/
│   │   ├── index.html
│   │   └── screenshot.jpg   # Section asset
│   └── shared-logo.svg      # Cross-section asset
└── index.html
```

## Performance Considerations

### Deduplication

The asset processor automatically deduplicates assets to prevent unnecessary copying:
- Same asset referenced multiple times: copied only once
- Cross-section assets that already exist in target section: skipped

### Parallel Processing

Asset copying happens in parallel where possible:
- Each section's assets are processed concurrently
- Cross-section assets are processed after all sections complete
- Thread-safe collections ensure data consistency

## Troubleshooting

### Missing Assets

If an asset is not appearing in your generated documentation:

1. **Check file extension**: Ensure the file extension is included in the asset extensions list
2. **Verify file location**: Confirm the asset exists in the expected section directory
3. **Review build output**: Look for asset processing messages in the build log
4. **Check xref syntax**: For cross-section assets, verify the `xref://` syntax is correct

### Build Performance

For large numbers of assets:

1. **Customize extensions**: Only include necessary file extensions in `asset_extensions`
2. **Optimize file sizes**: Compress images and other large assets
3. **Use appropriate formats**: Choose efficient formats (WebP for images, WOFF2 for fonts)

This comprehensive asset handling system ensures your documentation assets are properly managed and available in the generated output.