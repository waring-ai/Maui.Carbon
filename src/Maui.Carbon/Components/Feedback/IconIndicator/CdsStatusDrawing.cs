using Microsoft.Maui.Graphics;

namespace Maui.Carbon;

/// <summary>The status glyph shapes drawn by <see cref="CdsIconIndicator"/> and <see cref="CdsShapeIndicator"/>.</summary>
internal enum CdsStatusShape
{
    // Icon indicator glyphs.
    CircleX,
    TriangleDownBang,
    TriangleUpBang,
    DiamondQuestion,
    CircleCheck,
    RingCheck,
    HalfFilledCircle,
    HalfCircleDashed,
    DashedCircle,
    CircleDots,
    CircleQuestion,
    SquareBang,

    // Shape indicator shapes.
    OctagonX,
    Hexagon,
    TriangleUp,
    Diamond,
    TriangleDown,
    Circle,
    Ring,
    HalfFilled,
}

/// <summary>
/// Draws the indicator glyphs on a 16-unit artboard. The marks inside (x, check, !, ?, dots) are cut out with the
/// even-odd rule, so they show the surface behind. The bundled icon set does not contain these glyphs.
/// </summary>
internal static class CdsStatusDrawing
{
    const float Center = 8f;

    public static void Draw(ICanvas canvas, CdsStatusShape shape, float size, Color fill, Color? outline)
    {
        if (size <= 0) return;
        canvas.SaveState();
        var scale = size / 16f;
        canvas.Scale(scale, scale);
        canvas.FillColor = fill;
        canvas.StrokeColor = outline ?? fill;
        canvas.StrokeSize = 1;
        canvas.StrokeLineCap = LineCap.Butt;
        canvas.StrokeLineJoin = LineJoin.Miter;

        switch (shape)
        {
            case CdsStatusShape.CircleX:
            {
                var p = Circle(7f);
                AddPolygon(p, Rotated(Plus(0.75f, 3.1f), 45));
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.OctagonX:
            {
                var p = Polygon(Regular(8, 7.7f, 22.5f));
                AddPolygon(p, Rotated(Plus(0.75f, 3.1f), 45));
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.Hexagon:
                canvas.FillPath(Polygon(Regular(6, 7.6f, 0)), WindingMode.NonZero);
                break;
            case CdsStatusShape.TriangleDownBang:
            {
                var p = Polygon(Triangle(down: true));
                AddBang(p, 4.2f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.TriangleUpBang:
            {
                var p = Polygon(Triangle(down: false));
                AddBang(p, 5f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.TriangleUp:
                FillAndOutline(canvas, Polygon(Triangle(down: false)), outline);
                break;
            case CdsStatusShape.TriangleDown:
                FillAndOutline(canvas, Polygon(Triangle(down: true)), outline);
                break;
            case CdsStatusShape.Diamond:
                FillAndOutline(canvas, Polygon(DiamondPoints()), outline);
                break;
            case CdsStatusShape.DiamondQuestion:
            {
                var p = Polygon(DiamondPoints());
                AddQuestion(p, 0.8f, 0.4f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.CircleCheck:
            {
                var p = Circle(7f);
                AddPolygon(p, Check());
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.RingCheck:
            {
                var p = Circle(7f);
                p.AppendCircle(Center, Center, 5.8f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                canvas.FillPath(Polygon(Check()), WindingMode.NonZero);
                break;
            }
            case CdsStatusShape.HalfFilledCircle:
            case CdsStatusShape.HalfFilled:
            {
                var ring = Circle(7f);
                ring.AppendCircle(Center, Center, 5.8f);
                canvas.FillPath(ring, WindingMode.EvenOdd);
                canvas.FillPath(HalfDisc(4.4f), WindingMode.NonZero);
                break;
            }
            case CdsStatusShape.HalfCircleDashed:
            {
                canvas.StrokeSize = 1.2f;
                canvas.StrokeDashPattern = [1.6f, 1.6f];
                canvas.DrawCircle(Center, Center, 6.4f);
                canvas.StrokeDashPattern = null;
                canvas.FillPath(HalfDisc(4.4f), WindingMode.NonZero);
                break;
            }
            case CdsStatusShape.DashedCircle:
                canvas.StrokeSize = 1.4f;
                canvas.StrokeDashPattern = [1.8f, 1.8f];
                canvas.DrawCircle(Center, Center, 6.3f);
                canvas.StrokeDashPattern = null;
                break;
            case CdsStatusShape.Ring:
            {
                var ring = Circle(7f);
                ring.AppendCircle(Center, Center, 5.6f);
                canvas.FillPath(ring, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.CircleDots:
            {
                var p = Circle(7f);
                foreach (var x in new[] { 4.9f, 8f, 11.1f }) p.AppendCircle(x, Center, 1f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.CircleQuestion:
            {
                var p = Circle(7f);
                AddQuestion(p, 1f, 0f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            case CdsStatusShape.SquareBang:
            {
                var p = new PathF();
                p.AppendRectangle(1.5f, 1.5f, 13f, 13f);
                AddBang(p, 4f, 1.2f);
                canvas.FillPath(p, WindingMode.EvenOdd);
                break;
            }
            default:
                canvas.FillPath(Circle(7f), WindingMode.NonZero);
                break;
        }
        canvas.RestoreState();
    }

    static void FillAndOutline(ICanvas canvas, PathF path, Color? outline)
    {
        canvas.FillPath(path, WindingMode.NonZero);
        if (outline is null) return;
        canvas.StrokeColor = outline;
        canvas.StrokeSize = 1;
        canvas.DrawPath(path);
    }

    static PathF Circle(float radius)
    {
        var p = new PathF();
        p.AppendCircle(Center, Center, radius);
        return p;
    }

    static PathF HalfDisc(float radius)
    {
        // The right half of a disc: from the top to the bottom through the right.
        var points = new List<PointF> { new(Center, Center - radius) };
        for (var a = -90; a <= 90; a += 15)
            points.Add(new PointF(Center + radius * MathF.Cos(a * MathF.PI / 180), Center + radius * MathF.Sin(a * MathF.PI / 180)));
        return Polygon(points);
    }

    static PathF Polygon(IReadOnlyList<PointF> points)
    {
        var p = new PathF();
        AddPolygon(p, points);
        return p;
    }

    static void AddPolygon(PathF path, IReadOnlyList<PointF> points)
    {
        path.MoveTo(points[0]);
        for (var i = 1; i < points.Count; i++) path.LineTo(points[i]);
        path.Close();
    }

    static List<PointF> Regular(int sides, float radius, float startDegrees)
    {
        var points = new List<PointF>();
        for (var i = 0; i < sides; i++)
        {
            var a = (startDegrees + 360f * i / sides) * MathF.PI / 180;
            points.Add(new PointF(Center + radius * MathF.Cos(a), Center + radius * MathF.Sin(a)));
        }
        return points;
    }

    static List<PointF> Triangle(bool down) => down
        ? [new(1f, 2.2f), new(15f, 2.2f), new(8f, 14.6f)]
        : [new(8f, 1.4f), new(15f, 13.8f), new(1f, 13.8f)];

    static List<PointF> DiamondPoints() => [new(8f, 0.9f), new(15.1f, 8f), new(8f, 15.1f), new(0.9f, 8f)];

    static List<PointF> Plus(float halfThickness, float halfLength)
    {
        var h = halfThickness;
        var a = halfLength;
        return
        [
            new(-h, -a), new(h, -a), new(h, -h), new(a, -h), new(a, h), new(h, h),
            new(h, a), new(-h, a), new(-h, h), new(-a, h), new(-a, -h), new(-h, -h),
        ];
    }

    static List<PointF> Rotated(List<PointF> points, float degrees)
    {
        var r = degrees * MathF.PI / 180;
        var cos = MathF.Cos(r);
        var sin = MathF.Sin(r);
        return points.Select(p => new PointF(Center + p.X * cos - p.Y * sin, Center + p.X * sin + p.Y * cos)).ToList();
    }

    static List<PointF> Check() => ThickPolyline([new(4.5f, 8.3f), new(7f, 10.8f), new(11.6f, 5.4f)], 1.5f);

    static void AddBang(PathF path, float top, float scale = 1f)
    {
        // A bar and a dot, as two cut-out subpaths.
        path.AppendRectangle(Center - 0.7f * scale, top, 1.4f * scale, 5f * scale);
        path.AppendCircle(Center, top + 5f * scale + 1.9f * scale, 0.85f * scale);
    }

    static void AddQuestion(PathF path, float scale, float yShift)
    {
        var points = new List<PointF>();
        const float r = 2.1f;
        var cy = 6.3f + yShift;
        for (var a = 180; a <= 450; a += 15)
            points.Add(new PointF(Center + r * MathF.Cos(a * MathF.PI / 180), cy + r * MathF.Sin(a * MathF.PI / 180)));
        points.Add(new PointF(Center, cy + r + 0.9f));
        points.Add(new PointF(Center, cy + r + 1.6f));
        var thick = ThickPolyline(points, 1.4f * scale);
        AddPolygon(path, thick);
        path.AppendCircle(Center, cy + r + 3.6f * scale, 0.8f * scale);
    }

    /// <summary>Outlines an open polyline of the given width as one closed polygon (averaged normals).</summary>
    internal static List<PointF> ThickPolyline(IReadOnlyList<PointF> points, float width)
    {
        var half = width / 2;
        var left = new List<PointF>();
        var right = new List<PointF>();
        for (var i = 0; i < points.Count; i++)
        {
            var (nx, ny) = Normal(points, i);
            left.Add(new PointF(points[i].X + nx * half, points[i].Y + ny * half));
            right.Add(new PointF(points[i].X - nx * half, points[i].Y - ny * half));
        }
        right.Reverse();
        left.AddRange(right);
        return left;
    }

    static (float, float) Normal(IReadOnlyList<PointF> points, int i)
    {
        (float, float) Seg(int a, int b)
        {
            var dx = points[b].X - points[a].X;
            var dy = points[b].Y - points[a].Y;
            var len = MathF.Sqrt(dx * dx + dy * dy);
            return len < 1e-6f ? (0, 0) : (-dy / len, dx / len);
        }
        if (i == 0) return Seg(0, 1);
        if (i == points.Count - 1) return Seg(i - 1, i);
        var (ax, ay) = Seg(i - 1, i);
        var (bx, by) = Seg(i, i + 1);
        var mx = ax + bx;
        var my = ay + by;
        var dot = ax * bx + ay * by;
        var len2 = MathF.Sqrt(mx * mx + my * my);
        if (len2 < 1e-6f) return (ax, ay);
        // Miter: scale the averaged normal so the offset keeps the full width at the join.
        var miter = 1f / MathF.Max(MathF.Sqrt((1 + dot) / 2), 0.5f);
        return (mx / len2 * miter, my / len2 * miter);
    }
}

/// <summary>A GraphicsView that draws one status glyph in token colors.</summary>
internal sealed class CdsStatusGlyphView : GraphicsView, IDrawable
{
    public static readonly BindableProperty ShapeProperty = BindableProperty.Create(
        nameof(Shape), typeof(CdsStatusShape), typeof(CdsStatusGlyphView), CdsStatusShape.Circle, propertyChanged: Redraw);

    public static readonly BindableProperty FillProperty = BindableProperty.Create(
        nameof(Fill), typeof(Color), typeof(CdsStatusGlyphView), null, propertyChanged: Redraw);

    public static readonly BindableProperty OutlineProperty = BindableProperty.Create(
        nameof(Outline), typeof(Color), typeof(CdsStatusGlyphView), null, propertyChanged: Redraw);

    public static readonly BindableProperty GlyphSizeProperty = BindableProperty.Create(
        nameof(GlyphSize), typeof(double), typeof(CdsStatusGlyphView), 16d,
        propertyChanged: static (b, _, n) =>
        {
            var v = (CdsStatusGlyphView)b;
            v.WidthRequest = (double)n;
            v.HeightRequest = (double)n;
            v.Invalidate();
        });

    public CdsStatusGlyphView()
    {
        Drawable = new CdsWeakDrawable(this);
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        HorizontalOptions = LayoutOptions.Start;
        WidthRequest = 16;
        HeightRequest = 16;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    public CdsStatusShape Shape
    {
        get => (CdsStatusShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public Color? Fill
    {
        get => (Color?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Color? Outline
    {
        get => (Color?)GetValue(OutlineProperty);
        set => SetValue(OutlineProperty, value);
    }

    public double GlyphSize
    {
        get => (double)GetValue(GlyphSizeProperty);
        set => SetValue(GlyphSizeProperty, value);
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Fill is not { } fill) return;
        var size = (float)Math.Min(GlyphSize, Math.Min(Width > 0 ? Width : GlyphSize, Height > 0 ? Height : GlyphSize));
        CdsStatusDrawing.Draw(canvas, Shape, size, fill, Outline);
    }

    static void Redraw(BindableObject b, object? o, object? n) => ((CdsStatusGlyphView)b).Invalidate();
}
