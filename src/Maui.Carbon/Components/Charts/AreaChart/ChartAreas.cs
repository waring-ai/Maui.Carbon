namespace Maui.Carbon;

/// <summary>
/// Carbon's area graph (components/graphs/area.ts, area-stacked.ts and GradientUtils): area paths between two edges,
/// the baseline areas grow from, and the single-series gradient fill.
/// </summary>
internal static class ChartAreas
{
    /// <summary>Carbon's <c>area.opacity.selected</c>: every area draws at 40% opacity.</summary>
    public const float SelectedOpacity = 0.4f;

    /// <summary>Carbon's <c>area.opacity.unselected</c>: legend hover fades the other areas to 5%.</summary>
    public const float UnselectedOpacity = 0.05f;

    /// <summary>The gradient's opacity at its strong end (Carbon's GradientUtils stops).</summary>
    public const float GradientOpacity = 0.6f;

    /// <summary>One run of consecutive defined points: the edge drawn forward (values) and the edge drawn back (baseline or other bound).</summary>
    public sealed record Run(List<PointF> Forward, List<PointF> Back);

    /// <summary>
    /// The range value areas grow from: 0, or the domain start when <see cref="CdsChartAxis.IncludeZero"/> is off and the
    /// domain is positive (a log axis always starts at its domain start).
    /// </summary>
    public static double BaseValue(CartesianLayout cs, string? group)
    {
        var pos = cs.RangePositionOf(group);
        var a = cs.Axes[pos];
        var (min, max) = cs.Scales[pos].Domain;
        if (a.ScaleType == CdsChartScaleType.Log) return min;
        if (!a.IncludeZero && min > 0 && max > 0) return min;
        return 0;
    }

    /// <summary>
    /// Splits points sorted along the domain into runs of defined values (d3's <c>area.defined</c>): each entry is the
    /// domain pixel, the forward range pixel (<c>null</c> breaks the area) and the back range pixel.
    /// </summary>
    public static List<Run> Runs(CartesianLayout cs, IEnumerable<(double Domain, double? Forward, double Back)> points)
    {
        var runs = new List<Run>();
        Run? current = null;
        foreach (var (d, f, b) in points)
        {
            if (f is { } fy && !double.IsNaN(fy) && !double.IsNaN(d) && !double.IsNaN(b))
            {
                current ??= new Run([], []);
                current.Forward.Add(cs.XY(d, fy));
                current.Back.Add(cs.XY(d, b));
            }
            else if (current is not null)
            {
                runs.Add(current);
                current = null;
            }
        }
        if (current is not null) runs.Add(current);
        return runs;
    }

    /// <summary>The area path of the runs (d3's area: the forward edge with the curve, then the back edge reversed).</summary>
    public static PathF Path(IReadOnlyList<Run> runs, CdsChartCurve curve)
    {
        var path = new PathF();
        foreach (var r in runs)
        {
            ChartCurves.Append(path, r.Forward, curve, move: true);
            var back = new List<PointF>(r.Back);
            back.Reverse();
            ChartCurves.Append(path, back, curve, move: false);
            path.Close();
        }
        return path;
    }

    /// <summary>The polyline of a curve through <paramref name="points"/>, with Bezier segments flattened.</summary>
    public static List<PointF> Flatten(IReadOnlyList<PointF> points, CdsChartCurve curve)
    {
        var path = new PathF();
        ChartCurves.Append(path, points, curve);
        var list = new List<PointF>();
        for (var i = 0; i < path.OperationCount; i++)
        {
            var seg = path.GetPointsForSegment(i);
            switch (path.GetSegmentType(i))
            {
                case PathOperation.Move:
                case PathOperation.Line:
                    if (seg.Length > 0) list.Add(seg[^1]);
                    break;
                case PathOperation.Cubic when seg.Length >= 3 && list.Count > 0:
                {
                    var p0 = list[^1];
                    for (var s = 1; s <= 12; s++)
                    {
                        var t = s / 12f;
                        var u = 1 - t;
                        list.Add(new PointF(
                            u * u * u * p0.X + 3 * u * u * t * seg[0].X + 3 * u * t * t * seg[1].X + t * t * t * seg[2].X,
                            u * u * u * p0.Y + 3 * u * u * t * seg[0].Y + 3 * u * t * t * seg[1].Y + t * t * t * seg[2].Y));
                    }
                    break;
                }
                case PathOperation.Quad when seg.Length >= 2 && list.Count > 0:
                {
                    var p0 = list[^1];
                    for (var s = 1; s <= 12; s++)
                    {
                        var t = s / 12f;
                        var u = 1 - t;
                        list.Add(new PointF(u * u * p0.X + 2 * u * t * seg[0].X + t * t * seg[1].X, u * u * p0.Y + 2 * u * t * seg[0].Y + t * t * seg[1].Y));
                    }
                    break;
                }
                default:
                    if (seg.Length > 0) list.Add(seg[^1]);
                    break;
            }
        }
        return list;
    }

    /// <summary>
    /// Carbon's GradientUtils.getStops as (offset, opacity) along the area's bounding box from top to bottom: 60% at the top
    /// fading to 0 at 80%, or, when the value domain crosses zero, fading to 0 at zero and growing to 60% again at the bottom.
    /// </summary>
    public static (double Offset, double Opacity)[] GradientStops(double domainMin, double domainMax)
    {
        if (domainMin < 0 && domainMax > 0)
        {
            var ratio = Math.Round(Math.Abs(domainMax) * 100 / Math.Abs(domainMin - domainMax), 2) / 100;
            return [(0, GradientOpacity), (ratio, 0), (1, GradientOpacity)];
        }
        return [(0, GradientOpacity), (0.8, 0)];
    }

    /// <summary>
    /// The gradient fill of a vertical area between the curves and the baseline pixel <paramref name="baseY"/>: the area
    /// path filled with a vertical linear gradient through Carbon's stops, spanning the area's bounding box from top to
    /// bottom. Null when the area has no height.
    /// </summary>
    public static ChartPath? Gradient(PathF area, IReadOnlyList<List<PointF>> curves, float baseY, (double Offset, double Opacity)[] stops, string group)
    {
        var all = curves.SelectMany(c => c).ToList();
        if (all.Count == 0) return null;
        var top = Math.Min(all.Min(p => p.Y), baseY);
        var bottom = Math.Max(all.Max(p => p.Y), baseY);
        if (bottom - top <= 0) return null;
        var paint = ChartPaint.OfSeries(group);
        var x = all[0].X;
        return new ChartPath
        {
            Path = area,
            FillGradient = new ChartGradient(new PointF(x, top), new PointF(x, bottom), stops.Select(s => new ChartGradientStop((float)s.Offset, paint, (float)s.Opacity)).ToList()),
            Group = group,
            Hover = ChartHoverStyle.None,
        };
    }
}

/// <summary>Copies of a scene mark with a different tooltip anchor or spoken description (marks are immutable).</summary>
internal static class ChartMarkCopy
{
    public static ChartMark With(ChartMark m, PointF? anchor = null, string? description = null) => new()
    {
        Id = m.Id,
        Bounds = m.Bounds,
        Polygon = m.Polygon,
        Radius = m.Radius,
        Group = m.Group,
        Rows = m.Rows,
        Title = m.Title,
        Description = description ?? m.Description,
        IsReference = m.IsReference,
        RulerAt = m.RulerAt,
        Anchor = anchor ?? m.Anchor,
    };
}
