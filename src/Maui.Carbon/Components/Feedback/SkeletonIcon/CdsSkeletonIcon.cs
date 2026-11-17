namespace Maui.Carbon;

/// <summary>
/// A 16 by 16 pulsing placeholder shown where an icon will appear once content loads: <c>skeleton-background</c> with
/// a sweeping <c>skeleton-element</c> band, no border. Set <see cref="Size"/> to match 20, 24 or 32 icons so nothing
/// shifts when content arrives. Use it next to <see cref="CdsSkeletonText"/> for the label. Purely visual: no role, no
/// text, not focusable; mark the loading region as busy instead.
/// </summary>
public class CdsSkeletonIcon : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(CdsSkeletonIcon), 16d,
        coerceValue: static (_, v) => Math.Max(1, (double)v),
        propertyChanged: static (b, _, _) => ((CdsSkeletonIcon)b).UpdateSize());

    /// <summary>Creates a 16 by 16 skeleton icon.</summary>
    public CdsSkeletonIcon()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new CdsShimmerView());
        UpdateSize();
    }

    /// <summary>The width and height: 16 (default), or 20, 24 or 32 for larger icons.</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    void UpdateSize()
    {
        WidthRequest = Size;
        HeightRequest = Size;
    }
}
