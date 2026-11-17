namespace Maui.Carbon.Generator;

/// <summary>Converts design/carbon/icons/*.svg into CdsIconName / CdsIconData. Implemented separately.</summary>
public static partial class IconEmitter
{
    /// <summary>False until the SVG converter is implemented.</summary>
    public static bool IsAvailable => false;

    /// <summary>Returns (file name, body) pairs.</summary>
    public static IEnumerable<(string Name, string Body)> Emit(string iconsDir) => [];
}
