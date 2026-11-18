namespace Maui.Carbon;

/// <summary>
/// Steps everything in <see cref="Body"/> up one Carbon layer: the <c>-01</c> layer, field and border tokens resolve
/// to the next level's values (two, then three; deeper layers stay at three). Components always use the <c>-01</c>
/// tokens and let the nearest layer decide. A <see cref="CdsTheme"/> zone resets to level one.
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsLayer : ContentView
{
    /// <summary>Bindable property for <see cref="Level"/>.</summary>
    public static readonly BindableProperty LevelProperty = BindableProperty.Create(
        nameof(Level), typeof(int?), typeof(CdsLayer), null,
        propertyChanged: static (b, _, _) => ((CdsLayer)b).Recompute());

    /// <summary>Bindable property for <see cref="WithBackground"/>.</summary>
    public static readonly BindableProperty WithBackgroundProperty = BindableProperty.Create(
        nameof(WithBackground), typeof(bool), typeof(CdsLayer), false,
        propertyChanged: static (b, _, _) => ((CdsLayer)b).ApplyBackground());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsLayer), null,
        propertyChanged: static (b, _, n) => ((CdsLayer)b)._scope.Content = (View?)n);

    // Read from ancestors. CdsLayer itself never holds these keys: the overrides live on _scope.
    static readonly BindableProperty ContextThemeProperty = BindableProperty.Create(
        "ContextTheme", typeof(CdsThemeName), typeof(CdsLayer), CdsThemeName.White,
        propertyChanged: static (b, _, _) => ((CdsLayer)b).Recompute());

    static readonly BindableProperty ContextLevelProperty = BindableProperty.Create(
        "ContextLevel", typeof(int), typeof(CdsLayer), 1,
        propertyChanged: static (b, _, _) => ((CdsLayer)b).Recompute());

    readonly CdsScopeView _scope = new();
    (CdsThemeName Theme, int Level)? _applied;

    /// <summary>Creates a layer one level above its context.</summary>
    public CdsLayer()
    {
        Content = _scope;
        SetDynamicResource(ContextThemeProperty, CdsContextKeys.ThemeId);
        SetDynamicResource(ContextLevelProperty, CdsContextKeys.LayerLevel);
        Recompute();
    }

    /// <summary>
    /// Forces a level (1, 2 or 3) instead of stepping one above the enclosing layer. Unlike Carbon for the web,
    /// <c>Level="1"</c> really resets to level one.
    /// </summary>
    public int? Level
    {
        get => (int?)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    /// <summary>Paints the level's <c>layer-background</c> behind the content. Default <c>false</c>.</summary>
    public bool WithBackground
    {
        get => (bool)GetValue(WithBackgroundProperty);
        set => SetValue(WithBackgroundProperty, value);
    }

    /// <summary>The layered content.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>The level this layer applies to its content.</summary>
    public int EffectiveLevel => Level is { } l ? Math.Clamp(l, 1, 3) : Math.Min((int)GetValue(ContextLevelProperty) + 1, 3);

    /// <summary>The layer level in effect for <paramref name="element"/> (Carbon's <c>useLayer()</c>); 1 outside any layer.</summary>
    public static int GetLevel(Element element) =>
        CdsContext.Find(element, CdsContextKeys.LayerLevel) is int level ? level : 1;

    void Recompute()
    {
        var next = ((CdsThemeName)GetValue(ContextThemeProperty), EffectiveLevel);
        if (_applied == next) return;
        _applied = next;
        _scope.Resources = CdsPalettes.CreateLayerDictionary(next.Item1, next.Item2);
    }

    void ApplyBackground()
    {
        CdsTokens.SetColor(_scope, BackgroundColorProperty, WithBackground ? CdsColorKeys.LayerBackground01 : null);
    }
}
