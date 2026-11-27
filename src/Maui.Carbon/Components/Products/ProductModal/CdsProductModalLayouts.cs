using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// Gives its one child the offered width less an end inset computed from that width: the <c>padding-inline-end: 20%</c>
/// (and <c>calc(20% - 1rem)</c>) measures of the product modals' titles and paragraphs, decided while measuring.
/// </summary>
internal sealed class CdsEndInset : Layout
{
    readonly Func<double, double> _inset;

    public CdsEndInset(View child, Func<double, double> inset)
    {
        _inset = inset;
        Children.Add(child);
    }

    /// <summary>The wrapped view.</summary>
    public View Child => (View)Children[0];

    /// <summary>The end inset for a width.</summary>
    public double InsetFor(double width) => Math.Clamp(_inset(width), 0, Math.Max(0, width));

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsEndInset owner) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var child = (IView)owner.Children[0];
            if (double.IsInfinity(widthConstraint)) return child.Measure(widthConstraint, heightConstraint);
            var size = child.Measure(widthConstraint - owner.InsetFor(widthConstraint), heightConstraint);
            return new Size(widthConstraint, size.Height);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var child = (IView)owner.Children[0];
            child.Arrange(new Rect(bounds.X, bounds.Y, bounds.Width - owner.InsetFor(bounds.Width), bounds.Height));
            return bounds.Size;
        }
    }
}

/// <summary>Builders for the small parts the product modals share.</summary>
internal static class CdsProductModalParts
{
    /// <summary>A wrapping paragraph with an end inset (<c>__body</c>, <c>__description</c>).</summary>
    public static (CdsEndInset Box, CdsText Text) Paragraph(CdsTypeStyle style, Func<double, double> inset, Thickness margin)
    {
        var text = new CdsText { TypeStyle = style, LineBreakMode = LineBreakMode.WordWrap };
        var box = new CdsEndInset(text, inset) { Margin = margin, IsVisible = false };
        return (box, text);
    }

    /// <summary>
    /// A status line (<c>__messaging</c>): a 16px icon and its text 8 to the end, the text vertically centered on the icon.
    /// The icon carries no name: the text says what happened.
    /// </summary>
    public static (Grid Row, CdsIcon Icon, CdsText Text) MessageRow(CdsIconName icon, string colorKey, CdsTypeStyle style)
    {
        var glyph = new CdsIcon(icon, 16) { VerticalOptions = LayoutOptions.Start };
        CdsTokens.SetColor(glyph, CdsIcon.ColorProperty, colorKey);
        var text = new CdsText { TypeStyle = style, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0), VerticalOptions = LayoutOptions.Center };
        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            IsVisible = false,
        };
        row.Add(glyph, 0);
        row.Add(text, 1);
        return (row, glyph, text);
    }
}
