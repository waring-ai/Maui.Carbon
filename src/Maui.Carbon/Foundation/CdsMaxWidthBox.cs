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
/// its content; this box does neither.
/// </summary>
internal sealed class CdsMaxWidthBox : Layout
{
    public static readonly BindableProperty MaxContentWidthProperty = BindableProperty.Create(
        nameof(MaxContentWidth), typeof(double), typeof(CdsMaxWidthBox), double.PositiveInfinity,
        propertyChanged: static (b, _, _) => ((CdsMaxWidthBox)b).InvalidateMeasure());

    public CdsMaxWidthBox(View content, double maxContentWidth)
    {
        MaxContentWidth = maxContentWidth;
        Add(content);
    }

    public double MaxContentWidth
    {
        get => (double)GetValue(MaxContentWidthProperty);
        set => SetValue(MaxContentWidthProperty, value);
    }

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
