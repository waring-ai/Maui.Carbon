namespace Maui.Carbon;

/// <summary>Color keys of a menu row in one state. <c>null</c> background means transparent (the surface shows through).</summary>
/// <param name="Background">Row fill.</param>
/// <param name="Text">Label and shortcut color.</param>
/// <param name="Icon">Icon, checkmark and submenu caret color (menus draw icons with <c>currentColor</c>).</param>
public readonly record struct CdsMenuRowVisuals(string? Background, string Text, string Icon);

/// <summary>Maps a menu row's state to Carbon tokens (from <c>packages/styles/scss/components/menu</c>) and holds its measurements.</summary>
public static class CdsMenuTokens
{
    /// <summary>Minimum menu width: 10rem.</summary>
    public const double MinWidth = 160;

    /// <summary>Minimum width of a menu with icons: 12rem.</summary>
    public const double MinWidthWithIcons = 192;

    /// <summary>Maximum menu width: 18rem. Longer labels truncate.</summary>
    public const double MaxWidth = 288;

    /// <summary>Vertical padding of the surface: 0.25rem.</summary>
    public const double SurfacePadding = 4;

    /// <summary>Resolves a row's colors. <paramref name="highlighted"/> covers pointer hover and virtual focus.</summary>
    public static CdsMenuRowVisuals Resolve(bool isDanger, bool highlighted, bool isEnabled)
    {
        if (!isEnabled) return new(null, CdsColorKeys.TextDisabled, CdsColorKeys.TextDisabled);
        if (highlighted)
        {
            return isDanger
                ? new(CdsColorKeys.ButtonDangerPrimary, CdsColorKeys.TextOnColor, CdsColorKeys.TextOnColor)
                : new(CdsColorKeys.LayerHover01, CdsColorKeys.TextPrimary, CdsColorKeys.TextPrimary);
        }
        return new(null, CdsColorKeys.TextSecondary, CdsColorKeys.TextSecondary);
    }

    /// <summary>The surface fill for a background token: <c>layer-01</c> (default) or <c>background</c>.</summary>
    public static string SurfaceBackground(CdsMenuBackgroundToken token) =>
        token == CdsMenuBackgroundToken.Background ? CdsColorKeys.Background : CdsColorKeys.Layer01;

    /// <summary>The row height of a menu size: xs 24, sm 32 (default), md 40, lg 48. Larger sizes clamp to lg.</summary>
    public static double RowHeight(CdsSize size) => Coerce(size).Height();

    /// <summary>Clamps a size into the menu range xs..lg.</summary>
    public static CdsSize Coerce(CdsSize size) => size > CdsSize.Lg ? CdsSize.Lg : size;

    /// <summary>The surface minimum width for a menu, given whether it has icons and the minimum the trigger asks for.</summary>
    public static double MinimumWidth(bool hasIcons, double triggerMinimum = 0) =>
        Math.Min(MaxWidth, Math.Max(triggerMinimum, hasIcons ? MinWidthWithIcons : MinWidth));
}
