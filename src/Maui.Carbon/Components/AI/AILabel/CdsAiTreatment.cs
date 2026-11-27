using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The tokens of the Carbon AI presence (Carbon's <c>ai-*</c> theme tokens): the aura, the gradient border, shadows, the AI scrim,
/// the explainability callout and the AI skeleton. Every key follows the theme; the aura and shadow colors are translucent.
/// </summary>
public static class CdsAiTokens
{
    /// <summary>Start of the aura at the bottom of an AI container: tiles, modals, tables, callouts (<c>ai-aura-start</c>).</summary>
    public const string Aura = CdsColorKeys.AiAuraStart;

    /// <summary>Start of the smaller aura of an AI field or table cell (<c>ai-aura-start-sm</c>).</summary>
    public const string AuraSmall = CdsColorKeys.AiAuraStartSm;

    /// <summary>End of the aura gradient, transparent (<c>ai-aura-end</c>).</summary>
    public const string AuraEnd = CdsColorKeys.AiAuraEnd;

    /// <summary>Start of the aura on a hovered AI surface (<c>ai-aura-hover-start</c>).</summary>
    public const string AuraHoverStart = CdsColorKeys.AiAuraHoverStart;

    /// <summary>End of the hover aura, transparent (<c>ai-aura-hover-end</c>).</summary>
    public const string AuraHoverEnd = CdsColorKeys.AiAuraHoverEnd;

    /// <summary>Fill of a hovered AI surface under the hover aura (<c>ai-aura-hover-background</c>).</summary>
    public const string AuraHoverBackground = CdsColorKeys.AiAuraHoverBackground;

    /// <summary>The start (top) of the AI border gradient (<c>ai-border-start</c>).</summary>
    public const string BorderStart = CdsColorKeys.AiBorderStart;

    /// <summary>The end (bottom) of the AI border gradient (<c>ai-border-end</c>).</summary>
    public const string BorderEnd = CdsColorKeys.AiBorderEnd;

    /// <summary>The strong AI border: the bottom rule of an AI field (<c>ai-border-strong</c>).</summary>
    public const string BorderStrong = CdsColorKeys.AiBorderStrong;

    /// <summary>The drop shadow of an AI container (<c>ai-drop-shadow</c>).</summary>
    public const string DropShadow = CdsColorKeys.AiDropShadow;

    /// <summary>The inner glow of an AI container (<c>ai-inner-shadow</c>).</summary>
    public const string InnerShadow = CdsColorKeys.AiInnerShadow;

    /// <summary>The scrim behind an AI modal (<c>ai-overlay</c>).</summary>
    public const string Overlay = CdsColorKeys.AiOverlay;

    /// <summary>The callout fill and caret (<c>ai-popover-background</c>).</summary>
    public const string CalloutBackground = CdsColorKeys.AiPopoverBackground;

    /// <summary>The AI skeleton fill (<c>ai-skeleton-background</c>).</summary>
    public const string SkeletonBackground = CdsColorKeys.AiSkeletonBackground;

    /// <summary>The AI skeleton shimmer (<c>ai-skeleton-element-background</c>), drawn at 50% opacity.</summary>
    public const string SkeletonElement = CdsColorKeys.AiSkeletonElementBackground;

    /// <summary>Opacity of the AI skeleton shimmer.</summary>
    public const double SkeletonElementOpacity = 0.5;
}

/// <summary>
/// Binds up to four color keys to a view and calls back with the resolved colors whenever any of them changes (theme switches,
/// layers). It lets a <see cref="Brush"/> with gradient stops follow the theme: stops cannot take dynamic resources themselves,
/// so the brush is rebuilt from the colors.
/// <para>
/// A theme switch changes the bound colors one key at a time. On screen, the callback runs once after the switch (with
/// every color resolved), not once per key with a mix of old and new colors: each brush pushed to a native view costs
/// MAUI a stroke or background update. Off screen it runs at once, so the view is complete before it is shown.
/// </para>
/// </summary>
internal static class CdsColorProxies
{
    sealed class Binding
    {
        public readonly Color?[] Colors = new Color?[Slots];
        public required Action<Color?[]> Apply { get; init; }
        public int Count { get; init; }
        public bool Pending;
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
                    if (b is not VisualElement { Handler: not null, Dispatcher: { } dispatcher })
                    {
                        binding.Apply(binding.Colors);
                        return;
                    }
                    if (binding.Pending) return;
                    binding.Pending = true;
                    dispatcher.Dispatch(() =>
                    {
                        binding.Pending = false;
                        // Unbound (or bound again) meanwhile: the new binding applies its own colors.
                        if (s_bindings.TryGetValue(b, out var current) && ReferenceEquals(current, binding)) binding.Apply(binding.Colors);
                    });
                });
        }
        return properties;
    }

    /// <summary>Binds <paramref name="keys"/> (at most four) to <paramref name="target"/>; <paramref name="apply"/> receives the resolved colors (null while unresolved).</summary>
    public static void Bind(VisualElement target, Action<Color?[]> apply, params string[] keys)
    {
        if (keys.Length > Slots) throw new ArgumentException("At most four colors can be bound.", nameof(keys));
        Unbind(target);
        var binding = new Binding { Apply = apply, Count = keys.Length };
        s_bindings.Add(target, binding);
        for (var i = 0; i < keys.Length; i++) target.SetDynamicResource(s_properties[i], keys[i]);
        // Binding again to keys that resolve to the colors the proxies already hold raises no change: apply what they hold.
        var missed = false;
        for (var i = 0; i < keys.Length; i++)
        {
            var color = target.GetValue(s_properties[i]) as Color;
            if (Equals(binding.Colors[i], color)) continue;
            binding.Colors[i] = color;
            missed = true;
        }
        if (missed && !binding.Pending) apply(binding.Colors);
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
/// color, a 1px gradient border from <c>ai-border-start</c> to <c>ai-border-end</c> and a faint <c>ai-drop-shadow</c>. The glow stays at
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
        if (shadow) CdsTokens.SetShadow(surface, CdsAiTokens.DropShadow, offsetY: 4, radius: 8);
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
    /// (Carbon: <c>linear-gradient(to top, aura 0%, 15%, transparent 50%)</c> over the surface). The aura token is translucent, so it is
    /// composited over the surface color here: the brush replaces the surface fill and must be opaque where the surface is.
    /// </summary>
    public static LinearGradientBrush CreateFill(Color aura, Color surfaceColor) => new(
        [new GradientStop(Over(aura, surfaceColor), 0f), new GradientStop(surfaceColor, 0.5f), new GradientStop(surfaceColor, 1f)],
        new Point(0, 1), new Point(0, 0));

    /// <summary>Composites <paramref name="top"/> over <paramref name="bottom"/> (source-over).</summary>
    internal static Color Over(Color top, Color bottom)
    {
        var a = top.Alpha + bottom.Alpha * (1 - top.Alpha);
        if (a <= 0) return Colors.Transparent;
        float Mix(float t, float b) => (t * top.Alpha + b * bottom.Alpha * (1 - top.Alpha)) / a;
        return new Color(Mix(top.Red, bottom.Red), Mix(top.Green, bottom.Green), Mix(top.Blue, bottom.Blue), a);
    }

    /// <summary>The AI border: <paramref name="start"/> at the top to <paramref name="end"/> at the bottom.</summary>
    public static LinearGradientBrush CreateBorder(Color start, Color end) => new(
        [new GradientStop(start, 0f), new GradientStop(end, 1f)],
        new Point(0, 0), new Point(0, 1));
}
