---
name: maui-carbon
description: Build .NET MAUI interfaces with Maui.Carbon (Waring.Maui.Carbon), the Carbon Design System component library. Use when finding or using Cds controls, writing Carbon XAML or C# UI, setting up the package, configuring themes and layers, binding forms, tables, charts and data visualizations, navigation or overlays, or identifying supported components, their properties and companion APIs. For design decisions (layout, hierarchy, which component fits a need, copy, states, accessibility, conformance review), use the maui-carbon-design skill.
---

# Maui.Carbon

Use the library's existing components and tokens when building Carbon interfaces. This skill covers what exists and how to use it; the **maui-carbon-design** skill covers how to design a Carbon screen (layout, hierarchy, layering, color, type, spacing, patterns, copy, states and accessibility). Load both when designing and building a new screen. All public controls use the `Cds` prefix and the `Maui.Carbon` namespace. The NuGet package ID is `Waring.Maui.Carbon`.

## Find the right API

1. Read [the component catalog](references/components.md) to select a component and its companion types.
2. In a library checkout, open the matching `samples/Maui.Carbon.Gallery/Pages/<category>/<component>Page.xaml` and `.xaml.cs`. These demonstrate composition, items, templates, events and state. The catalog links to the relevant directories.
3. Check `src/Maui.Carbon/Components/<category>/<component>/`, including `*.Properties.cs`, for exact property types, binding modes, defaults and events. Some properties are inherited; read the base type as needed. Do not infer MAUI API names from Carbon React props.
4. For design intent (which component fits the need, how screens compose it, what the copy says), use the maui-carbon-design skill: `skills/maui-carbon-design/references/choosing-components.md` and `patterns.md`. Maintainers with the Carbon source snapshot can also read `design/carbon/components/<component>/README.md` (not checked in). Use the C# implementation as the API authority.
5. For whole screens, follow `samples/Maui.Carbon.Gallery/Pages/Patterns/` or the MVVM app in `samples/Maui.Carbon.Relay/`. Gallery wrappers such as `GalleryExample` and `ThemeStrip` belong to the sample, not the package.

Paths above are relative to the repository root. If installed outside this checkout, obtain the matching library version's source/examples before relying on a property not covered here. Do not assume the newest source matches an older installed package. For changes to library internals, also read `CONVENTIONS.md`.

## Configure an app

The checkout targets .NET 10 and requires the MAUI workload. Add the package to a consuming app:

```sh
dotnet add package Waring.Maui.Carbon
```

Register fonts, handlers and lifecycle support in `MauiProgram.cs`:

```csharp
using Maui.Carbon;

// Continue configuring the application's MauiAppBuilder.
builder.UseMauiApp<App>().UseCarbon();
```

Add resources in `App.xaml` (merge into existing resources rather than replacing them):

```xml
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:cds="https://github.com/waring-ai/Maui.Carbon"
             x:Class="MyApp.App">
    <Application.Resources>
        <cds:CdsResources Theme="White" />
    </Application.Resources>
</Application>
```

`MyApp.App` is a placeholder for the consuming app's class. `UseCarbon` defaults `InjectPageOverlayHost`, `ApplyImplicitStyles` and `RespectReducedMotion` to true. When disabling host injection, put `CdsOverlayHost` at each page's root; dropdowns, menus, tooltips, modals and toasts require a host. Check custom page styles if overlays stop appearing.

## Compose a form

The following is a page-content fragment; declare the `cds` namespace shown above on the page. Supply a BindingContext with the named properties and commands (and change notifications).

```xml
<cds:CdsForm>
    <cds:CdsText Text="Create cluster" TypeStyle="Heading03" />
    <cds:CdsTextInput Label="Cluster name"
                      Placeholder="e.g. prod-eu-de-1"
                      Text="{Binding ClusterName, Mode=TwoWay}"
                      IsInvalid="{Binding IsNameInvalid}"
                      InvalidText="{Binding NameError}" />
    <cds:CdsSelect Label="Region" SelectedItem="{Binding Region, Mode=TwoWay}">
        <cds:CdsSelectItem Value="eu-de" Text="Frankfurt" />
        <cds:CdsSelectItem Value="us-south" Text="Dallas" />
    </cds:CdsSelect>
    <HorizontalStackLayout Spacing="16">
        <cds:CdsButton Text="Cancel" Kind="Secondary" Command="{Binding CancelCommand}" />
        <cds:CdsButton Text="Create cluster" Icon="Add" Command="{Binding CreateCommand}" />
    </HorizontalStackLayout>
</cds:CdsForm>
```

Keep labels meaningful even when visually hidden. Validate on blur or submit; bind invalid state and an actionable message. Use `CdsFormGroup` with `Legend` for related fields. `CdsFormItem` and `CdsFormLabel` align custom inputs. Fluid inputs and `CdsFluidForm` use the denser label-inside-field treatment; they do not display helper text like standard inputs. Do not assume a fluid password input exists.

Use `CdsSelect` for simple choices, `CdsComboBox` for searchable single selection, and `CdsMultiSelect` for multiple choices. Inspect each component's item and selection contract before binding domain objects.

## Choose a local file

For a form field that picks a file to read where it is (an import, a certificate) rather than upload it, use `CdsFileUploader` bound to the view model; never a text field plus a raw MAUI `FilePicker` button.

```xml
<cds:CdsFileUploader LabelTitle="Domain list"
                     LabelDescription="A .zip export. It is read in place, never unpacked or copied."
                     ButtonLabel="Choose file" ChosenButtonLabel="Change file" ButtonKind="Tertiary" Size="Md"
                     Accept=".zip" AccessMode="Path"
                     FilePath="{Binding ArchivePath}"
                     IsInvalid="{Binding IsArchiveInvalid}"
                     ErrorSubject="{Binding ArchiveError}"
                     ErrorBody="{Binding ArchiveErrorHelp}" />
```

- `FilePath` (`string?`) is two-way: a pick or drop sets it, removing the row sets `null`, and the view model sets it to pre-fill a default (`null` or `""` shows no file). One file (`Multiple` false, the button default): a new pick replaces the row.
- Validate in the view model when the path changes and set `ErrorSubject` (and `ErrorBody`) before `IsInvalid`, so the alert is spoken once. Once used, the three describe whichever file is chosen. `FileStatus` (`CdsFileStatus?`) sets the row's spinner or check; `null` leaves it.
- `AccessMode="Path"`: on iOS and Mac Catalyst the uploader opens the document picker in open-in-place mode and holds the file's security-scoped access until the file leaves `Files`, so `File.OpenRead(path)` or `ZipFile.OpenRead(path)` reads the original (no copy, any size). Open it while it is chosen; access ends with the process (persist it with your own bookmark). The default `Stream` mode suits uploads: read through `CdsUploadedFile.OpenReadAsync()`, because a sandboxed Apple app may not open the bare path. Android's picker may hand over a cached copy.
- Several files: set `Multiple="True"` and bind `FilePaths` (`IReadOnlyList<string>?`, two-way; the control publishes a new array on every change and never edits yours; assign a new list to change the files; `null` changes nothing). Bind `FilePath` or `FilePaths`, not both. Per-file errors go on each `CdsUploadedFile` in `Files`.
- `FilesAddedCommand` gets the added paths (`IReadOnlyList<string>`) and `FileRemovedCommand` the removed path (`string?`), so a view model without MAUI references can react. `CdsUploadedFile` has `Path`, `FromPath(path)` and `OpenReadAsync()`.
- `Accept` takes extensions and MIME types (`.zip, .tar.gz, image/*, application/x-pem-file`); on Apple platforms each maps to a uniform type at runtime, so any extension filters the picker.

## Apply themes and tokens

Use `White`, `G10`, `G90` or `G100`. Switch the app with `CdsThemeManager.SetTheme(CdsThemeName.G100)` or `CdsThemeManager.FollowSystem()`.

```xml
<cds:CdsTheme Theme="G90">
    <cds:CdsLayer>
        <VerticalStackLayout BackgroundColor="{DynamicResource CdsLayer01}"
                             Padding="{x:Static cds:CdsSpacing.Spacing05}">
            <cds:CdsText Text="Environment" TypeStyle="Heading03" />
            <cds:CdsTextInput Label="Name" />
        </VerticalStackLayout>
    </cds:CdsLayer>
</cds:CdsTheme>
```

Theme zones nest and reset layering; `CdsLayer` advances the surface level. Reference the `-01` token keys and let the nearest layer resolve them. Use dynamic color resources, spacing/sizing constants, `CdsText` type styles and Carbon icons rather than fixed hex colors, custom font sizes or Unicode substitutes. Confirm icon names in `Generated/CdsIcons.g.cs`.

## Bind a table

```xml
<cds:CdsDataTable Title="Clusters" ItemsSource="{Binding Clusters}"
                   IsSortable="True" IsSearchable="True" Selection="Multi">
    <cds:CdsDataTableHeader Key="Name" Header="Name" Width="*" />
    <cds:CdsDataTableHeader Key="Region" Header="Region" Width="140" />
</cds:CdsDataTable>
```

Match header keys to the row's actual properties/keys; inspect value selectors and templates for complex cells. Use fixed/star column widths. For large data, `StickyHeader="True"` gives the table its own virtualized scrolling; provide a bounded layout area. Inside a vertical `ScrollView` the table also virtualizes against the page's scrolling (`VirtualizeInScrollView`, default true; virtualized rows are one line high). Bind every value in cell and expanded-row templates because views are reused. A table outside a scroll view, with an `Auto` column, or with `VirtualizeInScrollView="False"` builds every row; page large datasets with `CdsPagination`. Wire pagination to the actual slice of rows; adding a pager does not itself fetch or slice data. `CdsTable` and its parts are the manual static-table alternative.

## Plot a chart

Charts are native controls drawn in the app (Carbon Charts 1.27); they are not a WebView. Each one has a title, legend, toolbar (show as table, fullscreen, CSV/PNG/JPG export), tooltips, a keyboard and screen-reader data cursor, and a loading skeleton. Use the chart that matches the question, such as change over time, comparison, part of a whole or distribution. Use the maui-carbon-design skill's `references/data-visualization.md` to pick one and to design its labels, colors and dashboard placement; the [catalog](references/components.md#charts) lists every chart.

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

## Navigation and overlays

Use `CdsUIShell` with header and side navigation for a Carbon app frame. Use `CdsTabs` containing `CdsTab` for related panels; `CdsContentSwitcher` with `CdsSwitch` switches views of the same content. See the gallery for selection and close-request handling.

Use `CdsModal` for standard dialogs and `CdsComposedModal` with header/body/footer parts for custom structure. `CdsModal.IsOpen` supports two-way binding; handle submit/close requests as shown in the gallery. Modal scrims cover their page, including a Carbon shell, but cannot cover native MAUI Shell chrome.

For a transient success message from a page or another element on it:

```csharp
CdsToast.Show(this, new CdsToastOptions
{
    Kind = CdsNotificationKind.Success,
    Title = "Cluster created",
    Timeout = 5000,
});
```

Use inline notifications for contextual messages and actionable notifications when a response is needed. Do not auto-dismiss errors. Give icon-only actions an accessible name through the component's documented API, and preserve native focus and keyboard behavior.

## Verify and maintain

Build the consuming app for the target platform. Check the UI in light/dark themes, nested layers, narrow/wide layouts, keyboard focus, and applicable invalid, disabled, read-only and loading states. For repository changes, use `tools/verify.sh` (options in README). To find a slow or over-busy control, listen to the `Maui.Carbon` meter (instrument names on `CdsMetrics`; an optimized iOS, Mac Catalyst or Android build needs `<MetricsSupport>true</MetricsSupport>`) or run `tools/relay-exercise.sh` and read its "Carbon metrics" section. Documentation-only edits need link/API checks rather than a full MAUI build.

The README records screenshot review on Mac Catalyst; other targets being compiled does not establish visual parity. On iOS/macOS 27, follow the gallery's UIScene lifecycle configuration. Check the current README and platform files when changing target versions.

When updating this skill, compare its catalog with public classes in `src/Maui.Carbon/Components` and gallery pages. List actual concrete controls and companion types; keep abstract bases, token/metrics helpers, models and event arguments distinct from user-facing components. Do not turn gallery Patterns into invented package controls.
