namespace Maui.Carbon;

/// <summary>A content view whose resources carry Carbon context (theme zones, layers, overlay roots).</summary>
internal sealed class CdsScopeView : ContentView
{
}

/// <summary>Reads resource-carried context (<see cref="CdsContextKeys"/>) by walking up the tree.</summary>
internal static class CdsContext
{
    /// <summary>The nearest value of <paramref name="key"/> set by a Carbon scope at or above <paramref name="element"/>, else the app's.</summary>
    internal static object? Find(Element? element, string key)
    {
        for (var e = element; e is not null; e = e.Parent)
        {
            if (e is CdsScopeView scope && scope.Resources.TryGetValue(key, out var v))
                return v;
        }
        return CdsResources.Current is { } app && app.TryGetValue(key, out var a) ? a : null;
    }
}
