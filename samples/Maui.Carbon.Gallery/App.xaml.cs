using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell()) { Title = "Carbon Gallery" };
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            window.Width = 1280;
            window.Height = 900;
        }
        SnapshotRunner.AttachIfRequested(window);
        WalkRunner.AttachIfRequested(window);
        return window;
    }
}
