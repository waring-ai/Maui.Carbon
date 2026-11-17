namespace Maui.Carbon;

/// <summary>Measures element bounds relative to an ancestor, using native coordinates where available.</summary>
public static partial class CdsGeometry
{
    /// <summary>The bounds of <paramref name="element"/> in <paramref name="ancestor"/>'s coordinate space.</summary>
    public static Rect GetBoundsRelativeTo(VisualElement element, VisualElement ancestor)
    {
        if (TryGetPlatformBounds(element, ancestor, out var bounds)) return bounds;

        // Fallback: walk the logical tree.
        double x = 0, y = 0;
        for (Element? e = element; e is not null && e != ancestor; e = e.Parent)
        {
            if (e is VisualElement ve)
            {
                x += ve.X + ve.TranslationX;
                y += ve.Y + ve.TranslationY;
            }
            if (e.Parent is ScrollView sv)
            {
                x -= sv.ScrollX;
                y -= sv.ScrollY;
            }
        }
        return new Rect(x, y, element.Width, element.Height);
    }

    static partial void TryGetPlatformBoundsCore(VisualElement element, VisualElement ancestor, ref Rect? bounds);

    static bool TryGetPlatformBounds(VisualElement element, VisualElement ancestor, out Rect bounds)
    {
        Rect? result = null;
        TryGetPlatformBoundsCore(element, ancestor, ref result);
        bounds = result ?? default;
        return result.HasValue;
    }
}
