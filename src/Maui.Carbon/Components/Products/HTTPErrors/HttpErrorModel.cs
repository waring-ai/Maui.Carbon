namespace Maui.Carbon;

/// <summary>HTTP error layout rules (pure, tested), from <c>ibm-products-styles/.../HTTPErrors/_http-errors.scss</c>.</summary>
internal static class HttpErrorModel
{
    /// <summary>Space below the error code label (<c>spacing-02</c>).</summary>
    internal const double LabelGap = CdsSpacing.Spacing02;

    /// <summary>Space below the title (<c>spacing-04</c>).</summary>
    internal const double TitleGap = CdsSpacing.Spacing04;

    /// <summary>Space below the description (<c>spacing-06</c>).</summary>
    internal const double DescriptionGap = CdsSpacing.Spacing06;

    /// <summary>Space below each link (<c>spacing-02</c>).</summary>
    internal const double LinkGap = CdsSpacing.Spacing02;

    /// <summary>
    /// The top of the text block: its center sits at the middle of the area, then moves up by a quarter of its height
    /// below md (<c>translate(-50%, -75%)</c>), by half at md (<c>-100%</c>) and not at all from lg (<c>-50%</c>).
    /// </summary>
    internal static double ContentTop(CdsBreakpoint breakpoint, double areaHeight, double contentHeight)
    {
        var shift = breakpoint switch
        {
            CdsBreakpoint.Sm => 0.75,
            CdsBreakpoint.Md => 1.0,
            _ => 0.5,
        };
        return areaHeight / 2 - contentHeight * shift;
    }

    /// <summary>
    /// The illustration's frame: the whole area, or below md 1.5 times as large around its center and moved a tenth of
    /// the width toward the end (<c>inset-inline-start: 10%; transform: scale(1.5)</c>).
    /// </summary>
    internal static Rect ArtFrame(CdsBreakpoint breakpoint, Rect area)
    {
        if (breakpoint != CdsBreakpoint.Sm) return area;
        var width = area.Width * 1.5;
        var height = area.Height * 1.5;
        return new Rect(area.X + area.Width * 0.1 - (width - area.Width) / 2, area.Y - (height - area.Height) / 2, width, height);
    }

    /// <summary>The height to take when the container does not decide it: the illustration's at that width.</summary>
    internal static double NaturalHeight(IllustrationDrawing art, double width) => IllustrationModel.HeightForWidth(art, width);
}

/// <summary>Places an HTTP error's illustration behind its centered text, by breakpoint, while measuring.</summary>
internal sealed class HttpErrorLayout : Layout
{
    static readonly BindableProperty ContextBreakpointProperty = BindableProperty.Create(
        "ContextBreakpoint", typeof(CdsBreakpoint?), typeof(HttpErrorLayout), null,
        propertyChanged: static (b, _, _) => ((HttpErrorLayout)b).InvalidateMeasure());

    readonly IllustrationDrawing _drawing;

    internal HttpErrorLayout(CdsIllustrationView art, View content, IllustrationDrawing drawing)
    {
        _drawing = drawing;
        Art = art;
        Content = content;
        IsClippedToBounds = true;
        Add(art);
        Add(content);
        SetDynamicResource(ContextBreakpointProperty, CdsContextKeys.Breakpoint);
    }

    internal CdsIllustrationView Art { get; }

    internal View Content { get; }

    CdsBreakpoint BreakpointFor(double width) =>
        (CdsBreakpoint?)GetValue(ContextBreakpointProperty) ?? CdsBreakpointExtensions.FromWidth(width);

    protected override Microsoft.Maui.Layouts.ILayoutManager CreateLayoutManager() => new Manager(this);

    Size MeasureParts(double widthConstraint, double heightConstraint)
    {
        var width = double.IsFinite(widthConstraint) ? widthConstraint : CdsBreakpoints.Lg;
        var content = ((IView)Content).Measure(width, double.PositiveInfinity);
        var height = double.IsFinite(heightConstraint) ? heightConstraint : Math.Max(HttpErrorModel.NaturalHeight(_drawing, width), content.Height * 2);
        ((IView)Art).Measure(width, height);
        return new Size(width, height);
    }

    Size ArrangeParts(Rect bounds)
    {
        var breakpoint = BreakpointFor(bounds.Width);
        ((IView)Art).Arrange(HttpErrorModel.ArtFrame(breakpoint, bounds));
        var content = ((IView)Content).DesiredSize;
        var width = Math.Min(content.Width, bounds.Width);
        var top = HttpErrorModel.ContentTop(breakpoint, bounds.Height, content.Height);
        ((IView)Content).Arrange(new Rect(bounds.X + (bounds.Width - width) / 2, bounds.Y + top, width, content.Height));
        return bounds.Size;
    }

    sealed class Manager(HttpErrorLayout layout) : Microsoft.Maui.Layouts.ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint) => layout.MeasureParts(widthConstraint, heightConstraint);

        public Size ArrangeChildren(Rect bounds) => layout.ArrangeParts(bounds);
    }
}
