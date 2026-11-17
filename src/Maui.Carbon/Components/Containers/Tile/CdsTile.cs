namespace Maui.Carbon;

/// <summary>
/// A flat container on a <c>layer-01</c> fill with 16px padding that groups a small amount of related information (a
/// summary metric, a plan, a service). Tiles are square with no shadow or border; separation comes from the layer step,
/// so put a tile on a layer-01 surface inside a <see cref="CdsLayer"/>. Keep tiles in one group the same variant and
/// height. Use <see cref="CdsClickableTile"/> when the whole tile is one action.
/// </summary>
public class CdsTile : CdsTileBase
{
    /// <summary>Creates an empty tile.</summary>
    public CdsTile()
    {
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override CdsTileKind TileKind => CdsTileKind.Static;
}
