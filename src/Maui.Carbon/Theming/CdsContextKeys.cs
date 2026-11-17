namespace Maui.Carbon;

/// <summary>
/// Resource keys that carry Carbon context down the visual tree. Theme zones, layers and fluid forms set them on their
/// own scope; descendants read them with <c>SetDynamicResource</c>, so moving an element re-resolves its context.
/// </summary>
public static class CdsContextKeys
{
    /// <summary>The effective <see cref="CdsThemeName"/> of a subtree.</summary>
    public const string ThemeId = "Cds.ThemeId";

    /// <summary>The effective layer level (1, 2 or 3) of a subtree.</summary>
    public const string LayerLevel = "Cds.LayerLevel";

    /// <summary><c>true</c> inside a fluid form: inputs render their fluid variant.</summary>
    public const string IsFluid = "Cds.IsFluid";

    /// <summary>The current <see cref="CdsBreakpoint"/> of the page, published by the page host.</summary>
    public const string Breakpoint = "Cds.Breakpoint";

    /// <summary>The page width in device-independent units, published by the page host.</summary>
    public const string ViewportWidth = "Cds.ViewportWidth";

    /// <summary>The heading level (1–6) inside nested <c>CdsSection</c>s.</summary>
    public const string HeadingLevel = "Cds.HeadingLevel";
}
