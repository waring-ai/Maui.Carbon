namespace Maui.Carbon;

/// <summary>
/// The per-page host for Carbon overlays (popovers, menus, tooltips, modals, toasts). Every ContentPage gets one through
/// the implicit Carbon page style; when that is turned off, make a <see cref="CdsOverlayHost"/> the root of the page.
/// </summary>
public partial class CdsOverlayHost : Grid
{
    /// <summary>The page template used by the implicit page style: a host around the page's content.</summary>
    public static ControlTemplate PageTemplate { get; } = new(() => new CdsOverlayHost(new ContentPresenter()));

    readonly View _content;

    /// <summary>Creates a host. Put the page content in <see cref="Body"/>.</summary>
    public CdsOverlayHost() : this(new ContentView()) { }

    internal CdsOverlayHost(View content)
    {
        _content = content;
        Children.Add(content);
    }

    /// <summary>The page content (when the host is used explicitly).</summary>
    public View? Body
    {
        get => (_content as ContentView)?.Content;
        set { if (_content is ContentView cv) cv.Content = value; }
    }
}
