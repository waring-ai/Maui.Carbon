namespace Maui.Carbon;

/// <summary>Carbon button kinds (emphasis).</summary>
public enum CdsButtonKind
{
    /// <summary>The one main action of a view (<c>button-primary</c>).</summary>
    Primary,
    /// <summary>The negative choice next to a primary (Cancel, Back).</summary>
    Secondary,
    /// <summary>Supporting action: outlined, alone or beside a primary.</summary>
    Tertiary,
    /// <summary>Low-emphasis action: toolbars and supplementary actions.</summary>
    Ghost,
    /// <summary>Destructive action (Carbon's <c>danger</c> / <c>danger--primary</c>).</summary>
    Danger,
    /// <summary>Destructive, outlined (<c>danger--tertiary</c>).</summary>
    DangerTertiary,
    /// <summary>Destructive, low emphasis (<c>danger--ghost</c>).</summary>
    DangerGhost,
}
