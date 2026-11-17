namespace Maui.Carbon;

/// <summary>The emphasis of a <see cref="CdsMenuButton"/>.</summary>
public enum CdsMenuButtonKind
{
    /// <summary>Filled <c>button-primary</c> (default).</summary>
    Primary,
    /// <summary>Filled <c>button-secondary</c> (renders, though Carbon documents only the other three).</summary>
    Secondary,
    /// <summary>1px <c>button-tertiary</c> outline.</summary>
    Tertiary,
    /// <summary>Transparent with <c>link-primary</c> text; keeps its natural width so the chevron sits next to the label.</summary>
    Ghost,
}

/// <summary>Maps a menu button kind and state to Carbon tokens. It reuses the <see cref="CdsButtonTokens"/> of the matching button.</summary>
public static class CdsMenuButtonTokens
{
    /// <summary>Minimum width of a non-ghost trigger: 12rem.</summary>
    public const double MinWidth = 192;

    /// <summary>The button kind a menu button kind renders as.</summary>
    public static CdsButtonKind ToButtonKind(CdsMenuButtonKind kind) => kind switch
    {
        CdsMenuButtonKind.Secondary => CdsButtonKind.Secondary,
        CdsMenuButtonKind.Tertiary => CdsButtonKind.Tertiary,
        CdsMenuButtonKind.Ghost => CdsButtonKind.Ghost,
        _ => CdsButtonKind.Primary,
    };

    /// <summary>Resolves the trigger colors (the button's, in the same state).</summary>
    public static CdsButtonVisuals Resolve(CdsMenuButtonKind kind, CdsInteractionState state) =>
        CdsButtonTokens.Resolve(ToButtonKind(kind), state);

    /// <summary>The trigger height: xs 24, sm 32, md 40, lg 48 (default). Larger sizes clamp to lg.</summary>
    public static double Height(CdsSize size) => (size > CdsSize.Lg ? CdsSize.Lg : size).Height();

    /// <summary>The trigger minimum width: 12rem unless ghost.</summary>
    public static double MinimumWidth(CdsMenuButtonKind kind) => kind == CdsMenuButtonKind.Ghost ? 0 : MinWidth;
}
