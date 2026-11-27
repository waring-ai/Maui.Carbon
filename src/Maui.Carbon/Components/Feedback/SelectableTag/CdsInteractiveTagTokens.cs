namespace Maui.Carbon;

/// <summary>Colors of an operational or selectable tag.</summary>
/// <param name="Background">Fill; <c>null</c> is transparent.</param>
/// <param name="Border">1px outline.</param>
/// <param name="Foreground">Label and icon color.</param>
public readonly record struct CdsInteractiveTagVisuals(string? Background, string? Border, string Foreground);

/// <summary>Colors of the close button of a dismissible tag.</summary>
/// <param name="Hover">Fill of the hover circle; <c>null</c> when not hovered.</param>
/// <param name="Icon">Close icon color.</param>
/// <param name="Focus">Inset 1px focus ring color.</param>
public readonly record struct CdsTagCloseVisuals(string? Hover, string Icon, string Focus);

/// <summary>
/// Maps operational, selectable and dismissible tag states to Carbon tokens (<c>tag-border-*</c>, <c>tag-hover-*</c>,
/// <c>layer-selected-inverse</c>...).
/// </summary>
public static class CdsInteractiveTagTokens
{
    /// <summary>The <c>tag-border-*</c> key of a tag type. High contrast and outline have none and fall back to gray.</summary>
    public static string TagBorder(CdsTagType type) => type switch
    {
        CdsTagType.Red => CdsColorKeys.TagBorderRed,
        CdsTagType.Magenta => CdsColorKeys.TagBorderMagenta,
        CdsTagType.Purple => CdsColorKeys.TagBorderPurple,
        CdsTagType.Blue => CdsColorKeys.TagBorderBlue,
        CdsTagType.Cyan => CdsColorKeys.TagBorderCyan,
        CdsTagType.Teal => CdsColorKeys.TagBorderTeal,
        CdsTagType.Green => CdsColorKeys.TagBorderGreen,
        CdsTagType.CoolGray => CdsColorKeys.TagBorderCoolGray,
        CdsTagType.WarmGray => CdsColorKeys.TagBorderWarmGray,
        _ => CdsColorKeys.TagBorderGray,
    };

    /// <summary>
    /// An operational tag: <c>tag-background-*</c> fill, <c>tag-border-*</c> outline and <c>tag-color-*</c> text; hover (and
    /// pressed) switch the fill to <c>tag-hover-*</c>. Disabled: <c>layer-01</c>, <c>border-disabled</c>, <c>text-disabled</c>, no hover.
    /// </summary>
    public static CdsInteractiveTagVisuals Operational(CdsTagType type, CdsInteractionState state)
    {
        if ((state & CdsInteractionState.Disabled) != 0)
            return new(CdsColorKeys.Layer01, CdsColorKeys.BorderDisabled, CdsColorKeys.TextDisabled);
        if (type is CdsTagType.HighContrast or CdsTagType.Outline) type = CdsTagType.Gray;
        var v = CdsTagTokens.Resolve(type, false);
        var active = (state & (CdsInteractionState.Hover | CdsInteractionState.Pressed)) != 0;
        return new(active ? v.Hover : v.Background, TagBorder(type), v.Foreground);
    }

    /// <summary>
    /// A selectable tag. Unselected: <c>layer-01</c> fill, 1px <c>border-inverse</c>, <c>text-primary</c>, hover
    /// <c>layer-hover-01</c>. Selected: <c>layer-selected-inverse</c> with <c>text-inverse</c>, hover keeps the fill.
    /// Disabled (selected or not): <c>layer-01</c>, <c>border-disabled</c>, <c>text-disabled</c>.
    /// </summary>
    public static CdsInteractiveTagVisuals Selectable(bool selected, CdsInteractionState state)
    {
        if ((state & CdsInteractionState.Disabled) != 0)
            return new(CdsColorKeys.Layer01, CdsColorKeys.BorderDisabled, CdsColorKeys.TextDisabled);
        if (selected) return new(CdsColorKeys.LayerSelectedInverse, CdsColorKeys.BorderInverse, CdsColorKeys.TextInverse);
        var active = (state & (CdsInteractionState.Hover | CdsInteractionState.Pressed)) != 0;
        return new(active ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01, CdsColorKeys.BorderInverse, CdsColorKeys.TextPrimary);
    }

    /// <summary>
    /// The close button of a dismissible tag: a <c>tag-hover-*</c> circle on hover, the tag's foreground (or
    /// <c>icon-disabled</c>) for the icon, a <c>focus</c> ring (<c>focus-inverse</c> on high contrast).
    /// </summary>
    public static CdsTagCloseVisuals Close(CdsTagType type, bool hovering, bool disabled)
    {
        var v = CdsTagTokens.Resolve(type, disabled);
        return new(hovering && !disabled ? v.Hover : null, disabled ? CdsColorKeys.IconDisabled : v.Foreground,
            type == CdsTagType.HighContrast ? CdsColorKeys.FocusInverse : CdsColorKeys.Focus);
    }
}

/// <summary>Layout helper shared by the interactive tags: replaces the base tag's stack by a grid so labels truncate.</summary>
internal static class CdsTagParts
{
    /// <summary>
    /// Moves the icon and label out of the base <paramref name="row"/> into a grid [icon][label *][trailing], or with
    /// <paramref name="withDecorator"/> [icon][label *][decorator][trailing] (the dismissible tag).
    /// </summary>
    public static Grid Rebuild(Border pill, HorizontalStackLayout row, out CdsIcon icon, out CdsText label, bool withDecorator = false)
    {
        icon = (CdsIcon)row.Children[0];
        label = (CdsText)row.Children[1];
        row.Children.Clear();
        var grid = new Grid { VerticalOptions = LayoutOptions.Center, ColumnSpacing = 0 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        if (withDecorator) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.Add(icon, 0);
        grid.Add(label, 1);
        pill.Content = grid;
        return grid;
    }
}
