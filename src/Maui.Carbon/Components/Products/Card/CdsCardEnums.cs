namespace Maui.Carbon;

/// <summary>Where a card's actions or one of its buttons sit: in the header or in the bottom bar.</summary>
public enum CdsCardPlacement
{
    /// <summary>In the header, after the title (ghost buttons, small icon buttons).</summary>
    Top,
    /// <summary>In the bottom bar.</summary>
    Bottom,
}

/// <summary>Which part of a clickable productive card is the click target (Carbon for IBM Products' <c>clickZone</c>).</summary>
public enum CdsCardClickZone
{
    /// <summary>The whole card (default); its buttons stay separate targets on top.</summary>
    One,
    /// <summary>The header and the body.</summary>
    Two,
    /// <summary>The body only.</summary>
    Three,
}

/// <summary>Title size of a productive card.</summary>
public enum CdsCardTitleSize
{
    /// <summary><c>heading-compact-02</c> (default).</summary>
    Default,
    /// <summary><c>heading-03</c>.</summary>
    Large,
}

/// <summary>Where an expressive card shows its media.</summary>
public enum CdsCardMediaPosition
{
    /// <summary>Above the content, full width (default).</summary>
    Top,
    /// <summary>Beside the content, at the start edge.</summary>
    Left,
}

/// <summary>The kinds an expressive card's primary button can take.</summary>
public enum CdsCardPrimaryButtonKind
{
    /// <summary>A primary button (default).</summary>
    Primary,
    /// <summary>A ghost button.</summary>
    Ghost,
}

/// <summary>The kinds an expressive card's secondary button can take.</summary>
public enum CdsCardSecondaryButtonKind
{
    /// <summary>A secondary button (default).</summary>
    Secondary,
    /// <summary>A ghost button.</summary>
    Ghost,
}
