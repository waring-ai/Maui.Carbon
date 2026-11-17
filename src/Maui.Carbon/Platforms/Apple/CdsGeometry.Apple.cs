using UIKit;

namespace Maui.Carbon;

public static partial class CdsGeometry
{
    static partial void TryGetPlatformBoundsCore(VisualElement element, VisualElement ancestor, ref Rect? bounds)
    {
        if (element.Handler?.PlatformView is not UIView view || ancestor.Handler?.PlatformView is not UIView host) return;
        if (view.Window is null || host.Window is null) return;
        var r = view.ConvertRectToView(view.Bounds, host);
        bounds = new Rect(r.X, r.Y, r.Width, r.Height);
    }
}
