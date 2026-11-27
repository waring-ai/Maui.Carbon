namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>Flyout sections, in display order (README component categories).</summary>
public enum GalleryCategory
{
    Foundations,
    Actions,
    Forms,
    FluidForms,
    Data,
    Charts,
    Navigation,
    Containers,
    Overlays,
    Feedback,
    AI,
    Layout,
    Products,
    Patterns,
}

/// <summary>
/// Registers a gallery page. Pages are discovered by reflection, so adding a page never touches a shared file.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GalleryPageAttribute(string title, GalleryCategory category) : Attribute
{
    /// <summary>Flyout and page title (the component name, e.g. "Button").</summary>
    public string Title { get; } = title;

    /// <summary>Flyout section.</summary>
    public GalleryCategory Category { get; } = category;

    /// <summary>Sort order inside the section (then by title).</summary>
    public int Order { get; set; } = 100;

    /// <summary>One sentence shown under the page title.</summary>
    public string? Summary { get; set; }

    /// <summary>The design/carbon/components folder this page demonstrates (defaults to the title without spaces).</summary>
    public string? Spec { get; set; }

    /// <summary>Shell route.</summary>
    public string Route => Category.ToString().ToLowerInvariant() + "-" + new string(Title.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
