namespace Maui.Carbon;

/// <summary>
/// Which variant of a Carbon for IBM Products illustration to draw (<c>illustrationTheme</c>). The artwork has fixed
/// colors, so it comes in a light and a dark version.
/// </summary>
public enum CdsIllustrationTheme
{
    /// <summary>Follows the theme zone: dark in g90 and g100, light in white and g10 (the default).</summary>
    Auto,
    /// <summary>The light variant, for white and g10.</summary>
    Light,
    /// <summary>The dark variant, for g90 and g100.</summary>
    Dark,
}
