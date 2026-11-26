namespace Maui.Carbon;

/// <summary>
/// Font family aliases registered by <c>UseCarbon()</c>. Carbon uses three weights (300, 400, 600); pick the weight
/// with the alias, never with <c>FontAttributes.Bold</c> (which synthesizes a 700 weight).
/// </summary>
public static class CdsFonts
{
    /// <summary>IBM Plex Sans Light (300).</summary>
    public const string SansLight = "IBMPlexSansLight";
    /// <summary>IBM Plex Sans Regular (400): the UI default.</summary>
    public const string Sans = "IBMPlexSans";
    /// <summary>IBM Plex Sans SemiBold (600).</summary>
    public const string SansSemiBold = "IBMPlexSansSemiBold";
    /// <summary>IBM Plex Sans Condensed Regular (400): chart ticks, legends and labels (Carbon Charts' type).</summary>
    public const string SansCondensed = "IBMPlexSansCondensed";
    /// <summary>IBM Plex Sans Condensed Light (300): large chart words (word cloud words over 32px).</summary>
    public const string SansCondensedLight = "IBMPlexSansCondensedLight";
    /// <summary>IBM Plex Sans Condensed SemiBold (600): emphasized chart ticks (the first tick, a new day, month or year).</summary>
    public const string SansCondensedSemiBold = "IBMPlexSansCondensedSemiBold";
    /// <summary>IBM Plex Mono Regular (400), for code.</summary>
    public const string Mono = "IBMPlexMono";
    /// <summary>IBM Plex Serif Light (300), for quotations.</summary>
    public const string SerifLight = "IBMPlexSerifLight";
    /// <summary>IBM Plex Serif Regular (400), for quotations.</summary>
    public const string Serif = "IBMPlexSerif";

    /// <summary>Embedded font files and the alias each one registers.</summary>
    internal static readonly (string File, string Alias)[] Files =
    [
        ("IBMPlexSans-Light.ttf", SansLight),
        ("IBMPlexSans-Regular.ttf", Sans),
        ("IBMPlexSans-SemiBold.ttf", SansSemiBold),
        ("IBMPlexSansCondensed-Light.ttf", SansCondensedLight),
        ("IBMPlexSansCondensed-Regular.ttf", SansCondensed),
        ("IBMPlexSansCondensed-SemiBold.ttf", SansCondensedSemiBold),
        ("IBMPlexMono-Regular.ttf", Mono),
        ("IBMPlexSerif-Light.ttf", SerifLight),
        ("IBMPlexSerif-Regular.ttf", Serif),
    ];

    /// <summary>The Sans alias for a Carbon weight (300, 400 or 600).</summary>
    public static string SansForWeight(int weight) => weight switch
    {
        <= 300 => SansLight,
        >= 600 => SansSemiBold,
        _ => Sans,
    };
}
