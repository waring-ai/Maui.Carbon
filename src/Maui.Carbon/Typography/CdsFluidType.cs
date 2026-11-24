namespace Maui.Carbon;

/// <summary>
/// The breakpoint steps of Carbon's fluid type styles (<c>fluid-heading-03..06</c>, <c>fluid-paragraph-01</c>,
/// <c>fluid-quotation-01/02</c>, <c>fluid-display-01..04</c>). Each style's base (sm) value is its token in
/// <see cref="CdsTypeSpec.Of(CdsTypeStyle)"/>; from a breakpoint up it steps to a larger size (display-04 reaches 144px at
/// <c>max</c>). A step holds until the next one, like Carbon's media queries. <see cref="CdsText"/> applies the step of
/// the page breakpoint (<see cref="CdsContextKeys.Breakpoint"/>) by itself; the keyed Label styles cannot follow it and stay
/// at the base size.
/// </summary>
public static class CdsFluidType
{
    // Sizes from the type docs (design/carbon/tokens.json usage and README, Visual foundations, Type). The intermediate
    // display-04 sizes, the line height that goes with each size and the display tracking are Carbon's @carbon/type
    // fluid sets.
    readonly record struct Step(CdsBreakpoint From, double FontSize, double LineHeight, double? CharacterSpacing = null);

    static readonly Step[] Heading03 = [new(CdsBreakpoint.Max, 24, 1.334)];
    static readonly Step[] Heading04 = [new(CdsBreakpoint.Xlg, 32, 1.25)];
    static readonly Step[] Heading0506 =
    [
        new(CdsBreakpoint.Md, 36, 1.22),
        new(CdsBreakpoint.Lg, 42, 1.19),
        new(CdsBreakpoint.Xlg, 48, 1.17),
        new(CdsBreakpoint.Max, 60, 1.17),
    ];
    static readonly Step[] Paragraph01 = [new(CdsBreakpoint.Lg, 28, 1.28572), new(CdsBreakpoint.Max, 32, 1.25)];
    static readonly Step[] Quotation01 =
    [
        new(CdsBreakpoint.Lg, 24, 1.334),
        new(CdsBreakpoint.Xlg, 28, 1.28572),
        new(CdsBreakpoint.Max, 32, 1.25),
    ];
    static readonly Step[] Display0102 =
    [
        new(CdsBreakpoint.Lg, 54, 1.17),
        new(CdsBreakpoint.Xlg, 60, 1.17),
        new(CdsBreakpoint.Max, 84, 1.11),
    ];
    static readonly Step[] Display03 =
    [
        new(CdsBreakpoint.Md, 54, 1.18),
        new(CdsBreakpoint.Lg, 60, 1.16, -0.64),
        new(CdsBreakpoint.Xlg, 76, 1.13, -0.64),
        new(CdsBreakpoint.Max, 92, 1.11, -0.96),
    ];
    static readonly Step[] Display04 =
    [
        new(CdsBreakpoint.Md, 68, 1.15),
        new(CdsBreakpoint.Lg, 92, 1.11, -0.64),
        new(CdsBreakpoint.Xlg, 122, 1.07, -0.64),
        new(CdsBreakpoint.Max, 144, 1.05, -0.96),
    ];

    /// <summary>True for the fluid styles, whose size steps up at breakpoints.</summary>
    public static bool IsFluid(CdsTypeStyle style) => Steps(style).Length > 0;

    /// <summary>
    /// The metrics of <paramref name="style"/> at <paramref name="breakpoint"/>: the base token for every non-fluid style
    /// and at <c>sm</c>, else the last step at or below the breakpoint (font, weight and token name stay the base ones).
    /// </summary>
    public static CdsTypeSpec Resolve(CdsTypeStyle style, CdsBreakpoint breakpoint)
    {
        var spec = CdsTypeSpecs.Get(style);
        foreach (var step in Steps(style))
        {
            if (step.From > breakpoint) break;
            spec = spec with
            {
                FontSize = step.FontSize,
                LineHeight = step.LineHeight,
                CharacterSpacing = step.CharacterSpacing ?? spec.CharacterSpacing,
            };
        }
        return spec;
    }

    static Step[] Steps(CdsTypeStyle style) => style switch
    {
        CdsTypeStyle.FluidHeading03 => Heading03,
        CdsTypeStyle.FluidHeading04 => Heading04,
        CdsTypeStyle.FluidHeading05 or CdsTypeStyle.FluidHeading06 => Heading0506,
        CdsTypeStyle.FluidParagraph01 => Paragraph01,
        CdsTypeStyle.FluidQuotation01 => Quotation01,
        CdsTypeStyle.FluidQuotation02 => Heading0506,
        CdsTypeStyle.FluidDisplay01 or CdsTypeStyle.FluidDisplay02 => Display0102,
        CdsTypeStyle.FluidDisplay03 => Display03,
        CdsTypeStyle.FluidDisplay04 => Display04,
        _ => [],
    };
}
