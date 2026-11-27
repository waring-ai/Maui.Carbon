namespace Maui.Carbon;

/// <summary>Color keys of a card in one state. <c>null</c> means transparent.</summary>
/// <param name="Background">The card fill.</param>
/// <param name="Border">The card's 1px border.</param>
/// <param name="ZoneBackground">The fill of the click zone when only part of the card is clickable (zones two and three).</param>
public readonly record struct CdsCardVisuals(string Background, string? Border, string? ZoneBackground);

/// <summary>
/// Maps a card's click zone and state to tokens (from <c>ibm-products-styles/src/components/Card/_card.scss</c>): a
/// <c>layer-01</c> fill, and for a clickable card a <c>border-tile-01</c> border with <c>layer-hover-01</c> on hover. When
/// only the header and body (zone two) or the body (zone three) are clickable, that zone takes the hover fill instead.
/// </summary>
public static class CdsCardTokens
{
    /// <summary>Resolves the colors. <paramref name="clickZone"/> is <c>null</c> for a card that is not clickable.</summary>
    public static CdsCardVisuals Resolve(CdsCardClickZone? clickZone, CdsInteractionState state)
    {
        var hover = state.HasFlag(CdsInteractionState.Hover) && !state.HasFlag(CdsInteractionState.Disabled);
        return clickZone switch
        {
            null => new(CdsColorKeys.Layer01, null, null),
            CdsCardClickZone.One => new(hover ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01, CdsColorKeys.BorderTile01, null),
            _ => new(CdsColorKeys.Layer01, null, hover ? CdsColorKeys.LayerHover01 : null),
        };
    }

    /// <summary>The title style: <c>heading-03</c> on expressive cards and large productive titles, else <c>heading-compact-02</c>.</summary>
    public static CdsTypeStyle TitleStyle(bool productive, CdsCardTitleSize size) =>
        productive && size == CdsCardTitleSize.Default ? CdsTypeStyle.HeadingCompact02 : CdsTypeStyle.Heading03;

    /// <summary>The separator before a header ghost button (<c>::before</c>, 1x24, <c>layer-hover-01</c>).</summary>
    public const string HeaderSeparator = CdsColorKeys.LayerHover01;

    /// <summary>The rule above a productive card's bottom bar.</summary>
    public const string FooterRule = CdsColorKeys.BorderSubtle01;
}
