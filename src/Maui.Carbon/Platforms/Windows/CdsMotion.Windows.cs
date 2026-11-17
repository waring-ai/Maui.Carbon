namespace Maui.Carbon;

public static partial class CdsMotion
{
    static partial void PlatformPrefersReducedMotion(ref bool reduced) =>
        reduced = !new global::Windows.UI.ViewManagement.UISettings().AnimationsEnabled;
}
