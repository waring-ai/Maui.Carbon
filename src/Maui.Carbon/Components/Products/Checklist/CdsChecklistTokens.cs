namespace Maui.Carbon;

/// <summary>Color keys of a checklist link (a task with an action, or "View all").</summary>
/// <param name="Foreground">Text color.</param>
/// <param name="Underline">Whether the text is underlined.</param>
public readonly record struct CdsChecklistLinkVisuals(string Foreground, bool Underline);

/// <summary>Maps checklist task kinds and link states to tokens (from <c>ibm-products-styles/src/components/Checklist/_checklist.scss</c>).</summary>
public static class CdsChecklistTokens
{
    /// <summary>The progress chart's track: <c>layer-accent-01</c> (Carbon draws Gray 20 in light themes, Gray 70 in dark ones).</summary>
    public const string ChartTrack = CdsColorKeys.LayerAccent01;

    /// <summary>
    /// The progress chart's arc. Carbon draws Purple 50 in every theme; <c>data-vis-14</c> is Purple 50 in the light themes
    /// and Purple 30 in the dark ones.
    /// </summary>
    public const string ChartProgress = CdsColorKeys.DataVis14;

    /// <summary>The header, list and footer surface.</summary>
    public const string Background = CdsColorKeys.Layer01;

    /// <summary>The 1px rules under the header and between groups.</summary>
    public const string Rule = CdsColorKeys.BorderSubtle01;

    /// <summary>The status glyph of a kind.</summary>
    public static CdsIconName Icon(CdsChecklistTaskKind kind) => kind switch
    {
        CdsChecklistTaskKind.Error => CdsIconName.Warning,
        CdsChecklistTaskKind.Indeterminate => CdsIconName.Incomplete,
        CdsChecklistTaskKind.Checked => CdsIconName.CheckmarkOutline,
        _ => CdsIconName.CircleDash,
    };

    /// <summary>The status glyph's color: <c>interactive</c> for checked and in progress, <c>support-error</c>, <c>icon-disabled</c>, else <c>icon-primary</c>.</summary>
    public static string IconColor(CdsChecklistTaskKind kind) => kind switch
    {
        CdsChecklistTaskKind.Checked or CdsChecklistTaskKind.Indeterminate => CdsColorKeys.Interactive,
        CdsChecklistTaskKind.Error => CdsColorKeys.SupportError,
        CdsChecklistTaskKind.Disabled => CdsColorKeys.IconDisabled,
        _ => CdsColorKeys.IconPrimary,
    };

    /// <summary>The color of a plain (not clickable) task label: <c>text-disabled</c> for disabled tasks, else <c>text-primary</c>.</summary>
    public static string LabelColor(CdsChecklistTaskKind kind) =>
        kind == CdsChecklistTaskKind.Disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>A link: <c>link-primary</c>; hover <c>link-primary-hover</c> underlined; pressed <c>text-primary</c> underlined; disabled <c>text-disabled</c>.</summary>
    public static CdsChecklistLinkVisuals Link(CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Disabled)) return new(CdsColorKeys.TextDisabled, false);
        if (state.HasFlag(CdsInteractionState.Pressed)) return new(CdsColorKeys.TextPrimary, true);
        if (state.HasFlag(CdsInteractionState.Hover)) return new(CdsColorKeys.LinkPrimaryHover, true);
        return new(CdsColorKeys.LinkPrimary, false);
    }

    /// <summary>The default spoken status of a kind.</summary>
    public static string StatusText(CdsChecklistTaskKind kind) => kind switch
    {
        CdsChecklistTaskKind.Indeterminate => "In progress",
        CdsChecklistTaskKind.Checked => "Complete",
        CdsChecklistTaskKind.Disabled => "Not available",
        CdsChecklistTaskKind.Error => "Error",
        _ => "Not started",
    };
}
