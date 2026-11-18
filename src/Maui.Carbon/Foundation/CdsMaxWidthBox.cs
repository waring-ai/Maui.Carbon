using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>Pure width math of <see cref="CdsMaxWidthBox"/>.</summary>
internal static class MaxWidthModel
{
    /// <summary>
    /// The width of the content: all the available width up to <paramref name="max"/>; with unbounded width, the
    /// natural width up to <paramref name="max"/>.
    /// </summary>
    public static double ContentWidth(double available, double natural, double max) =>
        double.IsFinite(available) ? Math.Min(Math.Max(0, available), max) : Math.Min(natural, max);
}

/// <summary>
/// Hosts one view that fills the available width up to a maximum and is start aligned beyond it. A plain Grid with
/// <c>MaximumWidthRequest</c> is centered by MAUI once the space exceeds the maximum, and a start-aligned Grid shrinks to
/// its content; this box does neither. Use it for readable text columns, notifications and forms
/// (<c>&lt;cds:CdsMaxWidthBox MaxContentWidth="608"&gt;…&lt;/cds:CdsMaxWidthBox&gt;</c>).
/// </summary>
[ContentProperty(nameof(Content))]
public sealed class CdsMaxWidthBox : Layout
{
    /// <summary>Bindable property for <see cref="MaxContentWidth"/>.</summary>
    public static readonly BindableProperty MaxContentWidthProperty = BindableProperty.Create(
        nameof(MaxContentWidth), typeof(double), typeof(CdsMaxWidthBox), double.PositiveInfinity,
        propertyChanged: static (b, _, _) => ((CdsMaxWidthBox)b).InvalidateMeasure());

    /// <summary>Creates an empty box with no maximum.</summary>
    public CdsMaxWidthBox()
    {
    }

    /// <summary>Creates a box around <paramref name="content"/>.</summary>
    public CdsMaxWidthBox(View content, double maxContentWidth)
    {
        MaxContentWidth = maxContentWidth;
        Add(content);
    }

    /// <summary>The hosted view.</summary>
    public View? Content
    {
        get => Children.FirstOrDefault() as View;
        set
        {
            Clear();
            if (value is not null) Add(value);
        }
    }

    /// <summary>The widest the content gets; it fills narrower space completely.</summary>
    public double MaxContentWidth
    {
        get => (double)GetValue(MaxContentWidthProperty);
        set => SetValue(MaxContentWidthProperty, value);
    }

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsMaxWidthBox box) : LayoutManager(box)
    {
        IView? Child => box.FirstOrDefault(c => c.Visibility != Visibility.Collapsed);

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            if (Child is not { } child) return Size.Zero;
            var natural = double.IsFinite(widthConstraint)
                ? 0
                : child.Measure(double.PositiveInfinity, heightConstraint).Width;
            var width = MaxWidthModel.ContentWidth(widthConstraint, natural, box.MaxContentWidth);
            var measured = child.Measure(width, heightConstraint);
            return new Size(width, measured.Height);
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            if (Child is { } child)
            {
                var width = Math.Min(bounds.Width, box.MaxContentWidth);
                child.Arrange(new Rect(bounds.X, bounds.Y, width, bounds.Height));
            }
            return bounds.Size;
        }
    }
}
