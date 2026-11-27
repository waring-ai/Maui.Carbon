namespace Maui.Carbon;

/// <summary>
/// Maps a <see cref="CdsStatusIconKind"/> to its glyph and color tokens. Carbon for IBM Products paints status icons
/// from the IBM palette per light or dark theme; here each kind takes the <c>status-*</c> token of the same hue, which
/// follows the theme scope the icon sits in.
/// </summary>
public static class CdsStatusIconTokens
{
    /// <summary>Milliseconds per turn of the in-progress glyph (linear, endless).</summary>
    public const uint SpinDuration = 8000;

    /// <summary>The color key of a kind's glyph.</summary>
    public static string Color(CdsStatusIconKind kind) => kind switch
    {
        CdsStatusIconKind.Fatal => CdsColorKeys.IconPrimary,
        CdsStatusIconKind.Critical => CdsColorKeys.StatusRed,
        CdsStatusIconKind.MajorWarning => CdsColorKeys.StatusOrange,
        CdsStatusIconKind.MinorWarning => CdsColorKeys.StatusYellow,
        CdsStatusIconKind.Undefined => CdsColorKeys.StatusPurple,
        CdsStatusIconKind.Unknown or CdsStatusIconKind.Pending => CdsColorKeys.StatusGray,
        CdsStatusIconKind.Normal or CdsStatusIconKind.Running => CdsColorKeys.StatusGreen,
        _ => CdsColorKeys.StatusBlue,
    };

    /// <summary>
    /// The color key of the "!" of the warning triangles (Carbon for IBM Products paints it gray-100 so it stays dark on
    /// orange and yellow): <c>icon-primary</c> in light themes, <c>icon-inverse</c> in dark ones. <c>null</c> for the
    /// other kinds, whose marks are cut out.
    /// </summary>
    public static string? InnerColor(CdsStatusIconKind kind, bool darkTheme) =>
        kind is CdsStatusIconKind.MajorWarning or CdsStatusIconKind.MinorWarning
            ? (darkTheme ? CdsColorKeys.IconInverse : CdsColorKeys.IconPrimary)
            : null;

    /// <summary>
    /// The bundled icon of a kind, or <c>null</c> for <see cref="CdsStatusIconKind.MajorWarning"/>, which is drawn as the
    /// inverted status triangle (the icon set has no <c>warning--alt-inverted--filled</c>). <c>misuse</c> (fatal) has the
    /// same shape as <c>error--filled</c>; <c>undefined--filled</c>, <c>unknown--filled</c> and <c>pending</c> fall back
    /// to <c>undefined</c>, <c>unknown</c> and <c>pending--filled</c>.
    /// </summary>
    public static CdsIconName? Icon(CdsStatusIconKind kind) => kind switch
    {
        CdsStatusIconKind.Fatal or CdsStatusIconKind.Critical => CdsIconName.ErrorFilled,
        CdsStatusIconKind.MajorWarning => null,
        CdsStatusIconKind.MinorWarning => CdsIconName.WarningAltFilled,
        CdsStatusIconKind.Undefined => CdsIconName.Undefined,
        CdsStatusIconKind.Unknown => CdsIconName.Unknown,
        CdsStatusIconKind.Normal => CdsIconName.CheckmarkFilled,
        CdsStatusIconKind.Info => CdsIconName.InformationSquareFilled,
        CdsStatusIconKind.InProgress => CdsIconName.InProgress,
        CdsStatusIconKind.Running => CdsIconName.CheckmarkOutline,
        _ => CdsIconName.PendingFilled,
    };

    /// <summary>Glyph size of a size: sm 16 (and xs), md 20, lg 24, xl 32 (and 2xl).</summary>
    public static double GlyphSize(CdsSize size) => size switch
    {
        CdsSize.Md => 20,
        CdsSize.Lg => 24,
        CdsSize.Xl or CdsSize.Xxl => 32,
        _ => 16,
    };

    /// <summary>Whether a kind animates (the in-progress glyph turns).</summary>
    public static bool Spins(CdsStatusIconKind kind) => kind == CdsStatusIconKind.InProgress;
}
