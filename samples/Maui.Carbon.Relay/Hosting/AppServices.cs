using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Pages;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.Services;
using Microsoft.Extensions.Logging;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Toasts via <see cref="CdsToast"/> on the main page's overlay host. Each toast is logged too, at the level of its
/// kind, so a failure the view models caught and showed the user (an error toast) is an <c>[ERR]</c> line in the log.
/// </summary>
public sealed class UserFeedback(IServiceProvider services, ILogger<UserFeedback> logger) : IUserFeedback
{
    public void Toast(FeedbackKind kind, string title, string? subtitle = null)
    {
        var level = kind switch
        {
            FeedbackKind.Error => LogLevel.Error,
            FeedbackKind.Warning => LogLevel.Warning,
            _ => LogLevel.Information,
        };
        logger.Log(level, "{Kind} toast: {Title} | {Subtitle}", kind, title, subtitle);
        var page = services.GetRequiredService<MainPage>();
        CdsToast.Show(page, new CdsToastOptions
        {
            Kind = CarbonMap.Notification(kind),
            Title = title,
            Subtitle = subtitle,
            Caption = DateTime.Now.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture),
            Timeout = kind == FeedbackKind.Error ? 0 : 5000,
            Role = kind == FeedbackKind.Error ? CdsNotificationRole.Alert : CdsNotificationRole.Status,
        });
    }
}

/// <summary>Applies the theme choice with <see cref="CdsThemeManager"/>.</summary>
public sealed class ThemeService(ISettingsService settings) : IThemeService
{
    public ThemeChoice Current { get; private set; } = settings.Current.Theme;

    public void Apply(ThemeChoice theme)
    {
        Current = theme;
        settings.Current.Theme = theme;
        if (CarbonMap.Theme(theme) is { } fixedTheme)
        {
            CdsThemeManager.StopFollowingSystem();
            CdsThemeManager.SetTheme(fixedTheme);
        }
        else
        {
            CdsThemeManager.FollowSystem();
        }
    }
}

/// <summary>Swaps the window between the login page and the app frame.</summary>
public sealed class SessionHost(IServiceProvider services) : ISessionHost
{
    public void ShowApp()
    {
        var page = services.GetRequiredService<MainPage>();
        SetPage(page);
        var navigator = services.GetRequiredService<Navigator>();
        if (navigator.CurrentRoute is null) _ = navigator.NavigateAsync(Routes.Dashboard);
        services.GetRequiredService<LiveFeedTimer>().Start();
    }

    public void ShowLogin()
    {
        services.GetRequiredService<LiveFeedTimer>().Stop();
        services.GetRequiredService<Navigator>().Reset();
        SetPage(services.GetRequiredService<LoginPage>());
    }

    static void SetPage(Page page)
    {
        if (Application.Current?.Windows.FirstOrDefault() is { } window) window.Page = page;
    }
}

/// <summary>Adds a simulated notification every 45 seconds while signed in, so the header badge feels live.</summary>
public sealed class LiveFeedTimer(INotificationFeed feed)
{
    IDispatcherTimer? _timer;

    /// <summary>Off in screenshot mode, so captures are deterministic.</summary>
    public static bool Enabled { get; set; } = true;

    public void Start()
    {
        if (!Enabled || _timer is not null || Application.Current is null) return;
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(45);
        _timer.Tick += (_, _) => feed.SimulateIncoming();
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }
}
