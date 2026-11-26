using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>The width-to-height ratios of Carbon's <see cref="CdsAspectRatio"/> (2x Grid recommendations first).</summary>
public enum CdsRatio
{
    /// <summary>1:1 (default).</summary>
    Ratio1x1,
    /// <summary>2:1.</summary>
    Ratio2x1,
    /// <summary>2:3.</summary>
    Ratio2x3,
    /// <summary>3:2.</summary>
    Ratio3x2,
    /// <summary>4:3.</summary>
    Ratio4x3,
    /// <summary>16:9.</summary>
    Ratio16x9,
    /// <summary>9:16.</summary>
    Ratio9x16,
    /// <summary>1:2.</summary>
    Ratio1x2,
    /// <summary>3:4.</summary>
    Ratio3x4,
}

/// <summary>Pure ratio math for <see cref="CdsAspectRatio"/>.</summary>
public static class CdsAspectRatioModel
{
    /// <summary>The width divided by the height of a named ratio (16:9 is 1.7778).</summary>
    public static double Value(CdsRatio ratio) => ratio switch
    {
        CdsRatio.Ratio2x1 => 2d,
        CdsRatio.Ratio2x3 => 2d / 3,
        CdsRatio.Ratio3x2 => 3d / 2,
        CdsRatio.Ratio4x3 => 4d / 3,
        CdsRatio.Ratio16x9 => 16d / 9,
        CdsRatio.Ratio9x16 => 9d / 16,
        CdsRatio.Ratio1x2 => 0.5,
        CdsRatio.Ratio3x4 => 0.75,
        _ => 1d,
    };

    /// <summary>The height of a box <paramref name="width"/> wide with the given width-to-height <paramref name="ratio"/>.</summary>
    public static double HeightFor(double width, double ratio) => ratio > 0 ? width / ratio : 0;
}

/// <summary>
/// A box that keeps a fixed width-to-height ratio: its height is its width divided by the ratio. It is as wide as
/// its container (set a width on a wrapper or a <see cref="CdsColumn"/> to size it) and every child fills the box and is
/// clipped to it (images use <c>Aspect="AspectFill"</c>). It adds no background, border or padding. Use it for media and
/// placeholders that must line up in a row, not for content-driven boxes.
/// </summary>
public class CdsAspectRatio : Layout
{
    /// <summary>Bindable property for <see cref="Ratio"/>.</summary>
    public static readonly BindableProperty RatioProperty = BindableProperty.Create(
        nameof(Ratio), typeof(CdsRatio), typeof(CdsAspectRatio), CdsRatio.Ratio1x1,
        propertyChanged: static (b, _, _) => ((CdsAspectRatio)b).InvalidateMeasure());

    /// <summary>Bindable property for <see cref="CustomRatio"/>.</summary>
    public static readonly BindableProperty CustomRatioProperty = BindableProperty.Create(
        nameof(CustomRatio), typeof(double?), typeof(CdsAspectRatio), null,
        propertyChanged: static (b, _, _) => ((CdsAspectRatio)b).InvalidateMeasure());

    /// <summary>Creates a 1:1 box that clips its children.</summary>
    public CdsAspectRatio()
    {
        IsClippedToBounds = true;
        // In a row of boxes (a horizontal stack, a grid row) a Fill box would be stretched to the tallest neighbour.
        VerticalOptions = LayoutOptions.Start;
    }

    /// <summary>The named ratio. Default <see cref="CdsRatio.Ratio1x1"/>.</summary>
    public CdsRatio Ratio
    {
        get => (CdsRatio)GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    /// <summary>A custom width-to-height ratio (for example 2.39 for cinema); overrides <see cref="Ratio"/> when set.</summary>
    public double? CustomRatio
    {
        get => (double?)GetValue(CustomRatioProperty);
        set => SetValue(CustomRatioProperty, value);
    }

    /// <summary>The effective width-to-height ratio.</summary>
    public double EffectiveRatio => CustomRatio is > 0 ? CustomRatio.Value : CdsAspectRatioModel.Value(Ratio);

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new CdsAspectRatioLayoutManager(this);
}

sealed class CdsAspectRatioLayoutManager(CdsAspectRatio box) : LayoutManager(box)
{
    public override Size Measure(double widthConstraint, double heightConstraint)
    {
        var width = widthConstraint;
        if (!double.IsFinite(width))
        {
            // No width to follow (unbounded parent): take the widest child, else the 1x1 of a 160 box.
            width = 0;
            foreach (var child in box.Where(c => c.Visibility != Visibility.Collapsed))
                width = Math.Max(width, child.Measure(double.PositiveInfinity, double.PositiveInfinity).Width);
            if (width <= 0) width = CdsSizing.X2Large * 2;
        }
        var height = CdsAspectRatioModel.HeightFor(width, box.EffectiveRatio);
        foreach (var child in box.Where(c => c.Visibility != Visibility.Collapsed))
            child.Measure(width, height);
        return new Size(width, height);
    }

    public override Size ArrangeChildren(Rect bounds)
    {
        // The ratio holds whatever height the parent hands out: children fill a box the given width wide and as tall as
        // the ratio makes it, so a box a parent stretches (VerticalOptions="Fill") keeps its shape.
        var height = Math.Min(bounds.Height, CdsAspectRatioModel.HeightFor(bounds.Width, box.EffectiveRatio));
        var frame = new Rect(bounds.X, bounds.Y, bounds.Width, height);
        foreach (var child in box.Where(c => c.Visibility != Visibility.Collapsed))
            child.Arrange(frame);
        return new Size(bounds.Width, height);
    }
}
