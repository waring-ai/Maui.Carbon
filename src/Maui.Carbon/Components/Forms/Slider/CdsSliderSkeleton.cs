namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsSlider"/> (Carbon's <c>SliderSkeleton</c>): a label bar, then a 20 x 12 range
/// label bar on each side of a 2px <c>skeleton-background</c> track (200 to 640 wide) with a round
/// <c>skeleton-element</c> thumb at the middle, or two at a quarter and three quarters when <see cref="IsRange"/>.
/// It is hidden from assistive technology and takes no input.
/// </summary>
public class CdsSliderSkeleton : CdsControl
{
    const double Thumb = 14;
    const double TrackHost = 32 + 2; // .cds--slider: 1rem padding above and below the 2px track
    const double MinTrack = 200;
    const double MaxTrack = 640;

    /// <summary>Bindable property for <see cref="IsRange"/>.</summary>
    public static readonly BindableProperty IsRangeProperty = BindableProperty.Create(
        nameof(IsRange), typeof(bool), typeof(CdsSliderSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsSliderSkeleton)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsSliderSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsSliderSkeleton)b).UpdateLayout());

    readonly View _label = new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly AbsoluteLayout _track = new() { HeightRequest = TrackHost, MinimumWidthRequest = MinTrack, MaximumWidthRequest = MaxTrack, VerticalOptions = LayoutOptions.Center };
    readonly BoxView _rail = new() { HeightRequest = 2 };
    readonly BoxView _lowerThumb = new() { CornerRadius = Thumb / 2 };
    readonly BoxView _upperThumb = new() { CornerRadius = Thumb / 2 };

    /// <summary>Creates a single-thumb skeleton with a label bar.</summary>
    public CdsSliderSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        CdsTokens.SetColor(_rail, BoxView.ColorProperty, CdsColorKeys.SkeletonBackground);
        CdsTokens.SetColor(_lowerThumb, BoxView.ColorProperty, CdsColorKeys.SkeletonElement);
        CdsTokens.SetColor(_upperThumb, BoxView.ColorProperty, CdsColorKeys.SkeletonElement);
        AbsoluteLayout.SetLayoutBounds(_rail, new Rect(0, 0.5, 1, 2));
        AbsoluteLayout.SetLayoutFlags(_rail, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.YProportional | Microsoft.Maui.Layouts.AbsoluteLayoutFlags.WidthProportional);
        _track.Add(_rail);
        _track.Add(_lowerThumb);
        _track.Add(_upperThumb);

        var row = new Grid { ColumnSpacing = CdsSpacing.Spacing05, HorizontalOptions = LayoutOptions.Fill, MaximumWidthRequest = 920 };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.Add(RangeLabel(), 0);
        row.Add(_track, 1);
        row.Add(RangeLabel(), 2);

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_label);
        stack.Add(row);
        SetChrome(stack);
        UpdateLayout();
    }

    /// <summary>Two thumbs, for a range slider (Carbon's <c>twoHandles</c>).</summary>
    public bool IsRange
    {
        get => (bool)GetValue(IsRangeProperty);
        set => SetValue(IsRangeProperty, value);
    }

    /// <summary>Hides the label bar.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>The fractions of the track where the thumbs sit: 0.5, or 0.25 and 0.75 for a range.</summary>
    internal static double[] ThumbPositions(bool range) => range ? [0.25, 0.75] : [0.5];

    static View RangeLabel() => new CdsShimmerView { WidthRequest = 20, HeightRequest = 12, VerticalOptions = LayoutOptions.Center };

    void UpdateLayout()
    {
        _label.IsVisible = !HideLabel;
        var positions = ThumbPositions(IsRange);
        Place(_lowerThumb, positions[0]);
        _upperThumb.IsVisible = IsRange;
        if (IsRange) Place(_upperThumb, positions[1]);
    }

    static void Place(BoxView thumb, double fraction)
    {
        AbsoluteLayout.SetLayoutBounds(thumb, new Rect(fraction, 0.5, Thumb, Thumb));
        AbsoluteLayout.SetLayoutFlags(thumb, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.PositionProportional);
    }
}
