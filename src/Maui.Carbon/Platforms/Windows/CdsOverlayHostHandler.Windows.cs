using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Maui.Carbon;

public partial class CdsOverlayHostHandler
{
    KeyEventHandler? _keyHandler;
    global::Windows.Foundation.TypedEventHandler<UIElement, CharacterReceivedRoutedEventArgs>? _characterHandler;

    /// <inheritdoc/>
    protected override void ConnectHandler(LayoutPanel platformView)
    {
        base.ConnectHandler(platformView);
        _keyHandler = OnKeyDown;
        platformView.AddHandler(UIElement.KeyDownEvent, _keyHandler, handledEventsToo: true);
        _characterHandler = OnCharacterReceived;
        platformView.CharacterReceived += _characterHandler;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(LayoutPanel platformView)
    {
        if (_keyHandler is not null) platformView.RemoveHandler(UIElement.KeyDownEvent, _keyHandler);
        if (_characterHandler is not null) platformView.CharacterReceived -= _characterHandler;
        base.DisconnectHandler(platformView);
    }

    // A letter a text box did not take is type-ahead (a focused dropdown jumps to the match).
    void OnCharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs e)
    {
        if (!e.Handled && VirtualView is CdsOverlayHost host && host.RouteCharacter(e.Character)) e.Handled = true;
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
            VirtualKey.PageUp => CdsKey.PageUp,
            VirtualKey.PageDown => CdsKey.PageDown,
            VirtualKey.Delete => CdsKey.Delete,
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
