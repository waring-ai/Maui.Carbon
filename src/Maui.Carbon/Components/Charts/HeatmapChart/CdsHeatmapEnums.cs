namespace Maui.Carbon;

/// <summary>How a heat map or choropleth draws its color legend (Carbon's <c>colorLegend.type</c>).</summary>
public enum CdsChartColorLegendType
{
    /// <summary>A continuous 300 x 8px gradient through every step of the palette (default).</summary>
    Linear,
    /// <summary>One block per color step, as the quantize scale colors the data.</summary>
    Quantize,
}

/// <summary>When heat map cells are separated by 1px dividers in <c>background</c> (Carbon's <c>heatmap.divider.state</c>).</summary>
public enum CdsHeatmapDivider
{
    /// <summary>Dividers when cells are at least 16 x 16px (default).</summary>
    Auto,
    /// <summary>Always draw dividers.</summary>
    On,
    /// <summary>Never draw dividers.</summary>
    Off,
}
