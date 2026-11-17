namespace Maui.Carbon;

/// <summary>Opens Carbon overlays in the page host of an anchor (or of the current page).</summary>
public static class CdsOverlayService
{
    /// <summary>
    /// Opens <paramref name="content"/> as an overlay. Anchored popups use <see cref="CdsOverlayOptions.Anchor"/>;
    /// modals set <see cref="CdsOverlayOptions.IsModal"/>; anchorless non-modal content goes to the toast region.
    /// </summary>
    /// <param name="content">The overlay content.</param>
    /// <param name="options">Placement and behavior.</param>
    /// <param name="context">Any element on the target page when there is no anchor.</param>
    public static CdsOverlay Open(View content, CdsOverlayOptions options, Element? context = null)
    {
        var host = CdsOverlayHost.Find(options.Anchor ?? context) ?? FindCurrentHost()
            ?? throw new InvalidOperationException(
                "No CdsOverlayHost found. Call UseCarbon() and add <cds:CdsResources /> to Application.Resources so pages get " +
                "the Carbon page template, or make <cds:CdsOverlayHost> the root of the page.");
        return host.Open(content, options, context);
    }

    /// <summary>The host of the window's current page.</summary>
    public static CdsOverlayHost? FindCurrentHost()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        while (true)
        {
            switch (page)
            {
                case Shell shell:
                    page = shell.CurrentPage;
                    continue;
                case NavigationPage nav:
                    page = nav.CurrentPage;
                    continue;
                case FlyoutPage flyout:
                    page = flyout.Detail;
                    continue;
                case TabbedPage tabs:
                    page = tabs.CurrentPage;
                    continue;
            }
            break;
        }
        if (page?.Navigation.ModalStack.LastOrDefault() is ContentPage modal) page = modal;
        return page is ContentPage cp ? CdsOverlayHost.FindInPage(cp) : null;
    }
}
