using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Hosting;
using Maui.Carbon.Relay.Pages;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay;

public partial class App : Application
{
    readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _services.GetRequiredService<IThemeService>().Apply(_services.GetRequiredService<ISettingsService>().Current.Theme);
        var window = new Window(_services.GetRequiredService<LoginPage>()) { Title = "Relay" };
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            // Wide enough for the lg breakpoint, where the side nav is pinned open beside the content.
            window.Width = 1440;
            window.Height = 900;
        }
        RouteSnapshotRunner.AttachIfRequested(window, _services);
        ExerciseRunner.AttachIfRequested(window, _services);
        return window;
    }
}
