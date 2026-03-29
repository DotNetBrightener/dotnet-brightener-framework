# DotNetBrightener.UploadService.ImageOptimizer

Copyright © 2017 - 2026 Vampire Coder (formerly DotnetBrightener)

Image optimization and resizing functionality for DotNetBrightener Upload Service.

## 🔒 Migration Notice - Version 2026.0.1+

**Version 2026.0.1+** has migrated from **Magick.NET** to **SkiaSharp** due to security vulnerability **CVE-2025-65955**.

### Security Details

- **Vulnerability**: CVE-2025-65955 (CVSS 4.9/10 - Moderate)
- **Affected Package**: Magick.NET v14.9.1
- **Issue**: Use-after-free/double-free in ImageMagick's Magick++ layer
- **Resolution**: Migrated to SkiaSharp (Microsoft-backed, actively maintained)

---

## 🚀 Breaking Changes

### Old Method (Deprecated)
```csharp
services.AddUploadService(config)
    .UseImageMagickOptimizer(); // ⚠️ Deprecated - shows warning
```

### New Method (Recommended)
```csharp
services.AddUploadService(config)
    .UseSkiaSharpOptimizer(); // ✅ Use this instead
```

---

## ✨ Benefits of SkiaSharp

- ✅ **No security vulnerabilities** - Actively maintained by Microsoft
- ✅ **Better cross-platform support** - Windows, Linux, macOS, iOS, Android
- ✅ **High performance** - Native Skia graphics engine
- ✅ **Modern .NET API** - Designed for .NET Core and beyond
- ✅ **Comprehensive graphics features** - More than just image resizing

---

## 📦 Installation

```bash
dotnet add package DotNetBrightener.UploadService.ImageOptimizer
```

---

## 📖 Usage

### Basic Setup

```csharp
using Microsoft.Extensions.DependencyInjection;

// In your Startup.cs or Program.cs
services.AddUploadService(configuration)
    .UseSkiaSharpOptimizer();
```

### Advanced Configuration

```csharp
services.AddUploadService(configuration)
    .UseSkiaSharpOptimizer()
    .ConfigureUploadOptions(options =>
    {
        options.MaxFileSize = 10 * 1024 * 1024; // 10MB
        options.AllowedFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    });
```

---

## 🔄 Migration Guide

### Step 1: Update Package Reference

No changes needed if using centralized package management. The package will automatically use SkiaSharp.

### Step 2: Update Code

**Before:**
```csharp
services.AddUploadService(config)
    .UseImageMagickOptimizer();
```

**After:**
```csharp
services.AddUploadService(config)
    .UseSkiaSharpOptimizer();
```

### Step 3: Test Your Application

1. Build your application
2. Run unit tests
3. Test image upload and resize functionality
4. Verify image quality meets your requirements

### Backward Compatibility

The old `UseImageMagickOptimizer()` method still works but:
- Shows an `[Obsolete]` warning during compilation
- Internally redirects to `UseSkiaSharpOptimizer()`
- Will be removed in the next major version

---

## 📝 Supported Image Formats

SkiaSharp supports the following image formats:

- **PNG** (default output format)
- **JPEG** / **JPG**
- **GIF** (including animated)
- **WebP**
- **BMP**
- **ICO**
- **WBMP**

---

## 🛠️ API Reference

### SkiaSharpImageOptimizer

```csharp
public class SkiaSharpImageOptimizer : IImageResizer
{
    /// <summary>
    /// Resizes an image from the input stream to the specified dimensions
    /// </summary>
    /// <param name="inputStream">The input image stream</param>
    /// <param name="newWidth">The target width (absolute value will be used)</param>
    /// <param name="newHeight">The target height (absolute value will be used)</param>
    /// <returns>A new stream containing the resized image in PNG format</returns>
    Stream ResizeImageFromStream(Stream inputStream, int newWidth, int newHeight);
}
```

### Extension Methods

```csharp
// Recommended method
public static UploadServiceConfigurationBuilder UseSkiaSharpOptimizer(
    this UploadServiceConfigurationBuilder builder);

// Deprecated method (backward compatibility)
[Obsolete("Use UseSkiaSharpOptimizer instead")]
public static UploadServiceConfigurationBuilder UseImageMagickOptimizer(
    this UploadServiceConfigurationBuilder builder);
```

---

## 🧪 Testing

The package includes comprehensive unit tests:

- Valid image resizing
- Negative dimension handling (converts to absolute values)
- Invalid stream handling
- Various dimension scenarios
- Large image handling
- Output format verification (PNG)

To run tests:

```bash
cd src/UploadService/DotNetBrightener.UploadService.ImageOptimizer.Tests
dotnet test
```

---

## 📊 Performance

SkiaSharp provides excellent performance characteristics:

- **High-quality resizing** with `SKFilterQuality.High`
- **Native code execution** via Skia engine
- **Efficient memory usage** with proper disposal patterns
- **Cross-platform optimization** for each OS

---

## 🐛 Troubleshooting

### "Failed to decode image from input stream"

**Cause**: The input stream doesn't contain a valid image or format is not supported.

**Solution**: Verify the input stream contains valid image data and is in a supported format.

### "Failed to resize image"

**Cause**: Invalid target dimensions or corrupted source image.

**Solution**: Ensure width and height are positive integers and source image is valid.

---

## 🔗 Dependencies

- **SkiaSharp** (3.119.1 or later) - Main graphics library
- **Microsoft.AspNetCore.App** - Framework reference
- **DotNetBrightener.SimpleUploadService** - Upload service abstractions

---

## 📜 License

This package is part of the DotNetBrightener Framework.

© 2017 - 2025 Vampire Coder (formerly DotNet Brightener)

---

## 🆘 Support

For issues, questions, or contributions:

- **GitHub Repository**: https://github.com/dotnetbrightener/dotnet-brightener-framework
- **Issues**: https://github.com/dotnetbrightener/dotnet-brightener-framework/issues

---

## 📚 Additional Resources

- [SkiaSharp Documentation](https://github.com/mono/SkiaSharp)
- [DotNetBrightener Framework Documentation](https://github.com/dotnetbrightener/dotnet-brightener-framework)
- [CVE-2025-65955 Advisory](https://github.com/advisories/GHSA-q3hc-j9x5-mp9m)

