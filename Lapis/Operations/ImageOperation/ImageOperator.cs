using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lapis.Operations.ImageOperation;

public static class FontFamilies
{
    public static readonly FontCollection Collection = new();

    public static readonly FontFamily Regular =
        Collection.Add(Path.Combine(Environment.CurrentDirectory, "resource/font.otf"));

    public static readonly FontFamily Light =
        Collection.Add(Path.Combine(Environment.CurrentDirectory, "resource/font-light.otf"));

    public static readonly FontFamily Heavy =
        Collection.Add(Path.Combine(Environment.CurrentDirectory, "resource/font-heavy.otf"));

    public static readonly FontFamily Emoji =
        Collection.Add(Path.Combine(Environment.CurrentDirectory, "resource/emoji.ttf"));
}

public class Image : IDisposable
{
    private readonly Image<Rgba32> _imageSharpImage;

    public Image(string path)
    {
        _imageSharpImage = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public Image(int width, int height)
    {
        _imageSharpImage = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public Image(int width, int height, Color color)
    {
        _imageSharpImage = new Image<Rgba32>(width, height, ColorToRgba32(color));
        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public Image(MemoryStream stream)
    {
        _imageSharpImage = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public void Dispose()
    {
        _imageSharpImage.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool isWhiteOnDark()
    {
        var color = GetDominantColor();
        if (color.R * 0.3f + color.G * 0.59f + color.B * 0.11f < 0.6)
            return true;
        return false;
    }

    public void DrawText(string text, Color color, float fontSize, FontWeight fontWeight, float left, float top)
    {
        DrawText(text, color, fontSize, fontWeight, HorizontalAlignment.Left, left, top);
    }

    public void DrawText(string text, Color color, float fontSize, FontWeight fontWeight,
        HorizontalAlignment horizontalAlignment,
        float left, float top)
    {
        var font = FontFamilies.Regular.CreateFont(fontSize);

        switch (fontWeight)
        {
            case FontWeight.Heavy:
                font = FontFamilies.Heavy.CreateFont(fontSize);
                break;
            case FontWeight.Light:
                font = FontFamilies.Light.CreateFont(fontSize);
                break;
        }

        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(left, top - 2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = (SixLabors.Fonts.HorizontalAlignment)horizontalAlignment,
            FallbackFontFamilies = [FontFamilies.Emoji]
        };

        _imageSharpImage.Mutate(x => x.DrawText(textOptions, text, ColorToRgba32(color)));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void GaussianBlur(float radius)
    {
        _imageSharpImage.Mutate(x => x.GaussianBlur(radius));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void Resize(int width, int height)
    {
        _imageSharpImage.Mutate(x => x.Resize(width, height));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void Rotate(float angle)
    {
        _imageSharpImage.Mutate(x => x.Rotate(angle));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void Crop(int width, int height)
    {
        _imageSharpImage.Mutate(x => x.Crop(width, height));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void Scale(int percentageWidth, int percentageHeight)
    {
        _imageSharpImage.Mutate(x =>
            x.Resize((int)(Width * percentageWidth / 100f), (int)(Height * percentageHeight / 100f)));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void DrawImage(Image image, int left, int top)
    {
        _imageSharpImage.Mutate(x => x.DrawImage(image._imageSharpImage, new Point(left, top), 1f));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void DrawImage(Image image, int left, int top, CompositeOperator compositeOperator)
    {
        _imageSharpImage.Mutate(x => x.DrawImage(image._imageSharpImage, new Point(left, top), new GraphicsOptions
            {
                AlphaCompositionMode = compositeOperator == CompositeOperator.DstOut
                    ? PixelAlphaCompositionMode.DestOut
                    : PixelAlphaCompositionMode.DestOver
            }
        ));

        Width = _imageSharpImage.Width;
        Height = _imageSharpImage.Height;
    }

    public void FuseAlpha(Image image)
    {
        var gradient = image._imageSharpImage;

        _imageSharpImage.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                for (var x = 0; x < accessor.Width; x++)
                    pixelRow[x] = new Rgba32(pixelRow[x].R / 255f, pixelRow[x].G / 255f,
                        pixelRow[x].B / 255f, pixelRow[x].A * gradient[x, y].A / 255f / 255f);
            }
        });
    }

    private Rgba32 ColorToRgba32(Color color)
    {
        return new Rgba32(color.R, color.G, color.B, color.A);
    }

    private Color Rgba32ToColor(Rgba32 rgba32)
    {
        return new Color(rgba32.R / 255f, rgba32.G / 255f, rgba32.B / 255f, rgba32.A / 255f);
    }

    public Color GetDominantColor()
    {
        var imageSharpImage = _imageSharpImage.Clone();
        imageSharpImage.Mutate(x => x.Resize(5, 5));

        var colors = new List<Rgba32>();

        for (var y = 0; y < imageSharpImage.Height; y++)
        for (var x = 0; x < imageSharpImage.Width; x++)
        {
            var color = imageSharpImage[x, y];

            var r = (int)color.R;
            var g = (int)color.G;
            var b = (int)color.B;

            colors.Add(new Rgba32((byte)r, (byte)g, (byte)b));
        }

        var fitColors = new List<List<Rgba32>>();

        var sortedColors = new List<Rgba32>();

        foreach (var color in colors)
        {
            if (sortedColors.Contains(color))
                continue;

            var colorList = new List<Rgba32> { color };

            foreach (var secondColor in colors)
            {
                if (color == secondColor || sortedColors.Contains(secondColor))
                    continue;
                if (Math.Abs(secondColor.R - color.R) < 10 && Math.Abs(secondColor.G - color.G) < 10 &&
                    Math.Abs(secondColor.B - color.B) < 10)
                {
                    sortedColors.Add(secondColor);
                    colorList.Add(secondColor);
                }
            }

            sortedColors.Add(color);
            fitColors.Add(colorList);
        }

        var sortedFitColors = fitColors.OrderByDescending(f => f.Count).ToArray();

        var pickedColor = sortedFitColors[0][0];

        imageSharpImage.Dispose();

        return Rgba32ToColor(pickedColor);
    }

    public string ToBase64(bool ToBeCompressed)
    {
        using (var ms = new MemoryStream())
        {
            if (ToBeCompressed)
                _imageSharpImage.Save(ms, new JpegEncoder { Quality = 90 });
            else
                _imageSharpImage.SaveAsPng(ms);

            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public string ToBase64()
    {
        return ToBase64(false);
    }
}

public enum HorizontalAlignment
{
    Left,
    Right,
    Center
}

public enum CompositeOperator
{
    DstOut
}

public enum FontWeight
{
    Regular,
    Light,
    Heavy
}

public class Color
{
    public static readonly Color White = new(1, 1, 1, 1);
    public static readonly Color Black = new(0, 0, 0, 1);
    public static readonly Color Red = new(1, 0, 0, 1);
    public static readonly Color Transparent = new(0, 0, 0, 0);
    public float A;
    public float B;
    public float G;
    public float R;

    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color()
    {
    }
}