using Microsoft.Maui.Handlers;

namespace Maui.Carbon;

/// <summary>Layout handler for <see cref="CdsOverlayHost"/> that feeds platform key presses into the Carbon key router.</summary>
public partial class CdsOverlayHostHandler : LayoutHandler
{
    /// <summary>Routes a key from the platform; returns true when a Carbon overlay or control consumed it.</summary>
    internal static bool Route(CdsOverlayHost? host, CdsKey key, bool shift)
    {
        if (host is null) return false;
        var wanted = host.WantedKeys;
        var allowed = key switch
        {
            CdsKey.Escape => true,
            CdsKey.Up or CdsKey.Down or CdsKey.PageUp or CdsKey.PageDown => wanted.Vertical,
            CdsKey.Enter or CdsKey.Space => wanted.Activation,
            CdsKey.Tab => false,
            _ => wanted.Horizontal,
        };
        return allowed && host.RouteKey(new CdsKeyEventArgs(key, shift));
    }
}
