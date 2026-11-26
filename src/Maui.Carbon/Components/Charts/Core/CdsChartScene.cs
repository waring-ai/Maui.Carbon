namespace Maui.Carbon;

/// <summary>What a scene color refers to; resolved against the chart's palette at draw time, so a theme change only redraws.</summary>
internal enum ChartPaintKind : byte
{
    None,
    /// <summary>A theme color key (<see cref="CdsColorKeys"/>).</summary>
    Key,
    /// <summary>A Carbon Charts token (<see cref="CdsChartToken"/>).</summary>
    Token,
    /// <summary>The categorical color of a series (by group name).</summary>
    Series,
    /// <summary>A step of a sequential scale.</summary>
    Sequential,
    /// <summary>A step of a diverging scale.</summary>
    Diverging,
    /// <summary>A fixed color (supplied by the app).</summary>
    Color,
}

/// <summary>A color reference in a chart scene.</summary>
internal readonly record struct ChartPaint(ChartPaintKind Kind, string? Name = null, int A = 0, int B = 0, Color? Value = null)
{
    public static ChartPaint None => default;

    public bool IsNone => Kind == ChartPaintKind.None;

    public static ChartPaint OfKey(string key) => new(ChartPaintKind.Key, key);

    public static ChartPaint OfToken(CdsChartToken token) => new(ChartPaintKind.Token, A: (int)token);

    public static ChartPaint OfSeries(string? group) => new(ChartPaintKind.Series, group ?? "");

    /// <summary>Sequential <paramref name="hue"/> (1 purple, 2 blue, 3 cyan, 4 teal), step 0 to 10.</summary>
    public static ChartPaint OfSequential(int hue, int step) => new(ChartPaintKind.Sequential, A: hue, B: step);

    /// <summary>Diverging <paramref name="palette"/> (1 red-cyan, 2 purple-teal), step 0 to 16 (8 is zero).</summary>
    public static ChartPaint OfDiverging(int palette, int step) => new(ChartPaintKind.Diverging, A: palette, B: step);

    public static ChartPaint OfColor(Color color) => new(ChartPaintKind.Color, Value: color);
}

/// <summary>How a shape reacts when its mark is active (hovered or keyboard-focused).</summary>
internal enum ChartHoverStyle : byte
{
    /// <summary>A series fill darkens 7% (Carbon's <c>-hovered</c> colors); other paints stay.</summary>
    Darken,
    /// <summary>No change.</summary>
    None,
}

/// <summary>A drawn primitive of a chart scene.</summary>
internal abstract class ChartShape
{
    /// <summary>The series it belongs to: fades to <see cref="LegendFade"/> while another legend item is hovered.</summary>
    public string? Group { get; set; }

    /// <summary>The interactive mark it draws (<see cref="ChartMark.Id"/>), or -1. Several shapes may share a mark.</summary>
    public int Mark { get; set; } = -1;

    public ChartPaint Fill { get; set; }

    public ChartPaint Stroke { get; set; }

    public float FillOpacity { get; set; } = 1;

    public float StrokeOpacity { get; set; } = 1;

    /// <summary>Opacity of the whole shape.</summary>
    public float Opacity { get; set; } = 1;

    public float StrokeWidth { get; set; } = 1;

    public float[]? Dash { get; set; }

    public LineCap Cap { get; set; } = LineCap.Butt;

    public LineJoin Join { get; set; } = LineJoin.Miter;

    /// <summary>Clips the shape to the scene's plot rectangle.</summary>
    public bool Clip { get; set; }

    /// <summary>Drawn only while its mark is active (ruler dots, hover outlines).</summary>
    public bool HoverOnly { get; set; }

    public ChartHoverStyle Hover { get; set; }

    /// <summary>Fill opacity while the mark is active (filled scatter dots go from 0.3 to 1).</summary>
    public float? HoverFillOpacity { get; set; }

    /// <summary>A fill used only while the mark is active (unfilled dots fill with the series color).</summary>
    public ChartPaint HoverFill { get; set; }

    /// <summary>Stroke width while the mark is active.</summary>
    public float? HoverStrokeWidth { get; set; }

    /// <summary>Opacity multiplier while another series is hovered in the legend. Default 0.3 (Carbon).</summary>
    public float LegendFade { get; set; } = 0.3f;

    /// <summary>
    /// A linear gradient that fills the shape instead of <see cref="Fill"/> (Carbon's SVG gradients: area fades,
    /// alluvial links). Its colors still go through the palette, so a theme change only redraws.
    /// </summary>
    public ChartGradient? FillGradient { get; set; }

    /// <summary>
    /// A skeleton shape: while the scene shimmers (<see cref="ChartScene.Shimmer"/>), Carbon's moving
    /// <c>layer-01</c> highlight sweeps across this shape only, never across the backdrop.
    /// </summary>
    public bool Shimmers { get; set; }
}

/// <summary>A stop of a <see cref="ChartGradient"/>: an offset from 0 to 1, a palette color and its opacity.</summary>
internal readonly record struct ChartGradientStop(float Offset, ChartPaint Color, float Opacity = 1);

/// <summary>A linear gradient in scene coordinates, from <paramref name="Start"/> (offset 0) to <paramref name="End"/> (offset 1), padded beyond both ends.</summary>
internal sealed record ChartGradient(PointF Start, PointF End, IReadOnlyList<ChartGradientStop> Stops);

internal sealed class ChartRect : ChartShape
{
    public RectF Rect { get; set; }

    public float CornerRadius { get; set; }
}

internal sealed class ChartPath : ChartShape
{
    public required PathF Path { get; set; }

    public WindingMode Winding { get; set; } = WindingMode.NonZero;
}

internal sealed class ChartLine : ChartShape
{
    public PointF From { get; set; }

    public PointF To { get; set; }
}

internal sealed class ChartCircle : ChartShape
{
    public PointF Center { get; set; }

    public float Radius { get; set; }

    /// <summary>Radius while the mark is active.</summary>
    public float? HoverRadius { get; set; }
}

internal enum ChartTextAnchor : byte
{
    Start,
    Middle,
    End,
}

internal enum ChartTextBaseline : byte
{
    /// <summary>The point is the top of the line box.</summary>
    Top,
    /// <summary>The point is the vertical middle.</summary>
    Middle,
    /// <summary>The point is the bottom of the line box.</summary>
    Bottom,
}

/// <summary>A text label of a chart scene, shown as a real <see cref="CdsText"/> over the canvas.</summary>
internal sealed class ChartText
{
    public required string Text { get; init; }

    public float X { get; init; }

    public float Y { get; init; }

    public ChartFont Font { get; init; } = ChartFont.Tick;

    public ChartPaint Color { get; init; } = ChartPaint.OfKey(CdsColorKeys.TextSecondary);

    public ChartTextAnchor Anchor { get; init; } = ChartTextAnchor.Start;

    public ChartTextBaseline Baseline { get; init; } = ChartTextBaseline.Top;

    /// <summary>Degrees, clockwise, about the anchor point (-45 for rotated ticks, -90 for a left axis title).</summary>
    public float Rotation { get; init; }

    public float Opacity { get; init; } = 1;

    /// <summary>The series it belongs to (fades with legend hover when <see cref="FadesWithLegend"/>).</summary>
    public string? Group { get; init; }

    public bool FadesWithLegend { get; init; }

    /// <summary>The measured width (from <see cref="CdsChartTextMetrics"/>).</summary>
    public float Width => CdsChartTextMetrics.Width(Text, Font);
}

/// <summary>
/// A text halo (SVG <c>paint-order: stroke</c> with a round-joined stroke): copies of the label in the halo color,
/// offset around it, drawn before it. Labels are views over the canvas and cannot be stroked.
/// </summary>
internal static class ChartTextHalo
{
    /// <summary>Adds <paramref name="text"/> to <paramref name="texts"/> over a halo <paramref name="radius"/> wide (half the SVG stroke width).</summary>
    public static void Add(List<ChartText> texts, ChartText text, ChartPaint halo, float radius = 1.5f)
    {
        for (var i = 0; i < 8; i++)
        {
            var angle = i * MathF.PI / 4;
            texts.Add(new ChartText
            {
                Text = text.Text, X = text.X + radius * MathF.Cos(angle), Y = text.Y + radius * MathF.Sin(angle), Font = text.Font, Color = halo,
                Anchor = text.Anchor, Baseline = text.Baseline, Rotation = text.Rotation, Opacity = text.Opacity, Group = text.Group, FadesWithLegend = text.FadesWithLegend,
            });
        }
        texts.Add(text);
    }
}

/// <summary>One row of a chart tooltip: label, value and the series color strip.</summary>
internal sealed record ChartTooltipRow(string Label, string? Value, ChartPaint Color = default, bool Bold = false);

/// <summary>
/// An interactive element of a chart (a bar, slice, cell, node, or a ruler position): its hit area, tooltip and the text a
/// screen reader speaks when the data cursor reaches it.
/// </summary>
internal sealed class ChartMark
{
    public int Id { get; init; }

    /// <summary>The hit area (and the tooltip anchor for keyboard navigation).</summary>
    public RectF Bounds { get; init; }

    /// <summary>A polygon hit area (slices, irregular regions) instead of <see cref="Bounds"/>.</summary>
    public PointF[]? Polygon { get; init; }

    /// <summary>A circular hit area (dots, bubbles, circle-pack nodes) instead of <see cref="Bounds"/>.</summary>
    public float? Radius { get; init; }

    /// <summary>The series; hovering a mark does not fade the others (only legend hover does).</summary>
    public string? Group { get; init; }

    /// <summary>The tooltip rows. Empty with <see cref="Title"/> set: a title-only tooltip.</summary>
    public IReadOnlyList<ChartTooltipRow> Rows { get; init; } = [];

    public string? Title { get; init; }

    /// <summary>What a screen reader speaks for the mark ("Qty, Misc, 65,000"); defaults to the tooltip rows.</summary>
    public string? Description { get; init; }

    /// <summary>A reference mark (a threshold): hit before data marks in every hit mode and left out of the data cursor.</summary>
    public bool IsReference { get; init; }

    /// <summary>For ruler marks: the position along the domain axis where the dashed ruler line is drawn.</summary>
    public float? RulerAt { get; init; }

    /// <summary>The point the tooltip sits next to for keyboard navigation and taps (default: the bounds' center).</summary>
    public PointF? Anchor { get; init; }

    public PointF AnchorPoint => Anchor ?? new PointF(Bounds.Center.X, Bounds.Center.Y);

    public string SpokenText => Description ?? (Title is { } t && Rows.Count == 0
        ? t
        : string.Join(", ", Rows.Select(r => string.IsNullOrEmpty(r.Value) ? r.Label : r.Label + " " + r.Value)));

    public bool Contains(PointF p)
    {
        if (Radius is { } r)
        {
            var c = AnchorPoint;
            var dx = p.X - c.X;
            var dy = p.Y - c.Y;
            return dx * dx + dy * dy <= r * r;
        }
        if (Polygon is { Length: > 2 } poly) return ChartGeometry.PolygonContains(poly, p);
        return Bounds.Contains(p);
    }
}

/// <summary>How the plot finds the mark under the pointer.</summary>
internal enum ChartHitMode : byte
{
    /// <summary>The topmost mark containing the point (bars, slices, cells, nodes).</summary>
    Contains,
    /// <summary>
    /// The ruler mark nearest along the domain axis, within half the spacing of the marks (at least 8): line, area, scatter
    /// and stacked charts, where one position shows every series.
    /// </summary>
    Ruler,
    /// <summary>The mark whose anchor is nearest to the point, within 24 (dense dot charts).</summary>
    Nearest,
}

/// <summary>
/// A chart's drawing at one size: shapes (drawn in order), text labels, and interactive marks (in data-cursor order).
/// Immutable once built; hover only changes which mark is active.
/// </summary>
internal sealed class ChartScene
{
    public static readonly ChartScene Empty = new() { Width = 0, Height = 0 };

    public float Width { get; init; }

    public float Height { get; init; }

    /// <summary>The plot rectangle shapes with <see cref="ChartShape.Clip"/> are clipped to (and the ruler spans).</summary>
    public RectF Plot { get; init; }

    public List<ChartShape> Shapes { get; } = [];

    /// <summary>Shapes drawn above the labels layer's level in the canvas (none: labels are views, so they are always on top).</summary>
    public List<ChartText> Texts { get; } = [];

    public List<ChartMark> Marks { get; } = [];

    public ChartHitMode HitMode { get; init; } = ChartHitMode.Contains;

    /// <summary>For <see cref="ChartHitMode.Ruler"/>: true when the domain runs along x (vertical ruler line).</summary>
    public bool RulerVertical { get; init; } = true;

    /// <summary>Sweeps the loading shimmer across the shapes marked <see cref="ChartShape.Shimmers"/> (the loading skeleton).</summary>
    public bool Shimmer { get; init; }

    /// <summary>A short summary for screen readers ("8 bars"), used in the plot's name.</summary>
    public string? Summary { get; set; }

    public ChartMark? FindMark(int id)
    {
        foreach (var m in Marks) if (m.Id == id) return m;
        return null;
    }

    /// <summary>The mark under <paramref name="p"/> for <see cref="HitMode"/>, or null.</summary>
    public ChartMark? HitTest(PointF p)
    {
        if (Marks.Count == 0) return null;
        for (var i = Marks.Count - 1; i >= 0; i--)
            if (Marks[i].IsReference && Marks[i].Contains(p)) return Marks[i];
        switch (HitMode)
        {
            case ChartHitMode.Ruler:
            {
                if (!Plot.IsEmpty && !Plot.Inflate(1, 1).Contains(p)) return null;
                var positions = Marks.Where(m => m.RulerAt.HasValue).Select(m => m.RulerAt!.Value).Distinct().Order().ToList();
                var step = float.MaxValue;
                for (var i = 1; i < positions.Count; i++) step = Math.Min(step, positions[i] - positions[i - 1]);
                var threshold = step == float.MaxValue ? 40 : Math.Max(step / 2, 8);
                ChartMark? best = null;
                var bestDist = float.MaxValue;
                var at = RulerVertical ? p.X : p.Y;
                foreach (var m in Marks)
                {
                    if (m.RulerAt is not { } r || m.IsReference) continue;
                    var d = Math.Abs(r - at);
                    if (d <= threshold && d < bestDist - 0.01f)
                    {
                        best = m;
                        bestDist = d;
                    }
                }
                return best;
            }
            case ChartHitMode.Nearest:
            {
                ChartMark? best = null;
                var bestDist = 24f * 24f;
                foreach (var m in Marks)
                {
                    if (m.IsReference) continue;
                    if (m.Contains(p)) return m;
                    var c = m.AnchorPoint;
                    var d = (c.X - p.X) * (c.X - p.X) + (c.Y - p.Y) * (c.Y - p.Y);
                    if (d < bestDist)
                    {
                        best = m;
                        bestDist = d;
                    }
                }
                return best;
            }
            default:
                for (var i = Marks.Count - 1; i >= 0; i--)
                    if (Marks[i].Contains(p)) return Marks[i];
                return null;
        }
    }
}

/// <summary>Geometry helpers for scenes.</summary>
internal static class ChartGeometry
{
    public static bool PolygonContains(IReadOnlyList<PointF> poly, PointF p)
    {
        var inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            var a = poly[i];
            var b = poly[j];
            if ((a.Y > p.Y) != (b.Y > p.Y) && p.X < (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X) inside = !inside;
        }
        return inside;
    }

    /// <summary>The polygon of an annular sector (angles in radians, 0 at 12 o'clock, clockwise), as pie slices are drawn.</summary>
    public static PointF[] ArcPolygon(PointF center, float inner, float outer, float start, float end, int segments = 24)
    {
        var points = new List<PointF>(segments * 2 + 2);
        for (var i = 0; i <= segments; i++)
            points.Add(Polar(center, outer, start + (end - start) * i / segments));
        if (inner > 0)
            for (var i = segments; i >= 0; i--) points.Add(Polar(center, inner, start + (end - start) * i / segments));
        else
            points.Add(center);
        return points.ToArray();
    }

    /// <summary>A point at <paramref name="radius"/> and <paramref name="angle"/> (radians, 0 at 12 o'clock, clockwise).</summary>
    public static PointF Polar(PointF center, float radius, float angle) =>
        new(center.X + radius * MathF.Sin(angle), center.Y - radius * MathF.Cos(angle));

    /// <summary>An annular sector path (d3 <c>arc</c> without corner radius), angles as in <see cref="Polar"/>.</summary>
    public static PathF ArcPath(PointF center, float inner, float outer, float start, float end, float padAngle = 0)
    {
        var path = new PathF();
        var pad = Math.Min(padAngle / 2, (end - start) / 2);
        var s = start + pad;
        var e = end - pad;
        var segments = Math.Max(2, (int)Math.Ceiling(Math.Abs(e - s) / (MathF.PI / 90)));
        for (var i = 0; i <= segments; i++)
        {
            var pt = Polar(center, outer, s + (e - s) * i / segments);
            if (i == 0) path.MoveTo(pt);
            else path.LineTo(pt);
        }
        if (inner > 0)
        {
            // Keep the inner edge's pad as a constant width, like d3: a smaller angular pad on the inner radius.
            var innerPad = outer > 0 ? pad * outer / inner : pad;
            var si = start + Math.Min(innerPad, (end - start) / 2);
            var ei = end - Math.Min(innerPad, (end - start) / 2);
            for (var i = segments; i >= 0; i--) path.LineTo(Polar(center, inner, si + (ei - si) * i / segments));
        }
        else
        {
            path.LineTo(center);
        }
        path.Close();
        return path;
    }
}
