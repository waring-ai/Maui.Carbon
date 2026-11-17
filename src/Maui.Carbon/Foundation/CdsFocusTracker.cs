namespace Maui.Carbon;

/// <summary>Tracks the Carbon control that currently has focus, for keyboard routing.</summary>
internal static class CdsFocusTracker
{
    static WeakReference<CdsControl>? s_focused;

    internal static CdsControl? Focused => s_focused is not null && s_focused.TryGetTarget(out var c) ? c : null;

    internal static void SetFocused(CdsControl control) => s_focused = new WeakReference<CdsControl>(control);

    internal static void ClearFocused(CdsControl control)
    {
        if (Focused == control) s_focused = null;
    }

    /// <summary>The focused control and its Carbon ancestors, innermost first.</summary>
    internal static IEnumerable<CdsControl> FocusChain()
    {
        for (Element? e = Focused; e is not null; e = e.Parent)
            if (e is CdsControl c) yield return c;
    }
}
