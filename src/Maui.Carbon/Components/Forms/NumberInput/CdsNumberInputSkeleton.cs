namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsNumberInput"/> (Carbon's <c>NumberInputSkeleton</c>): a 75 x 14 label bar over a
/// field bar as wide as the input and as tall as its <see cref="Size"/> (40 by default).
/// It is hidden from assistive technology and takes no input.
/// </summary>
public class CdsNumberInputSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsNumberInputSkeleton), CdsSize.Md,
        propertyChanged: static (b, _, _) => ((CdsNumberInputSkeleton)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsNumberInputSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsNumberInputSkeleton)b).UpdateLayout());

    readonly View _label = new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly View _field = new CdsShimmerView();

    /// <summary>Creates a skeleton with a label bar.</summary>
    public CdsNumberInputSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_label);
        stack.Add(_field);
        SetChrome(stack);
        UpdateLayout();
    }

    /// <summary>Field height, as the input it stands for: sm 32, md 40 (default), lg 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Hides the label bar.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    void UpdateLayout()
    {
        _field.HeightRequest = CdsListBoxTokens.Coerce(Size).Height();
        _label.IsVisible = !HideLabel;
    }
}
