using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>
/// Colors a project card's cover band from its category with Carbon tag tokens (so it follows the theme):
/// <c>p:ProjectsTint.Fill="{Binding Category}"</c> sets a view's background to the category's tag background and
/// <c>p:ProjectsTint.Ink="{Binding Category}"</c> sets a <see cref="CdsIcon"/> (or text) to the matching tag color. The
/// hue matches <see cref="CarbonMap.Tag(ProjectCategory)"/>.
/// </summary>
public static class ProjectsTint
{
    public static readonly BindableProperty FillProperty = BindableProperty.CreateAttached(
        "Fill", typeof(ProjectCategory?), typeof(ProjectsTint), null,
        propertyChanged: (b, _, n) =>
        {
            if (b is VisualElement view && n is ProjectCategory c)
                CdsTokens.SetColor(view, VisualElement.BackgroundColorProperty, "CdsTagBackground" + Hue(c));
        });

    public static readonly BindableProperty InkProperty = BindableProperty.CreateAttached(
        "Ink", typeof(ProjectCategory?), typeof(ProjectsTint), null,
        propertyChanged: (b, _, n) =>
        {
            if (n is not ProjectCategory c) return;
            var key = "CdsTagColor" + Hue(c);
            switch (b)
            {
                case CdsIcon icon: CdsTokens.SetColor(icon, CdsIcon.ColorProperty, key); break;
                case Label label: CdsTokens.SetColor(label, Label.TextColorProperty, key); break;
            }
        });

    public static ProjectCategory? GetFill(BindableObject target) => (ProjectCategory?)target.GetValue(FillProperty);

    public static void SetFill(BindableObject target, ProjectCategory? value) => target.SetValue(FillProperty, value);

    public static ProjectCategory? GetInk(BindableObject target) => (ProjectCategory?)target.GetValue(InkProperty);

    public static void SetInk(BindableObject target, ProjectCategory? value) => target.SetValue(InkProperty, value);

    /// <summary>The tag hue name used in the token keys ("Blue" → <c>CdsTagBackgroundBlue</c>).</summary>
    static string Hue(ProjectCategory category) => CarbonMap.Tag(category) switch
    {
        CdsTagType.Blue => "Blue",
        CdsTagType.Purple => "Purple",
        CdsTagType.Cyan => "Cyan",
        CdsTagType.Magenta => "Magenta",
        CdsTagType.Teal => "Teal",
        CdsTagType.Green => "Green",
        CdsTagType.Red => "Red",
        CdsTagType.CoolGray => "CoolGray",
        CdsTagType.WarmGray => "WarmGray",
        _ => "Gray",
    };
}
