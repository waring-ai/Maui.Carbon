namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsTextArea"/> (Carbon's <c>TextAreaSkeleton</c>): a 75 x 14 label bar over a 100px
/// (6.25rem) field bar as wide as the text area.
/// It is hidden from assistive technology and takes no input.
/// </summary>
public class CdsTextAreaSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsTextAreaSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsTextAreaSkeleton)b).UpdateLayout());

    readonly View _label = new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly View _field = new CdsShimmerView();

    /// <summary>Creates a skeleton with a label bar.</summary>
    public CdsTextAreaSkeleton()
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

    /// <summary>Height of the field bar: 6.25rem.</summary>
    internal const double FieldHeight = 100;

    void UpdateLayout()
    {
        _field.HeightRequest = FieldHeight;
        _label.IsVisible = !HideLabel;
    }
}
