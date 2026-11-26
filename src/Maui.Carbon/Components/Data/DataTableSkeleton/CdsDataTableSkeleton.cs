using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>Measurements and tokens of <see cref="CdsDataTableSkeleton"/> (from <c>data-table/skeleton</c>).</summary>
public static class CdsDataTableSkeletonTokens
{
    /// <summary>Width of a cell and header bar: 64.</summary>
    public const double BarWidth = 64;

    /// <summary>Height of a cell and header bar: 16.</summary>
    public const double BarHeight = 16;

    /// <summary>Title bar: 120 by 24.</summary>
    public static (double Width, double Height) TitleBar => (120, 24);

    /// <summary>Description bar: 160 by 16, 8 below the title.</summary>
    public static (double Width, double Height) DescriptionBar => (160, 16);

    /// <summary>Width of the toolbar's button skeleton: 150.</summary>
    public const double ToolbarButtonWidth = 150;

    /// <summary>Row, header and toolbar height: 48.</summary>
    public const double RowHeight = CdsSizing.Large;

    /// <summary>The fill of a body row: <c>layer-01</c>, or <c>layer-accent-01</c> on every second row when zebra.</summary>
    public static string RowFill(int rowIndex, bool zebra) => zebra && rowIndex % 2 == 1 ? CdsColorKeys.LayerAccent01 : CdsColorKeys.Layer01;

    /// <summary>The 1px divider under a row: <c>border-subtle-01</c>, or the row's own fill when zebra (no visible rule).</summary>
    public static string RowRule(int rowIndex, bool zebra) => zebra ? RowFill(rowIndex, true) : CdsColorKeys.BorderSubtle01;

    /// <summary>Header bars invert the shimmer tokens: they sit on <c>skeleton-element</c> and sweep in <c>skeleton-background</c>.</summary>
    public static (string Base, string Band) BarColors(bool inverted) =>
        inverted ? (CdsColorKeys.SkeletonElement, CdsColorKeys.SkeletonBackground) : (CdsColorKeys.SkeletonBackground, CdsColorKeys.SkeletonElement);
}

/// <summary>Pure sizing of <see cref="CdsSkeletonBar"/>.</summary>
internal static class SkeletonBarModel
{
    /// <summary>The measured size of a bar, no wider than <paramref name="widthConstraint"/> (its cell, padding included).</summary>
    public static Size Fit(Size measured, double widthConstraint) =>
        double.IsFinite(widthConstraint) && measured.Width > widthConstraint
            ? new Size(Math.Max(0, widthConstraint), measured.Height)
            : measured;
}

/// <summary>
/// A skeleton bar like <see cref="CdsSkeletonText"/>'s, optionally with the tokens swapped (header bars). Decorative. It
/// does not animate itself: its <see cref="CdsDataTableSkeleton"/> runs one loop for all of its bars.
/// </summary>
internal sealed class CdsSkeletonBar : Grid
{
    readonly BoxView _background = new();
    readonly BoxView _band = new() { AnchorX = 0 };

    public CdsSkeletonBar(double width, double height, bool inverted)
    {
        var (baseKey, bandKey) = CdsDataTableSkeletonTokens.BarColors(inverted);
        CdsTokens.SetColor(_background, BoxView.ColorProperty, baseKey);
        CdsTokens.SetColor(_band, BoxView.ColorProperty, bandKey);
        Children.Add(_background);
        Children.Add(_band);
        WidthRequest = width;
        HeightRequest = height;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        InputTransparent = true;
        CascadeInputTransparent = true;
        IsClippedToBounds = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        // A bar never asks for more than its cell: in a column narrower than the bar and its padding it shrinks to fit,
        // rather than running past the column (and, in the last column, past the table's edge).
        var size = base.MeasureOverride(widthConstraint, heightConstraint);
        return SkeletonBarModel.Fit(size, widthConstraint);
    }

    /// <summary>Shows the sweep at <paramref name="phase"/> (0 to 1).</summary>
    public void Apply(double phase)
    {
        var (scale, fromStart, opacity) = ShimmerModel.At(phase);
        _band.AnchorX = fromStart ? 0 : 1;
        _band.ScaleX = Math.Max(scale, 0.001);
        _band.Opacity = scale <= 0.001 ? 0 : opacity;
    }
}

/// <summary>
/// A loading placeholder in the shape of a data table: an optional title block, a toolbar with a button skeleton, a header row
/// and <see cref="RowCount"/> body rows of <see cref="ColumnCount"/> pulsing 64 by 16 bars (48 high rows, <c>border-subtle-01</c>
/// dividers; header bars invert the skeleton tokens). Show it in the place the table will appear while the first rows load, and
/// match <see cref="ColumnCount"/>, <see cref="Headers"/> and about <see cref="RowCount"/> to the real table so nothing shifts;
/// keep real titles and headers visible when you know them. When reloading or paging a visible table keep its rows and use an
/// inline loading indicator. The bars stop pulsing under reduced motion. The skeleton is decorative: name it with
/// <see cref="Label"/> ("Clusters (loading)"), mark the region busy and announce completion with the loaded content.
/// </summary>
[ContentProperty(nameof(Headers))]
public class CdsDataTableSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="RowCount"/>.</summary>
    public static readonly BindableProperty RowCountProperty = BindableProperty.Create(
        nameof(RowCount), typeof(int), typeof(CdsDataTableSkeleton), 5,
        coerceValue: static (_, v) => Math.Max(0, (int)v), propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ColumnCount"/>.</summary>
    public static readonly BindableProperty ColumnCountProperty = BindableProperty.Create(
        nameof(ColumnCount), typeof(int), typeof(CdsDataTableSkeleton), 5,
        coerceValue: static (_, v) => Math.Max(1, (int)v), propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ShowHeader"/>.</summary>
    public static readonly BindableProperty ShowHeaderProperty = BindableProperty.Create(
        nameof(ShowHeader), typeof(bool), typeof(CdsDataTableSkeleton), true, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ShowToolbar"/>.</summary>
    public static readonly BindableProperty ShowToolbarProperty = BindableProperty.Create(
        nameof(ShowToolbar), typeof(bool), typeof(CdsDataTableSkeleton), true, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="UseZebraStyles"/>.</summary>
    public static readonly BindableProperty UseZebraStylesProperty = BindableProperty.Create(
        nameof(UseZebraStyles), typeof(bool), typeof(CdsDataTableSkeleton), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsDataTableSkeleton), "Table (loading)", propertyChanged: OnChanged);

    const string AnimationName = "cds-skeleton-bars";
    const double RestPhase = 0.2;
    readonly CdsItemCollection<string> _headers = [];
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly List<CdsSkeletonBar> _bars = [];
    double _phase = RestPhase;

    /// <summary>Creates a 5 by 5 skeleton with a title block and toolbar.</summary>
    public CdsDataTableSkeleton()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        _headers.CollectionChanged += (_, _) => Rebuild();
        SetChrome(_root);
        Rebuild();
        // One loop sweeps every bar in step (a table of bars as one loop each kept the UI thread busy).
        Loaded += (_, _) => CdsLoop.Start(this, AnimationName, ApplyPhase, RestPhase, ShimmerModel.LoopMilliseconds);
        Unloaded += (_, _) => CdsLoop.Stop(this, AnimationName);
    }

    /// <summary>The bars the shimmer sweeps (header, toolbar, column-header and cell bars).</summary>
    internal IReadOnlyList<CdsSkeletonBar> Bars => _bars;

    /// <summary>Body rows. Default 5.</summary>
    public int RowCount
    {
        get => (int)GetValue(RowCountProperty);
        set => SetValue(RowCountProperty, value);
    }

    /// <summary>Columns. Default 5 (the number of <see cref="Headers"/> when more are given).</summary>
    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    /// <summary>Real header labels shown instead of header bars (the content property).</summary>
    public IList<string> Headers => _headers;

    /// <summary>Shows the title and description bars above the table. Default true.</summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    /// <summary>Shows the toolbar with a button skeleton. Default true.</summary>
    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    /// <summary>Alternates the row fill with <c>layer-accent-01</c>.</summary>
    public bool UseZebraStyles
    {
        get => (bool)GetValue(UseZebraStylesProperty);
        set => SetValue(UseZebraStylesProperty, value);
    }

    /// <summary>The accessible name ("Clusters (loading)"). The bars themselves are invisible to screen readers.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsDataTableSkeleton)b).Rebuild();

    void ApplyPhase(double phase)
    {
        _phase = phase;
        foreach (var bar in _bars) bar.Apply(phase);
    }

    CdsSkeletonBar Bar(double width, double height, bool inverted)
    {
        var bar = new CdsSkeletonBar(width, height, inverted);
        bar.Apply(_phase);
        _bars.Add(bar);
        return bar;
    }

    void Rebuild()
    {
        _root.Children.Clear();
        _bars.Clear();
        var columns = Math.Max(ColumnCount, _headers.Count);

        if (ShowHeader)
        {
            var block = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing06) };
            CdsTokens.SetColor(block, BackgroundColorProperty, CdsColorKeys.Layer01);
            block.Add(Bar(CdsDataTableSkeletonTokens.TitleBar.Width, CdsDataTableSkeletonTokens.TitleBar.Height, false));
            block.Add(Bar(CdsDataTableSkeletonTokens.DescriptionBar.Width, CdsDataTableSkeletonTokens.DescriptionBar.Height, false));
            _root.Add(block);
        }

        if (ShowToolbar)
        {
            var toolbar = new Grid { HeightRequest = CdsDataTableSkeletonTokens.RowHeight };
            CdsTokens.SetColor(toolbar, BackgroundColorProperty, CdsColorKeys.Layer01);
            var button = Bar(CdsDataTableSkeletonTokens.ToolbarButtonWidth, CdsDataTableSkeletonTokens.RowHeight, false);
            button.HorizontalOptions = LayoutOptions.End;
            button.VerticalOptions = LayoutOptions.Fill;
            toolbar.Add(button);
            SemanticProperties.SetDescription(toolbar, "data table toolbar");
            _root.Add(toolbar);
        }

        var table = new Grid { RowSpacing = 0, ColumnSpacing = 0 };
        for (var c = 0; c < columns; c++) table.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // Header row.
        table.RowDefinitions.Add(new RowDefinition(new GridLength(CdsDataTableSkeletonTokens.RowHeight)));
        var headerFill = new BoxView { InputTransparent = true };
        CdsTokens.SetColor(headerFill, BoxView.ColorProperty, CdsColorKeys.LayerAccent01);
        table.Add(headerFill, 0, 0);
        Grid.SetColumnSpan(headerFill, columns);
        for (var c = 0; c < columns; c++)
        {
            View cell = c < _headers.Count
                ? new CdsText(_headers[c], CdsTypeStyle.HeadingCompact01) { VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation }
                : Bar(CdsDataTableSkeletonTokens.BarWidth, CdsDataTableSkeletonTokens.BarHeight, true);
            cell.Margin = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, 0);
            table.Add(cell, c, 0);
        }

        // Body rows.
        for (var r = 0; r < RowCount; r++)
        {
            table.RowDefinitions.Add(new RowDefinition(new GridLength(CdsDataTableSkeletonTokens.RowHeight)));
            var fill = new BoxView { InputTransparent = true };
            CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsDataTableSkeletonTokens.RowFill(r, UseZebraStyles));
            table.Add(fill, 0, r + 1);
            Grid.SetColumnSpan(fill, columns);
            var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
            CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsDataTableSkeletonTokens.RowRule(r, UseZebraStyles));
            table.Add(rule, 0, r + 1);
            Grid.SetColumnSpan(rule, columns);
            for (var c = 0; c < columns; c++)
            {
                var bar = Bar(CdsDataTableSkeletonTokens.BarWidth, CdsDataTableSkeletonTokens.BarHeight, false);
                bar.Margin = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, 0);
                table.Add(bar, c, r + 1);
            }
        }
        _root.Add(table);
        SemanticProperties.SetDescription(_root, Label);
    }
}
