# Supported components and companion types

This inventory is grounded in public concrete classes in the checked-in implementation. Companion items, headers, rows, options and services are listed alongside controls; not every listed type is a view. Abstract base classes, token/metrics helpers, internal renderers, models, converters and event-argument types are excluded. Read source for enums and the complete API.

Links below are relative to this file in the repository. When copying the skill elsewhere, use the same paths from a matching Maui.Carbon checkout.

## Foundations and supporting APIs

| Purpose | APIs | Source |
|---|---|---|
| Registration | `UseCarbon`, `CdsOptions` | [Hosting](../../../src/Maui.Carbon/Hosting/) |
| Theme resources and scopes | `CdsResources`, `CdsThemeManager`, `CdsThemeName`, `CdsTheme`, `CdsLayer` | [Theming](../../../src/Maui.Carbon/Theming/) |
| Text and typography | `CdsText`, `CdsTypography`, `CdsTypeSpec`, `CdsTypeStyle`, `CdsFonts` | [Typography](../../../src/Maui.Carbon/Typography/), [generated APIs](../../../src/Maui.Carbon/Generated/) |
| Icons | `CdsIcon`, `CdsIcons`, `CdsGlyph`, `CdsIconName` | [Icons](../../../src/Maui.Carbon/Icons/), [generated names](../../../src/Maui.Carbon/Generated/CdsIcons.g.cs) |
| Tokens | `CdsColorKeys`, `CdsSpacing`, `CdsSizing`, `CdsDuration`, `CdsEasing`, `CdsSize` | [Generated](../../../src/Maui.Carbon/Generated/), [Foundation](../../../src/Maui.Carbon/Foundation/) |
| Overlay infrastructure | `CdsOverlayHost`, `CdsOverlayService`, `CdsOverlayOptions`, `CdsOverlay`, `CdsPlacement` | [Overlays](../../../src/Maui.Carbon/Overlays/) |
| Motion | `CdsMotion`, `CdsCubicBezier` | [Motion](../../../src/Maui.Carbon/Motion/) |

## AI

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| AILabel | `CdsAILabel`, `CdsAILabelActions`, `CdsAILabelContent`, `CdsAiTreatment` | [source](../../../src/Maui.Carbon/Components/AI/AILabel/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/AI/) |
| AISkeleton | `CdsAISkeletonIcon`, `CdsAISkeletonPlaceholder`, `CdsAISkeletonText` | [source](../../../src/Maui.Carbon/Components/AI/AISkeleton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/AI/) |
| ChatButton | `CdsChatButton` | [source](../../../src/Maui.Carbon/Components/AI/ChatButton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/AI/) |

## Actions

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| Button | `CdsButton` | [source](../../../src/Maui.Carbon/Components/Actions/Button/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| ButtonSet | `CdsButtonSet` | [source](../../../src/Maui.Carbon/Components/Actions/ButtonSet/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| ComboButton | `CdsComboButton` | [source](../../../src/Maui.Carbon/Components/Actions/ComboButton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| CopyButton | `CdsCopyButton` | [source](../../../src/Maui.Carbon/Components/Actions/CopyButton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| IconButton | `CdsIconButton` | [source](../../../src/Maui.Carbon/Components/Actions/IconButton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| Menu | `CdsMenu`, `CdsMenuItem`, `CdsMenuItemDivider`, `CdsMenuItemGroup`, `CdsMenuItemRadioGroup`, `CdsMenuItemSelectable` | [source](../../../src/Maui.Carbon/Components/Actions/Menu/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| MenuButton | `CdsMenuButton` | [source](../../../src/Maui.Carbon/Components/Actions/MenuButton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |
| OverflowMenu | `CdsOverflowMenu` | [source](../../../src/Maui.Carbon/Components/Actions/OverflowMenu/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Actions/) |

## Charts

Every chart takes `Data` (mostly `CdsChartPoint`s: Group, Key, Date, X, Value, Low/High, Size; hierarchy, flow, network, bullet, map and parallel charts have their own item types), a `Title`, legend, toolbar and tooltip options, and `HeightRequest` for the whole chart (default 400). Axis charts take `LeftAxis`/`BottomAxis`/`RightAxis`/`TopAxis` (`CdsChartAxis`): the labels or time axis is the domain; put the labels axis on the left for horizontal charts. Choose a chart with `design/carbon/choosing-components.md` (Charts) and `design/carbon/data-visualization.md`.

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| Shared | `CdsChartPoint`, `CdsChartAxis`, `CdsChartThreshold`, `CdsChartExportingEventArgs`, `CdsChartSelectedGroupsChangedEventArgs` (bases: `CdsChart`, `CdsCartesianChart`) | [source](../../../src/Maui.Carbon/Components/Charts/Core/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| AlluvialChart | `CdsAlluvialChart`, `CdsAlluvialLink`, `CdsAlluvialNode` | [source](../../../src/Maui.Carbon/Components/Charts/AlluvialChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| AreaChart | `CdsAreaChart` | [source](../../../src/Maui.Carbon/Components/Charts/AreaChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| BoxplotChart | `CdsBoxplotChart` | [source](../../../src/Maui.Carbon/Components/Charts/BoxplotChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| BubbleChart | `CdsBubbleChart` | [source](../../../src/Maui.Carbon/Components/Charts/BubbleChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| BulletChart | `CdsBulletChart`, `CdsBulletChartItem` | [source](../../../src/Maui.Carbon/Components/Charts/BulletChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| ChoroplethChart | `CdsChoroplethChart`, `CdsGeoBounds` | [source](../../../src/Maui.Carbon/Components/Charts/ChoroplethChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| CirclePackChart | `CdsCirclePackChart` | [source](../../../src/Maui.Carbon/Components/Charts/CirclePackChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| ComboChart | `CdsComboChart`, `CdsComboChartType` | [source](../../../src/Maui.Carbon/Components/Charts/ComboChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| ConnectionMap | `CdsConnectionMap`, `CdsGeoConnection` | [source](../../../src/Maui.Carbon/Components/Charts/ConnectionMap/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| DonutChart | `CdsDonutChart` | [source](../../../src/Maui.Carbon/Components/Charts/DonutChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| GaugeChart | `CdsGaugeChart` | [source](../../../src/Maui.Carbon/Components/Charts/GaugeChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| GroupedBarChart | `CdsGroupedBarChart` | [source](../../../src/Maui.Carbon/Components/Charts/GroupedBarChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| HeatmapChart | `CdsHeatmapChart` | [source](../../../src/Maui.Carbon/Components/Charts/HeatmapChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| Hierarchy | `CdsChartNode` | [source](../../../src/Maui.Carbon/Components/Charts/Hierarchy/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| HistogramChart | `CdsHistogramChart` | [source](../../../src/Maui.Carbon/Components/Charts/HistogramChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| LineChart | `CdsLineChart` | [source](../../../src/Maui.Carbon/Components/Charts/LineChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| LollipopChart | `CdsLollipopChart` | [source](../../../src/Maui.Carbon/Components/Charts/LollipopChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| MeterChart | `CdsMeterChart`, `CdsMeterStatusRange` | [source](../../../src/Maui.Carbon/Components/Charts/MeterChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| NetworkDiagram | `CdsNetworkDiagram`, `CdsNetworkEdge`, `CdsNetworkNode` | [source](../../../src/Maui.Carbon/Components/Charts/NetworkDiagram/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| ParallelCoordinatesChart | `CdsParallelCoordinatesChart`, `CdsParallelDimension`, `CdsParallelFilter`, `CdsParallelRecord` | [source](../../../src/Maui.Carbon/Components/Charts/ParallelCoordinatesChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| PieChart | `CdsPieChart` | [source](../../../src/Maui.Carbon/Components/Charts/PieChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| ProportionalSymbolMap | `CdsGeoPoint`, `CdsProportionalSymbolMap` | [source](../../../src/Maui.Carbon/Components/Charts/ProportionalSymbolMap/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| RadarChart | `CdsRadarChart` | [source](../../../src/Maui.Carbon/Components/Charts/RadarChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| ScatterChart | `CdsScatterChart` | [source](../../../src/Maui.Carbon/Components/Charts/ScatterChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| SimpleBarChart | `CdsSimpleBarChart` | [source](../../../src/Maui.Carbon/Components/Charts/SimpleBarChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| SparklineChart | `CdsSparklineChart` | [source](../../../src/Maui.Carbon/Components/Charts/SparklineChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| StackedAreaChart | `CdsStackedAreaChart` | [source](../../../src/Maui.Carbon/Components/Charts/StackedAreaChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| StackedBarChart | `CdsStackedBarChart` | [source](../../../src/Maui.Carbon/Components/Charts/StackedBarChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| TreeChart | `CdsTreeChart` | [source](../../../src/Maui.Carbon/Components/Charts/TreeChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| TreemapChart | `CdsTreemapChart` | [source](../../../src/Maui.Carbon/Components/Charts/TreemapChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |
| WordCloudChart | `CdsWordCloudChart` | [source](../../../src/Maui.Carbon/Components/Charts/WordCloudChart/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Charts/) |

## Containers

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| ExpandableTile | `CdsExpandableTile` | [source](../../../src/Maui.Carbon/Components/Containers/ExpandableTile/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Containers/) |
| RadioTile | `CdsRadioTile`, `CdsTileGroup` | [source](../../../src/Maui.Carbon/Components/Containers/RadioTile/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Containers/) |
| SelectableTile | `CdsSelectableTile` | [source](../../../src/Maui.Carbon/Components/Containers/SelectableTile/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Containers/) |
| Tile | `CdsClickableTile`, `CdsTile` | [source](../../../src/Maui.Carbon/Components/Containers/Tile/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Containers/) |

## Data

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| Accordion | `CdsAccordion`, `CdsAccordionItem` | [source](../../../src/Maui.Carbon/Components/Data/Accordion/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| CodeSnippet | `CdsCodeSnippet` | [source](../../../src/Maui.Carbon/Components/Data/CodeSnippet/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| ContainedList | `CdsContainedList`, `CdsContainedListItem` | [source](../../../src/Maui.Carbon/Components/Data/ContainedList/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| DataTable | `CdsDataTable`, `CdsDataTableBatchAction`, `CdsDataTableEmptyState`, `CdsDataTableHeader`, `CdsDataTableRowAction`, `CdsTable`, `CdsTableCell`, `CdsTableHead`, `CdsTableHeader`, `CdsTableRow` | [source](../../../src/Maui.Carbon/Components/Data/DataTable/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| DataTableSkeleton | `CdsDataTableSkeleton` | [source](../../../src/Maui.Carbon/Components/Data/DataTableSkeleton/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| OrderedList | `CdsListItem`, `CdsOrderedList`, `CdsUnorderedList` | [source](../../../src/Maui.Carbon/Components/Data/OrderedList/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| Pagination | `CdsPagination` | [source](../../../src/Maui.Carbon/Components/Data/Pagination/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| PaginationNav | `CdsPaginationNav` | [source](../../../src/Maui.Carbon/Components/Data/PaginationNav/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| StructuredList | `CdsStructuredList`, `CdsStructuredListColumn`, `CdsStructuredListRow` | [source](../../../src/Maui.Carbon/Components/Data/StructuredList/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |
| TreeView | `CdsTreeNode`, `CdsTreeView` | [source](../../../src/Maui.Carbon/Components/Data/TreeView/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Data/) |

## Feedback

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| ActionableNotification | `CdsActionableNotification` | [source](../../../src/Maui.Carbon/Components/Feedback/ActionableNotification/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| BadgeIndicator | `CdsBadgeIndicator` | [source](../../../src/Maui.Carbon/Components/Feedback/BadgeIndicator/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| Callout | `CdsCallout` | [source](../../../src/Maui.Carbon/Components/Feedback/Callout/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| DismissibleTag | `CdsDismissibleTag` | [source](../../../src/Maui.Carbon/Components/Feedback/DismissibleTag/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| IconIndicator | `CdsIconIndicator` | [source](../../../src/Maui.Carbon/Components/Feedback/IconIndicator/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| InlineLoading | `CdsInlineLoading` | [source](../../../src/Maui.Carbon/Components/Feedback/InlineLoading/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| InlineNotification | `CdsInlineNotification` | [source](../../../src/Maui.Carbon/Components/Feedback/InlineNotification/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| Loading | `CdsLoading` | [source](../../../src/Maui.Carbon/Components/Feedback/Loading/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| OperationalTag | `CdsOperationalTag` | [source](../../../src/Maui.Carbon/Components/Feedback/OperationalTag/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| ProgressBar | `CdsProgressBar` | [source](../../../src/Maui.Carbon/Components/Feedback/ProgressBar/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| ProgressIndicator | `CdsProgressIndicator`, `CdsProgressStep` | [source](../../../src/Maui.Carbon/Components/Feedback/ProgressIndicator/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| SelectableTag | `CdsSelectableTag` | [source](../../../src/Maui.Carbon/Components/Feedback/SelectableTag/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| ShapeIndicator | `CdsShapeIndicator` | [source](../../../src/Maui.Carbon/Components/Feedback/ShapeIndicator/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| SkeletonIcon | `CdsSkeletonIcon` | [source](../../../src/Maui.Carbon/Components/Feedback/SkeletonIcon/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| SkeletonPlaceholder | `CdsSkeletonPlaceholder` | [source](../../../src/Maui.Carbon/Components/Feedback/SkeletonPlaceholder/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| SkeletonText | `CdsSkeletonText` | [source](../../../src/Maui.Carbon/Components/Feedback/SkeletonText/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| Tag | `CdsTag` | [source](../../../src/Maui.Carbon/Components/Feedback/Tag/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |
| ToastNotification | `CdsToast`, `CdsToastNotification`, `CdsToastOptions` | [source](../../../src/Maui.Carbon/Components/Feedback/ToastNotification/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Feedback/) |

## FluidForms

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| ComboBox | `CdsFluidComboBox` | [source](../../../src/Maui.Carbon/Components/FluidForms/ComboBox/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| DatePicker | `CdsFluidDatePicker` | [source](../../../src/Maui.Carbon/Components/FluidForms/DatePicker/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| Dropdown | `CdsFluidDropdown` | [source](../../../src/Maui.Carbon/Components/FluidForms/Dropdown/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| FluidForm | `CdsFluidForm`, `CdsFluidFormRow` | [source](../../../src/Maui.Carbon/Components/FluidForms/FluidForm/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| MultiSelect | `CdsFluidMultiSelect` | [source](../../../src/Maui.Carbon/Components/FluidForms/MultiSelect/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| NumberInput | `CdsFluidNumberInput` | [source](../../../src/Maui.Carbon/Components/FluidForms/NumberInput/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| Search | `CdsFluidSearch` | [source](../../../src/Maui.Carbon/Components/FluidForms/Search/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| Select | `CdsFluidSelect` | [source](../../../src/Maui.Carbon/Components/FluidForms/Select/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| TextArea | `CdsFluidTextArea` | [source](../../../src/Maui.Carbon/Components/FluidForms/TextArea/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| TextInput | `CdsFluidTextInput` | [source](../../../src/Maui.Carbon/Components/FluidForms/TextInput/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |
| TimePicker | `CdsFluidTimePicker` | [source](../../../src/Maui.Carbon/Components/FluidForms/TimePicker/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/FluidForms/) |

## Forms

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| Checkbox | `CdsCheckbox` | [source](../../../src/Maui.Carbon/Components/Forms/Checkbox/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| CheckboxGroup | `CdsCheckboxGroup` | [source](../../../src/Maui.Carbon/Components/Forms/CheckboxGroup/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| ComboBox | `CdsComboBox` | [source](../../../src/Maui.Carbon/Components/Forms/ComboBox/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| DatePicker | `CdsDatePicker`, `CdsDatePickerSkeleton` | [source](../../../src/Maui.Carbon/Components/Forms/DatePicker/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| Dropdown | `CdsDropdown`, `CdsDropdownSkeleton` | [source](../../../src/Maui.Carbon/Components/Forms/Dropdown/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| FileUploader | `CdsFileUploader`, `CdsFileUploaderItem`, `CdsUploadedFile`, `CdsFileUploaderSkeleton` (also the MVVM local-file picker: `FilePath`/`FilePaths`, `AccessMode="Path"`) | [source](../../../src/Maui.Carbon/Components/Forms/FileUploader/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| Form | `CdsForm`, `CdsFormGroup`, `CdsFormItem`, `CdsFormLabel` | [source](../../../src/Maui.Carbon/Components/Forms/Form/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| MultiSelect | `CdsMultiSelect` | [source](../../../src/Maui.Carbon/Components/Forms/MultiSelect/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| NumberInput | `CdsNumberInput` | [source](../../../src/Maui.Carbon/Components/Forms/NumberInput/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| PasswordInput | `CdsPasswordInput` | [source](../../../src/Maui.Carbon/Components/Forms/PasswordInput/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| RadioButtonGroup | `CdsRadioButton`, `CdsRadioButtonGroup` | [source](../../../src/Maui.Carbon/Components/Forms/RadioButtonGroup/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| Search | `CdsSearch` | [source](../../../src/Maui.Carbon/Components/Forms/Search/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| Select | `CdsSelect`, `CdsSelectItem`, `CdsSelectItemGroup` | [source](../../../src/Maui.Carbon/Components/Forms/Select/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| Slider | `CdsSlider` | [source](../../../src/Maui.Carbon/Components/Forms/Slider/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| TextArea | `CdsTextArea` | [source](../../../src/Maui.Carbon/Components/Forms/TextArea/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| TextInput | `CdsTextInput` | [source](../../../src/Maui.Carbon/Components/Forms/TextInput/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| TimePicker | `CdsTimePicker` | [source](../../../src/Maui.Carbon/Components/Forms/TimePicker/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |
| Toggle | `CdsToggle` | [source](../../../src/Maui.Carbon/Components/Forms/Toggle/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Forms/) |

## Layout

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| AspectRatio | `CdsAspectRatio` | [source](../../../src/Maui.Carbon/Components/Layout/AspectRatio/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Layout/) |
| Grid | `CdsColumn`, `CdsGrid` | [source](../../../src/Maui.Carbon/Components/Layout/Grid/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Layout/) |
| Heading | `CdsHeading`, `CdsSection` | [source](../../../src/Maui.Carbon/Components/Layout/Heading/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Layout/) |
| Stack | `CdsStack` | [source](../../../src/Maui.Carbon/Components/Layout/Stack/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Layout/) |

## Navigation

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| Breadcrumb | `CdsBreadcrumb`, `CdsBreadcrumbItem` | [source](../../../src/Maui.Carbon/Components/Navigation/Breadcrumb/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |
| ContentSwitcher | `CdsContentSwitcher`, `CdsSwitch` | [source](../../../src/Maui.Carbon/Components/Navigation/ContentSwitcher/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |
| HeaderPanel | `CdsHeaderPanel` | [source](../../../src/Maui.Carbon/Components/Navigation/HeaderPanel/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |
| Link | `CdsLink`, `CdsLinkSpan` | [source](../../../src/Maui.Carbon/Components/Navigation/Link/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |
| PageHeader | `CdsPageHeader`, `CdsPageHeaderActions`, `CdsPageHeaderBreadcrumbBar`, `CdsPageHeaderContent`, `CdsPageHeaderContentText`, `CdsPageHeaderHeroImage`, `CdsPageHeaderTabBar` | [source](../../../src/Maui.Carbon/Components/Navigation/PageHeader/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |
| Tabs | `CdsTab`, `CdsTabs` | [source](../../../src/Maui.Carbon/Components/Navigation/Tabs/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |
| UIShell | `CdsHeader`, `CdsHeaderGlobalAction`, `CdsHeaderGlobalBar`, `CdsHeaderMenu`, `CdsHeaderMenuButton`, `CdsHeaderMenuItem`, `CdsHeaderName`, `CdsHeaderNavigation`, `CdsHeaderSideNavItems`, `CdsSideNav`, `CdsSideNavDivider`, `CdsSideNavFooter`, `CdsSideNavHeader`, `CdsSideNavItems`, `CdsSideNavLink`, `CdsSideNavMenu`, `CdsSideNavMenuItem`, `CdsSkipToContent`, `CdsSwitcher`, `CdsSwitcherDivider`, `CdsSwitcherItem`, `CdsUIShell` | [source](../../../src/Maui.Carbon/Components/Navigation/UIShell/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Navigation/) |

## Overlays

| Family | Components and companion types | Source | Gallery |
|---|---|---|---|
| DefinitionTooltip | `CdsDefinitionTooltip` | [source](../../../src/Maui.Carbon/Components/Overlays/DefinitionTooltip/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Overlays/) |
| Dialog | `CdsDialog` | [source](../../../src/Maui.Carbon/Components/Overlays/Dialog/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Overlays/) |
| Modal | `CdsComposedModal`, `CdsModal`, `CdsModalBody`, `CdsModalFooter`, `CdsModalHeader` | [source](../../../src/Maui.Carbon/Components/Overlays/Modal/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Overlays/) |
| Popover | `CdsPopover` | [source](../../../src/Maui.Carbon/Components/Overlays/Popover/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Overlays/) |
| Toggletip | `CdsToggletip`, `CdsToggletipActions` | [source](../../../src/Maui.Carbon/Components/Overlays/Toggletip/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Overlays/) |
| Tooltip | `CdsTooltip` | [source](../../../src/Maui.Carbon/Components/Overlays/Tooltip/) | [examples](../../../samples/Maui.Carbon.Gallery/Pages/Overlays/) |

## Composed product patterns

[Gallery Patterns](../../../samples/Maui.Carbon.Gallery/Pages/Patterns/) demonstrates create forms, resource lists, settings, filtering, empty states, loading, notifications, confirmation, read-only/disabled states, overflow content and login. These are compositions rather than additional library controls.

For an application with bindings, commands and navigation, see [Relay](../../../samples/Maui.Carbon.Relay/).
