# Charts

Charts are native controls drawn in the app (Carbon Charts 1.27), not a WebView. Each has a title, legend, toolbar (show as table, fullscreen, CSV/PNG/JPG export), tooltips, a keyboard and screen-reader data cursor, and a loading skeleton. The [catalog](components.md#charts) lists every chart; to pick one and design its labels, colors and dashboard placement, use the maui-carbon-design skill's `references/data-visualization.md`.

```xml
<cds:CdsLineChart Title="Weekly revenue (USD)" Data="{Binding Revenue}" Curve="MonotoneX" HeightRequest="320">
    <cds:CdsLineChart.BottomAxis>
        <cds:CdsChartAxis ScaleType="Time" />
    </cds:CdsLineChart.BottomAxis>
    <cds:CdsLineChart.LeftAxis>
        <cds:CdsChartAxis Title="Revenue (USD)" TickFormat="C0">
            <cds:CdsChartAxis.Thresholds>
                <cds:CdsChartThreshold Value="50000" Label="Target" />
            </cds:CdsChartAxis.Thresholds>
        </cds:CdsChartAxis>
    </cds:CdsLineChart.LeftAxis>
</cds:CdsLineChart>
```

```csharp
// One CdsChartPoint per value: Group is the series (legend item and color).
Revenue = rows.Select(r => new CdsChartPoint(r.Region, r.WeekStart, r.Amount)).ToList();
```

- **Data.** Most charts take `CdsChartPoint`s, and the axes decide which fields are read: a `Labels` axis reads `Key`, a `Time` axis reads `Date`, a `Linear` domain axis reads `X`, and the value axis reads `Value`. The constructors `(group, key, value)`, `(group, date, value)` and `(group, x, value)` cover these cases. `Low`/`High` make floating bars and bounds, `Size` is a bubble's third measure, and `Tag` carries your own row. A `null` `Value` leaves a gap. The chart does not watch the points themselves: replace a point, or the whole collection, to change it. An `ObservableCollection` redraws on add or remove.
- **Axes.** Axis charts derive from `CdsCartesianChart` and take `LeftAxis`/`BottomAxis`/`RightAxis`/`TopAxis`. With no axes set, a chart draws a linear value axis on the left and a labels axis at the bottom. The labels or time axis is the domain; put it on the left for horizontal bars. Title the value axis with its measure and unit. Keep `IncludeZero` for bars and areas. For a second value axis, put `CorrespondingDatasets` (group names) on the right axis. `IsPercentage` shows stacked charts as shares. `Thresholds` draws dashed target lines; use one or two.
- **Other item types.** `CdsTreemapChart`, `CdsCirclePackChart` and `CdsTreeChart` take a hierarchy of `CdsChartNode` (`Name`, `Value`, `Children`). `CdsAlluvialChart` takes `CdsAlluvialLink`s in `Data` and `CdsAlluvialNode`s in `Nodes`; always set `Units`. `CdsNetworkDiagram` takes `CdsNetworkNode`s that the app positions (`X`/`Y` centers; there is no automatic layout) and `CdsNetworkEdge`s in `Edges`. `CdsBulletChart` takes `CdsBulletChartItem` (value, target, `Ranges`). `CdsParallelCoordinatesChart` takes `CdsParallelRecord`s with `Dimensions` and `Filters`; there is no pointer brushing, so drive `Filters` from form controls.
- **Single-value charts.** `CdsGaugeChart` takes `Value`/`Delta`/`Status` and no `Data`. `CdsMeterChart` takes one point (`Group` is the title, `Value` is the percentage) with `StatusRanges` and `Peak`, or with `IsProportional`, `Total` and `Unit`, one point per part. `CdsDonutChart` has `CenterLabel`/`CenterNumber`, and `CdsSparklineChart` has `Type`.
- **Other layouts.** `CdsComboChart` holds `CdsComboChartType` children; each has a `Graph` (`SimpleBar`, `GroupedBar`, `StackedBar`, `Line`, `Area`…) and the `CorrespondingDatasets` it draws. In `CdsHeatmapChart` a point's `Key` is the column and its `Group` is the row. Pie and donut slices are named by `Group`. Histogram and box plot charts take one point per raw observation and compute the bins or statistics.
- **Maps.** `CdsChoroplethChart`, `CdsProportionalSymbolMap` and `CdsConnectionMap` need `GeoData`: TopoJSON or GeoJSON text that the app supplies. The package ships no world map; the gallery loads a Natural Earth file once and binds it. Choropleth points match regions by `Key`, against the feature id or name. Symbol maps take `CdsGeoPoint`s. Connection maps take `CdsGeoConnection`s, with coordinates or with named ends listed once in `Nodes`. Use `Projection`, `FitTo` and `FitBounds` to frame the map.
- **Look and behavior.** `HeightRequest` covers the whole chart, including its title, toolbar and legend (default 400; meters size themselves). Width fills the container. Colors come from Carbon's data-vis pairings (`ColorPairingOption`). Use `ColorScale` (group to `CdsColorKeys.DataVis01`…) to give a dataset the same color on every chart of a dashboard, never a hex color. Put `IsLoading` on the chart while data loads. `SelectedGroups` is two-way (legend isolation), and `SelectedGroupsChanged` reports changes. `ValueFormat`/`ValueFormatter` and `Culture` format tooltips and the table. Handle `Exporting` (set `Handled`) to save exports yourself instead of using the share sheet. Hierarchy and network charts raise `NodeClicked`/`CircleClicked`.
- **Accessibility.** The title names the chart for screen readers, so make it state the measure and unit. Keep "Show as table" in the toolbar for charts where color alone carries the value, such as heat maps and choropleths. Describe network diagrams in text near the chart.
