namespace Maui.Carbon;

/// <summary>Keys an open overlay wants routed to it (Escape always is).</summary>
[Flags]
public enum CdsOverlayKeys
{
    /// <summary>Only Escape.</summary>
    None = 0,
    /// <summary>Up/Down/Left/Right, Home, End, Page Up and Page Down.</summary>
    Navigation = 1,
    /// <summary>Enter and Space (only for overlays without a focused text field).</summary>
    Activation = 2,
    /// <summary>
    /// Only Up/Down and Page Up/Page Down: for overlays opened from a focused text field (combo box, date input), so
    /// Left/Right/Home/End keep moving the caret.
    /// </summary>
    VerticalNavigation = 4,
}

/// <summary>The page edge an anchorless overlay is docked to (side panels, tearsheets).</summary>
public enum CdsOverlayDock
{
    /// <summary>Not docked: a modal is centered, a non-modal anchorless overlay stacks with the toasts.</summary>
    None,
    /// <summary>Full height against the start edge; slides in from that edge.</summary>
    Start,
    /// <summary>Full height against the end edge; slides in from that edge.</summary>
    End,
    /// <summary>Full width against the bottom edge; slides up from it (tearsheets).</summary>
    Bottom,
    /// <summary>
    /// In the top end corner, sliding down from above (the notifications panel under the UI shell header). It floats
    /// with the popups, so <see cref="CdsOverlayOptions.LightDismiss"/> closes it on a tap outside.
    /// </summary>
    TopEnd,
}

/// <summary>How an overlay is shown by <see cref="CdsOverlayService"/>.</summary>
public sealed class CdsOverlayOptions
{
    /// <summary>The element the popup is anchored to. <c>null</c> for anchorless overlays (modals, toasts).</summary>
    public VisualElement? Anchor { get; set; }

    /// <summary>Preferred placement next to the anchor. Default <see cref="CdsPlacement.BottomStart"/>.</summary>
    public CdsPlacement Placement { get; set; } = CdsPlacement.BottomStart;

    /// <summary>Gap between anchor and popup.</summary>
    public double Offset { get; set; }

    /// <summary>Flip to the opposite side when the preferred side does not fit. Default <c>true</c>.</summary>
    public bool AutoFlip { get; set; } = true;

    /// <summary>Make the popup exactly as wide as the anchor (list boxes). Default <c>false</c>.</summary>
    public bool MatchAnchorWidth { get; set; }

    /// <summary>A tap outside closes the overlay. Default <c>true</c>.</summary>
    public bool LightDismiss { get; set; } = true;

    /// <summary>Escape closes the overlay. Default <c>true</c>.</summary>
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Show as a modal: a <c>overlay</c> scrim covers the page, the page stops receiving input and is hidden from
    /// screen readers until the overlay closes. The content is centered unless it sets its own alignment.
    /// </summary>
    public bool IsModal { get; set; }

    /// <summary>
    /// Docks an anchorless overlay to an edge of the page: it fills that edge and slides in from it (Carbon's side panel
    /// and tearsheet motion) instead of dropping in from above. With <see cref="IsModal"/> it sits over the scrim; without,
    /// it floats over the page, which stays usable, under menus and other popups. The content's own size and margins
    /// still apply along the edge. Default <see cref="CdsOverlayDock.None"/>.
    /// </summary>
    public CdsOverlayDock Dock { get; set; }

    /// <summary>
    /// Called when the scrim beside a docked modal overlay is tapped (a side panel's overlay). A centered modal covers the
    /// scrim with its own surface and handles those taps itself.
    /// </summary>
    public Action? ScrimTapped { get; set; }

    /// <summary>The popup ignores input (hover tooltips).</summary>
    public bool InputTransparent { get; set; }

    /// <summary>
    /// Extra keys routed to <see cref="KeyHandler"/> while the overlay is on top. An overlay with no keys, no handler and
    /// <see cref="CloseOnEscape"/> off (a toast) is passive: keys skip it and go to the overlay under it.
    /// </summary>
    public CdsOverlayKeys Keys { get; set; }

    /// <summary>Handles keys while the overlay is on top; return <c>true</c> to consume.</summary>
    public Func<CdsKeyEventArgs, bool>? KeyHandler { get; set; }

    /// <summary>Element focused when the overlay opens (for example Cancel in a danger modal).</summary>
    public VisualElement? InitialFocus { get; set; }

    /// <summary>
    /// Return focus to <see cref="ReturnFocusTo"/> (or the anchor) when the overlay closes. An anchorless overlay (a modal
    /// or dialog) without <see cref="ReturnFocusTo"/> returns focus to the Carbon control that had focus when it opened,
    /// if that control is still on the page. Default <c>true</c>; tooltips and toasts turn it off.
    /// </summary>
    public bool ReturnFocus { get; set; } = true;

    /// <summary>Return focus to this element (default: the anchor, else the control focused before opening) when the overlay closes.</summary>
    public VisualElement? ReturnFocusTo { get; set; }

    /// <summary>Animate the overlay in and out with Carbon motion. Default <c>true</c>.</summary>
    public bool Animate { get; set; } = true;
}
