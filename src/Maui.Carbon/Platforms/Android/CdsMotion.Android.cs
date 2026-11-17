namespace Maui.Carbon;

public static partial class CdsMotion
{
    static partial void PlatformPrefersReducedMotion(ref bool reduced)
    {
        var resolver = Android.App.Application.Context.ContentResolver;
        reduced = resolver is not null &&
            Android.Provider.Settings.Global.GetFloat(resolver, Android.Provider.Settings.Global.AnimatorDurationScale, 1f) == 0f;
    }
}
