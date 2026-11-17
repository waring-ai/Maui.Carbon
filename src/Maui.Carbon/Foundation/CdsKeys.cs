namespace Maui.Carbon;

/// <summary>Keys the Carbon keyboard router reports.</summary>
public enum CdsKey
{
    /// <summary>Escape.</summary>
    Escape,
    /// <summary>Enter / Return.</summary>
    Enter,
    /// <summary>Space bar.</summary>
    Space,
    /// <summary>Up arrow.</summary>
    Up,
    /// <summary>Down arrow.</summary>
    Down,
    /// <summary>Left arrow.</summary>
    Left,
    /// <summary>Right arrow.</summary>
    Right,
    /// <summary>Home.</summary>
    Home,
    /// <summary>End.</summary>
    End,
    /// <summary>Tab.</summary>
    Tab,
}

/// <summary>A key press routed to Carbon controls and overlays.</summary>
public sealed class CdsKeyEventArgs : EventArgs
{
    /// <summary>Creates key event arguments.</summary>
    public CdsKeyEventArgs(CdsKey key, bool shift = false)
    {
        Key = key;
        Shift = shift;
    }

    /// <summary>The key.</summary>
    public CdsKey Key { get; }

    /// <summary>True while Shift is held.</summary>
    public bool Shift { get; }

    /// <summary>Set by a handler that consumed the key.</summary>
    public bool Handled { get; set; }
}
