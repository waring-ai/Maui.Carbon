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

    /// <summary>The popup ignores input (hover tooltips).</summary>
    public bool InputTransparent { get; set; }

    /// <summary>Extra keys routed to <see cref="KeyHandler"/> while the overlay is on top.</summary>
    public CdsOverlayKeys Keys { get; set; }

    /// <summary>Handles keys while the overlay is on top; return <c>true</c> to consume.</summary>
    public Func<CdsKeyEventArgs, bool>? KeyHandler { get; set; }

    /// <summary>Element focused when the overlay opens (for example Cancel in a danger modal).</summary>
    public VisualElement? InitialFocus { get; set; }

    /// <summary>Return focus to <see cref="ReturnFocusTo"/> (or the anchor) when the overlay closes. Default <c>true</c>; tooltips turn it off.</summary>
    public bool ReturnFocus { get; set; } = true;

    /// <summary>Return focus to this element (default: the anchor) when the overlay closes.</summary>
    public VisualElement? ReturnFocusTo { get; set; }

    /// <summary>Animate the overlay in and out with Carbon motion. Default <c>true</c>.</summary>
    public bool Animate { get; set; } = true;
}
