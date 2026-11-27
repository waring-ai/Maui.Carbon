namespace Maui.Carbon;

/// <summary>Where a full-page error puts its text and drawing at a width (pure, tested).</summary>
internal readonly record struct FullPageErrorColumns(bool Stacked, double TextWidth, double TextEndPadding, double ArtX, double ArtWidth);

/// <summary>
/// FullPageError layout rules (pure, tested): Carbon's grid with the text in 6 of 16 columns at lg and up (3 of 8 at md)
/// and the drawing in the rest; below md the drawing goes under the text.
/// </summary>
internal static class FullPageErrorModel
{
    /// <summary>The arrow before the label (hidden from screen readers).</summary>
    internal const string LabelArrow = "↳ ";

    /// <summary>Inline margin of the container: <c>spacing-06</c>, <c>spacing-03</c> below md.</summary>
    internal static double HorizontalMargin(CdsBreakpoint breakpoint) => breakpoint == CdsBreakpoint.Sm ? CdsSpacing.Spacing03 : CdsSpacing.Spacing06;

    /// <summary>Block padding of the grid (<c>spacing-07</c>).</summary>
    internal const double VerticalPadding = CdsSpacing.Spacing07;

    /// <summary>The drawing column's padding: the grid gutter (16) plus the svg container's <c>spacing-03</c>, and <c>spacing-11</c> below.</summary>
    internal static Thickness ArtPadding => new(CdsSpacing.Spacing05 + CdsSpacing.Spacing03, CdsSpacing.Spacing03, CdsSpacing.Spacing05 + CdsSpacing.Spacing03, CdsSpacing.Spacing11);

    /// <summary>Space below the label and title (<c>spacing-09</c>).</summary>
    internal const double TitleGap = CdsSpacing.Spacing09;

    /// <summary>Space below the description (<c>spacing-07</c>).</summary>
    internal const double DescriptionGap = CdsSpacing.Spacing07;

    /// <summary>The columns at <paramref name="breakpoint"/> for a content width (inside the margins).</summary>
    internal static FullPageErrorColumns Columns(CdsBreakpoint breakpoint, double width)
    {
        width = Math.Max(0, width);
        if (breakpoint == CdsBreakpoint.Sm) return new(true, width, 0, 0, width);
        var columns = breakpoint == CdsBreakpoint.Md ? 8 : 16;
        var textSpan = breakpoint == CdsBreakpoint.Md ? 3 : 6;
        var text = width / columns * textSpan;
        // lg and up add 32 between the text and the drawing ("padding: 0 $spacing-07 0 0").
        var endPadding = breakpoint >= CdsBreakpoint.Lg ? CdsSpacing.Spacing07 : 0;
        return new(false, text, endPadding, text, width - text);
    }

    /// <summary>The drawing for a kind.</summary>
    internal static IllustrationDrawing Art(CdsFullPageErrorKind kind) => kind switch
    {
        CdsFullPageErrorKind.Forbidden => IllustrationArt.FullPageError403,
        CdsFullPageErrorKind.NotFound => IllustrationArt.FullPageError404,
        _ => IllustrationArt.FullPageErrorCustom,
    };

    /// <summary>What the heading reads as: the label then the title (the arrow before the label is hidden).</summary>
    internal static string HeadingDescription(string? label, string? title) =>
        string.Join(". ", new[] { label, title }.Where(s => !string.IsNullOrWhiteSpace(s)));
}

/// <summary>Lays out the text and drawing of a <see cref="CdsFullPageError"/> by breakpoint while measuring.</summary>
internal sealed class FullPageErrorLayout : Layout
{
    static readonly BindableProperty ContextBreakpointProperty = BindableProperty.Create(
        "ContextBreakpoint", typeof(CdsBreakpoint?), typeof(FullPageErrorLayout), null,
        propertyChanged: static (b, _, _) => ((FullPageErrorLayout)b).InvalidateMeasure());

    internal FullPageErrorLayout(View text, View art)
    {
        Text = text;
        Art = art;
        Add(text);
        Add(art);
        SetDynamicResource(ContextBreakpointProperty, CdsContextKeys.Breakpoint);
    }

    internal View Text { get; }

    internal View Art { get; }

    CdsBreakpoint BreakpointFor(double width) =>
        (CdsBreakpoint?)GetValue(ContextBreakpointProperty) ?? CdsBreakpointExtensions.FromWidth(width);

    protected override Microsoft.Maui.Layouts.ILayoutManager CreateLayoutManager() => new Manager(this);

    Size MeasureParts(double widthConstraint)
    {
        var width = double.IsFinite(widthConstraint) ? widthConstraint : CdsBreakpoints.Lg;
        var (columns, _) = Plan(width);
        var text = ((IView)Text).Measure(Math.Max(0, columns.TextWidth - columns.TextEndPadding), double.PositiveInfinity);
        var art = ((IView)Art).Measure(columns.ArtWidth, double.PositiveInfinity);
        var height = columns.Stacked ? text.Height + art.Height : Math.Max(text.Height, art.Height);
        return new Size(width, height + 2 * FullPageErrorModel.VerticalPadding);
    }

    Size ArrangeParts(Rect bounds)
    {
        var (columns, margin) = Plan(bounds.Width);
        var x = bounds.X + margin;
        var y = bounds.Y + FullPageErrorModel.VerticalPadding;
        var textHeight = ((IView)Text).DesiredSize.Height;
        ((IView)Text).Arrange(new Rect(x, y, Math.Max(0, columns.TextWidth - columns.TextEndPadding), textHeight));
        var artY = columns.Stacked ? y + textHeight : y;
        ((IView)Art).Arrange(new Rect(x + columns.ArtX, artY, columns.ArtWidth, ((IView)Art).DesiredSize.Height));
        return bounds.Size;
    }

    (FullPageErrorColumns Columns, double Margin) Plan(double width)
    {
        var breakpoint = BreakpointFor(width);
        var margin = FullPageErrorModel.HorizontalMargin(breakpoint);
        return (FullPageErrorModel.Columns(breakpoint, width - 2 * margin), margin);
    }

    sealed class Manager(FullPageErrorLayout layout) : Microsoft.Maui.Layouts.ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint) => layout.MeasureParts(widthConstraint);

        public Size ArrangeChildren(Rect bounds) => layout.ArrangeParts(bounds);
    }
}
