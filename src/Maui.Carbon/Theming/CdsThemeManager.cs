namespace Maui.Carbon;

/// <summary>Switches the app-wide Carbon theme (the theme outside any <see cref="CdsTheme"/> zone).</summary>
public static class CdsThemeManager
{
    static (CdsThemeName Light, CdsThemeName Dark)? s_follow;

    /// <summary>The current app theme.</summary>
    public static CdsThemeName Current => CdsResources.Current?.Theme ?? CdsThemeName.White;

    /// <summary>Raised after the app theme changes.</summary>
    public static event EventHandler? ThemeChanged;

    /// <summary>Sets the app theme. Requires <see cref="CdsResources"/> in the application resources.</summary>
    public static void SetTheme(CdsThemeName theme)
    {
        var resources = CdsResources.Current ?? throw new InvalidOperationException(
            "No CdsResources found. Add <cds:CdsResources /> to Application.Resources (App.xaml) before switching themes.");
        if (resources.Theme == theme) return;
        var start = CdsMeter.Start(CdsMeter.ThemeSwitch);
        try
        {
            resources.Theme = theme;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
        finally
        {
            CdsMeter.Stop(CdsMeter.ThemeSwitch, start, CdsMetrics.ThemeTag, theme.ToString());
        }
    }

    /// <summary>
    /// Follows the system light/dark appearance: <paramref name="light"/> when the OS is light, <paramref name="dark"/>
    /// when it is dark. Call <see cref="StopFollowingSystem"/> to keep the theme fixed again.
    /// </summary>
    public static void FollowSystem(CdsThemeName light = CdsThemeName.White, CdsThemeName dark = CdsThemeName.G100)
    {
        var app = Application.Current ?? throw new InvalidOperationException("FollowSystem needs a running Application.");
        if (s_follow is null) app.RequestedThemeChanged += OnRequestedThemeChanged;
        s_follow = (light, dark);
        Apply(app.RequestedTheme);
    }

    /// <summary>Stops following the system appearance.</summary>
    public static void StopFollowingSystem()
    {
        if (s_follow is null) return;
        if (Application.Current is { } app) app.RequestedThemeChanged -= OnRequestedThemeChanged;
        s_follow = null;
    }

    static void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e) => Apply(e.RequestedTheme);

    static void Apply(AppTheme theme)
    {
        if (s_follow is not { } f) return;
        SetTheme(theme == AppTheme.Dark ? f.Dark : f.Light);
    }
}
