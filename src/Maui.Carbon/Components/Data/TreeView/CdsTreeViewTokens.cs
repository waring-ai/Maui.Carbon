namespace Maui.Carbon;

/// <summary>Maps tree node states to Carbon tokens (from <c>packages/styles/scss/components/treeview</c>).</summary>
public static class CdsTreeViewTokens
{
    /// <summary>
    /// The row fill: <c>layer-01</c>; <c>layer-hover-01</c> on hover; <c>layer-selected-01</c> when selected
    /// (<c>layer-selected-hover-01</c> on hover); <c>field-01</c> when disabled, whatever else applies.
    /// </summary>
    public static string Fill(bool selected, bool hover, bool disabled)
    {
        if (disabled) return CdsColorKeys.Field01;
        if (selected) return hover ? CdsColorKeys.LayerSelectedHover01 : CdsColorKeys.LayerSelected01;
        return hover ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01;
    }

    /// <summary>The label color: <c>text-secondary</c>, <c>text-primary</c> when hovered or selected, <c>text-disabled</c> when disabled.</summary>
    public static string Text(bool selected, bool hover, bool disabled)
    {
        if (disabled) return CdsColorKeys.TextDisabled;
        return selected || hover ? CdsColorKeys.TextPrimary : CdsColorKeys.TextSecondary;
    }

    /// <summary>The chevron and node icon color: <c>icon-secondary</c>, <c>icon-primary</c> when hovered or selected, <c>icon-disabled</c> when disabled.</summary>
    public static string Icon(bool selected, bool hover, bool disabled)
    {
        if (disabled) return CdsColorKeys.IconDisabled;
        return selected || hover ? CdsColorKeys.IconPrimary : CdsColorKeys.IconSecondary;
    }

    /// <summary>The 4px bar at the start edge of the active node: <c>interactive</c>.</summary>
    public static string ActiveBar => CdsColorKeys.Interactive;

    /// <summary>The width of the active bar: 4.</summary>
    public const double ActiveBarWidth = 4;

    /// <summary>The chevron rotation: -90 while collapsed, 0 while expanded.</summary>
    public static double ChevronRotation(bool expanded) => expanded ? 0 : -90;
}
