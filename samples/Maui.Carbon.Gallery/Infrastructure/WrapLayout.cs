using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>
/// A row of children that wraps onto new lines (what a <see cref="FlexLayout"/> with <c>Wrap="Wrap"</c> is used for),
/// placing each child where its margins say. Use it instead of a FlexLayout for rows of cards, tiles, buttons and tags.
/// </summary>
/// <remarks>
/// MAUI's FlexLayout counts margins twice. A child's measured size already includes its margins and the flex engine
/// adds them again, so a measured child sits its end margin twice from the next one and its row is its bottom margin
/// too tall (<c>Margin="0,0,24,24"</c> puts rows 48 apart). A child with a WidthRequest gets a slot without its
/// margins, which its own arrange shrinks by them again, so it moves left by half its horizontal margins. This layout
/// lays rows out itself: each child takes its measured size (margins included, once), a child that would overflow the
/// row starts the next one, and a row is as tall as its tallest child. <see cref="AlignItems"/> places children in
/// their row and <see cref="JustifyContent"/> places each row in the width, as their FlexLayout namesakes do.
/// </remarks>
public class WrapLayout : Layout
{
    /// <summary>Bindable property for <see cref="AlignItems"/>.</summary>
    public static readonly BindableProperty AlignItemsProperty = BindableProperty.Create(
        nameof(AlignItems), typeof(FlexAlignItems), typeof(WrapLayout), FlexAlignItems.Stretch, propertyChanged: Invalidate);

    /// <summary>Bindable property for <see cref="JustifyContent"/>.</summary>
    public static readonly BindableProperty JustifyContentProperty = BindableProperty.Create(
        nameof(JustifyContent), typeof(FlexJustify), typeof(WrapLayout), FlexJustify.Start, propertyChanged: Invalidate);

    /// <summary>Where a child sits in the height of its row; <c>Stretch</c> (the default, as in FlexLayout) gives it the
    /// whole row height.</summary>
    public FlexAlignItems AlignItems
    {
        get => (FlexAlignItems)GetValue(AlignItemsProperty);
        set => SetValue(AlignItemsProperty, value);
    }

    /// <summary>Where each row's children sit in the layout's width, and how the space left over is shared.</summary>
    public FlexJustify JustifyContent
    {
        get => (FlexJustify)GetValue(JustifyContentProperty);
        set => SetValue(JustifyContentProperty, value);
    }

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    static void Invalidate(BindableObject bindable, object oldValue, object newValue) => ((WrapLayout)bindable).InvalidateMeasure();

    sealed class Manager(WrapLayout layout) : LayoutManager(layout)
    {
        // The IView members: Width is the WidthRequest here (VisualElement.Width is the arranged width).
        readonly IView _view = layout;

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var padding = layout.Padding;
            // Wrap at the width the layout will get, not at a wider constraint it is clamped from afterwards.
            var width = Dimension.IsExplicitSet(_view.Width) ? _view.Width : widthConstraint;
            width = Math.Min(width, _view.MaximumWidth);
            var available = Math.Max(0, width - padding.HorizontalThickness);
            foreach (var child in Visible())
                child.Measure(available, double.PositiveInfinity);

            var rows = Rows(available);
            var contentWidth = rows.Count == 0 ? 0 : rows.Max(r => r.Width);
            var contentHeight = rows.Sum(r => r.Height);
            return new Size(
                ResolveConstraints(widthConstraint, _view.Width, contentWidth + padding.HorizontalThickness, _view.MinimumWidth, _view.MaximumWidth),
                ResolveConstraints(heightConstraint, _view.Height, contentHeight + padding.VerticalThickness, _view.MinimumHeight, _view.MaximumHeight));
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            var padding = layout.Padding;
            var available = Math.Max(0, bounds.Width - padding.HorizontalThickness);
            var y = bounds.Top + padding.Top;
            foreach (var row in Rows(available))
            {
                var (start, gap) = Justify(layout.JustifyContent, available - row.Width, row.Children.Count);
                var x = bounds.Left + padding.Left + start;
                foreach (var child in row.Children)
                {
                    var size = child.DesiredSize;
                    var (top, height) = layout.AlignItems switch
                    {
                        FlexAlignItems.Start => (y, size.Height),
                        FlexAlignItems.Center => (y + (row.Height - size.Height) / 2, size.Height),
                        FlexAlignItems.End => (y + row.Height - size.Height, size.Height),
                        _ => (y, row.Height),
                    };
                    // The rect includes the margins; the child's arrange takes them off once.
                    child.Arrange(new Rect(x, top, size.Width, height));
                    x += size.Width + gap;
                }
                y += row.Height;
            }
            return bounds.Size;
        }

        IEnumerable<IView> Visible() => layout.Where(child => child.Visibility != Visibility.Collapsed);

        List<Row> Rows(double available)
        {
            var rows = new List<Row>();
            Row? row = null;
            foreach (var child in Visible())
            {
                var size = child.DesiredSize;
                if (row is null || (row.Children.Count > 0 && row.Width + size.Width > available + 0.001))
                    rows.Add(row = new Row());
                row.Children.Add(child);
                row.Width += size.Width;
                row.Height = Math.Max(row.Height, size.Height);
            }
            return rows;
        }

        static (double Start, double Gap) Justify(FlexJustify justify, double free, int count)
        {
            free = Math.Max(0, free);
            return justify switch
            {
                FlexJustify.Center => (free / 2, 0),
                FlexJustify.End => (free, 0),
                FlexJustify.SpaceBetween => count > 1 ? (0, free / (count - 1)) : (0, 0),
                FlexJustify.SpaceAround => (free / count / 2, free / count),
                FlexJustify.SpaceEvenly => (free / (count + 1), free / (count + 1)),
                _ => (0, 0),
            };
        }

        sealed class Row
        {
            public List<IView> Children { get; } = [];
            public double Width { get; set; }
            public double Height { get; set; }
        }
    }
}
