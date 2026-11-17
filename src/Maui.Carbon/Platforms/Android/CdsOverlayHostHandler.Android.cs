using Android.Content;
using Android.Views;
using Microsoft.Maui.Platform;

namespace Maui.Carbon;

public partial class CdsOverlayHostHandler
{
    /// <inheritdoc/>
    protected override LayoutViewGroup CreatePlatformView()
    {
        var view = new CdsAndroidHostView(Context) { CrossPlatformLayout = VirtualView };
        view.SetClipChildren(false);
        if (VirtualView is CdsOverlayHost host) view.Owner = new WeakReference<CdsOverlayHost>(host);
        return view;
    }
}

/// <summary>The host's ViewGroup: sees every key event on its way to the focused child.</summary>
internal sealed class CdsAndroidHostView : LayoutViewGroup
{
    public CdsAndroidHostView(Context context) : base(context)
    {
    }

    public WeakReference<CdsOverlayHost>? Owner { get; set; }

    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e is { Action: KeyEventActions.Down } && Owner is not null && Owner.TryGetTarget(out var host))
        {
            CdsKey? key = e.KeyCode switch
            {
                Keycode.Escape or Keycode.Back when host.TopOverlay is not null => CdsKey.Escape,
                Keycode.DpadUp => CdsKey.Up,
                Keycode.DpadDown => CdsKey.Down,
                Keycode.DpadLeft => CdsKey.Left,
                Keycode.DpadRight => CdsKey.Right,
                Keycode.MoveHome => CdsKey.Home,
                Keycode.MoveEnd => CdsKey.End,
                Keycode.PageUp => CdsKey.PageUp,
                Keycode.PageDown => CdsKey.PageDown,
                Keycode.ForwardDel => CdsKey.Delete,
                Keycode.Enter or Keycode.NumpadEnter => CdsKey.Enter,
                Keycode.Space => CdsKey.Space,
                _ => null,
            };
            if (key is { } k && CdsOverlayHostHandler.Route(host, k, e.IsShiftPressed)) return true;
        }
        return base.DispatchKeyEvent(e);
    }
}
