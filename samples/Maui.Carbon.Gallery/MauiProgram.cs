using Microsoft.Extensions.Logging;

namespace Maui.Carbon.Gallery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseCarbon();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
