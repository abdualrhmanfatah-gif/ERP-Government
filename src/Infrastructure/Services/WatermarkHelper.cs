using QuestPDF.Infrastructure;
using SkiaSharp;

namespace ERP_Government.Infrastructure.Services;

public static class WatermarkHelper
{
    public static Image LoadWithTransparency(string filePath, float opacity)
    {
        using var original = SKImage.FromEncodedData(filePath);
        var info = new SKImageInfo(original.Width, original.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateBlendMode(
                SKColors.White.WithAlpha((byte)(opacity * 255)),
                SKBlendMode.DstIn)
        };

        canvas.DrawImage(original, new SKPoint(0, 0), paint);

        using var encoded = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);
        return Image.FromBinaryData(encoded.ToArray());
    }
}
