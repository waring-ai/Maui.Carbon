namespace Maui.Carbon;

/// <summary>
/// Carbon's fluid spacing scale applied: <c>fluid-spacing-01</c> to <c>-04</c> (0, 2, 5 and 10 percent of the page width,
/// the web's <c>vw</c>) as steps 1 to 4. Use it between sections of expressive pages, never inside components: the space
/// grows and shrinks with the window. The page width comes from <see cref="CdsContextKeys.ViewportWidth"/>, which every
/// page host publishes; without it a fluid space is 0. For a gap between stacked sections use
/// <see cref="CdsStack.FluidGap"/>; for space around a view, the attached <see cref="PaddingProperty">Padding</see> and
/// <see cref="MarginProperty">Margin</see>, whose thickness values are steps
/// (<c>cds:CdsFluidSpacing.Padding="3,0"</c>: <c>fluid-spacing-03</c> left and right).
/// </summary>
public static partial class CdsFluidSpacing
{
    /// <summary>
    /// Attached: the padding of a layout, content view, border or page as fluid steps per side (0 or 1: none, 2 to 4:
    /// <c>fluid-spacing-02</c> to <c>-04</c>). It owns the view's <c>Padding</c> while set.
    /// </summary>
    public static readonly BindableProperty PaddingProperty = BindableProperty.CreateAttached(
        "Padding", typeof(Thickness), typeof(CdsFluidSpacing), Thickness.Zero,
        propertyChanged: static (b, _, _) => OnStepsChanged(b));

    /// <summary>
    /// Attached: the margin of a view as fluid steps per side (0 or 1: none, 2 to 4: <c>fluid-spacing-02</c> to
    /// <c>-04</c>). It owns the view's <c>Margin</c> while set.
    /// </summary>
    public static readonly BindableProperty MarginProperty = BindableProperty.CreateAttached(
        "Margin", typeof(Thickness), typeof(CdsFluidSpacing), Thickness.Zero,
        propertyChanged: static (b, _, _) => OnStepsChanged(b));

    // The page width, bound to CdsContextKeys.ViewportWidth once a view takes fluid padding or margin.
    static readonly BindableProperty ViewportWidthProperty = BindableProperty.CreateAttached(
        "CdsFluidViewportWidth", typeof(double), typeof(CdsFluidSpacing), double.NaN,
        propertyChanged: static (b, _, _) => Apply(b));

    static readonly BindableProperty TracksViewportProperty = BindableProperty.CreateAttached(
        "CdsFluidTracksViewport", typeof(bool), typeof(CdsFluidSpacing), false);

    /// <summary>Gets <see cref="PaddingProperty"/>.</summary>
    public static Thickness GetPadding(BindableObject view) => (Thickness)view.GetValue(PaddingProperty);
    /// <summary>Sets <see cref="PaddingProperty"/>.</summary>
    public static void SetPadding(BindableObject view, Thickness value) => view.SetValue(PaddingProperty, value);
    /// <summary>Gets <see cref="MarginProperty"/>.</summary>
    public static Thickness GetMargin(BindableObject view) => (Thickness)view.GetValue(MarginProperty);
    /// <summary>Sets <see cref="MarginProperty"/>.</summary>
    public static void SetMargin(BindableObject view, Thickness value) => view.SetValue(MarginProperty, value);

    /// <summary>The fraction of the page width a step stands for: 1 is 0, 2 is 0.02, 3 is 0.05, 4 is 0.1 (0 and steps out of range are clamped).</summary>
    public static double Fraction(int step) => Math.Clamp(step, 0, 4) switch
    {
        2 => Spacing02,
        3 => Spacing03,
        4 => Spacing04,
        _ => Spacing01,
    };

    /// <summary>The space of a step on a page <paramref name="viewportWidth"/> wide; 0 when the width is unknown.</summary>
    public static double Resolve(int step, double viewportWidth) =>
        double.IsFinite(viewportWidth) && viewportWidth > 0 ? Fraction(step) * viewportWidth : 0;

    /// <summary>A thickness whose sides are steps, resolved on a page <paramref name="viewportWidth"/> wide.</summary>
    public static Thickness Resolve(Thickness steps, double viewportWidth) => new(
        Resolve(Step(steps.Left), viewportWidth), Resolve(Step(steps.Top), viewportWidth),
        Resolve(Step(steps.Right), viewportWidth), Resolve(Step(steps.Bottom), viewportWidth));

    static int Step(double value) => (int)Math.Round(value);

    static void OnStepsChanged(BindableObject target)
    {
        if (target is Element element && !(bool)element.GetValue(TracksViewportProperty))
        {
            element.SetValue(TracksViewportProperty, true);
            element.SetDynamicResource(ViewportWidthProperty, CdsContextKeys.ViewportWidth);
        }
        Apply(target);
    }

    static void Apply(BindableObject target)
    {
        var width = (double)target.GetValue(ViewportWidthProperty);
        if (target.IsSet(PaddingProperty))
        {
            var padding = Resolve(GetPadding(target), width);
            switch (target)
            {
                case Layout layout: layout.Padding = padding; break;
                case ContentView content: content.Padding = padding; break;
                case Border border: border.Padding = padding; break;
                case Page page: page.Padding = padding; break;
            }
        }
        if (target.IsSet(MarginProperty) && target is View view) view.Margin = Resolve(GetMargin(target), width);
    }
}
