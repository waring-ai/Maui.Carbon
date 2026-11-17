namespace Maui.Carbon;

/// <summary>The four Carbon themes. White and Gray 10 are light; Gray 90 and Gray 100 are dark.</summary>
public enum CdsThemeName
{
    /// <summary>White: the default light theme.</summary>
    White,
    /// <summary>Gray 10: a light theme with a gray page background.</summary>
    G10,
    /// <summary>Gray 90: a dark theme.</summary>
    G90,
    /// <summary>Gray 100: the darkest theme.</summary>
    G100,
}

/// <summary>Helpers for <see cref="CdsThemeName"/>.</summary>
public static class CdsThemeNameExtensions
{
    /// <summary>True for the dark themes (Gray 90 and Gray 100).</summary>
    public static bool IsDark(this CdsThemeName theme) => theme is CdsThemeName.G90 or CdsThemeName.G100;
}
