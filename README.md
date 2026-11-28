# Maui.Carbon

An unofficial .NET MAUI implementation of IBM's [Carbon Design System](https://carbondesignsystem.com) (v11):
the four themes (White, Gray 10, Gray 90, Gray 100) with nestable theme zones, the layering model, the IBM Plex type
scale, Carbon icons and the Carbon component set, for Android, iOS, Mac Catalyst and Windows.

> Not affiliated with or endorsed by IBM. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Getting started

```bash
dotnet add package Waring.Maui.Carbon
```

The package ID is `Waring.Maui.Carbon`; the assembly and namespace are `Maui.Carbon`.

`MauiProgram.cs`:

```csharp
builder
    .UseMauiApp<App>()
    .UseCarbon();          // registers IBM Plex, the Carbon handlers and the page overlay host
```

`App.xaml`:

```xml
<Application xmlns:cds="https://github.com/waring-ai/Maui.Carbon" ...>
    <Application.Resources>
        <cds:CdsResources Theme="White" />
    </Application.Resources>
</Application>
```

A page:

```xml
<ContentPage xmlns:cds="https://github.com/waring-ai/Maui.Carbon" ...>
    <VerticalStackLayout Padding="32" Spacing="24">
        <cds:CdsText Text="Create cluster" TypeStyle="Heading05" />
        <cds:CdsTextInput Label="Cluster name" Placeholder="e.g. prod-eu-de-1" />
        <cds:CdsButton Text="Create cluster" Icon="Add" Clicked="OnCreate" />
    </VerticalStackLayout>
</ContentPage>
```

## Components

Every public control is prefixed `Cds` (`CdsButton`, `CdsDataTable`) and lives in the `Maui.Carbon` namespace
(XAML: `xmlns:cds="https://github.com/waring-ai/Maui.Carbon"`). The gallery has a page for each.

| Category | Components |
|---|---|
| Foundations | `CdsResources`, `CdsThemeManager`, `CdsTheme`, `CdsLayer`, `CdsText`, `CdsIcon`, tokens (`CdsColorKeys`, `CdsSpacing`, `CdsSizing`, `CdsTypeStyle`, `CdsDuration`, `CdsEasing`) |
| Actions | Button, ButtonSet, IconButton, CopyButton, Menu, OverflowMenu, MenuButton, ComboButton |
| Forms | Form (FormGroup, FormItem, FormLabel), TextInput, PasswordInput, TextArea, NumberInput, Search, Select, Dropdown, ComboBox, MultiSelect, Checkbox, CheckboxGroup, RadioButtonGroup, Toggle, Slider, DatePicker, TimePicker, FileUploader |
| Fluid forms | FluidForm and the Fluid variants of the text, number, search, select, time, dropdown, combo box, multi-select and date inputs |
| Data | DataTable (+ Table primitives), DataTableSkeleton, Pagination, PaginationNav, TreeView, StructuredList, ContainedList, Accordion, CodeSnippet, OrderedList / UnorderedList |
| Charts | SimpleBarChart, GroupedBarChart, StackedBarChart, LineChart, AreaChart, StackedAreaChart, SparklineChart, ScatterChart, BubbleChart, LollipopChart, ComboChart, HistogramChart, BoxplotChart, BulletChart, PieChart, DonutChart, GaugeChart, MeterChart, RadarChart, HeatmapChart, TreemapChart, CirclePackChart, TreeChart, AlluvialChart, WordCloudChart, NetworkDiagram, ParallelCoordinatesChart, ChoroplethChart, ProportionalSymbolMap, ConnectionMap (Carbon Charts 1.27, drawn natively: tooltips, legend isolation, keyboard and screen-reader data cursor, show as table, fullscreen, CSV/PNG/JPG export) |
| Navigation | UIShell (Header, SideNav, Switcher...), HeaderPanel, PageHeader, Tabs, Breadcrumb, ContentSwitcher, Link |
| Containers | Tile, ClickableTile, ExpandableTile, SelectableTile, RadioTile / TileGroup |
| Overlays | Modal (+ ComposedModal), Dialog, Popover, Tooltip, Toggletip, DefinitionTooltip |
| Feedback | InlineNotification, ActionableNotification, ToastNotification (`CdsToast.Show`), Callout, Tag, DismissibleTag, OperationalTag, SelectableTag, BadgeIndicator, IconIndicator, ShapeIndicator, Loading, InlineLoading, ProgressBar, ProgressIndicator, SkeletonText, SkeletonPlaceholder, SkeletonIcon |
| AI | AILabel (+ content and actions, `CdsAiTreatment`), AISkeleton, ChatButton |
| Layout | Grid (2x grid, breakpoints), Stack, AspectRatio, Heading / Section |

The gallery's **Patterns** section composes them into Carbon's product patterns (create form, resource list,
settings, filtering, empty states, loading, notifications, confirmation, read-only and disabled, overflow content,
login).

## How it works

- **Theming**: color tokens are dynamic resources. `CdsResources` holds the app theme; `CdsTheme` zones and `CdsLayer`
  scopes re-bind them for a subtree, so components only ever reference the `-01` tokens.
- **Interaction**: each control draws its own visuals and lays an invisible native button (`CdsHitTarget`) over them,
  which gives real keyboard focus, Tab order, Enter/Space and screen-reader roles on every platform.
- **Overlays**: every `ContentPage` gets a `CdsOverlayHost` through an implicit page style; menus, list boxes,
  popovers, tooltips, modals and toasts open there, mirror the theme and layer of their trigger, and route Escape and
  arrow keys.
- **Metrics**: the library publishes `System.Diagnostics.Metrics` instruments on the meter `Maui.Carbon`
  (names in `CdsMetrics`): draws of self-drawing views, `ApplyVisuals` runs, measure and arrange passes, coalesced
  rebuilds, activations, theme switches and loading-animation frames, each tagged with the control type. They cost one
  flag check until a `MeterListener`, `dotnet-counters monitor --counters Maui.Carbon` or OpenTelemetry listens.
  Optimized iOS, Mac Catalyst and Android builds trim metrics away unless the app sets `<MetricsSupport>true</MetricsSupport>`.
- **Requirements**: .NET 11 with the MAUI workload (Microsoft.Maui.Controls 11.0.10 or later). On iOS/macOS 27 your
  app must adopt the UIScene lifecycle (see the gallery's `Platforms/*/Info.plist` and `SceneDelegate.cs`).

## Themes and layers

- Switch the app theme at runtime: `CdsThemeManager.SetTheme(CdsThemeName.G100)` (or `FollowSystem()`).
- Re-theme a region: `<cds:CdsTheme Theme="G90">…</cds:CdsTheme>`. Zones nest and reset layering.
- Step a surface up a layer: `<cds:CdsLayer>…</cds:CdsLayer>`. Components always use the `-01` tokens; the nearest
  layer decides whether they resolve to `-02` or `-03`.
- Use tokens, never hex values: `BackgroundColor="{DynamicResource CdsLayer01}"`,
  `Padding="{x:Static cds:CdsSpacing.Spacing05}"`, `<cds:CdsText TypeStyle="BodyCompact01" />`.

## Differences from the web components

- **Tables**: a `CdsDataTable` is virtualized: it builds only the rows in view and re-binds them as the body scrolls,
  and sorts, searches and selects 10,000+ rows instantly. With `StickyHeader="True"` it owns its scrolling; inside a
  vertical `ScrollView` it follows the page's scrolling (`VirtualizeInScrollView`, on by default). Cell and
  expanded-row templates are re-used for other rows, so bind everything they show to the item (state a template keeps
  on its own is lost when its row scrolls away). Virtualized rows are one line high and rows out of view are not in the
  accessibility tree. A table outside any scroll view, with an `Auto` column, or with `VirtualizeInScrollView="False"`
  builds every row, so page large data sets with `CdsPagination`. Columns take fixed or star widths rather than sizing to their
  content; a table whose columns need more than the available width scrolls sideways, as Carbon's does.
- **Modals** cover the page they open on. The scrim cannot cover native Shell chrome (navigation bar, flyout); use
  `CdsUIShell` for a fully Carbon-styled frame.
- **Keyboard**: on Mac Catalyst, Tab focus follows the macOS *Keyboard navigation* setting.
- **Verification**: the gallery is run and screenshot-reviewed on Mac Catalyst in all four themes. iOS, Android and
  Windows are compiled (CI builds them) but have not yet been reviewed on a device.

## Repository

| Path | What |
|---|---|
| `src/Maui.Carbon` | the library (NuGet package) |
| `samples/Maui.Carbon.Gallery` | the gallery app: every component in every theme, plus Carbon's patterns |
| `samples/Maui.Carbon.Relay` | Relay, a mock project and issue tracker: a whole app on `CdsUIShell` with MVVM (CommunityToolkit.Mvvm), DI and an in-memory backend |
| `samples/Maui.Carbon.Relay.Core` | Relay's models, mock services and view models (plain `net11.0`, no MAUI) |
| `tests/Maui.Carbon.Relay.Tests` | unit tests for Relay's logic and view models |
| `tests/Maui.Carbon.Tests` | unit tests (tokens, theming, layers, component logic) |
| `tools/Maui.Carbon.Generator` | regenerates `src/Maui.Carbon/Generated` from `design/carbon` |
| `design/carbon` | the Carbon design-system snapshot this port is built from |
| `CONVENTIONS.md` | how components are built |

Build and test: `tools/verify.sh` (add `--samples` to build the gallery and Relay; `--fast` for the inner loop).
Run the gallery: `dotnet build samples/Maui.Carbon.Gallery -t:Run -f net11.0-maccatalyst -r maccatalyst-arm64`.
Run Relay: `dotnet build samples/Maui.Carbon.Relay -t:Run -f net11.0-maccatalyst -r maccatalyst-arm64` (sign in as
`maya.chen@relay.dev` with any password of 6+ characters). Relay writes a Serilog text log, `relay-<date>.log`, to
`Logs/<app id>` under its app data directory (on Mac Catalyst `~/Library/Logs/ai.waring.mauicarbon.relay`), or to
`RELAY_LOG_DIR` when set; check it for `[ERR]` and `[FTL]` lines after a test session.
Screenshot every gallery page: `tools/snapshots.sh out/shots white,g10,g90,g100 Release` (after a `-c Release` build);
every Relay screen: `APP=relay tools/snapshots.sh out/relay white,g100`.
Profile every gallery page: `tools/profile-gallery.sh` walks the sidebar under dotTrace's Timeline mode and writes
`out/profile/<time>/walk-report.md` (slowest pages, longest main-thread stalls, allocations) and `trace/gallery-walk.dtt`;
`--profiler none` for unprofiled timings, `--profiler memory` for dotMemory, `--help` for filters and passes.
Exercise Relay to catch framework faults (every screen, state, theme and window size; layout overflow, contrast, leaks,
UI hangs, crashes, every logged warning attributed to the step that caused it, and a "Carbon metrics" section naming the
components that draw, lay out, re-apply visuals or rebuild the most and what keeps drawing on a settled screen):
`tools/relay-exercise.sh out/exercise` after a `-c Release` build (`QUICK=1` for 2 themes x 2 sizes, ~8 minutes).

After updating `design/carbon/tokens.json` or the icons, regenerate with
`dotnet run --project tools/Maui.Carbon.Generator`.

## Agent skills

Two skills ship with the repository:

- [Maui.Carbon](skills/maui-carbon/SKILL.md) (`maui-carbon`) covers using the library: setup, the
  [component catalog](skills/maui-carbon/references/components.md) with source and gallery paths,
  bindings, themes, layers, tables, charts and overlays.
- [Maui.Carbon design](skills/maui-carbon-design/SKILL.md) (`maui-carbon-design`) covers designing with Carbon:
  a workflow for new screens, foundations (layers, color, type, spacing, grid, motion) in Maui.Carbon terms, choosing
  components by need, patterns, content, accessibility, data visualization and a review checklist.

To use them with Codex, copy `skills/maui-carbon` and `skills/maui-carbon-design` into `.agents/skills/` in your
consuming project (or your user-level skills directory). Copy the whole folders, including `references`. The skills are written for apps that reference the NuGet package:
they look members up in the package's XML documentation and read examples from this repository at the matching tag.
You can also read the skills directly here.
