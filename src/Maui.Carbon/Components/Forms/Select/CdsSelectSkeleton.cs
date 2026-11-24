namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsSelect"/> (Carbon's <c>SelectSkeleton</c>): a 75 x 14 label bar over a 40px
/// field bar as wide as the select. It is hidden from assistive technology and takes no input.
/// </summary>
public class CdsSelectSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsSelectSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsSelectSkeleton)b).UpdateLayout());

    readonly View _label = new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly View _field = new CdsShimmerView { HeightRequest = CdsSize.Md.Height() };

    /// <summary>Creates a skeleton with a label bar.</summary>
    public CdsSelectSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_label);
        stack.Add(_field);
        SetChrome(stack);
        UpdateLayout();
    }

    /// <summary>Hides the label bar.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    void UpdateLayout() => _label.IsVisible = !HideLabel;
}
