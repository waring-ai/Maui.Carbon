namespace Maui.Carbon;

/// <summary>Non-interactive legend entries that explain marks which are not series (Carbon's <c>legend.additionalItems</c>).</summary>
internal enum ChartLegendExtraKind
{
    /// <summary>Two nested circles: bubble size.</summary>
    Radius,
    /// <summary>A short line: a reference or trend line.</summary>
    Line,
    /// <summary>A zone-filled box (performance areas 1 to 3 of a bullet chart).</summary>
    Area,
    /// <summary>Two nested boxes: size.</summary>
    Size,
    /// <summary>A box with a tick: quartiles.</summary>
    Quartile,
    /// <summary>A zoom-in magnifier: "Click to zoom" (circle pack with zoom on).</summary>
    Zoom,
}

internal sealed record ChartLegendExtra(ChartLegendExtraKind Kind, string Name, int Index = 0);

/// <summary>
/// The chart legend (Carbon's <c>cds--cc--legend</c>): one checkbox item per series, clickable to isolate series, and the
/// additional items after them. Wraps in rows (top or bottom) or stacks (left or right).
/// </summary>
internal sealed class CdsChartLegend : ContentView
{
    readonly CdsChart _chart;
    readonly FlexLayout _flex = new() { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Direction = Microsoft.Maui.Layouts.FlexDirection.Row, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly List<CdsChartLegendItem> _items = [];
    bool _vertical;

    public CdsChartLegend(CdsChart chart)
    {
        _chart = chart;
        Content = _flex;
        CdsSemantics.SetGroup(this, "Data groups");
    }

    internal IReadOnlyList<CdsChartLegendItem> Items => _items;

    public void SetOrientation(bool vertical, CdsChartLegendAlignment alignment)
    {
        _flex.JustifyContent = alignment switch
        {
            CdsChartLegendAlignment.Center => Microsoft.Maui.Layouts.FlexJustify.Center,
            CdsChartLegendAlignment.End => Microsoft.Maui.Layouts.FlexJustify.End,
            _ => Microsoft.Maui.Layouts.FlexJustify.Start,
        };
        if (_vertical == vertical && Content is not null) return;
        _vertical = vertical;
        var children = _flex.Children.Concat(_stack.Children).Cast<View>().ToList();
        _flex.Clear();
        _stack.Clear();
        var target = vertical ? (Layout)_stack : _flex;
        foreach (var c in children) target.Add(c);
        Content = target;
    }

    /// <summary>Rebuilds the items for the groups (reusing views by position) and their active state and colors.</summary>
    public void Update(IReadOnlyList<string> groups, Func<string, bool> isActive, bool hasDeactivated, bool clickable, ChartColors colors, IReadOnlyList<ChartLegendExtra> extras)
    {
        var host = _vertical ? (Layout)_stack : _flex;
        host.Clear();
        for (var i = 0; i < groups.Count; i++)
        {
            var item = i < _items.Count ? _items[i] : Add();
            item.Set(groups[i], isActive(groups[i]), hasDeactivated, clickable, colors);
            host.Add(item);
        }
        if (_items.Count > groups.Count) _items.RemoveRange(groups.Count, _items.Count - groups.Count);
        foreach (var extra in extras) host.Add(new CdsChartLegendExtraView(extra, colors));
    }

    CdsChartLegendItem Add()
    {
        var item = new CdsChartLegendItem(_chart);
        _items.Add(item);
        return item;
    }
}

/// <summary>One series item of the legend: a 13px checkbox in the series color and a label; a checkbox to screen readers.</summary>
internal sealed class CdsChartLegendItem : CdsControl
{
    readonly WeakReference<CdsChart> _chart;
    readonly Border _box = new() { WidthRequest = 13, HeightRequest = 13, StrokeThickness = 1, Padding = 0, InputTransparent = true, VerticalOptions = LayoutOptions.Center };
    readonly Border _ring = new() { WidthRequest = 17, HeightRequest = 17, StrokeThickness = 2, Padding = 0, InputTransparent = true, IsVisible = false, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(-2, 0, 0, 0) };
    readonly CdsIcon _check = new(CdsIconName.Checkmark, 11) { IsVisible = false };
    readonly CdsText _label = new() { FontFamily = CdsFonts.SansCondensed, FontSize = 12, CharacterSpacing = 0, FontAutoScalingEnabled = false, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.NoWrap, MaxLines = 1, VerticalTextAlignment = TextAlignment.Center, InputTransparent = true };
    readonly CdsHitTarget _hit;
    string _group = "";
    bool _active;
    bool _clickable = true;
    ChartColors? _colors;

    public CdsChartLegendItem(CdsChart chart)
    {
        _chart = new WeakReference<CdsChart>(chart);
        Margin = new Thickness(0, 0, CdsSpacing.Spacing04, CdsSpacing.Spacing03);
        HorizontalOptions = LayoutOptions.Start;
        var rr = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 };
        _box.StrokeShape = rr;
        _ring.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 3 };
        _box.Content = _check;
        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = CdsSpacing.Spacing02,
            InputTransparent = true,
        };
        row.Add(_ring, 0);
        row.Add(_box, 0);
        row.Add(_label, 1);
        AutomationProperties.SetExcludedWithChildren(row, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Checkbox);
        var root = new Grid();
        root.Add(row);
        root.Add(_hit);
        SetChrome(root);
    }

    public string Group => _group;

    public bool IsActive => _active;

    public void Set(string group, bool active, bool hasDeactivated, bool clickable, ChartColors colors)
    {
        _group = group;
        _active = active;
        _clickable = clickable;
        _colors = colors;
        _label.Text = CdsChartTruncate.Apply(group);
        _check.IsVisible = active && hasDeactivated;
        _hit.IsChecked = active;
        _hit.IsEnabled = clickable;
        SemanticProperties.SetDescription(_hit, group);
        RefreshVisuals();
    }

    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        var hover = Is(CdsInteractionState.Hover) || Is(CdsInteractionState.FocusVisible);
        var was = (previous & (CdsInteractionState.Hover | CdsInteractionState.FocusVisible)) != 0;
        if (hover != was) Hover(hover);
    }

    void Hover(bool on)
    {
        if (_chart.TryGetTarget(out var chart)) chart.HoverLegend(on ? _group : null, _group);
    }

    protected override void ApplyVisuals()
    {
        var focus = _clickable && (Is(CdsInteractionState.Hover) || Is(CdsInteractionState.FocusVisible));
        _ring.IsVisible = focus;
        SetBrush(_ring, Border.StrokeProperty, CdsColorKeys.Focus);
        SetColor(_check, CdsIcon.ColorProperty, CdsColorKeys.Background);
        if (_active && _colors is not null)
        {
            SetBrush(_box, Border.StrokeProperty, CdsColorKeys.Background);
            // A series color is not a resource key: set it directly (re-applied when the chart's theme changes).
            ForgetColors(new HashSet<BindableObject> { _box });
            _box.BackgroundColor = _colors.Series(_group);
        }
        else
        {
            SetBrush(_box, Border.StrokeProperty, CdsColorKeys.TextSecondary);
            SetColor(_box, BackgroundColorProperty, CdsColorKeys.Background);
        }
    }

    protected override void OnActivated()
    {
        if (_clickable && _chart.TryGetTarget(out var chart)) chart.ToggleGroup(_group);
    }
}

/// <summary>An additional (non-series) legend entry: an icon drawn in zone and stroke colors, and its name.</summary>
internal sealed class CdsChartLegendExtraView : ContentView, IDrawable
{
    readonly ChartLegendExtra _extra;
    readonly ChartColors _colors;

    public CdsChartLegendExtraView(ChartLegendExtra extra, ChartColors colors)
    {
        _extra = extra;
        _colors = colors;
        InputTransparent = true;
        Margin = new Thickness(0, 0, CdsSpacing.Spacing04, CdsSpacing.Spacing03);
        HorizontalOptions = LayoutOptions.Start;
        var (w, h) = extra.Kind switch
        {
            ChartLegendExtraKind.Radius => (14, 14),
            ChartLegendExtraKind.Line => (24, 12),
            ChartLegendExtraKind.Area => (24, 14),
            ChartLegendExtraKind.Zoom => (12, 12),
            _ => (24, 13),
        };
        var icon = new GraphicsView { WidthRequest = w, HeightRequest = h, Drawable = new CdsWeakDrawable(this), InputTransparent = true, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center };
        var label = new CdsText { Text = extra.Name, FontFamily = CdsFonts.SansCondensed, FontSize = 12, CharacterSpacing = 0, FontAutoScalingEnabled = false, Tone = CdsTextTone.Secondary, VerticalTextAlignment = TextAlignment.Center };
        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing02, InputTransparent = true };
        row.Add(icon);
        row.Add(label);
        Content = row;
    }

    void IDrawable.Draw(ICanvas canvas, RectF r)
    {
        var p = _colors.Palette;
        var gray = p.Token(CdsChartToken.ZoneStroke);
        canvas.StrokeSize = 1;
        switch (_extra.Kind)
        {
            case ChartLegendExtraKind.Radius:
                canvas.StrokeColor = gray;
                canvas.DrawCircle(7, 7, 6.5f);
                canvas.DrawCircle(7, 10, 3.5f);
                break;
            case ChartLegendExtraKind.Line:
                canvas.StrokeColor = gray;
                canvas.StrokeSize = 1.4f;
                canvas.DrawLine(0, 6, 24, 6);
                break;
            case ChartLegendExtraKind.Area:
                canvas.FillColor = p.Token(_extra.Index switch { 1 => CdsChartToken.ZoneFill02, >= 2 => CdsChartToken.ZoneFill03, _ => CdsChartToken.ZoneFill01 });
                canvas.FillRectangle(0.5f, 0.5f, 23, 13);
                canvas.StrokeColor = gray;
                canvas.DrawRectangle(0.5f, 0.5f, 23, 13);
                break;
            case ChartLegendExtraKind.Size:
                canvas.StrokeColor = gray;
                canvas.DrawRectangle(0.5f, 0.5f, 23, 12);
                canvas.DrawRectangle(0.5f, 6.5f, 13, 6);
                break;
            case ChartLegendExtraKind.Quartile:
                canvas.FillColor = p.Token(CdsChartToken.ZoneFill02);
                canvas.FillRectangle(0, 0, 24, 13);
                canvas.StrokeColor = gray;
                canvas.DrawRectangle(0.5f, 0.5f, 23, 12);
                canvas.FillColor = p.Token(CdsChartToken.LayerInverseAbsolute);
                canvas.FillRectangle(11, 4, 1, 4);
                break;
            case ChartLegendExtraKind.Zoom:
                // Carbon's zoom-in glyph in the additional items' gray: a lens with a plus and a handle.
                canvas.StrokeColor = gray;
                canvas.StrokeSize = 1.2f;
                canvas.DrawCircle(5, 5, 4.2f);
                canvas.DrawLine(3, 5, 7, 5);
                canvas.DrawLine(5, 3, 5, 7);
                canvas.StrokeSize = 1.6f;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawLine(8.2f, 8.2f, 11.2f, 11.2f);
                break;
        }
    }
}
