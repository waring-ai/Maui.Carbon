namespace Maui.Carbon;

/// <summary>Options for <see cref="CarbonAppBuilderExtensions.UseCarbon"/>.</summary>
public sealed class CdsOptions
{
    internal static CdsOptions Current { get; set; } = new();

    /// <summary>
    /// Give every ContentPage the Carbon overlay host (needed by dropdowns, menus, tooltips, modals and toasts) through
    /// an implicit page style. Default <c>true</c>. When off, put a <see cref="CdsOverlayHost"/> at the root of each page.
    /// </summary>
    public bool InjectPageOverlayHost { get; set; } = true;

    /// <summary>Add implicit Carbon styles for ContentPage and Label (Plex font, <c>text-primary</c>). Default <c>true</c>.</summary>
    public bool ApplyImplicitStyles { get; set; } = true;

    /// <summary>Skip animations when the OS asks for reduced motion. Default <c>true</c>.</summary>
    public bool RespectReducedMotion { get; set; } = true;
}
