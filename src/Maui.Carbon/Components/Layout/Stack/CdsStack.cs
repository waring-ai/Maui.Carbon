namespace Maui.Carbon;

/// <summary>
/// Lays children out in one direction (<see cref="StackLayout.Orientation"/>, vertical by default) with a gap from the Carbon spacing scale: <see cref="Gap"/> 1 to 13 maps to
/// <c>spacing-01</c> to <c>spacing-13</c> (2 to 160), or set <see cref="GapValue"/> for an arbitrary gap. Use it
/// instead of margins on the children so components stay margin-free. It never wraps (use a <c>FlexLayout</c>
/// for that) and is not for two-dimensional layout (use <see cref="CdsGrid"/>). Common gaps: 3 (8) inside a group,
/// 5 (16) between related controls, 6 or 7 (24, 32) between form fields and 9 (48) between sections. Between sections
/// of an expressive page, <see cref="FluidGap"/> takes the gap from the fluid scale instead (it grows with the page).
/// </summary>
public class CdsStack : StackLayout
{
    /// <summary>Bindable property for <see cref="Gap"/>.</summary>
    public static readonly BindableProperty GapProperty = BindableProperty.Create(
        nameof(Gap), typeof(int), typeof(CdsStack), 0,
        coerceValue: static (_, v) => Math.Clamp((int)v, 0, 13),
        propertyChanged: static (b, _, _) => ((CdsStack)b).ApplyGap());

    /// <summary>Bindable property for <see cref="GapValue"/>.</summary>
    public static readonly BindableProperty GapValueProperty = BindableProperty.Create(
        nameof(GapValue), typeof(double?), typeof(CdsStack), null,
        propertyChanged: static (b, _, _) => ((CdsStack)b).ApplyGap());

    /// <summary>Bindable property for <see cref="FluidGap"/>.</summary>
    public static readonly BindableProperty FluidGapProperty = BindableProperty.Create(
        nameof(FluidGap), typeof(int?), typeof(CdsStack), null,
        coerceValue: static (_, v) => v is int step ? Math.Clamp(step, 1, 4) : null,
        propertyChanged: static (b, _, _) => ((CdsStack)b).OnFluidGapChanged());

    // The page width, bound to CdsContextKeys.ViewportWidth once FluidGap is first set.
    static readonly BindableProperty ViewportWidthProperty = BindableProperty.Create(
        "ViewportWidth", typeof(double), typeof(CdsStack), double.NaN,
        propertyChanged: static (b, _, _) => ((CdsStack)b).ApplyGap());

    bool _tracksViewport;

    /// <summary>The spacing scale step between children, 0 (none, the default) through 13.</summary>
    public int Gap
    {
        get => (int)GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    /// <summary>A raw gap in device-independent units; overrides <see cref="Gap"/> when set.</summary>
    public double? GapValue
    {
        get => (double?)GetValue(GapValueProperty);
        set => SetValue(GapValueProperty, value);
    }

    /// <summary>
    /// A fluid gap between children: step 1 to 4 is <c>fluid-spacing-01</c> to <c>-04</c> (0, 2, 5 and 10 percent of the
    /// page width, see <see cref="CdsFluidSpacing"/>). Overrides <see cref="Gap"/> when set; <see cref="GapValue"/> still
    /// wins. For sections of expressive pages only, never inside components. Default <c>null</c>.
    /// </summary>
    public int? FluidGap
    {
        get => (int?)GetValue(FluidGapProperty);
        set => SetValue(FluidGapProperty, value);
    }

    /// <summary>The gap in units for a spacing scale step: 0 for 0, otherwise <c>spacing-NN</c> (1 = 2 … 13 = 160).</summary>
    public static double GapForStep(int step) => Math.Clamp(step, 0, 13) switch
    {
        1 => CdsSpacing.Spacing01,
        2 => CdsSpacing.Spacing02,
        3 => CdsSpacing.Spacing03,
        4 => CdsSpacing.Spacing04,
        5 => CdsSpacing.Spacing05,
        6 => CdsSpacing.Spacing06,
        7 => CdsSpacing.Spacing07,
        8 => CdsSpacing.Spacing08,
        9 => CdsSpacing.Spacing09,
        10 => CdsSpacing.Spacing10,
        11 => CdsSpacing.Spacing11,
        12 => CdsSpacing.Spacing12,
        13 => CdsSpacing.Spacing13,
        _ => 0,
    };

    void OnFluidGapChanged()
    {
        if (FluidGap is not null && !_tracksViewport)
        {
            _tracksViewport = true;
            SetDynamicResource(ViewportWidthProperty, CdsContextKeys.ViewportWidth);
        }
        ApplyGap();
    }

    void ApplyGap() => Spacing = GapValue
        ?? (FluidGap is { } step ? CdsFluidSpacing.Resolve(step, (double)GetValue(ViewportWidthProperty)) : GapForStep(Gap));
}
