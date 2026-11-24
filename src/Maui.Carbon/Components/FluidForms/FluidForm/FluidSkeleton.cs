namespace Maui.Carbon;

/// <summary>
/// Builds Carbon's fluid skeletons (<c>.cds--fluid__skeleton</c>): a <c>skeleton-background</c> field 64px tall (128px for
/// a text area) with a 1px <c>skeleton-element</c> bottom rule, a label bar (25% wide, at most 6rem) 17px from the top and a
/// value bar (50% wide) 13px under it, both 8px shimmering bars. Pickers split two or three such parts with 1px
/// <c>skeleton-element</c> lines.
/// </summary>
internal static class FluidSkeleton
{
    /// <summary>Height of the label and value bars: 0.5rem.</summary>
    public const double BarHeight = 8;

    /// <summary>Widest the label bar gets: 6rem.</summary>
    public const double LabelBarMaxWidth = 96;

    /// <summary>Distance of the label bar from the top of the field: 1.0625rem.</summary>
    public const double TopPadding = 17;

    /// <summary>Space between the label bar and the value bar: 0.8125rem.</summary>
    public const double BarGap = 13;

    /// <summary>One skeleton field of the given height.</summary>
    public static Grid Field(double height)
    {
        var field = new Grid
        {
            HeightRequest = height,
            Padding = new Thickness(CdsSpacing.Spacing05, TopPadding, CdsSpacing.Spacing05, 0),
            RowSpacing = 0,
            ColumnSpacing = 0,
            InputTransparent = true,
        };
        field.RowDefinitions.Add(new RowDefinition(BarHeight));
        field.RowDefinitions.Add(new RowDefinition(BarGap));
        field.RowDefinitions.Add(new RowDefinition(BarHeight));
        field.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        // 25% | 25% | 50% of the content width.
        field.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        field.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        field.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
        CdsTokens.SetColor(field, VisualElement.BackgroundColorProperty, CdsColorKeys.SkeletonBackground);

        field.Add(new CdsMaxWidthBox(new CdsShimmerView(), LabelBarMaxWidth), 0, 0);
        var value = new CdsShimmerView();
        field.Add(value, 0, 2);
        Grid.SetColumnSpan(value, 2);

        var rule = new BoxView
        {
            HeightRequest = 1,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(-CdsSpacing.Spacing05, 0),
        };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.SkeletonElement);
        field.Add(rule, 0, 3);
        Grid.SetColumnSpan(rule, 3);
        AutomationProperties.SetIsInAccessibleTree(field, false);
        return field;
    }

    /// <summary>
    /// A row of 64px skeleton fields with the given star widths (time picker: 1, 1, 2; range date picker: 1, 1), each part
    /// after the first drawing a 1px <c>skeleton-element</c> line on its start edge.
    /// </summary>
    public static Grid Parts(params double[] widths)
    {
        var row = new Grid { ColumnSpacing = 0, RowSpacing = 0, InputTransparent = true };
        for (var i = 0; i < widths.Length; i++)
        {
            row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(widths[i], GridUnitType.Star)));
            row.Add(Field(CdsFieldChrome.FluidFieldHeight), i);
            if (i == 0) continue;
            var divider = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start };
            CdsTokens.SetColor(divider, BoxView.ColorProperty, CdsColorKeys.SkeletonElement);
            row.Add(divider, i);
        }
        AutomationProperties.SetIsInAccessibleTree(row, false);
        return row;
    }
}
