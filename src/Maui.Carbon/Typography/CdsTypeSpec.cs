namespace Maui.Carbon;

/// <summary>The resolved metrics of one Carbon type style.</summary>
/// <param name="TokenName">Carbon token name, e.g. <c>body-compact-01</c>.</param>
/// <param name="FontFamily">The registered font alias (see <see cref="CdsFonts"/>).</param>
/// <param name="FontWeight">300, 400 or 600.</param>
/// <param name="FontSize">Size in device-independent units.</param>
/// <param name="LineHeight">CSS line-height ratio (line box height ÷ font size).</param>
/// <param name="CharacterSpacing">Tracking in px; MAUI's <c>CharacterSpacing</c> takes this value directly.</param>
public readonly record struct CdsTypeSpec(string TokenName, string FontFamily, int FontWeight, double FontSize, double LineHeight, double CharacterSpacing)
{
    /// <summary>IBM Plex's natural line height as a multiple of the font size (ascender + descender).</summary>
    public const double NaturalLineHeight = 1.3;

    /// <summary>
    /// Token line heights from here to <see cref="LooseLimit"/> keep Plex's natural line height (the compact styles: 1.28572,
    /// 1.3, 1.33333 are within half a pixel of it at their sizes).
    /// </summary>
    const double TightLimit = 1.27;

    /// <summary>The upper end of the line heights that keep the natural height.</summary>
    const double LooseLimit = 1.35;

    /// <summary>
    /// The value for MAUI's <c>Label.LineHeight</c> (a multiple of the font's natural line height), or -1 to keep the
    /// natural height. Compact single-line styles keep the natural height; paragraph styles get the extra leading, and the
    /// large headings and displays, whose token is tighter than 1.3, get a multiple below 1 so their line box is the
    /// token's (heading-05: 40px, not 41.6px).
    /// </summary>
    public double MauiLineHeight => LineHeight is > LooseLimit or < TightLimit ? Math.Round(LineHeight / NaturalLineHeight, 3) : -1;

    /// <summary>The CSS line box height in device-independent units.</summary>
    public double LineBoxHeight => Math.Round(FontSize * LineHeight, 2);

    /// <summary>The spec of a style.</summary>
    public static CdsTypeSpec Of(CdsTypeStyle style) => CdsTypeSpecs.Get(style);

    /// <summary>The spec of a style at a breakpoint (fluid styles step up; see <see cref="CdsFluidType"/>).</summary>
    public static CdsTypeSpec Of(CdsTypeStyle style, CdsBreakpoint breakpoint) => CdsFluidType.Resolve(style, breakpoint);
}
