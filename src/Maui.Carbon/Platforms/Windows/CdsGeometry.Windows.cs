namespace Maui.Carbon;

public static partial class CdsGeometry
{
    static partial void TryGetPlatformBoundsCore(VisualElement element, VisualElement ancestor, ref Rect? bounds)
    {
        if (element.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement view ||
            ancestor.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement host) return;
        if (view.XamlRoot is null) return;
        try
        {
            var p = view.TransformToVisual(host).TransformPoint(new Windows.Foundation.Point(0, 0));
            bounds = new Rect(p.X, p.Y, view.ActualWidth, view.ActualHeight);
        }
        catch (ArgumentException)
        {
            // Not in the same visual tree yet.
        }
    }
}
