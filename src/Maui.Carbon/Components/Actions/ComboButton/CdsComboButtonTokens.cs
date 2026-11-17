namespace Maui.Carbon;

/// <summary>Measurements and token logic of <see cref="CdsComboButton"/>.</summary>
public static class CdsComboButtonTokens
{
    /// <summary>Minimum total width of both halves: 10rem.</summary>
    public const double MinTotalWidth = 160;

    /// <summary>The gap between the halves: 1px of page background.</summary>
    public const double Gap = 1;

    /// <summary>Minimum width of the primary action: 111px (6.9375rem).</summary>
    public const double PrimaryMinWidth = 111;

    /// <summary>Maximum width of the primary action: 239px (14.9375rem). Longer labels truncate.</summary>
    public const double PrimaryMaxWidth = 239;

    /// <summary>The height of both halves and the width of the chevron half: sm 32, md 40, lg 48 (default); xs 24 also renders.</summary>
    public static double Side(CdsSize size) => (size > CdsSize.Lg ? CdsSize.Lg : size).Height();

    /// <summary>The minimum width of the primary action so both halves together reach 10rem: <c>160 - 1 - side</c>, at least 111.</summary>
    public static double PrimaryMinimumWidth(CdsSize size) => Math.Max(PrimaryMinWidth, MinTotalWidth - Gap - Side(size));

    /// <summary>Both halves are filled primary buttons: resolves the colors of either half.</summary>
    public static CdsButtonVisuals Resolve(CdsInteractionState state) => CdsButtonTokens.Resolve(CdsButtonKind.Primary, state);
}
