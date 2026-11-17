namespace Maui.Carbon;

/// <summary>The interaction and validation states a Carbon control can be in at once.</summary>
[Flags]
public enum CdsInteractionState
{
    /// <summary>No state.</summary>
    None = 0,
    /// <summary>The pointer is over the control.</summary>
    Hover = 1 << 0,
    /// <summary>The control is being pressed.</summary>
    Pressed = 1 << 1,
    /// <summary>The control has input focus.</summary>
    Focused = 1 << 2,
    /// <summary>The control has focus from the keyboard: the Carbon focus ring is visible.</summary>
    FocusVisible = 1 << 3,
    /// <summary>The control is disabled (<c>IsEnabled = false</c>).</summary>
    Disabled = 1 << 4,
    /// <summary>The control is selected, checked or active.</summary>
    Selected = 1 << 5,
    /// <summary>The control is invalid (error state).</summary>
    Invalid = 1 << 6,
    /// <summary>The control has a warning.</summary>
    Warn = 1 << 7,
    /// <summary>The control is read-only: focusable and readable, not editable.</summary>
    ReadOnly = 1 << 8,
    /// <summary>The control renders as a loading skeleton.</summary>
    Skeleton = 1 << 9,
    /// <summary>The control is expanded or its popup is open.</summary>
    Open = 1 << 10,
}
