namespace Maui.Carbon;

/// <summary>
/// An animated block standing in for a chart, image, tile or any non-text content while it loads: 100 by 100 by
/// default, square corners, <c>skeleton-background</c> with a sweeping <c>skeleton-element</c> band. Size it with
/// <c>WidthRequest</c> and <c>HeightRequest</c> so it matches the content it will become. It is not interactive and
/// invisible to screen readers: mark the loading region as busy and name what is loading yourself. For text use
/// <see cref="CdsSkeletonText"/>.
/// </summary>
public class CdsSkeletonPlaceholder : CdsControl
{
    /// <summary>The default width and height.</summary>
    public const double DefaultSize = 100;

    /// <summary>Creates a 100 by 100 skeleton block.</summary>
    public CdsSkeletonPlaceholder()
    {
        WidthRequest = DefaultSize;
        HeightRequest = DefaultSize;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new CdsShimmerView());
    }
}
