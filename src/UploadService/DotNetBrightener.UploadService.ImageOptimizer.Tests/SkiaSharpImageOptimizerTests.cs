using DotNetBrightener.UploadService.ImageOptimizer;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace DotNetBrightener.UploadService.ImageOptimizer.Tests;

/// <summary>
///     Unit tests for SkiaSharpImageOptimizer
/// </summary>
public class SkiaSharpImageOptimizerTests
{
	private readonly SkiaSharpImageOptimizer _optimizer;

	public SkiaSharpImageOptimizerTests()
	{
		_optimizer = new SkiaSharpImageOptimizer();
	}

	[Fact]
	public void ResizeImageFromStream_ValidImage_ShouldReturnResizedStream()
	{
		// Arrange
		using var inputStream = CreateTestImageStream(800, 600);
		var targetWidth = 400;
		var targetHeight = 300;

		// Act
		using var result = _optimizer.ResizeImageFromStream(inputStream, targetWidth, targetHeight);

		// Assert
		result.ShouldNotBeNull();
		result.Length.ShouldBeGreaterThan(0);
		result.Position.ShouldBe(0); // Stream should be at beginning

		// Verify the image can be decoded and has correct dimensions
		using var resultData = SKData.Create(result);
		using var resultBitmap = SKBitmap.Decode(resultData);
		resultBitmap.ShouldNotBeNull();
		resultBitmap.Width.ShouldBe(targetWidth);
		resultBitmap.Height.ShouldBe(targetHeight);
	}

	[Fact]
	public void ResizeImageFromStream_NegativeDimensions_ShouldUseAbsoluteValues()
	{
		// Arrange
		using var inputStream = CreateTestImageStream(800, 600);
		var targetWidth = -400;
		var targetHeight = -300;

		// Act
		using var result = _optimizer.ResizeImageFromStream(inputStream, targetWidth, targetHeight);

		// Assert
		result.ShouldNotBeNull();
		result.Length.ShouldBeGreaterThan(0);

		// Verify absolute values were used
		using var resultData = SKData.Create(result);
		using var resultBitmap = SKBitmap.Decode(resultData);
		resultBitmap.Width.ShouldBe(Math.Abs(targetWidth));
		resultBitmap.Height.ShouldBe(Math.Abs(targetHeight));
	}

	[Fact]
	public void ResizeImageFromStream_InvalidStream_ShouldThrowException()
	{
		// Arrange
		using var invalidStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });

		// Act & Assert
		Should.Throw<InvalidOperationException>(() =>
			_optimizer.ResizeImageFromStream(invalidStream, 100, 100));
	}

	[Fact]
	public void ResizeImageFromStream_EmptyStream_ShouldThrowException()
	{
		// Arrange
		using var emptyStream = new MemoryStream();

		// Act & Assert
		Should.Throw<InvalidOperationException>(() =>
			_optimizer.ResizeImageFromStream(emptyStream, 100, 100));
	}

	[Theory]
	[InlineData(100, 100)]
	[InlineData(1920, 1080)]
	[InlineData(50, 50)]
	[InlineData(3840, 2160)]
	public void ResizeImageFromStream_VariousDimensions_ShouldProduceCorrectSize(int width, int height)
	{
		// Arrange
		using var inputStream = CreateTestImageStream(800, 600);

		// Act
		using var result = _optimizer.ResizeImageFromStream(inputStream, width, height);

		// Assert
		result.ShouldNotBeNull();
		using var resultData = SKData.Create(result);
		using var resultBitmap = SKBitmap.Decode(resultData);
		resultBitmap.Width.ShouldBe(width);
		resultBitmap.Height.ShouldBe(height);
	}

	[Fact]
	public void ResizeImageFromStream_LargeImage_ShouldHandleCorrectly()
	{
		// Arrange
		using var inputStream = CreateTestImageStream(4000, 3000);
		var targetWidth = 800;
		var targetHeight = 600;

		// Act
		using var result = _optimizer.ResizeImageFromStream(inputStream, targetWidth, targetHeight);

		// Assert
		result.ShouldNotBeNull();
		result.Length.ShouldBeGreaterThan(0);
		using var resultData = SKData.Create(result);
		using var resultBitmap = SKBitmap.Decode(resultData);
		resultBitmap.Width.ShouldBe(targetWidth);
		resultBitmap.Height.ShouldBe(targetHeight);
	}

	[Fact]
	public void ResizeImageFromStream_OutputFormatIsPng()
	{
		// Arrange
		using var inputStream = CreateTestImageStream(800, 600);

		// Act
		using var result = _optimizer.ResizeImageFromStream(inputStream, 400, 300);

		// Assert
		result.Position = 0;
		var buffer = new byte[8];
		result.Read(buffer, 0, 8);

		// PNG signature: 137 80 78 71 13 10 26 10
		buffer[0].ShouldBe((byte)137);
		buffer[1].ShouldBe((byte)80);
		buffer[2].ShouldBe((byte)78);
		buffer[3].ShouldBe((byte)71);
	}

	/// <summary>
	///     Creates a test image stream with specified dimensions
	/// </summary>
	private Stream CreateTestImageStream(int width, int height)
	{
		using var bitmap = new SKBitmap(width, height);
		using var canvas = new SKCanvas(bitmap);

		// Create a simple colored rectangle for testing
		canvas.Clear(SKColors.Blue);

		// Draw a red rectangle in the center
		var paint = new SKPaint
		{
			Color = SKColors.Red,
			Style = SKPaintStyle.Fill
		};
		canvas.DrawRect(width / 4, height / 4, width / 2, height / 2, paint);

		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);

		var stream = new MemoryStream();
		data.SaveTo(stream);
		stream.Position = 0;
		return stream;
	}
}
