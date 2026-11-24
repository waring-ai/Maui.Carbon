namespace Maui.Carbon;

/// <summary>
/// Carbon's notification widths, which follow the viewport (the page width the host publishes), not the container
/// (pure, tested). A viewport of NaN means no page host: the medium-breakpoint sizes apply.
/// </summary>
internal static class NotificationWidthModel
{
    /// <summary>The narrowest inline actionable notification or callout, and the width of a toast below the max breakpoint (18rem).</summary>
    public const double Narrow = 288;

    /// <summary>The cap of an inline notification, actionable notification or callout from the md breakpoint (38rem).</summary>
    public const double Medium = 608;

    /// <summary>
    /// Toast width: 288, or 352 (22rem) from the max breakpoint (1584 up).
    /// </summary>
    public static double Toast(double viewport) => viewport >= CdsBreakpoints.Max ? CdsToastNotification.WideToastWidth : Narrow;

    /// <summary>
    /// Cap of an inline actionable notification or callout: 288 below md (672), then 608, 736 from lg (1056) and 832 from
    /// max (1584).
    /// </summary>
    public static double Inline(double viewport) => viewport switch
    {
        double.NaN => Medium,
        < CdsBreakpoints.Md => Narrow,
        < CdsBreakpoints.Lg => Medium,
        < CdsBreakpoints.Max => 736,
        _ => 832,
    };

    /// <summary>
    /// Whether an inline actionable notification puts its action on the same row as the text: from the md breakpoint (672
    /// up). Without a page host, when the container gives it its full medium width (or unbounded width).
    /// </summary>
    public static bool IsSingleRow(double viewport, double widthConstraint) =>
        double.IsNaN(viewport)
            ? !double.IsFinite(widthConstraint) || widthConstraint >= Medium - 0.5
            : viewport >= CdsBreakpoints.Md;
}
