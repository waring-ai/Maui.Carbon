namespace Maui.Carbon;

/// <summary>The emphasis of a <see cref="CdsMenuItem"/>.</summary>
public enum CdsMenuItemKind
{
    /// <summary>A normal item.</summary>
    Default,
    /// <summary>A destructive item: fills with <c>button-danger-primary</c> on hover and focus. Last in the list, after a divider.</summary>
    Danger,
}

/// <summary>
/// Where a menu opens relative to the button that triggers it (Carbon's <c>menuAlignment</c>). With <see cref="Bottom"/>
/// and <see cref="Top"/> the menu starts at the trigger's start edge and is never narrower than the trigger; the
/// <c>Start</c>/<c>End</c> variants use the menu's own width, aligned to that edge of the trigger.
/// </summary>
public enum CdsMenuAlignment
{
    /// <summary>Below the trigger, at least as wide as it.</summary>
    Bottom,
    /// <summary>Below the trigger, start edges aligned.</summary>
    BottomStart,
    /// <summary>Below the trigger, end edges aligned.</summary>
    BottomEnd,
    /// <summary>Above the trigger, at least as wide as it.</summary>
    Top,
    /// <summary>Above the trigger, start edges aligned.</summary>
    TopStart,
    /// <summary>Above the trigger, end edges aligned.</summary>
    TopEnd,
}

/// <summary>Maps <see cref="CdsMenuAlignment"/> to overlay placement.</summary>
public static class CdsMenuAlignmentExtensions
{
    /// <summary>The overlay placement for an alignment (the full-width variants anchor at the start edge).</summary>
    public static CdsPlacement ToPlacement(this CdsMenuAlignment alignment) => alignment switch
    {
        CdsMenuAlignment.BottomEnd => CdsPlacement.BottomEnd,
        CdsMenuAlignment.Top or CdsMenuAlignment.TopStart => CdsPlacement.TopStart,
        CdsMenuAlignment.TopEnd => CdsPlacement.TopEnd,
        _ => CdsPlacement.BottomStart,
    };

    /// <summary>True when the menu must be at least as wide as its trigger (<c>Bottom</c> and <c>Top</c>).</summary>
    public static bool FillsTrigger(this CdsMenuAlignment alignment) =>
        alignment is CdsMenuAlignment.Bottom or CdsMenuAlignment.Top;
}
