using Microsoft.Maui.Controls.Internals;

namespace Maui.Carbon;

/// <summary>
/// The Carbon resources for an app: every color token of the current theme (as dynamic resources), the keyed
/// type styles (<c>CdsType.BodyCompact01</c>…) and the implicit page and label styles. Add it once to
/// <c>Application.Resources</c>: <c>&lt;cds:CdsResources Theme="White" /&gt;</c>.
/// </summary>
public class CdsResources : ResourceDictionary
{
    static WeakReference<CdsResources>? s_current;

    readonly ResourceDictionary _styles;
    // The palette of the theme the resources were created in, never removed, and on top of it the values of the current
    // theme that differ from it (see Theme).
    readonly ResourceDictionary _palette;
    readonly CdsThemeName _baseTheme;
    ResourceDictionary? _delta;
    CdsThemeName _theme;

    /// <summary>Creates the Carbon resources in the White theme.</summary>
    public CdsResources()
    {
        _styles = CreateStyles(CdsOptions.Current);
        _baseTheme = _theme;
        _palette = CdsPalettes.CreateThemeDictionary(_theme);
        MergedDictionaries.Add(_styles);
        MergedDictionaries.Add(_palette);
        s_current = new WeakReference<CdsResources>(this);
    }

    /// <summary>The most recently created instance (normally the app's).</summary>
    internal static CdsResources? Current => s_current is not null && s_current.TryGetTarget(out var r) ? r : null;

    /// <summary>The app theme. Changing it re-themes everything outside a <see cref="CdsTheme"/> zone.</summary>
    public CdsThemeName Theme
    {
        get => _theme;
        set
        {
            if (_theme == value) return;
            var previous = _theme;
            _theme = value;
            // Adding a merged dictionary makes MAUI re-resolve every key it holds on every element of the app (attached
            // screens included), and its cost grows with the keys raised. Removing one raises nothing. So the full palette
            // of the first theme stays, and a small dictionary on top holds only the keys whose value differs from it now
            // or did before this switch (those revert to it silently otherwise): White and G10 share all but 17 colors, and
            // the 25 colors every theme shares and the constant context keys are never raised again.
            var delta = CdsPalettes.CreateThemeDelta(_baseTheme, previous, value);
            if (_delta is not null) MergedDictionaries.Remove(_delta);
            _delta = delta;
            MergedDictionaries.Add(delta);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Raised after <see cref="Theme"/> changes.</summary>
    public event EventHandler? ThemeChanged;

    static ResourceDictionary CreateStyles(CdsOptions options)
    {
        var rd = new ResourceDictionary();
        foreach (var style in Enum.GetValues<CdsTypeStyle>())
            rd.Add(CdsTypography.StyleKey(style), CdsTypography.CreateLabelStyle(style));

        var pageStyle = CdsPageStyles.CreateContentPageStyle(options);
        rd.Add(CdsPageStyles.ContentPageStyleKey, pageStyle);

        if (options.ApplyImplicitStyles)
        {
            rd.Add(pageStyle); // implicit: TargetType ContentPage, ApplyToDerivedTypes
            var label = new Style(typeof(Label)) { ApplyToDerivedTypes = true };
            label.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = CdsFonts.Sans });
            label.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = new DynamicResource(CdsColorKeys.TextPrimary) });
            rd.Add(label);
        }
        return rd;
    }
}
