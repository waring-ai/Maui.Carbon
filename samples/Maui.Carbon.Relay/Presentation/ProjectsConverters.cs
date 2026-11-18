namespace Maui.Carbon.Relay.Presentation;

/// <summary>Converters used only by the projects screen.</summary>
public static class ProjectsConverters
{
    /// <summary>Starred → the filled star; otherwise the outline star.</summary>
    public static IValueConverter StarIcon { get; } = new FuncConverter<bool, CdsIconName>(starred => starred ? CdsIconName.StarFilled : CdsIconName.Star);
}
