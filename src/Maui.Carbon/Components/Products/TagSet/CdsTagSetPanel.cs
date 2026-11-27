using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// The row of a <see cref="CdsTagSet"/>. With measuring copies (<see cref="Sizers"/>, one per tag, invisible and out of
/// the accessibility tree) it decides while measuring how many tags fit and asks the owner to apply that count
/// (<see cref="CountNeeded"/>); without them it wraps every visible tag (the "All tags" modal).
/// </summary>
internal sealed class CdsTagSetPanel : Layout, ILayoutManager
{
    static readonly Size Unbounded = new(double.PositiveInfinity, double.PositiveInfinity);

    public List<View> Sizers { get; } = [];
    public List<View> Tags { get; } = [];
    public View? OverflowSizer { get; set; }
    public View? Overflow { get; set; }
    public CdsTagSetAlign Align { get; set; }
    public bool Multiline { get; set; }
    public int MaxVisible { get; set; }
    public double Gap { get; set; } = CdsTagSetTokens.Gap;

    /// <summary>Raised from arrange with the number of tags that fit the arranged width.</summary>
    public event Action<int>? CountNeeded;

    bool Fitting => Sizers.Count == Tags.Count && OverflowSizer is not null;

    protected override ILayoutManager CreateLayoutManager() => this;

    Size ILayoutManager.Measure(double widthConstraint, double heightConstraint)
    {
        foreach (var s in Sizers) s.Measure(Unbounded.Width, Unbounded.Height);
        OverflowSizer?.Measure(Unbounded.Width, Unbounded.Height);
        foreach (var t in Tags) if (t.IsVisible) t.Measure(Unbounded.Width, Unbounded.Height);
        if (Overflow is { IsVisible: true } o) o.Measure(Unbounded.Width, Unbounded.Height);

        var (_, result) = Compute(widthConstraint);
        var width = double.IsInfinity(widthConstraint) ? result.ContentSize.Width : widthConstraint;
        return new Size(width, result.ContentSize.Height);
    }

    Size ILayoutManager.ArrangeChildren(Rect bounds)
    {
        var (count, result) = Compute(bounds.Width);
        foreach (var s in Sizers) s.Arrange(new Rect(bounds.X, bounds.Y, s.DesiredSize.Width, s.DesiredSize.Height));
        OverflowSizer?.Arrange(new Rect(bounds.X, bounds.Y, OverflowSizer.DesiredSize.Width, OverflowSizer.DesiredSize.Height));

        var shown = Fitting ? Tags : Tags.Where(t => t.IsVisible).ToList();
        for (var i = 0; i < shown.Count; i++)
        {
            var frame = i < result.Tags.Length && shown[i].IsVisible ? result.Tags[i] : Rect.Zero;
            shown[i].Arrange(frame.Offset(bounds.X, bounds.Y));
        }
        if (Overflow is not null)
            Overflow.Arrange(result.Overflow is { } of && Overflow.IsVisible ? of.Offset(bounds.X, bounds.Y) : Rect.Zero);

        if (Fitting) CountNeeded?.Invoke(count);
        return bounds.Size;
    }

    (int Count, TagSetLayoutResult Result) Compute(double width)
    {
        if (!Fitting)
        {
            var visible = Tags.Where(t => t.IsVisible).Select(t => t.DesiredSize).ToList();
            return (visible.Count, TagSetModel.Arrange(visible, visible.Count, null, width, Align, true, Gap));
        }

        var widths = new List<double>(Sizers.Count);
        var sizes = new List<Size>(Sizers.Count);
        for (var i = 0; i < Sizers.Count; i++)
        {
            var sizer = Sizers[i].DesiredSize;
            widths.Add(sizer.Width + Gap);
            // A shown tag is laid out at its own size; a hidden one (not measured) at its copy's.
            sizes.Add(Tags[i].IsVisible && Tags[i].DesiredSize.Width > 0 ? Tags[i].DesiredSize : sizer);
        }
        var overflowSizer = OverflowSizer!.DesiredSize;
        var count = TagSetModel.FitCount(widths, width, overflowSizer.Width + Gap, MaxVisible, Multiline);
        Size? overflow = count < Tags.Count
            ? (Overflow is { IsVisible: true } o && o.DesiredSize.Width > 0 ? o.DesiredSize : overflowSizer)
            : null;
        return (count, TagSetModel.Arrange(sizes, count, overflow, width, Align, Multiline, Gap));
    }
}
