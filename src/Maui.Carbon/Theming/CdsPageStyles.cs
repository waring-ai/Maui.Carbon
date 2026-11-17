using Microsoft.Maui.Controls.Internals;

namespace Maui.Carbon;

/// <summary>The Carbon page style: <c>background</c> fill and the overlay host template.</summary>
public static class CdsPageStyles
{
    /// <summary>
    /// Key of the keyed ContentPage style. If your app defines its own implicit ContentPage style, base it on this one
    /// (<c>BasedOn="{StaticResource CdsContentPageStyle}"</c>) so pages keep the Carbon overlay host.
    /// </summary>
    public const string ContentPageStyleKey = "CdsContentPageStyle";

    internal static Style CreateContentPageStyle(CdsOptions options)
    {
        var style = new Style(typeof(ContentPage)) { ApplyToDerivedTypes = true };
        style.Setters.Add(new Setter { Property = VisualElement.BackgroundColorProperty, Value = new DynamicResource(CdsColorKeys.Background) });
        if (options.InjectPageOverlayHost)
            style.Setters.Add(new Setter { Property = TemplatedPage.ControlTemplateProperty, Value = CdsOverlayHost.PageTemplate });
        return style;
    }
}
