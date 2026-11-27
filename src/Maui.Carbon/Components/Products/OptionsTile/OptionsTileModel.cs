namespace Maui.Carbon;

/// <summary>The pure rules of <see cref="CdsOptionsTile"/> (Carbon for IBM Products' <c>OptionsTile.tsx</c>).</summary>
internal static class OptionsTileModel
{
    /// <summary>The validation state shown in the summary line: invalid wins over warning, warning over locked.</summary>
    public static CdsOptionsTileStatus Status(bool invalid, bool warn, bool locked) =>
        invalid ? CdsOptionsTileStatus.Invalid
        : warn ? CdsOptionsTileStatus.Warning
        : locked ? CdsOptionsTileStatus.Locked
        : CdsOptionsTileStatus.None;

    /// <summary>The toggle is disabled (and the locked note shown in the content) only while locked and neither invalid nor warning.</summary>
    public static bool IsToggleLocked(bool invalid, bool warn, bool locked) => !invalid && !warn && locked;

    /// <summary>
    /// The text of the summary line: the invalid or warning text in those states; when locked, the locked text on a static
    /// tile and on an expandable tile only when it has no summary (it keeps the summary and shows the locked note inside);
    /// otherwise the summary.
    /// </summary>
    public static string? SummaryText(CdsOptionsTileStatus status, bool expandable, string? summary, string? invalidText, string? warnText, string? lockedText) => status switch
    {
        CdsOptionsTileStatus.Invalid => invalidText,
        CdsOptionsTileStatus.Warning => warnText,
        CdsOptionsTileStatus.Locked => !expandable || string.IsNullOrEmpty(summary) ? lockedText : summary,
        _ => summary,
    };

    /// <summary>The summary line hides while the tile is open, unless it shows a validation state.</summary>
    public static bool IsSummaryHidden(CdsOptionsTileStatus status, bool open) => status == CdsOptionsTileStatus.None && open;

    /// <summary>Only lg (48, default) and xl (64) exist: smaller sizes read as lg, larger as xl.</summary>
    public static CdsSize Normalize(CdsSize size) => size >= CdsSize.Xl ? CdsSize.Xl : CdsSize.Lg;

    /// <summary>Header height: 48 at lg, 64 at xl.</summary>
    public static double HeaderHeight(CdsSize size) => Normalize(size) == CdsSize.Xl ? CdsSpacing.Spacing10 : CdsSpacing.Spacing09;

    /// <summary>Gap between the title and the summary: 2 at lg, 4 at xl.</summary>
    public static double SummaryGap(CdsSize size) => Normalize(size) == CdsSize.Xl ? CdsSpacing.Spacing02 : CdsSpacing.Spacing01;

    /// <summary>Width of the toggle column at the end: 16 + the 32px small toggle + 16.</summary>
    public const double ToggleColumnWidth = CdsSpacing.Spacing05 + CdsSpacing.Spacing07 + CdsSpacing.Spacing05;

    /// <summary>Width of the chevron column of an expandable tile (3rem).</summary>
    public const double ChevronColumnWidth = CdsSpacing.Spacing09;

    /// <summary>The open content's padding: 48 at the start (under the title), 16 at the end, 8 above and 24 below.</summary>
    public static Thickness ContentPadding => new(CdsSpacing.Spacing09, CdsSpacing.Spacing03, CdsSpacing.Spacing05, CdsSpacing.Spacing06);

    /// <summary>Height at <paramref name="progress"/> (clamped to 0..1) between two heights.</summary>
    public static double HeightAt(double from, double to, double progress) => from + (to - from) * Math.Clamp(progress, 0, 1);
}
