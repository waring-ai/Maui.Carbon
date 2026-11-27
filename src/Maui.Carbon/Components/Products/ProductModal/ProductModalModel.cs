namespace Maui.Carbon;

/// <summary>Pure measures shared by the product modals (from <c>packages/ibm-products-styles/src/components/*Modal</c>).</summary>
internal static class ProductModalModel
{
    /// <summary>
    /// The end inset of the header's texts for a header <paramref name="width"/> wide (the container width). Carbon pads the
    /// header 48 at the end for the close button and the title another <c>calc(20% - 3rem)</c>; the import modal pads the
    /// header <c>calc(20% - 1rem)</c> instead, the create modal 20%, and the about modal's product name 20%.
    /// </summary>
    public static double HeaderEndInset(CdsProductModalHeaderStyle style, double width)
    {
        if (width <= 0 || double.IsInfinity(width) || double.IsNaN(width)) return 0;
        switch (style)
        {
            case CdsProductModalHeaderStyle.About:
                return 0.2 * width;
            case CdsProductModalHeaderStyle.Create:
            {
                var header = 0.2 * width;
                return header + TitlePadding(width - CdsSpacing.Spacing05 - header);
            }
            case CdsProductModalHeaderStyle.Import:
            {
                var header = Math.Max(0, 0.2 * width - CdsSpacing.Spacing05);
                return header + TitlePadding(width - CdsSpacing.Spacing05 - header);
            }
            default:
                return CdsModalSizing.CloseButtonSize + CdsModalSizing.HeadingEndPadding(width);
        }
    }

    /// <summary><c>padding-inline-end: calc(20% - 3rem)</c> of a title whose header content box is <paramref name="contentWidth"/> wide.</summary>
    static double TitlePadding(double contentWidth) => Math.Max(0, 0.2 * contentWidth - CdsModalSizing.CloseButtonSize);

    /// <summary>A paragraph's <c>padding-inline-end: 20%</c> (remove modal body, about modal body).</summary>
    public static double TwentyPercent(double width) => width <= 0 || double.IsInfinity(width) ? 0 : 0.2 * width;

    /// <summary>A paragraph's <c>padding-inline-end: calc(20% - 1rem)</c> (export, import, API key and create bodies), never negative.</summary>
    public static double TwentyPercentLessSpacing05(double width) =>
        width <= 0 || double.IsInfinity(width) ? 0 : Math.Max(0, 0.2 * width - CdsSpacing.Spacing05);
}
