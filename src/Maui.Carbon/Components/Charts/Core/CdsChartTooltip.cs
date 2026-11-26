namespace Maui.Carbon;

/// <summary>
/// The chart tooltip (Carbon's <c>cds--cc--tooltip</c>): a <c>layer-02</c> surface with a soft shadow; one row per
/// value with a 4px series color strip, rows divided by <c>tooltip-line-border</c>, a bold total last. It never takes input.
/// </summary>
internal sealed class CdsChartTooltip : ContentView
{
    readonly Border _surface;
    readonly VerticalStackLayout _rows = new() { Spacing = 0 };

    public CdsChartTooltip()
    {
        InputTransparent = true;
        IsVisible = false;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        _surface = new Border { StrokeThickness = 0, Padding = 0, Content = _rows, MaximumWidthRequest = 270 };
        CdsTokens.SetColor(_surface, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(_surface, CdsColorKeys.Shadow, 1, 6);
        // One layer up from where the chart sits: the surface's layer-01 key resolves to layer-02.
        Content = new CdsLayer { Body = _surface, InputTransparent = true };
    }

    public void Show(ChartMark mark, ChartColors colors)
    {
        _rows.Clear();
        if (mark.Rows.Count == 0)
        {
            _rows.Add(Text(mark.Title ?? "", ChartFont.Tick, new Thickness(6, 6, 6, 6)));
        }
        else
        {
            if (mark.Title is { Length: > 0 } title) AddRow(new ChartTooltipRow(title, null), colors, first: true);
            for (var i = 0; i < mark.Rows.Count; i++) AddRow(mark.Rows[i], colors, first: i == 0 && mark.Title is not { Length: > 0 });
        }
        IsVisible = true;
    }

    public void Hide() => IsVisible = false;

    void AddRow(ChartTooltipRow row, ChartColors colors, bool first)
    {
        if (!first)
        {
            var line = new BoxView { HeightRequest = 1, InputTransparent = true };
            line.Color = colors.Palette.Token(CdsChartToken.TooltipLineBorder);
            _rows.Add(line);
        }
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            InputTransparent = true,
            MinimumWidthRequest = 20,
        };
        if (!row.Color.IsNone)
        {
            var strip = new BoxView { WidthRequest = 4, InputTransparent = true, Color = colors.Resolve(row.Color) };
            grid.Add(strip, 0);
        }
        var font = row.Bold ? new ChartFont(ChartFontFamily.CondensedSemiBold, 12) : ChartFont.Tick;
        var left = row.Color.IsNone ? 4 : 8;
        grid.Add(Text(row.Label, font, new Thickness(left, 4, 8, 4)), 1);
        if (!string.IsNullOrEmpty(row.Value)) grid.Add(Text(row.Value, font, new Thickness(6, 4, 4, 4)), 2);
        _rows.Add(grid);
    }

    static CdsText Text(string text, ChartFont font, Thickness margin)
    {
        var t = new CdsText
        {
            Text = text,
            FontFamily = font.Alias,
            FontSize = font.Size,
            CharacterSpacing = 0,
            LineHeight = 1,
            FontAutoScalingEnabled = false,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            Margin = margin,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
        };
        return t;
    }
}
