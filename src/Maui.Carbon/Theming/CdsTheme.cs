namespace Maui.Carbon;

/// <summary>
/// A theme zone: switches every color token in <see cref="Body"/> to <see cref="Theme"/>, paints <c>background</c> and
/// resets layering to level one. Zones nest. Use one contrasting zone per view at most (a g100 header on a white page).
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsTheme : ContentView
{
    /// <summary>Bindable property for <see cref="Theme"/>.</summary>
    public static readonly BindableProperty ThemeProperty = BindableProperty.Create(
        nameof(Theme), typeof(CdsThemeName), typeof(CdsTheme), CdsThemeName.White,
        propertyChanged: static (b, _, _) => ((CdsTheme)b).ApplyTheme());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsTheme), null,
        propertyChanged: static (b, _, n) => ((CdsTheme)b)._scope.Content = (View?)n);

    readonly CdsScopeView _scope = new();

    /// <summary>Creates a White theme zone.</summary>
    public CdsTheme()
    {
        Content = _scope;
        _scope.SetDynamicResource(BackgroundColorProperty, CdsColorKeys.Background);
        ApplyTheme();
    }

    /// <summary>The zone's theme. Default <see cref="CdsThemeName.White"/>.</summary>
    public CdsThemeName Theme
    {
        get => (CdsThemeName)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    /// <summary>The themed content.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>
    /// The theme in effect for <paramref name="element"/>: the nearest zone's theme, else the app theme
    /// (Carbon's <c>useTheme()</c>).
    /// </summary>
    public static CdsThemeName GetEffectiveTheme(Element element) =>
        CdsContext.Find(element, CdsContextKeys.ThemeId) is CdsThemeName t ? t : CdsThemeManager.Current;

    void ApplyTheme() => _scope.Resources = CdsPalettes.CreateThemeDictionary(Theme);
}
