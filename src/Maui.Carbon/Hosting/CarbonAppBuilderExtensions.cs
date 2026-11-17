namespace Maui.Carbon;

/// <summary>Registers Maui.Carbon with a MAUI app.</summary>
public static class CarbonAppBuilderExtensions
{
    /// <summary>
    /// Registers the IBM Plex fonts, the Carbon handlers and the options. Also add <c>&lt;cds:CdsResources /&gt;</c> to
    /// <c>Application.Resources</c>.
    /// </summary>
    public static MauiAppBuilder UseCarbon(this MauiAppBuilder builder, Action<CdsOptions>? configure = null)
    {
        var options = new CdsOptions();
        configure?.Invoke(options);
        CdsOptions.Current = options;

        builder.ConfigureFonts(fonts =>
        {
            var assembly = typeof(CdsResources).Assembly;
            foreach (var (file, alias) in CdsFonts.Files)
                fonts.AddEmbeddedResourceFont(assembly, file, alias);
        });

        builder.ConfigureMauiHandlers(CdsHandlerRegistry.Register);
        CdsHandlerRegistry.ConfigureLifecycle(builder);
        return builder;
    }
}
