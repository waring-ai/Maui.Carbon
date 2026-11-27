using Microsoft.Maui.Graphics;

namespace Maui.Carbon;

/// <summary>What paints an illustration shape.</summary>
internal enum IllustrationPaintKind
{
    /// <summary>One artwork color (<see cref="IllustrationPalette"/>).</summary>
    Solid,
    /// <summary>A linear gradient between artwork colors.</summary>
    Linear,
    /// <summary>The drawing's theme fill (FullPageError: <c>background</c>).</summary>
    ThemeFill,
    /// <summary>The drawing's theme stroke (FullPageError: <c>border-inverse</c>).</summary>
    ThemeStroke,
}

/// <summary>Where the artwork sits in a box larger than its aspect ratio (SVG <c>preserveAspectRatio</c>, always <c>meet</c>).</summary>
internal enum IllustrationAlignment
{
    /// <summary><c>xMidYMid</c>: centered (the SVG default).</summary>
    Center,
    /// <summary><c>xMinYMax</c>: at the bottom start (the HTTP error illustrations).</summary>
    BottomStart,
}

/// <summary>A gradient stop: offset (0 to 1), artwork color and opacity.</summary>
internal readonly record struct IllustrationStop(float Offset, uint Rgb, float Alpha);

/// <summary>An illustration paint. Gradient points are in the artwork's viewBox.</summary>
internal sealed class IllustrationPaint
{
    Paint? _gradient;

    IllustrationPaint(IllustrationPaintKind kind, uint rgb = 0, float alpha = 1, float x1 = 0, float y1 = 0, float x2 = 0, float y2 = 0, IllustrationStop[]? stops = null)
    {
        Kind = kind;
        Rgb = rgb;
        Alpha = alpha;
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Stops = stops ?? [];
    }

    internal static readonly IllustrationPaint ThemeFill = new(IllustrationPaintKind.ThemeFill);
    internal static readonly IllustrationPaint ThemeStroke = new(IllustrationPaintKind.ThemeStroke);

    internal IllustrationPaintKind Kind { get; }
    internal uint Rgb { get; }
    internal float Alpha { get; }
    internal float X1 { get; }
    internal float Y1 { get; }
    internal float X2 { get; }
    internal float Y2 { get; }
    internal IllustrationStop[] Stops { get; }

    internal static IllustrationPaint Solid(uint rgb, float alpha = 1) => new(IllustrationPaintKind.Solid, rgb, alpha);

    internal static IllustrationPaint Linear(float x1, float y1, float x2, float y2, IllustrationStop[] stops) =>
        new(IllustrationPaintKind.Linear, x1: x1, y1: y1, x2: x2, y2: y2, stops: stops);

    /// <summary>The MAUI gradient for this paint, its points relative to an artwork of <paramref name="width"/> by <paramref name="height"/>.</summary>
    internal Paint Gradient(float width, float height) => _gradient ??= new LinearGradientPaint(
        Stops.Select(s => new PaintGradientStop(s.Offset, IllustrationPalette.ToColor(s.Rgb, s.Alpha))).ToArray(),
        new Point(X1 / width, Y1 / height), new Point(X2 / width, Y2 / height));
}

/// <summary>
/// One shape of an illustration: normalized path data (absolute <c>M</c>, <c>L</c>, <c>C</c> and <c>Z</c>), a fill, an
/// optional stroke (width in viewBox units, dash pattern in multiples of the width), its opacity and an optional clip.
/// </summary>
internal sealed record IllustrationShape(
    string Data,
    IllustrationPaint? Fill,
    IllustrationPaint? Stroke = null,
    float StrokeWidth = 1,
    float Opacity = 1,
    string? Clip = null,
    LineCap Cap = LineCap.Butt,
    LineJoin Join = LineJoin.Miter,
    float MiterLimit = 4,
    float[]? Dash = null);

/// <summary>A vector illustration: a viewBox size and shapes in paint order. Paths are parsed on first draw and kept.</summary>
internal sealed class IllustrationDrawing(string name, float width, float height, IllustrationShape[] shapes)
{
    PathF?[]? _paths;
    PathF?[]? _clips;

    internal string Name { get; } = name;
    internal float Width { get; } = width;
    internal float Height { get; } = height;
    internal IReadOnlyList<IllustrationShape> Shapes { get; } = shapes;

    internal PathF PathAt(int index)
    {
        _paths ??= new PathF?[Shapes.Count];
        return _paths[index] ??= IllustrationPath.Parse(Shapes[index].Data);
    }

    internal PathF? ClipAt(int index)
    {
        if (Shapes[index].Clip is not { } clip) return null;
        _clips ??= new PathF?[Shapes.Count];
        return _clips[index] ??= IllustrationPath.Parse(clip);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>Parses the normalized path data of the ported illustrations.</summary>
internal static class IllustrationPath
{
    /// <summary>
    /// Builds a path from absolute <c>M x y</c>, <c>L x y</c>, <c>C x1 y1 x2 y2 x y</c> and <c>Z</c> commands (numbers
    /// separated by spaces, commas or a sign). Throws <see cref="FormatException"/> on anything else.
    /// </summary>
    internal static PathF Parse(string data)
    {
        var path = new PathF();
        var i = 0;
        Span<float> n = stackalloc float[6];
        while (true)
        {
            SkipSeparators(data, ref i);
            if (i >= data.Length) break;
            var command = data[i++];
            switch (command)
            {
                case 'M':
                    Read(data, ref i, n[..2]);
                    path.MoveTo(n[0], n[1]);
                    break;
                case 'L':
                    Read(data, ref i, n[..2]);
                    path.LineTo(n[0], n[1]);
                    break;
                case 'C':
                    Read(data, ref i, n);
                    path.CurveTo(n[0], n[1], n[2], n[3], n[4], n[5]);
                    break;
                case 'Z':
                    path.Close();
                    break;
                default:
                    throw new FormatException($"Unexpected '{command}' at {i - 1} in illustration path data.");
            }
        }
        return path;
    }

    static void Read(string data, ref int i, Span<float> into)
    {
        for (var k = 0; k < into.Length; k++)
        {
            SkipSeparators(data, ref i);
            var start = i;
            if (i < data.Length && (data[i] == '-' || data[i] == '+')) i++;
            while (i < data.Length && (char.IsAsciiDigit(data[i]) || data[i] == '.' || data[i] == 'e' || (data[i] == '-' && data[i - 1] == 'e'))) i++;
            if (!float.TryParse(data.AsSpan(start, i - start), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out into[k]))
                throw new FormatException($"Expected a number at {start} in illustration path data.");
        }
    }

    static void SkipSeparators(string data, ref int i)
    {
        while (i < data.Length && (data[i] == ' ' || data[i] == ',')) i++;
    }
}

/// <summary>The scale and offset that fit an artwork in a box (SVG <c>meet</c>).</summary>
internal readonly record struct IllustrationFit(float Scale, float OffsetX, float OffsetY);

/// <summary>Pure illustration math (tested).</summary>
internal static class IllustrationModel
{
    /// <summary>
    /// Fits an artwork of <paramref name="artWidth"/> by <paramref name="artHeight"/> inside a box, keeping its aspect
    /// ratio, aligned as <paramref name="alignment"/> says. An empty box or artwork gives scale 0.
    /// </summary>
    internal static IllustrationFit Fit(float artWidth, float artHeight, float boxWidth, float boxHeight, IllustrationAlignment alignment)
    {
        if (artWidth <= 0 || artHeight <= 0 || boxWidth <= 0 || boxHeight <= 0) return new(0, 0, 0);
        var scale = Math.Min(boxWidth / artWidth, boxHeight / artHeight);
        var spareX = boxWidth - artWidth * scale;
        var spareY = boxHeight - artHeight * scale;
        return alignment == IllustrationAlignment.BottomStart
            ? new(scale, 0, spareY)
            : new(scale, spareX / 2, spareY / 2);
    }

    /// <summary>The height the artwork takes at <paramref name="width"/> (its aspect ratio).</summary>
    internal static double HeightForWidth(IllustrationDrawing art, double width) =>
        art.Width <= 0 ? 0 : width * art.Height / art.Width;

    /// <summary>
    /// Whether to draw the dark variant: <see cref="CdsIllustrationTheme.Dark"/>, or <see cref="CdsIllustrationTheme.Auto"/>
    /// in a dark theme zone (g90, g100). Carbon for IBM Products asks apps to pass <c>illustrationTheme</c> for the app theme;
    /// Auto does that for them.
    /// </summary>
    internal static bool UsesDark(CdsIllustrationTheme theme, CdsThemeName contextTheme) =>
        theme == CdsIllustrationTheme.Dark || (theme == CdsIllustrationTheme.Auto && contextTheme.IsDark());

    /// <summary>The drawing to show: the dark one when <see cref="UsesDark"/> and there is one, else the light one.</summary>
    internal static IllustrationDrawing? Pick(IllustrationDrawing? light, IllustrationDrawing? dark, CdsIllustrationTheme theme, CdsThemeName contextTheme) =>
        UsesDark(theme, contextTheme) && dark is not null ? dark : light;
}

/// <summary>Shape and paint helpers for the artwork files.</summary>
internal static partial class IllustrationArt
{
    static IllustrationPaint ThemeFill => IllustrationPaint.ThemeFill;

    static IllustrationPaint ThemeStroke => IllustrationPaint.ThemeStroke;

    static IllustrationPaint Solid(uint rgb, float alpha = 1) => IllustrationPaint.Solid(rgb, alpha);

    static IllustrationPaint Linear(float x1, float y1, float x2, float y2, IllustrationStop[] stops) => IllustrationPaint.Linear(x1, y1, x2, y2, stops);

    static IllustrationStop Stop(float offset, uint rgb, float alpha = 1) => new(offset, rgb, alpha);

    static IllustrationShape S(
        string data,
        IllustrationPaint? fill,
        IllustrationPaint? stroke = null,
        float strokeWidth = 1,
        float opacity = 1,
        string? clip = null,
        LineCap cap = LineCap.Butt,
        LineJoin join = LineJoin.Miter,
        float miterLimit = 4,
        float[]? dash = null) => new(data, fill, stroke, strokeWidth, opacity, clip, cap, join, miterLimit, dash);

    /// <summary>Every drawing, for tests.</summary>
    internal static IEnumerable<IllustrationDrawing> All =>
    [
        ErrorLight, ErrorDark, NoDataLight, NoDataDark, NoTagsLight, NoTagsDark, NotFoundLight, NotFoundDark,
        NotificationsLight, NotificationsDark, UnauthorizedLight, UnauthorizedDark,
        FullPageError403, FullPageError404, FullPageErrorCustom,
        HttpError403, HttpError404, HttpErrorOther,
    ];
}
