# Changelog

## [2026.0.0] - 2025-12-06

### 🔒 Security

- **BREAKING**: Migrated from Magick.NET to SkiaSharp due to **CVE-2025-65955**
- Removed vulnerable `Magick.NET-Q16-AnyCPU` v14.9.1 dependency
- Removed vulnerable `Magick.NET.Core` v14.9.1 dependency
- Vulnerability details: Use-after-free/double-free in ImageMagick's Magick++ layer (CVSS 4.9/10)

### ✨ Added

- New `SkiaSharpImageOptimizer` class using SkiaSharp 3.119.1
- New `UseSkiaSharpOptimizer()` extension method for DI registration
- Comprehensive unit tests for SkiaSharpImageOptimizer
- Enhanced error messages with detailed exception information
- Support for all SkiaSharp-supported image formats (PNG, JPEG, GIF, WebP, BMP, ICO, WBMP)
- High-quality image resizing with `SKFilterQuality.High`

### 🔄 Changed

- Default image resizing now uses SkiaSharp instead of ImageMagick
- Image output format remains PNG with 100% quality
- Improved cross-platform compatibility (Windows, Linux, macOS, iOS, Android)
- Better performance with native Skia graphics engine
- Stream positioning now guaranteed to be at position 0 after resize
