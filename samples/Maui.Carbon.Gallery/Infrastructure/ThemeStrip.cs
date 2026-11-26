using Microsoft.Maui.Layouts;

namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>
/// Renders <see cref="Template"/> four times, once in each Carbon theme zone, so one screenshot shows every theme. Set
/// <see cref="Layered"/> to also wrap each copy in a <see cref="CdsLayer"/>.
/// </summary>
/// <remarks>
/// The zones sit side by side while every copy fits a quarter of the width. When a copy is wider than its zone (a
/// fixed 288 toast, a 320 form), the strip wraps to two columns, then to one, so no example runs into the next zone.
/// Content that shrinks to fit instead of overflowing (a diagram that scales its text down) cannot be detected: give
/// it the width it needs with <see cref="MinimumZoneWidth"/>.
/// </remarks>
public class ThemeStrip : ContentView
{
    public static readonly BindableProperty TemplateProperty = BindableProperty.Create(
        nameof(Template), typeof(DataTemplate), typeof(ThemeStrip), null,
        propertyChanged: static (b, _, _) => ((ThemeStrip)b).Build());

    public static readonly BindableProperty LayeredProperty = BindableProperty.Create(
        nameof(Layered), typeof(bool), typeof(ThemeStrip), false,
        propertyChanged: static (b, _, _) => ((ThemeStrip)b).Build());

    public static readonly BindableProperty MinimumZoneWidthProperty = BindableProperty.Create(
        nameof(MinimumZoneWidth), typeof(double), typeof(ThemeStrip), 0d,
        propertyChanged: static (b, _, _) => ((ThemeStrip)b).Build());

    public DataTemplate? Template
    {
        get => (DataTemplate?)GetValue(TemplateProperty);
        set => SetValue(TemplateProperty, value);
    }

    public bool Layered
    {
        get => (bool)GetValue(LayeredProperty);
        set => SetValue(LayeredProperty, value);
    }

    /// <summary>The narrowest a theme zone may get before the strip wraps to fewer columns. Default 0.</summary>
    public double MinimumZoneWidth
    {
        get => (double)GetValue(MinimumZoneWidthProperty);
        set => SetValue(MinimumZoneWidthProperty, value);
    }

    void Build()
    {
        if (Template is null)
        {
            Content = null;
            return;
        }
        // The example's own width inside a zone: the zone padding, plus the layer's padding when layered.
        var inset = 2 * CdsSpacing.Spacing05 + (Layered ? 2 * CdsSpacing.Spacing05 : 0);
        var zones = new ThemeStripLayout(MinimumZoneWidth, inset);
        foreach (var theme in Enum.GetValues<CdsThemeName>())
        {
            var body = (View)Template.CreateContent();
            var column = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05, Padding = new Thickness(CdsSpacing.Spacing05) };
            column.Add(new CdsText(theme switch
            {
                CdsThemeName.White => "White",
                CdsThemeName.G10 => "Gray 10",
                CdsThemeName.G90 => "Gray 90",
                _ => "Gray 100",
            }, CdsTypeStyle.Label01, CdsTextTone.Secondary));
            column.Add(Layered ? new CdsLayer { WithBackground = true, Body = new ContentView { Padding = CdsSpacing.Spacing05, Content = body } } : body);
            zones.AddZone(new CdsTheme { Theme = theme, Body = column }, body);
        }
        Content = zones;
    }
}

/// <summary>
/// Lays the theme zones out in 4, 2 or 1 equal columns: the most columns in which every example fits its zone, measured
/// by whether any view in it asks for more width than the zone has.
/// </summary>
sealed class ThemeStripLayout(double minimumZoneWidth, double inset) : Layout
{
    readonly List<View> _bodies = [];
    readonly double _minimumZoneWidth = minimumZoneWidth;
    readonly double _inset = inset;

    internal int Columns { get; private set; } = 1;

    internal void AddZone(View zone, View body)
    {
        Add(zone);
        _bodies.Add(body);
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);


    /// <summary>True when nothing in <paramref name="body"/> asks for more than <paramref name="width"/>.</summary>
    static bool Fits(IVisualTreeElement element, double width)
    {
        if (element is VisualElement { IsVisible: false })
            return true;
        if (element is VisualElement view && (view.DesiredSize.Width > width + 0.5 || view.WidthRequest > width + 0.5))
            return false;
        // A horizontal scroller is meant to hold content wider than itself.
        if (element is ScrollView { Orientation: ScrollOrientation.Horizontal or ScrollOrientation.Both })
            return true;
        foreach (var child in element.GetVisualChildren())
        {
            if (!Fits(child, width))
                return false;
        }
        return true;
    }

    sealed class Manager(ThemeStripLayout strip) : LayoutManager(strip)
    {
        double _measuredWidth = -1;

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var count = strip.Count;
            if (count == 0)
                return Size.Zero;
            if (double.IsInfinity(widthConstraint))
            {
                double width = 0, height = 0;
                for (var i = 0; i < count; i++)
                {
                    var size = strip[i].Measure(double.PositiveInfinity, double.PositiveInfinity);
                    width += size.Width;
                    height = Math.Max(height, size.Height);
                }
                strip.Columns = count;
                _measuredWidth = -1;
                return new Size(width, height);
            }

            var columns = count;
            while (true)
            {
                var zoneWidth = widthConstraint / columns;
                if (columns == 1 || zoneWidth >= strip._minimumZoneWidth)
                {
                    for (var i = 0; i < count; i++)
                        strip[i].Measure(zoneWidth, double.PositiveInfinity);
                    if (columns == 1 || strip._bodies.TrueForAll(b => Fits(b, zoneWidth - strip._inset)))
                        break;
                }
                columns = columns > 2 ? (columns + 1) / 2 : 1;
            }
            strip.Columns = columns;
            _measuredWidth = widthConstraint;
            return new Size(widthConstraint, RowHeights(columns).Sum());
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            if (strip.Count == 0)
                return bounds.Size;
            if (Math.Abs(bounds.Width - _measuredWidth) > 0.5)
                Measure(bounds.Width, double.PositiveInfinity);
            var columns = strip.Columns;
            var zoneWidth = bounds.Width / columns;
            var rows = RowHeights(columns);
            var y = bounds.Top;
            for (var row = 0; row < rows.Count; row++)
            {
                for (var column = 0; column < columns && row * columns + column < strip.Count; column++)
                    strip[row * columns + column].Arrange(new Rect(bounds.Left + column * zoneWidth, y, zoneWidth, rows[row]));
                y += rows[row];
            }
            return new Size(bounds.Width, y - bounds.Top);
        }

        List<double> RowHeights(int columns)
        {
            var rows = new List<double>();
            for (var i = 0; i < strip.Count; i += columns)
            {
                double height = 0;
                for (var j = i; j < Math.Min(i + columns, strip.Count); j++)
                    height = Math.Max(height, strip[j].DesiredSize.Height);
                rows.Add(height);
            }
            return rows;
        }
    }

}
