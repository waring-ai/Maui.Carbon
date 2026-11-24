namespace Maui.Carbon;

/// <summary>The width steps of a modal or dialog (a share of the viewport that shrinks as the window grows).</summary>
public enum CdsModalSize
{
    /// <summary>xs: 48 / 32 / 24% of the viewport (short confirmations).</summary>
    Xs,
    /// <summary>sm: 60 / 42 / 36% (short messages).</summary>
    Sm,
    /// <summary>md: 84 / 60 / 48% (most dialogs). The default.</summary>
    Md,
    /// <summary>lg: 96 / 84 / 72% (complex content such as a table).</summary>
    Lg,
}

/// <summary>
/// Pure sizing rules of modals and dialogs, taken from <c>packages/styles/scss/components/modal</c> and <c>dialog</c>:
/// width and maximum height as a share of the viewport at the 42rem / 66rem / 82rem breakpoints, the footer button
/// columns and the text measures.
/// </summary>
public static class CdsModalSizing
{
    /// <summary>42rem: below this the modal is full width.</summary>
    public const double BreakpointMd = 672;

    /// <summary>66rem.</summary>
    public const double BreakpointLg = 1056;

    /// <summary>82rem.</summary>
    public const double BreakpointXlg = 1312;

    /// <summary>Height of the footer and of its buttons (4rem).</summary>
    public const double FooterHeight = 64;

    /// <summary>Size of the close button (3rem).</summary>
    public const double CloseButtonSize = 48;

    /// <summary>Height of the fade at the bottom of a scrolling body (80px: a 32px ramp over 48px of solid).</summary>
    public const double FadeHeight = 80;

    /// <summary>The share (0-1) of the viewport width a modal container takes.</summary>
    public static double WidthFraction(CdsModalSize size, double viewportWidth)
    {
        if (viewportWidth < BreakpointMd) return 1;
        var step = viewportWidth >= BreakpointXlg ? 2 : viewportWidth >= BreakpointLg ? 1 : 0;
        return size switch
        {
            CdsModalSize.Xs => step switch { 0 => 0.48, 1 => 0.32, _ => 0.24 },
            CdsModalSize.Sm => step switch { 0 => 0.60, 1 => 0.42, _ => 0.36 },
            CdsModalSize.Lg => step switch { 0 => 0.96, 1 => 0.84, _ => 0.72 },
            _ => step switch { 0 => 0.84, 1 => 0.60, _ => 0.48 },
        };
    }

    /// <summary>The share (0-1) of the viewport height a modal container may take.</summary>
    public static double MaxHeightFraction(CdsModalSize size, double viewportWidth)
    {
        if (viewportWidth < BreakpointMd) return 1;
        if (viewportWidth < BreakpointLg) return 0.90;
        return size switch
        {
            CdsModalSize.Xs => 0.48,
            CdsModalSize.Sm => 0.72,
            CdsModalSize.Lg => 0.96,
            _ => 0.84,
        };
    }

    /// <summary>The width of a <see cref="CdsModal"/> container.</summary>
    public static double ModalWidth(CdsModalSize size, double viewportWidth) => WidthFraction(size, viewportWidth) * viewportWidth;

    /// <summary>The maximum height of a <see cref="CdsModal"/> container.</summary>
    public static double ModalMaxHeight(CdsModalSize size, double viewportWidth, double viewportHeight) =>
        MaxHeightFraction(size, viewportWidth) * viewportHeight;

    /// <summary>The width of a modal <see cref="CdsDialog"/>: 48rem at most (lg: the full share), 32 less than the viewport when narrow.</summary>
    public static double DialogWidth(CdsModalSize size, double viewportWidth)
    {
        if (viewportWidth < BreakpointMd) return Math.Max(0, viewportWidth - 32);
        var share = WidthFraction(size, viewportWidth) * viewportWidth;
        return size == CdsModalSize.Lg ? share : Math.Min(768, share);
    }

    /// <summary>The width of a non-modal (inline) dialog: 24, 36, 48 or 72rem.</summary>
    public static double DialogInlineWidth(CdsModalSize size) => size switch
    {
        CdsModalSize.Xs => 384,
        CdsModalSize.Sm => 576,
        CdsModalSize.Lg => 1152,
        _ => 768,
    };

    /// <summary>The maximum height of a dialog: 100% below 42rem, then 90vh and 84vh from 66rem.</summary>
    public static double DialogMaxHeight(double viewportWidth, double viewportHeight) =>
        (viewportWidth < BreakpointMd ? 1 : viewportWidth < BreakpointLg ? 0.90 : 0.84) * viewportHeight;

    /// <summary>
    /// Extra end padding of the heading so it wraps at about 80% of the header: <c>calc(20% - 3rem)</c> of the header's
    /// content width (the header width minus its 16 + 48 padding).
    /// </summary>
    public static double HeadingEndPadding(double headerWidth) => Math.Max(0, 0.2 * Math.Max(0, headerWidth - 64) - 48);

    /// <summary>Extra end padding of a paragraph body: <c>calc(20% - 2rem)</c> of the body's content width.</summary>
    public static double ParagraphEndPadding(double bodyWidth) => Math.Max(0, 0.2 * Math.Max(0, bodyWidth - 32) - 32);

    /// <summary>True when the bottom fade is drawn: the content overflows its viewport or scrolling is forced.</summary>
    public static bool ShowsFade(double contentHeight, double viewportHeight, bool forced) =>
        forced || (viewportHeight > 0 && contentHeight > viewportHeight + 0.5);

    /// <summary>
    /// The footer's star columns for a number of buttons: buttons are 50% wide with one button, fill the footer with two
    /// and take 25% each with three. <c>Spacer</c> is the empty start column (0 = none), <c>Button</c> each button's width.
    /// </summary>
    public static (double Spacer, double Button) FooterColumns(int buttonCount) => buttonCount switch
    {
        <= 0 => (1, 0),
        1 => (1, 1),
        2 => (0, 1),
        _ => (1, 1),
    };
}

/// <summary>Maps modal parts and states to Carbon tokens.</summary>
public static class CdsModalTokens
{
    /// <summary>Fill of the 48px close button: transparent, <c>layer-hover-01</c> on hover, <c>layer-active-01</c> pressed; transparent while disabled.</summary>
    public static string? CloseButtonBackground(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Disabled) ? null
        : state.HasFlag(CdsInteractionState.Pressed) ? CdsColorKeys.LayerActive01
        : state.HasFlag(CdsInteractionState.Hover) ? CdsColorKeys.LayerHover01
        : null;

    /// <summary>Close icon color.</summary>
    public static string CloseButtonIcon(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Disabled) ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>The kind of the primary footer button.</summary>
    public static CdsButtonKind PrimaryKind(bool danger) => danger ? CdsButtonKind.Danger : CdsButtonKind.Primary;

    /// <summary>The 1px border of the container (<c>border-subtle-01</c>).</summary>
    public static string ContainerBorder => CdsColorKeys.BorderSubtle01;

    /// <summary>The container fill (<c>layer-01</c>).</summary>
    public static string ContainerFill => CdsColorKeys.Layer01;
}
