# Logo Test Page

This page tests cross-section xref image references by displaying the Tanka Docs Generator logo from the root section.

## Tanka Logo (Cross-Section Xref)

Here's the Tanka logo referenced from the root section using xref:// syntax:

![Tanka Docs Generator Logo](xref://root@HEAD:tanka-logo.svg)

This image should be:
1. Resolved from `xref://root@HEAD:tanka-logo.svg` to an actual file path
2. Copied to the output directory during build
3. Accessible when viewing the generated documentation

## Asset Processing Test

This tests the unified asset handling system that:
- Tracks cross-section asset references during page rendering
- Uses thread-safe collections for parallel processing  
- Copies referenced assets to the correct output locations
- Follows the same path pattern as HTML pages

The logo file is located in the `root` section, while this markdown file is in the `getting-started` section, making it a perfect test for cross-section xref asset handling.