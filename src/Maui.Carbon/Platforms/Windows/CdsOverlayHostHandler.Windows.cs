using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Maui.Carbon;

public partial class CdsOverlayHostHandler
{
    KeyEventHandler? _keyHandler;

    /// <inheritdoc/>
    protected override void ConnectHandler(LayoutPanel platformView)
    {
        base.ConnectHandler(platformView);
        _keyHandler = OnKeyDown;
        platformView.AddHandler(UIElement.KeyDownEvent, _keyHandler, handledEventsToo: true);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(LayoutPanel platformView)
    {
        if (_keyHandler is not null) platformView.RemoveHandler(UIElement.KeyDownEvent, _keyHandler);
        base.DisconnectHandler(platformView);
    }

    void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        CdsKey? key = e.Key switch
        {
            VirtualKey.Escape => CdsKey.Escape,
            VirtualKey.Up => CdsKey.Up,
            VirtualKey.Down => CdsKey.Down,
            VirtualKey.Left => CdsKey.Left,
            VirtualKey.Right => CdsKey.Right,
            VirtualKey.Home => CdsKey.Home,
            VirtualKey.End => CdsKey.End,
            VirtualKey.Enter => CdsKey.Enter,
            VirtualKey.Space => CdsKey.Space,
            _ => null,
        };
        if (key is null) return;
        var shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
            .HasFlag(global::Windows.UI.Core.CoreVirtualKeyStates.Down);
        if (Route(VirtualView as CdsOverlayHost, key.Value, shift)) e.Handled = true;
    }
}
