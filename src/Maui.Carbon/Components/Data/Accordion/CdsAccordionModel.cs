namespace Maui.Carbon;

/// <summary>Where the chevron of an accordion heading sits.</summary>
public enum CdsAccordionAlign
{
    /// <summary>At the end edge (default): titles line up with the rest of the page.</summary>
    End,
    /// <summary>Before the title: for tree-like content.</summary>
    Start,
}

/// <summary>Measurements of the accordion (from <c>packages/styles/scss/components/accordion</c>).</summary>
public static class CdsAccordionMetrics
{
    /// <summary>Minimum height of a heading: sm 32, md 40 (default), lg 48. Other sizes use md.</summary>
    public static double HeadingHeight(CdsSize size) => size switch
    {
        CdsSize.Sm or CdsSize.Xs => 32,
        CdsSize.Lg or CdsSize.Xl or CdsSize.Xxl => 48,
        _ => 40,
    };

    /// <summary>End padding of an open panel's content: 16, 48 from 480 wide, 25% from 640 wide.</summary>
    public static double ContentEndPadding(double panelWidth) =>
        panelWidth >= 640 ? panelWidth * 0.25 : panelWidth >= 480 ? 48 : CdsSpacing.Spacing05;

    /// <summary>Start inset of the panel content: 16, or 32 more when the chevron is at the start.</summary>
    public static double ContentStartPadding(CdsAccordionAlign align) =>
        align == CdsAccordionAlign.Start ? CdsSpacing.Spacing05 + CdsSpacing.Spacing05 + CdsSpacing.Spacing05 : CdsSpacing.Spacing05;

    /// <summary>Rotation of the chevron: pointing down when closed (0), up when open (180).</summary>
    public static double ChevronRotation(bool open) => open ? 180 : 0;
}

/// <summary>Maps accordion parts and states to Carbon tokens.</summary>
public static class CdsAccordionTokens
{
    /// <summary>Heading fill: transparent, <c>layer-hover-01</c> on hover (not when disabled).</summary>
    public static string? Heading(bool hover, bool disabled) => hover && !disabled ? CdsColorKeys.LayerHover01 : null;

    /// <summary>Title text color.</summary>
    public static string Title(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>Chevron color.</summary>
    public static string Chevron(bool disabled) => disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>
    /// A divider: <c>border-subtle-01</c>, tinted <c>layer-hover-01</c> when the item above or below it is hovered while
    /// closed and enabled.
    /// </summary>
    public static string Divider(bool tinted) => tinted ? CdsColorKeys.LayerHover01 : CdsColorKeys.BorderSubtle01;

    /// <summary>Panel text color.</summary>
    public static string Content => CdsColorKeys.TextPrimary;
}
