using Foundation;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Maui.Carbon;

public partial class CdsOverlayHostHandler
{
    /// <inheritdoc/>
    protected override LayoutView CreatePlatformView()
    {
        var view = new CdsAppleHostView { CrossPlatformLayout = VirtualView };
        if (VirtualView is CdsOverlayHost host) view.Owner = new WeakReference<CdsOverlayHost>(host);
        return view;
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(LayoutView platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is CdsOverlayHost host) host.KeyStateChanged += OnKeyStateChanged;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(LayoutView platformView)
    {
        if (VirtualView is CdsOverlayHost host) host.KeyStateChanged -= OnKeyStateChanged;
        base.DisconnectHandler(platformView);
    }

    void OnKeyStateChanged(object? sender, EventArgs e)
    {
        if (sender is not CdsOverlayHost host || PlatformView is not CdsAppleHostView view) return;
        // Overlays without a text field (menus, list boxes) take first responder so their keys arrive even when
        // nothing else has focus; overlays opened from a text field leave the field focused (it is in our chain).
        if (host.WantedKeys.Activation && !view.IsFirstResponder) view.BecomeFirstResponder();
        else if (host.TopOverlay is null && view.IsFirstResponder) view.ResignFirstResponder();
    }
}

/// <summary>The host's UIView: exposes Carbon key commands only while an overlay or composite control wants them.</summary>
internal sealed class CdsAppleHostView : LayoutView
{
    const string ActionName = "cdsKeyCommand:";
    static readonly Selector s_action = new(ActionName);
    static readonly Dictionary<CdsWantedKeys, UIKeyCommand[]> s_sets = [];

    public WeakReference<CdsOverlayHost>? Owner { get; set; }

    CdsOverlayHost? Host => Owner is not null && Owner.TryGetTarget(out var h) ? h : null;

    public override bool CanBecomeFirstResponder => Host?.TopOverlay is not null;

    public override UIKeyCommand[] KeyCommands
    {
        get
        {
            if (Host is not { } host) return [];
            var w = host.WantedKeys;
            if (!w.Escape && !w.Vertical && !w.Horizontal && !w.Activation) return [];
            if (!s_sets.TryGetValue(w, out var set)) s_sets[w] = set = Build(w);
            return set;
        }
    }

    // While an open list box holds first responder, the letters typed reach the host: route them as type-ahead.
    public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        if (TypedCharacter(presses) is { } c && Host?.RouteCharacter(c) == true) return;
        base.PressesBegan(presses, evt);
    }

    /// <summary>The printable character of a single key press without Command, Control or Option; <c>null</c> otherwise.</summary>
    internal static char? TypedCharacter(NSSet<UIPress> presses)
    {
        if (presses.Count != 1) return null;
        foreach (var press in presses)
        {
            if (press.Key is not { } key) return null;
            if ((key.ModifierFlags & (UIKeyModifierFlags.Command | UIKeyModifierFlags.Control | UIKeyModifierFlags.Alternate)) != 0) return null;
            var characters = key.Characters;
            if (characters is not { Length: 1 } || char.IsControl(characters[0]) || char.IsWhiteSpace(characters[0])) return null;
            return characters[0];
        }
        return null;
    }

    [Export(ActionName)]
    void OnKeyCommand(UIKeyCommand command)
    {
        if (Map(command.Input) is { } key)
            CdsOverlayHostHandler.Route(Host, key, command.ModifierFlags.HasFlag(UIKeyModifierFlags.Shift));
    }

    static UIKeyCommand[] Build(CdsWantedKeys w)
    {
        var inputs = new List<NSString>();
        var modernKeys = OperatingSystem.IsIOSVersionAtLeast(13, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 4);
        if (w.Escape) inputs.Add(UIKeyCommand.Escape);
        if (w.Vertical)
        {
            inputs.AddRange([UIKeyCommand.UpArrow, UIKeyCommand.DownArrow]);
            if (modernKeys) inputs.AddRange([UIKeyCommand.PageUp, UIKeyCommand.PageDown]);
        }
        if (w.Horizontal)
        {
            inputs.AddRange([UIKeyCommand.LeftArrow, UIKeyCommand.RightArrow]);
            if (modernKeys) inputs.AddRange([UIKeyCommand.Home, UIKeyCommand.End]);
        }
        if (w.Activation) inputs.AddRange([new NSString("\r"), new NSString(" ")]);

        return inputs.Select(input =>
        {
            var cmd = UIKeyCommand.Create(input, 0, s_action);
            if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
                cmd.WantsPriorityOverSystemBehavior = true;
            return cmd;
        }).ToArray();
    }

    internal static CdsKey? Map(string? input)
    {
        if (input is null) return null;
        if (input == UIKeyCommand.Escape) return CdsKey.Escape;
        if (input == UIKeyCommand.UpArrow) return CdsKey.Up;
        if (input == UIKeyCommand.DownArrow) return CdsKey.Down;
        if (input == UIKeyCommand.LeftArrow) return CdsKey.Left;
        if (input == UIKeyCommand.RightArrow) return CdsKey.Right;
        if ((OperatingSystem.IsIOSVersionAtLeast(13, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 4)))
        {
            if (input == UIKeyCommand.Home) return CdsKey.Home;
            if (input == UIKeyCommand.End) return CdsKey.End;
            if (input == UIKeyCommand.PageUp) return CdsKey.PageUp;
            if (input == UIKeyCommand.PageDown) return CdsKey.PageDown;
        }
        return input switch { "\r" => CdsKey.Enter, " " => CdsKey.Space, "\t" => CdsKey.Tab, _ => null };
    }

    internal static CdsKey? Map(UIKeyboardHidUsage usage) => usage switch
    {
        UIKeyboardHidUsage.KeyboardEscape => CdsKey.Escape,
        UIKeyboardHidUsage.KeyboardUpArrow => CdsKey.Up,
        UIKeyboardHidUsage.KeyboardDownArrow => CdsKey.Down,
        UIKeyboardHidUsage.KeyboardLeftArrow => CdsKey.Left,
        UIKeyboardHidUsage.KeyboardRightArrow => CdsKey.Right,
        UIKeyboardHidUsage.KeyboardHome => CdsKey.Home,
        UIKeyboardHidUsage.KeyboardEnd => CdsKey.End,
        UIKeyboardHidUsage.KeyboardPageUp => CdsKey.PageUp,
        UIKeyboardHidUsage.KeyboardPageDown => CdsKey.PageDown,
        UIKeyboardHidUsage.KeyboardDeleteForward => CdsKey.Delete,
        UIKeyboardHidUsage.KeyboardReturnOrEnter => CdsKey.Enter,
        UIKeyboardHidUsage.KeyboardSpacebar => CdsKey.Space,
        _ => null,
    };
}
