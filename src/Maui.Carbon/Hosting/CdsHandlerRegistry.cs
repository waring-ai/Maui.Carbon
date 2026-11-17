namespace Maui.Carbon;

/// <summary>Registers the Carbon handlers and lifecycle hooks.</summary>
internal static partial class CdsHandlerRegistry
{
    internal static void Register(IMauiHandlersCollection handlers)
    {
        RegisterFoundation(handlers);
    }

    internal static void ConfigureLifecycle(MauiAppBuilder builder)
    {
        ConfigurePlatformLifecycle(builder);
    }

    static partial void RegisterFoundation(IMauiHandlersCollection handlers);

    static partial void ConfigurePlatformLifecycle(MauiAppBuilder builder);
}
