namespace Maui.Carbon;

/// <summary>
/// The gradients of Carbon's AI presence in a data table (<c>ai-table-gradient</c> in the <c>ai-gradient</c> utilities). Each is a
/// translucent tint that is drawn over a surface the table already fills: the aura token at its strongest on the start edge,
/// fading to nothing by the middle. The tint keeps the aura color at every stop and only changes its alpha, so it blends the
/// way CSS does (premultiplied) on every platform, and it sits on whatever fill the row, header or cell has in that state.
/// </summary>
internal static class CdsAiTableGradient
{
    /// <summary>Where the tint has faded out: the middle of the surface (Carbon: <c>50%</c>).</summary>
    public const float Span = 0.5f;

    /// <summary>Where the hover tint reaches half of its alpha (Carbon: the <c>15%</c> color hint).</summary>
    public const float HoverHint = 0.15f;

    /// <summary>
    /// Rows, header cells and cells of an AI column, in the default and selected states (Carbon:
    /// <c>linear-gradient(to right, ai-aura-start-sm 0%, ai-aura-end 50%, transparent 50%)</c>), left to right.
    /// </summary>
    public static LinearGradientBrush Row(Color auraStartSm) => Brush(
        [Stop(auraStartSm, 0f, 1f), Stop(auraStartSm, Span, 0f), Stop(auraStartSm, 1f, 0f)], horizontal: true);

    /// <summary>
    /// A hovered AI row (Carbon: <c>linear-gradient(to right, ai-aura-hover-start 0%, 15%, ai-aura-hover-end 50%)</c>, over
    /// <c>ai-aura-hover-background</c>). CSS color hints have no counterpart in a brush, so the curve of the hint (the midpoint of the
    /// transition at 15% of the surface) is sampled with a few stops.
    /// </summary>
    public static LinearGradientBrush RowHover(Color auraHoverStart)
    {
        // The hint H puts the 50% color at H: the blend at position t is (t / span) ^ (ln 0.5 / ln(H / span)).
        var exponent = Math.Log(0.5) / Math.Log(HoverHint / Span);
        var stops = new List<GradientStop>();
        foreach (var x in new[] { 0f, 0.05f, HoverHint, 0.25f, 0.35f, Span })
        {
            var blend = Math.Pow(x / Span, exponent);
            stops.Add(Stop(auraHoverStart, x, (float)(1 - blend)));
        }
        stops.Add(Stop(auraHoverStart, 1f, 0f));
        return Brush(stops.ToArray(), horizontal: true);
    }

    /// <summary>
    /// The whole-table tint (Carbon: <c>linear-gradient(to top, ai-aura-start 0%, ai-aura-end 50%, transparent 50%)</c>): the aura rising
    /// from the bottom edge of the body to its middle.
    /// </summary>
    public static LinearGradientBrush FullTable(Color auraStart) => Brush(
        [Stop(auraStart, 0f, 1f), Stop(auraStart, Span, 0f), Stop(auraStart, 1f, 0f)], horizontal: false);

    static GradientStop Stop(Color color, float offset, float alphaFactor) => new(color.WithAlpha(color.Alpha * alphaFactor), offset);

    static LinearGradientBrush Brush(GradientStop[] stops, bool horizontal) => horizontal
        ? new LinearGradientBrush([.. stops], new Point(0, 0), new Point(1, 0))
        : new LinearGradientBrush([.. stops], new Point(0, 1), new Point(0, 0));
}
