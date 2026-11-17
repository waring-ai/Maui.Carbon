namespace Maui.Carbon;

public static partial class CdsGeometry
{
    static partial void TryGetPlatformBoundsCore(VisualElement element, VisualElement ancestor, ref Rect? bounds)
    {
        if (element.Handler?.PlatformView is not Android.Views.View view || ancestor.Handler?.PlatformView is not Android.Views.View host) return;
        if (!view.IsAttachedToWindow || !host.IsAttachedToWindow) return;
        var density = view.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
        var a = new int[2];
        var h = new int[2];
        view.GetLocationInWindow(a);
        host.GetLocationInWindow(h);
        bounds = new Rect((a[0] - h[0]) / density, (a[1] - h[1]) / density, view.Width / density, view.Height / density);
    }
}
