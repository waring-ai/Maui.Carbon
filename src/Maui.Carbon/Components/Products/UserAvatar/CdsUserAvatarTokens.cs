namespace Maui.Carbon;

/// <summary>
/// Sizes, type and colors of <see cref="CdsUserAvatar"/>. Carbon for IBM Products uses IBM palette steps (cyan-60 in light
/// themes and cyan-50 in dark ones, and so on); the theme has no tokens for them, so each order resolves to the theme
/// token closest in hue and lightness, chosen per light or dark theme. Text is <c>text-inverse</c> and icons
/// <c>icon-inverse</c> on every color.
/// </summary>
public static class CdsUserAvatarTokens
{
    /// <summary>Initials color: <c>text-inverse</c>.</summary>
    public const string Foreground = CdsColorKeys.TextInverse;

    /// <summary>Icon color: <c>icon-inverse</c>.</summary>
    public const string IconColor = CdsColorKeys.IconInverse;

    /// <summary>
    /// The background token of an order. Light themes (white, g10): orders 2, 3, 5 and 8 match gray-60, green-60, purple-60
    /// and gray-80 exactly; 1, 4, 6, 9 and 11 use the 70 step (<c>tag-color-*</c>) and 7 cyan-90; the theme has no magenta or
    /// teal deeper than 70, so 10 and 12 share the color of 4 and 6 there. Dark themes (g90, g100):
    /// orders 1 to 6 match the 50 step exactly (<c>tag-border-*</c>); 8, 9 and 11 match the 30 step; 7, 10 and 12 use the
    /// 20 step.
    /// </summary>
    public static string Background(CdsUserAvatarColor color, bool darkTheme) => darkTheme
        ? color switch
        {
            CdsUserAvatarColor.Order2Gray => CdsColorKeys.TagBorderGray,
            CdsUserAvatarColor.Order3Green => CdsColorKeys.TagBorderGreen,
            CdsUserAvatarColor.Order4Magenta => CdsColorKeys.TagBorderMagenta,
            CdsUserAvatarColor.Order5Purple => CdsColorKeys.TagBorderPurple,
            CdsUserAvatarColor.Order6Teal => CdsColorKeys.TagBorderTeal,
            CdsUserAvatarColor.Order7Cyan => CdsColorKeys.TagColorCyan,
            CdsUserAvatarColor.Order8Gray => CdsColorKeys.TextSecondary,
            CdsUserAvatarColor.Order9Green => CdsColorKeys.DataVis07,
            CdsUserAvatarColor.Order10Magenta => CdsColorKeys.TagColorMagenta,
            CdsUserAvatarColor.Order11Purple => CdsColorKeys.DataVis14,
            CdsUserAvatarColor.Order12Teal => CdsColorKeys.TagColorTeal,
            _ => CdsColorKeys.TagBorderCyan,
        }
        : color switch
        {
            CdsUserAvatarColor.Order2Gray => CdsColorKeys.StatusGray,
            CdsUserAvatarColor.Order3Green => CdsColorKeys.DataVis07,
            CdsUserAvatarColor.Order4Magenta => CdsColorKeys.TagColorMagenta,
            CdsUserAvatarColor.Order5Purple => CdsColorKeys.StatusPurple,
            CdsUserAvatarColor.Order6Teal => CdsColorKeys.TagColorTeal,
            CdsUserAvatarColor.Order7Cyan => CdsColorKeys.DataVis12,
            CdsUserAvatarColor.Order8Gray => CdsColorKeys.BackgroundInverse,
            CdsUserAvatarColor.Order9Green => CdsColorKeys.TagColorGreen,
            CdsUserAvatarColor.Order10Magenta => CdsColorKeys.TagColorMagenta,
            CdsUserAvatarColor.Order11Purple => CdsColorKeys.TagColorPurple,
            CdsUserAvatarColor.Order12Teal => CdsColorKeys.TagColorTeal,
            _ => CdsColorKeys.TagColorCyan,
        };

    /// <summary>Circle diameter: sm 24 (and xs), md 32, lg 48, xl 64 (and 2xl).</summary>
    public static double Diameter(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => CdsSpacing.Spacing06,
        CdsSize.Lg => CdsSpacing.Spacing09,
        CdsSize.Xl or CdsSize.Xxl => CdsSpacing.Spacing10,
        _ => CdsSpacing.Spacing07,
    };

    /// <summary>Initials type: sm <c>label-01</c>, md <c>body-compact-01</c>, lg <c>heading-03</c>, xl <c>heading-04</c>.</summary>
    public static CdsTypeStyle TypeStyle(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => CdsTypeStyle.Label01,
        CdsSize.Lg => CdsTypeStyle.Heading03,
        CdsSize.Xl or CdsSize.Xxl => CdsTypeStyle.Heading04,
        _ => CdsTypeStyle.BodyCompact01,
    };

    /// <summary>Icon size: sm 16, md 20, lg 24, xl 32.</summary>
    public static double IconSize(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => 16,
        CdsSize.Lg => 24,
        CdsSize.Xl or CdsSize.Xxl => 32,
        _ => 20,
    };
}
