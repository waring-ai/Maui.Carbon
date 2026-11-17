using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The tokens of the Carbon AI presence. Carbon's <c>ai-*</c> tokens are not part of this token set, so the AI gradient, border
/// and skeleton shimmer are built from <c>highlight</c>, <c>tag-background-blue</c>, <c>border-interactive</c> and <c>interactive</c>
/// (close to Carbon in white, brighter and more saturated in g100).
/// </summary>
public static class CdsAiTokens
{
    /// <summary>The aura at the bottom of an AI surface (<c>--cds-ai-aura-start</c>, <c>--cds-ai-inner-shadow</c>).</summary>
    public const string Aura = CdsColorKeys.Highlight;

    /// <summary>The start (top) of the AI border gradient (<c>--cds-ai-border-start</c>).</summary>
    public const string BorderStart = CdsColorKeys.TagBackgroundBlue;

    /// <summary>The end (bottom) of the AI border gradient and the strong border (<c>--cds-ai-border-end</c>).</summary>
    public const string BorderEnd = CdsColorKeys.BorderInteractive;

    /// <summary>The callout fill and caret (<c>--cds-ai-popover-background</c>).</summary>
    public const string CalloutBackground = CdsColorKeys.Background;

    /// <summary>The AI skeleton fill (<c>--cds-ai-skeleton-background</c>).</summary>
    public const string SkeletonBackground = CdsColorKeys.TagBackgroundBlue;

    /// <summary>The AI skeleton shimmer (<c>--cds-ai-skeleton-element</c>), drawn at 50% opacity.</summary>
    public const string SkeletonElement = CdsColorKeys.Interactive;

    /// <summary>Opacity of the AI skeleton shimmer.</summary>
    public const double SkeletonElementOpacity = 0.5;
}

/// <summary>
/// Binds up to four color keys to a view and calls back with the resolved colors whenever any of them changes (theme switches,
/// layers). It lets a <see cref="Brush"/> with gradient stops follow the theme: stops cannot take dynamic resources themselves,
/// so the brush is rebuilt from the colors.
/// </summary>
internal static class CdsColorProxies
{
    sealed class Binding
    {
        public readonly Color?[] Colors = new Color?[Slots];
        public required Action<Color?[]> Apply { get; init; }
        public int Count { get; init; }
    }

    const int Slots = 4;
    static readonly ConditionalWeakTable<BindableObject, Binding> s_bindings = [];
    static readonly BindableProperty[] s_properties = CreateProperties();

    static BindableProperty[] CreateProperties()
    {
        var properties = new BindableProperty[Slots];
        for (var i = 0; i < Slots; i++)
        {
            var slot = i;
            properties[i] = BindableProperty.CreateAttached(
                $"CdsColorProxy{slot}", typeof(Color), typeof(CdsColorProxies), null,
                propertyChanged: (b, _, n) =>
                {
                    if (!s_bindings.TryGetValue(b, out var binding)) return;
                    binding.Colors[slot] = n as Color;
                    binding.Apply(binding.Colors);
                });
        }
        return properties;
    }

    /// <summary>Binds <paramref name="keys"/> (at most four) to <paramref name="target"/>; <paramref name="apply"/> receives the resolved colors (null while unresolved).</summary>
    public static void Bind(VisualElement target, Action<Color?[]> apply, params string[] keys)
    {
        if (keys.Length > Slots) throw new ArgumentException("At most four colors can be bound.", nameof(keys));
        Unbind(target);
        s_bindings.Add(target, new Binding { Apply = apply, Count = keys.Length });
        for (var i = 0; i < keys.Length; i++) target.SetDynamicResource(s_properties[i], keys[i]);
    }

    /// <summary>Removes the bindings of <see cref="Bind"/>.</summary>
    public static void Unbind(VisualElement target)
    {
        if (!s_bindings.TryGetValue(target, out var binding)) return;
        s_bindings.Remove(target);
        for (var i = 0; i < binding.Count; i++) target.RemoveDynamicResource(s_properties[i]);
    }
}

/// <summary>
/// Applies Carbon's AI treatment to a container (a tile, a callout, a panel): a soft aura rising from the bottom edge into the surface
/// color, a 1px gradient border from <c>tag-background-blue</c> to <c>border-interactive</c> and a faint drop shadow. The glow stays at
/// the container's edges so text contrast is unaffected, and every color is token-bound so it follows the theme and layer. When a
/// person edits AI content, call <see cref="Remove"/> and show the revert button of <see cref="CdsAILabel"/>.
/// </summary>
public static class CdsAiTreatment
{
    /// <summary>
    /// Attached: turns the AI treatment on or off for a container from XAML (<c>cds:CdsAiTreatment.IsActive="True"</c>), over its
    /// <c>layer-01</c> fill. Set it to <c>false</c> when the person edits the AI content.
    /// </summary>
    public static readonly BindableProperty IsActiveProperty = BindableProperty.CreateAttached(
        "IsActive", typeof(bool), typeof(CdsAiTreatment), false,
        propertyChanged: static (b, _, n) =>
        {
            if (b is not VisualElement element) return;
            if ((bool)n) Apply(element);
            else Remove(element);
        });

    /// <summary>Gets <see cref="IsActiveProperty"/>.</summary>
    public static bool GetIsActive(BindableObject element) => (bool)element.GetValue(IsActiveProperty);

    /// <summary>Sets <see cref="IsActiveProperty"/>.</summary>
    public static void SetIsActive(BindableObject element, bool value) => element.SetValue(IsActiveProperty, value);

    /// <summary>
    /// Gives <paramref name="surface"/> the AI gradient fill and, for a <see cref="Border"/>, the gradient border (set a corner radius
    /// first). The surface keeps the fill it would have had: pass its base color key (default <c>layer-01</c>).
    /// </summary>
    /// <param name="surface">The container to style. Its <c>BackgroundColor</c> token binding is replaced by the gradient.</param>
    /// <param name="baseColorKey">The surface color the aura fades into.</param>
    /// <param name="shadow">Add the soft <c>0 4px 8px</c> shadow Carbon gives AI tiles.</param>
    public static void Apply(VisualElement surface, string baseColorKey = CdsColorKeys.Layer01, bool shadow = true)
    {
        ArgumentNullException.ThrowIfNull(surface);
        surface.RemoveDynamicResource(VisualElement.BackgroundColorProperty);
        if (surface is Border border && border.StrokeThickness < 1) border.StrokeThickness = 1;
        CdsColorProxies.Bind(surface, colors =>
        {
            if (colors[0] is { } aura && colors[1] is { } surfaceColor) surface.Background = CreateFill(aura, surfaceColor);
            if (surface is Border b && colors[2] is { } start && colors[3] is { } end) b.Stroke = CreateBorder(start, end);
        }, CdsAiTokens.Aura, baseColorKey, CdsAiTokens.BorderStart, CdsAiTokens.BorderEnd);
        if (shadow) CdsTokens.SetShadow(surface, CdsColorKeys.Shadow, offsetY: 4, radius: 8);
    }

    /// <summary>
    /// Removes the AI treatment and restores a flat fill (<paramref name="baseColorKey"/>, or none when <c>null</c>), no gradient border and no shadow.
    /// </summary>
    public static void Remove(VisualElement surface, string? baseColorKey = CdsColorKeys.Layer01)
    {
        ArgumentNullException.ThrowIfNull(surface);
        CdsColorProxies.Unbind(surface);
        CdsTokens.SetShadow(surface, null);
        if (surface is Border border)
        {
            border.Stroke = Brush.Transparent;
        }
        CdsTokens.SetColor(surface, VisualElement.BackgroundColorProperty, baseColorKey);
    }

    /// <summary>
    /// The AI fill: <paramref name="aura"/> at the bottom edge fading into <paramref name="surfaceColor"/> by the middle of the surface
    /// (Carbon: <c>linear-gradient(to top, aura 0%, 15%, transparent 50%)</c> over the surface).
    /// </summary>
    public static LinearGradientBrush CreateFill(Color aura, Color surfaceColor) => new(
        [new GradientStop(aura, 0f), new GradientStop(surfaceColor, 0.5f), new GradientStop(surfaceColor, 1f)],
        new Point(0, 1), new Point(0, 0));

    /// <summary>The AI border: <paramref name="start"/> at the top to <paramref name="end"/> at the bottom.</summary>
    public static LinearGradientBrush CreateBorder(Color start, Color end) => new(
        [new GradientStop(start, 0f), new GradientStop(end, 1f)],
        new Point(0, 0), new Point(0, 1));
}
