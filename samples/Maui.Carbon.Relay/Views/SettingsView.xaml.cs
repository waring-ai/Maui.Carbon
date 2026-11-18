using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The settings tabs. Bindings carry the values; this code-behind only keeps the theme choice in step when the theme
/// changes elsewhere (the account panel).
/// </summary>
public partial class SettingsView : ContentView
{
    public SettingsView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        CdsTokens.SetColor(Rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        Loaded += (_, _) => CdsThemeManager.ThemeChanged += OnThemeChanged;
        Unloaded += (_, _) => CdsThemeManager.ThemeChanged -= OnThemeChanged;
    }

    void OnThemeChanged(object? sender, EventArgs e) => (BindingContext as SettingsViewModel)?.SyncTheme();
}
