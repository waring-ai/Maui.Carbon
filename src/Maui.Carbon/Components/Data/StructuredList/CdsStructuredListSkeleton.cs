namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsStructuredList"/> (Carbon's <c>StructuredListSkeleton</c>): a header row of three columns
/// (8%, 30% and 15% of the width, scaled to fill it), each with a 16px bar three quarters of the column wide, over
/// <see cref="RowCount"/> empty 40px rows, every row closed by a 1px <c>border-subtle-01</c> rule. It is hidden from assistive
/// technology and takes no input.
/// </summary>
public class CdsStructuredListSkeleton : CdsControl
{
    /// <summary>The relative widths of the three columns (Carbon: 8%, 30% and 15%).</summary>
    internal static readonly double[] ColumnWeights = [8, 30, 15];

    /// <summary>Bindable property for <see cref="RowCount"/>.</summary>
    public static readonly BindableProperty RowCountProperty = BindableProperty.Create(
        nameof(RowCount), typeof(int), typeof(CdsStructuredListSkeleton), 5,
        coerceValue: static (_, v) => Math.Max(0, (int)v),
        propertyChanged: static (b, _, _) => ((CdsStructuredListSkeleton)b).Rebuild());

    readonly VerticalStackLayout _stack = new() { Spacing = 0 };

    /// <summary>Creates a skeleton with five body rows.</summary>
    public CdsStructuredListSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        SetChrome(_stack);
        Rebuild();
    }

    /// <summary>The number of empty body rows under the header (default 5).</summary>
    public int RowCount
    {
        get => (int)GetValue(RowCountProperty);
        set => SetValue(RowCountProperty, value);
    }

    /// <summary>The rows of the skeleton, header first (for tests).</summary>
    internal int BuiltRows => _stack.Children.Count;

    void Rebuild()
    {
        _stack.Children.Clear();
        _stack.Add(Header());
        // An empty body cell is its padding: 16 above and 24 below.
        for (var i = 0; i < RowCount; i++) _stack.Add(Row(new BoxView { HeightRequest = 16 + 24, Color = Colors.Transparent }));
    }

    static Grid Header()
    {
        var header = Columns();
        for (var c = 0; c < ColumnWeights.Length; c++)
        {
            // The bar fills three quarters of the cell inside the header padding.
            var cell = new Grid { Padding = CdsStructuredListModel.CellPadding(true, false, false, false, c == 0), ColumnSpacing = 0 };
            cell.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));
            cell.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            cell.Add(new CdsShimmerView { HeightRequest = 16 }, 0);
            header.Add(cell, c);
        }
        return Row(header);
    }

    static Grid Columns()
    {
        var grid = new Grid { ColumnSpacing = 0, RowSpacing = 0 };
        foreach (var weight in ColumnWeights) grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(weight, GridUnitType.Star)));
        return grid;
    }

    static Grid Row(View content)
    {
        var row = new Grid { RowSpacing = 0, InputTransparent = true };
        row.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        row.RowDefinitions.Add(new RowDefinition(1));
        row.Add(content, 0, 0);
        var rule = new BoxView { HeightRequest = 1 };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        row.Add(rule, 0, 1);
        return row;
    }
}
