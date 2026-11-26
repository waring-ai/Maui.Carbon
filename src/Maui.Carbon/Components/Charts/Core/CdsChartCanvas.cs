namespace Maui.Carbon;

/// <summary>
/// Resolves scene paints for one chart: the palette where it sits, and which categorical color each series takes
/// (Carbon's pairing by series count, or the app's <see cref="CdsChart.ColorScale"/>).
/// </summary>
internal sealed class ChartColors
{
    readonly Dictionary<string, int> _index;
    readonly IDictionary<string, string>? _scale;

    public ChartColors(CdsChartPalette palette, IReadOnlyList<string> groups, int option, IDictionary<string, string>? scale)
    {
        Palette = palette;
        _index = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < groups.Count; i++) _index.TryAdd(groups[i], i);
        Variants = groups.Count;
        Option = option;
        _scale = scale;
    }

    public CdsChartPalette Palette { get; }

    public int Variants { get; }

    public int Option { get; }

    /// <summary>The color of a series; <paramref name="hovered"/> gives Carbon's 7% darker hover color.</summary>
    public Color Series(string? group, bool hovered = false)
    {
        group ??= "";
        if (_scale is not null && _scale.TryGetValue(group, out var key) && CdsPalettes.IsColorKey(key))
        {
            var c = Palette.Key(key);
            return hovered ? ChartColorMath.Darken(c) : c;
        }
        var index = _index.TryGetValue(group, out var i) ? i : 0;
        return Palette.Series(Math.Max(1, Variants), Option, index, hovered);
    }

    public Color Resolve(ChartPaint paint, bool hovered = false) => paint.Kind switch
    {
        ChartPaintKind.Key => Palette.Key(paint.Name!),
        ChartPaintKind.Token => Palette.Token((CdsChartToken)paint.A),
        ChartPaintKind.Series => Series(paint.Name, hovered),
        ChartPaintKind.Sequential => Palette.Sequential(paint.A, paint.B),
        ChartPaintKind.Diverging => Palette.Diverging(paint.A, paint.B),
        ChartPaintKind.Color => hovered ? ChartColorMath.Darken(paint.Value!) : paint.Value!,
        _ => Colors.Transparent,
    };
}

/// <summary>Color arithmetic Carbon Charts applies (Sass <c>color.adjust($c, $lightness: -7%)</c> for hover).</summary>
internal static class ChartColorMath
{
    public static Color Darken(Color c, float amount = 0.07f)
    {
        c.ToHsl(out var h, out var s, out var l);
        return Color.FromHsla(h, s, Math.Max(0, l - amount), c.Alpha);
    }
}

/// <summary>What the canvas draws besides the scene: the active mark and the hovered legend series.</summary>
internal readonly record struct ChartDrawState(int ActiveMark, string? LegendGroup, float ShimmerPhase);

/// <summary>Draws a <see cref="ChartScene"/>.</summary>
internal static class ChartRenderer
{
    public static void Draw(ICanvas canvas, ChartScene scene, ChartColors colors, ChartDrawState state)
    {
        var active = state.ActiveMark;
        foreach (var shape in scene.Shapes)
        {
            var on = active >= 0 && shape.Mark == active;
            if (shape.HoverOnly && !on) continue;
            var opacity = shape.Opacity;
            if (state.LegendGroup is not null && shape.Group is not null && shape.Group != state.LegendGroup) opacity *= shape.LegendFade;
            if (opacity <= 0) continue;
            canvas.SaveState();
            if (shape.Clip && !scene.Plot.IsEmpty) canvas.ClipRectangle(scene.Plot);
            canvas.Alpha = opacity;
            var fillPaint = on && !shape.HoverFill.IsNone ? shape.HoverFill : shape.Fill;
            var darken = on && shape.Hover == ChartHoverStyle.Darken && fillPaint.Kind == ChartPaintKind.Series;
            Color? fill = fillPaint.IsNone ? null : colors.Resolve(fillPaint, darken);
            var fillOpacity = on && shape.HoverFillOpacity is { } hfo ? hfo : shape.FillOpacity;
            if (fill is not null) canvas.FillColor = fill.WithAlpha(fill.Alpha * fillOpacity);
            if (shape.FillGradient is { } gradient)
            {
                fill = Colors.Transparent;
                SetGradient(canvas, gradient, colors, fillOpacity);
            }
            Color? stroke = shape.Stroke.IsNone ? null : colors.Resolve(shape.Stroke);
            if (stroke is not null)
            {
                canvas.StrokeColor = stroke.WithAlpha(stroke.Alpha * shape.StrokeOpacity);
                canvas.StrokeSize = on && shape.HoverStrokeWidth is { } hsw ? hsw : shape.StrokeWidth;
                canvas.StrokeDashPattern = shape.Dash;
                canvas.StrokeLineCap = shape.Cap;
                canvas.StrokeLineJoin = shape.Join;
            }
            switch (shape)
            {
                case ChartRect r:
                    if (r.CornerRadius > 0)
                    {
                        if (fill is not null) canvas.FillRoundedRectangle(r.Rect, r.CornerRadius);
                        if (stroke is not null) canvas.DrawRoundedRectangle(r.Rect, r.CornerRadius);
                    }
                    else
                    {
                        if (fill is not null) canvas.FillRectangle(r.Rect);
                        if (stroke is not null) canvas.DrawRectangle(r.Rect);
                    }
                    break;
                case ChartPath p:
                    if (fill is not null) canvas.FillPath(p.Path, p.Winding);
                    if (stroke is not null) canvas.DrawPath(p.Path);
                    break;
                case ChartLine l:
                    if (stroke is not null) canvas.DrawLine(l.From, l.To);
                    break;
                case ChartCircle c:
                    var radius = on && c.HoverRadius is { } hr ? hr : c.Radius;
                    if (fill is not null) canvas.FillCircle(c.Center, radius);
                    if (stroke is not null) canvas.DrawCircle(c.Center, radius);
                    break;
            }
            canvas.RestoreState();
            if (scene.Shimmer && shape.Shimmers && scene.Width > 0) DrawShimmer(canvas, scene, shape, colors, state.ShimmerPhase);
        }

        // The ruler: a dashed line through the active position (Carbon's ruler-line, layer-inverse-absolute).
        if (active >= 0 && scene.FindMark(active) is { RulerAt: { } at } && !scene.Plot.IsEmpty)
        {
            canvas.SaveState();
            canvas.StrokeColor = colors.Palette.Token(CdsChartToken.LayerInverseAbsolute);
            canvas.StrokeSize = 1;
            canvas.StrokeDashPattern = [2, 2];
            var p = scene.Plot;
            if (scene.RulerVertical) canvas.DrawLine(at, p.Top, at, p.Bottom);
            else canvas.DrawLine(p.Left, at, p.Right, at);
            canvas.RestoreState();
        }
    }

    /// <summary>Sets a scene gradient as the fill paint (the canvas maps its relative points onto the rectangle given).</summary>
    static void SetGradient(ICanvas canvas, ChartGradient g, ChartColors colors, float opacity)
    {
        var stops = g.Stops.Select(s =>
        {
            var c = colors.Resolve(s.Color);
            return new PaintGradientStop(s.Offset, c.WithAlpha(c.Alpha * s.Opacity * opacity));
        }).ToArray();
        var x = Math.Min(g.Start.X, g.End.X);
        var y = Math.Min(g.Start.Y, g.End.Y);
        var w = Math.Max(1, Math.Abs(g.End.X - g.Start.X));
        var h = Math.Max(1, Math.Abs(g.End.Y - g.Start.Y));
        Point Rel(PointF p) => new((p.X - x) / w, (p.Y - y) / h);
        canvas.SetFillPaint(new LinearGradientPaint(stops, Rel(g.Start), Rel(g.End)), new RectF(x, y, w, h));
    }

    /// <summary>
    /// Carbon's skeleton shimmer (bundle.css <c>.stop-bg</c> / <c>.stop-shimmer</c>): the skeleton shape's
    /// <c>layer-accent-01</c> turns to <c>layer-01</c> inside a band sweeping across the chart every 2 seconds.
    /// The band is clipped to the shape, so the backdrop around it never changes.
    /// </summary>
    static void DrawShimmer(ICanvas canvas, ChartScene scene, ChartShape shape, ChartColors colors, float phase)
    {
        var band = Math.Max(scene.Width / 2, 40);
        var x = -band + (scene.Width + band) * phase;
        canvas.SaveState();
        switch (shape)
        {
            case ChartRect r:
                canvas.ClipRectangle(r.Rect);
                break;
            case ChartPath p:
                canvas.ClipPath(p.Path, p.Winding);
                break;
            case ChartCircle c:
            {
                var circle = new PathF();
                circle.AppendCircle(c.Center.X, c.Center.Y, c.Radius);
                canvas.ClipPath(circle);
                break;
            }
            case ChartLine l:
            {
                // A skeleton line: its stroke's box (skeleton lines are horizontal or vertical).
                var half = Math.Max(0.5f, shape.StrokeWidth / 2);
                canvas.ClipRectangle(Math.Min(l.From.X, l.To.X) - half, Math.Min(l.From.Y, l.To.Y) - half,
                    Math.Abs(l.To.X - l.From.X) + 2 * half, Math.Abs(l.To.Y - l.From.Y) + 2 * half);
                break;
            }
        }
        var c1 = colors.Palette.Key(CdsColorKeys.Layer01);
        canvas.SetFillPaint(new LinearGradientPaint(
            [new PaintGradientStop(0, c1.WithAlpha(0)), new PaintGradientStop(0.5f, c1), new PaintGradientStop(1, c1.WithAlpha(0))],
            new Point(0, 0.5), new Point(1, 0.5)), new RectF(x, 0, band, scene.Height));
        canvas.FillRectangle(x, 0, band, scene.Height);
        canvas.RestoreState();
    }
}

/// <summary>
/// The chart's drawing surface: draws the scene; input is handled by the plot's hit target above it. Its draws are
/// measured under the chart's type (the constructor's component), so a busy canvas names its chart.
/// </summary>
internal sealed class CdsChartCanvas : GraphicsView, IDrawable
{
    ChartScene _scene = ChartScene.Empty;
    ChartColors? _colors;
    ChartDrawState _state = new(-1, null, 0);

    public CdsChartCanvas(string component)
    {
        Drawable = new CdsWeakDrawable(this, component);
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    public ChartScene Scene => _scene;

    public void Show(ChartScene scene, ChartColors colors)
    {
        _scene = scene;
        _colors = colors;
        Invalidate();
    }

    public void SetState(ChartDrawState state)
    {
        if (_state == state) return;
        _state = state;
        Invalidate();
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_colors is null) return;
        ChartRenderer.Draw(canvas, _scene, _colors, _state);
    }
}
