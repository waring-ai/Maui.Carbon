namespace Maui.Carbon;

public static partial class CdsMotion
{
    static partial void PlatformPrefersReducedMotion(ref bool reduced) => reduced = UIKit.UIAccessibility.IsReduceMotionEnabled;
}
