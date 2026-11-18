namespace Maui.Carbon;

/// <summary>
/// Binds element properties to Carbon color tokens. Always use these (or <c>SetDynamicResource</c>) instead of reading
/// a color: the value then follows theme zones, layers and runtime theme switches.
/// </summary>
public static class CdsTokens
{
    static readonly Dictionary<BindableProperty, BindableProperty> s_brushProxies = [];

    /// <summary>
    /// Binds a <see cref="Color"/> property to a color key. A <c>null</c> key binds it to
    /// <see cref="CdsContextKeys.Transparent"/>. Never mix this with plain assignments of the same property: a local
    /// value outranks dynamic resources, so a later key would not show.
    /// </summary>
    public static void SetColor(Element target, BindableProperty colorProperty, string? key)
    {
        // Clear any local value first (an initializer such as BackgroundColor = Colors.Transparent): it would
        // outrank the dynamic resource and the token would never show.
        target.ClearValue(colorProperty);
        target.SetDynamicResource(colorProperty, key ?? CdsContextKeys.Transparent);
    }

    /// <summary>
    /// Binds a <see cref="Brush"/> property (for example <c>Border.Stroke</c>) to a color key as a solid brush.
    /// A <c>null</c> key sets the brush to transparent.
    /// </summary>
    public static void SetBrush(Element target, BindableProperty brushProperty, string? key)
    {
        var proxy = GetBrushProxy(brushProperty);
        target.SetDynamicResource(proxy, key ?? CdsContextKeys.Transparent);
    }

    static readonly BindableProperty ShadowColorProperty = BindableProperty.CreateAttached(
        "CdsShadowColor", typeof(Color), typeof(CdsTokens), null,
        propertyChanged: static (b, _, n) =>
        {
            if (b is VisualElement ve && n is Color { Alpha: > 0 } c && ve.GetValue(ShadowSpecProperty) is ShadowSpec spec)
                ve.Shadow = new Shadow { Brush = new SolidColorBrush(c), Offset = spec.Offset, Radius = spec.Radius, Opacity = 1 };
        });

    static readonly BindableProperty ShadowSpecProperty = BindableProperty.CreateAttached(
        "CdsShadowSpec", typeof(ShadowSpec), typeof(CdsTokens), null);

    sealed record ShadowSpec(Point Offset, float Radius);

    /// <summary>
    /// Gives <paramref name="target"/> a drop shadow whose color follows a color key (Carbon uses <c>shadow</c> for
    /// floating surfaces such as menus and popovers). A <c>null</c> key removes the shadow.
    /// </summary>
    public static void SetShadow(VisualElement target, string? key, double offsetY = 2, double radius = 2)
    {
        if (key is null)
        {
            target.SetDynamicResource(ShadowColorProperty, CdsContextKeys.Transparent);
            target.Shadow = null!;
            return;
        }
        target.SetValue(ShadowSpecProperty, new ShadowSpec(new Point(0, offsetY), (float)radius));
        target.SetDynamicResource(ShadowColorProperty, key);
    }

    static BindableProperty GetBrushProxy(BindableProperty brushProperty)
    {
        lock (s_brushProxies)
        {
            if (s_brushProxies.TryGetValue(brushProperty, out var proxy)) return proxy;
            proxy = BindableProperty.CreateAttached(
                $"CdsBrushColor_{brushProperty.DeclaringType.Name}_{brushProperty.PropertyName}",
                typeof(Color), typeof(CdsTokens), null,
                propertyChanged: (b, _, n) =>
                {
                    if (n is Color c) b.SetValue(brushProperty, new SolidColorBrush(c));
                });
            s_brushProxies[brushProperty] = proxy;
            return proxy;
        }
    }
}
