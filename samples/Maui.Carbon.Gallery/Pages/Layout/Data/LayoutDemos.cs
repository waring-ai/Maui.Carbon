using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Gallery.Pages.Layout.Data;

/// <summary>Small builders shared by the layout and navigation gallery pages (package F).</summary>
internal static class LayoutDemos
{
    /// <summary>The grid preview's demo cell: <c>layer-accent-01</c> with a <c>border-interactive</c> outline.</summary>
    public static Border Cell(string text, double minHeight = 32)
    {
        var label = new CdsText(text, CdsTypeStyle.Label01)
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
        };
        var cell = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new Rectangle(),
            MinimumHeightRequest = minHeight,
            Padding = new Thickness(4, 8),
            Content = label,
        };
        CdsTokens.SetColor(cell, VisualElement.BackgroundColorProperty, CdsColorKeys.LayerAccent01);
        CdsTokens.SetBrush(cell, Border.StrokeProperty, CdsColorKeys.BorderInteractive);
        return cell;
    }

    /// <summary>A grid column holding a demo cell.</summary>
    public static CdsColumn Column(string text, int? span = null, int? sm = null, int? md = null, int? lg = null, int? offset = null, int? offsetLg = null)
    {
        var column = new CdsColumn { Content = Cell(text) };
        column.Span = span;
        column.SpanSm = sm;
        column.SpanMd = md;
        column.SpanLg = lg;
        column.Offset = offset;
        column.OffsetLg = offsetLg;
        return column;
    }
}
