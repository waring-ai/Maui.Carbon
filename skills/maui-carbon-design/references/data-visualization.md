# Data visualization

Design guidance for Maui.Carbon's charts (native ports of Carbon Charts 1.27). For the data and axis API, see the maui-carbon skill's "Plot a chart" section and catalog.

## Start from the question

| The data answers | Use |
|---|---|
| How do categories compare? | `CdsSimpleBarChart`; `CdsGroupedBarChart` for several series per category; `CdsLollipopChart` when many categories make bars heavy |
| What makes up each total? | `CdsStackedBarChart` (`IsPercentage` for shares); `CdsStackedAreaChart` over continuous time |
| How does it change over time? | `CdsLineChart` (step curve for step changes); `CdsAreaChart` for one or two volumes; `CdsSparklineChart` in a tile or table |
| Two measures that read best in different forms | `CdsComboChart` (bars with a line, optionally on a right axis) |
| How do two or three measures relate? | `CdsScatterChart`; `CdsBubbleChart` when size carries a third |
| How are values distributed? | `CdsHistogramChart`; `CdsBoxplotChart` to compare distributions |
| What share of one whole? (up to about five parts) | `CdsDonutChart` (center total) or `CdsPieChart`; `CdsMeterChart` proportional for parts of a capacity |
| How close to a goal or limit? | `CdsMeterChart` (bar with status), `CdsGaugeChart` (percentage with delta), `CdsBulletChart` (value, target, ranges) |
| How does one profile score on several measures? | `CdsRadarChart`; `CdsParallelCoordinatesChart` for many records across many measures |
| Values across two categories | `CdsHeatmapChart` |
| Part to whole in a hierarchy | `CdsTreemapChart`; `CdsCirclePackChart` when structure matters more than exact size; `CdsTreeChart` for structure alone |
| Flows between stages | `CdsAlluvialChart`; `CdsNetworkDiagram` for topologies you lay out |
| Values by place | `CdsChoroplethChart` (areas), `CdsProportionalSymbolMap` (points), `CdsConnectionMap` (routes) |
| How often do words occur? | `CdsWordCloudChart` for an impression only; a bar chart when counts matter |
| One number | Not a chart: a large `CdsText` figure (`Heading05`+) in a `CdsTile`, optionally with a sparkline |

Prefer the simplest chart that answers the question; bars and lines beat novel forms. Avoid pies with more than five slices, 3D, dual axes that imply a false correlation, and stacked charts where the middle series must be compared.

## Color

- Categorical colors come from the chart: one to five series use Carbon's curated pairings (`ColorPairingOption`), six or more the fourteen `DataVis01`…`DataVis14` colors in order; dark themes switch to the dark set. Do not pick series colors by hand.
- Keep a dataset the same color on every chart of a screen with `ColorScale` (group to `CdsColorKeys.DataVis01`…). One series uses a single color; it needs no legend.
- Sequential (heat maps, choropleths): one hue, strongest for the largest value. Diverging scales only for data with a meaningful midpoint (zero, a target).
- Status colors (`DataVisAlert*`, support colors) mean status only, in meters, gauges and thresholds, always with a label. Never use red and green as ordinary series colors.
- More than about five series is hard to read: group the rest into "Other", split into small multiples, or let the legend isolate.

## Anatomy and labelling

- Every chart has a title that names the measure and unit ("Weekly revenue (USD)"); it may state the insight ("Revenue up 12% since March"). The title is also its screen-reader name.
- Title the value axis with its unit; format ticks (`TickFormat`, `ValueFormat`, `Culture`) as people read the numbers (currency, %, compact thousands).
- Bars and areas start at zero (`IncludeZero`); lines and scatter may crop to show change.
- Mark gaps instead of interpolating: a `null` value breaks the line.
- One or two thresholds (`CdsChartThreshold`) for targets or limits, labelled; not a grid of annotations.
- Legend at the bottom by default (`LegendPosition`), top where vertical space is scarce; the same position for every chart on a dashboard.
- Keep the toolbar ("Show as table", fullscreen, export); it is how values too small to see are read.
- Size: `HeightRequest` covers title, toolbar, legend and plot. Give axis charts at least about 300; sparklines and meters are small by design.
- Loading: `IsLoading` shows the chart skeleton. No data: say so in the tile ("No incidents in the last 30 days"), do not show an empty axis.

## Dashboards

- The most important chart or figure top left with the most space; headline numbers first, then trends, then breakdowns.
- Group related charts in tiles on the grid with consistent heights; one unit system and one color per dataset throughout.
- Strip what does not help interpretation: gridlines are already subtle; avoid extra borders, backgrounds and decorative icons.
- Each tile links to the detail behind it when there is one.

## Accessibility

- Never encode meaning in color alone: legends or direct labels, axis titles with units, labels with status colors.
- Heat maps and maps keep their color legend and "Show as table"; adjacent color steps do not meet contrast, so values must also be in the tooltip and table.
- Describe a network diagram or a complex flow in text near the chart.
