namespace Maui.Carbon;

/// <summary>
/// Shows a scene's text as real <see cref="CdsText"/> labels over the canvas, positioned absolutely at their anchor
/// points. Labels are pooled: a rebuild reuses them. The layer never takes input.
/// </summary>
internal sealed class CdsChartLabelLayer : AbsoluteLayout
{
    readonly List<CdsText> _pool = [];
    int _used;

    public CdsChartLabelLayer()
    {
        InputTransparent = true;
        CascadeInputTransparent = true;
        IsClippedToBounds = false;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    /// <summary>The labels in use (for tests).</summary>
    internal IEnumerable<CdsText> Labels => _pool.Take(_used);

    public void Show(IReadOnlyList<ChartText> texts, ChartColors colors, string? legendGroup)
    {
        BatchBegin();
        try
        {
            for (var i = 0; i < texts.Count; i++)
            {
                var label = i < _pool.Count ? _pool[i] : Create();
                Apply(label, texts[i], colors, legendGroup);
            }
            for (var i = texts.Count; i < _used; i++) _pool[i].IsVisible = false;
            _used = texts.Count;
        }
        finally
        {
            BatchCommit();
        }
    }

    CdsText Create()
    {
        var label = new CdsText
        {
            LineBreakMode = LineBreakMode.NoWrap,
            MaxLines = 1,
            InputTransparent = true,
            FontAutoScalingEnabled = false,
            VerticalTextAlignment = TextAlignment.Center,
            CharacterSpacing = 0,
        };
        AutomationProperties.SetIsInAccessibleTree(label, false);
        _pool.Add(label);
        Children.Add(label);
        return label;
    }

    static void Apply(CdsText label, ChartText t, ChartColors colors, string? legendGroup)
    {
        // A generous frame: the text is aligned inside it by its anchor, so a font that renders slightly wider than the
        // measured advances still sits exactly at its anchor point.
        var w = (float)Math.Ceiling(t.Width * 1.15f + 8);
        var h = t.Font.LineHeight;
        var x = t.Anchor switch { ChartTextAnchor.Middle => t.X - w / 2, ChartTextAnchor.End => t.X - w, _ => t.X };
        var y = t.Baseline switch { ChartTextBaseline.Middle => t.Y - h / 2, ChartTextBaseline.Bottom => t.Y - h, _ => t.Y };
        label.Text = t.Text;
        label.FontFamily = t.Font.Alias;
        label.FontSize = t.Font.Size;
        label.LineHeight = 1;
        label.HorizontalTextAlignment = t.Anchor switch { ChartTextAnchor.Middle => TextAlignment.Center, ChartTextAnchor.End => TextAlignment.End, _ => TextAlignment.Start };
        label.AnchorX = t.Anchor switch { ChartTextAnchor.Middle => 0.5, ChartTextAnchor.End => 1, _ => 0 };
        label.AnchorY = t.Baseline switch { ChartTextBaseline.Middle => 0.5, ChartTextBaseline.Bottom => 1, _ => 0 };
        label.Rotation = t.Rotation;
        var opacity = t.Opacity;
        if (t.FadesWithLegend && legendGroup is not null && t.Group is not null && t.Group != legendGroup) opacity *= 0.3f;
        label.Opacity = opacity;
        if (t.Color.Kind == ChartPaintKind.Key) CdsTokens.SetColor(label, Label.TextColorProperty, t.Color.Name);
        else label.TextColor = colors.Resolve(t.Color);
        label.IsVisible = true;
        SetLayoutBounds(label, new Rect(x, y, w, h));
    }
}
