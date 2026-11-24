namespace Maui.Carbon;

/// <summary>
/// The loading placeholder of <see cref="CdsFileUploader"/> (Carbon's <c>FileUploaderSkeleton</c>): a 100 x 24 heading
/// bar, a 225 x 14 description bar and a 152 x 48 button bar, start-aligned. It is hidden from assistive technology and
/// takes no input.
/// </summary>
public class CdsFileUploaderSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFileUploaderSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        var stack = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Start };
        // .cds--skeleton__heading (1.5rem) with 0.5rem below; .cds--label-description (0.875rem) with 1rem below;
        // .cds--file-btn--skeleton 9.5rem x 3rem.
        stack.Add(Bar(100, 24, CdsSpacing.Spacing03));
        stack.Add(Bar(225, 14, CdsSpacing.Spacing05));
        stack.Add(Bar(152, CdsSize.Lg.Height(), 0));
        SetChrome(stack);
    }

    static View Bar(double width, double height, double below) => new CdsShimmerView
    {
        WidthRequest = width,
        HeightRequest = height,
        HorizontalOptions = LayoutOptions.Start,
        Margin = new Thickness(0, 0, 0, below),
    };
}
