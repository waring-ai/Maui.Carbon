namespace Maui.Carbon;

/// <summary>
/// Measurements and type of <see cref="CdsEmptyState"/> (from <c>ibm-products-styles/.../EmptyStates/_empty-state.scss</c>).
/// </summary>
public static class CdsEmptyStateTokens
{
    /// <summary>Space below the illustration (<c>spacing-05</c>), in every position.</summary>
    public const double IllustrationGap = CdsSpacing.Spacing05;

    /// <summary>Space below the title (<c>spacing-03</c>).</summary>
    public const double TitleGap = CdsSpacing.Spacing03;

    /// <summary>Space below the subtitle (<c>spacing-05</c>).</summary>
    public const double SubtitleGap = CdsSpacing.Spacing05;

    /// <summary>Space below the action button (<c>spacing-05</c>).</summary>
    public const double ActionGap = CdsSpacing.Spacing05;

    /// <summary>The title color (<c>text-primary</c>).</summary>
    public const string TitleColor = CdsColorKeys.TextPrimary;

    /// <summary>The subtitle color (<c>text-secondary</c>).</summary>
    public const string SubtitleColor = CdsColorKeys.TextSecondary;

    /// <summary>The illustration width: 80 (<c>spacing-11</c>) at lg, 64 (<c>spacing-10</c>) at sm.</summary>
    public static double IllustrationSize(CdsEmptyStateSize size) => size == CdsEmptyStateSize.Sm ? CdsSpacing.Spacing10 : CdsSpacing.Spacing11;

    /// <summary>The title style: <c>heading-03</c> at lg, <c>body-compact-02</c> at sm.</summary>
    public static CdsTypeStyle TitleStyle(CdsEmptyStateSize size) => size == CdsEmptyStateSize.Sm ? CdsTypeStyle.BodyCompact02 : CdsTypeStyle.Heading03;

    /// <summary>The subtitle style: <c>body-01</c> at lg, <c>label-01</c> at sm.</summary>
    public static CdsTypeStyle SubtitleStyle(CdsEmptyStateSize size) => size == CdsEmptyStateSize.Sm ? CdsTypeStyle.Label01 : CdsTypeStyle.Body01;

    /// <summary>
    /// The action button kind: primary, secondary or tertiary as asked, tertiary (the default) for anything else. The
    /// button is always small.
    /// </summary>
    public static CdsButtonKind ActionKind(CdsButtonKind requested) =>
        requested is CdsButtonKind.Primary or CdsButtonKind.Secondary or CdsButtonKind.Tertiary ? requested : CdsButtonKind.Tertiary;
}
