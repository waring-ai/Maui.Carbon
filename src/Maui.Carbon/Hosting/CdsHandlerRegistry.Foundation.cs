namespace Maui.Carbon;

internal static partial class CdsHandlerRegistry
{
    static partial void RegisterFoundation(IMauiHandlersCollection handlers)
    {
        handlers.AddHandler<CdsHitTarget, CdsHitTargetHandler>();
        handlers.AddHandler<CdsOverlayHost, CdsOverlayHostHandler>();
        handlers.AddHandler<CdsNativeEntry, CdsNativeEntryHandler>();
        handlers.AddHandler<CdsNativeEditor, CdsNativeEditorHandler>();
        handlers.AddHandler<CdsNativePicker, CdsNativePickerHandler>();
    }
}
