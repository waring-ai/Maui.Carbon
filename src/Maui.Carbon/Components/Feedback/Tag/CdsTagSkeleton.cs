using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of a tag (Carbon's <c>TagSkeleton</c>): a 60 wide shimmering pill as tall as the tag's
/// <see cref="Size"/> (18, 24 or 32). It is hidden from assistive technology and takes no input.
/// </summary>
public class CdsTagSkeleton : CdsControl
{
    /// <summary>Width of the pill: 3.75rem.</summary>
    internal const double PillWidth = 60;

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsTagSize), typeof(CdsTagSkeleton), CdsTagSize.Md,
        propertyChanged: static (b, _, _) => ((CdsTagSkeleton)b).UpdateLayout());

    readonly Border _pill = new() { StrokeThickness = 0, WidthRequest = PillWidth, HorizontalOptions = LayoutOptions.Start, Content = new CdsShimmerView() };

    /// <summary>Creates a medium tag skeleton.</summary>
    public CdsTagSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        SetChrome(_pill);
        UpdateLayout();
    }

    /// <summary>Height, as the tag it stands for: sm 18, md 24 (default), lg 32.</summary>
    public CdsTagSize Size
    {
        get => (CdsTagSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    void UpdateLayout()
    {
        var height = CdsTagTokens.Height(Size);
        _pill.HeightRequest = height;
        _pill.StrokeShape = new RoundRectangle { CornerRadius = height / 2 };
    }
}
