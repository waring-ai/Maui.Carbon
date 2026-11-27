using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsToggle"/> (Carbon's <c>ToggleSkeleton</c>): a 75 x 14 label bar over a
/// <c>skeleton-background</c> track (48 x 24, or 32 x 16 small) with a <c>skeleton-element</c> knob at its start, and a 24 x 8 bar for
/// the state text beside it. It is hidden from assistive technology and takes no input.
/// </summary>
public class CdsToggleSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsToggleSize), typeof(CdsToggleSkeleton), CdsToggleSize.Md,
        propertyChanged: static (b, _, _) => ((CdsToggleSkeleton)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsToggleSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsToggleSkeleton)b).UpdateLayout());

    readonly View _label = new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start };
    readonly Border _track = new() { StrokeThickness = 0, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
    readonly BoxView _knob = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
    readonly View _text = new CdsShimmerView { HeightRequest = 8, WidthRequest = 24, VerticalOptions = LayoutOptions.Center };

    /// <summary>Creates a medium skeleton with a label bar.</summary>
    public CdsToggleSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        CdsTokens.SetColor(_track, BackgroundColorProperty, CdsColorKeys.SkeletonBackground);
        CdsTokens.SetColor(_knob, BoxView.ColorProperty, CdsColorKeys.SkeletonElement);
        _track.Content = _knob;

        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03 };
        row.Add(_track);
        row.Add(_text);
        var stack = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Start };
        stack.Add(_label);
        stack.Add(row);
        SetChrome(stack);
        UpdateLayout();
    }

    /// <summary>Track size, as the toggle it stands for: md 48 x 24 (default) or sm 32 x 16.</summary>
    public CdsToggleSize Size
    {
        get => (CdsToggleSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Hides the label bar.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>The track width and height and the knob diameter of a size: 48 x 24 with an 18 knob, or 32 x 16 with a 10 knob.</summary>
    internal static (double Width, double Height, double Knob) Metrics(CdsToggleSize size) =>
        size == CdsToggleSize.Sm ? (32, 16, 10) : (48, 24, 18);

    void UpdateLayout()
    {
        var (width, height, knob) = Metrics(Size);
        _track.WidthRequest = width;
        _track.HeightRequest = height;
        _track.StrokeShape = new RoundRectangle { CornerRadius = height / 2 };
        _knob.WidthRequest = knob;
        _knob.HeightRequest = knob;
        _knob.CornerRadius = knob / 2;
        _knob.Margin = new Thickness((height - knob) / 2, 0, 0, 0);
        _label.IsVisible = !HideLabel;
        // The label sits 16 above a medium track and 8 above a small one, as on the toggle.
        _label.Margin = new Thickness(0, 0, 0, Size == CdsToggleSize.Sm ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05);
    }
}
