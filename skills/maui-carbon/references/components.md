# Supported components and companion types

The package's public controls and their companion types (items, headers, rows, options, services), so not every type is a view. Abstract bases, token/metrics helpers, models, converters and event-argument types are left out. For members, search the package's XML documentation as SKILL.md describes.

Examples and source are in the public repository; read them at the tag of the installed version (`https://github.com/waring-ai/Maui.Carbon/tree/v<version>/`, or `raw.githubusercontent.com/waring-ai/Maui.Carbon/v<version>/` for file contents). For every family below:

- Gallery examples: `samples/Maui.Carbon.Gallery/Pages/<Category>/<Family>Page.xaml` (a few families share a page; list the folder if the name does not match)
- Source: `src/Maui.Carbon/Components/<Category>/<Family>/` (properties in `*.Properties.cs`)

where `<Category>` is the section heading and `<Family>` the first column.

## Foundations and supporting APIs

Folders are under `src/Maui.Carbon/` in the repository:

| Purpose | APIs | Folder |
|---|---|---|
| Registration | `UseCarbon`, `CdsOptions` | `Hosting` |
| Theme resources and scopes | `CdsResources`, `CdsThemeManager`, `CdsThemeName`, `CdsTheme`, `CdsLayer` | `Theming` |
| Text and typography | `CdsText`, `CdsTypography`, `CdsTypeSpec`, `CdsTypeStyle`, `CdsFonts` | `Typography`, `Generated` |
| Icons | `CdsIcon`, `CdsIcons`, `CdsGlyph`, `CdsIconName` | `Icons`, `Generated/CdsIcons.g.cs` |
| Tokens | `CdsColorKeys`, `CdsSpacing`, `CdsSizing`, `CdsDuration`, `CdsEasing`, `CdsSize` | `Generated`, `Foundation` |
| Overlay infrastructure | `CdsOverlayHost`, `CdsOverlayService`, `CdsOverlayOptions`, `CdsOverlay`, `CdsPlacement` | `Overlays` |
| Motion | `CdsMotion`, `CdsCubicBezier` | `Motion` |
| Layout helpers | `CdsDeck` (one active child of many kept attached: app screens, large panes; cheaper to switch than `IsVisible`), `CdsMaxWidthBox`, `CdsEqualWidthRow` | `Foundation` |

## AI

| Family | Types |
|---|---|
| AILabel | `CdsAILabel`, `CdsAILabelActions`, `CdsAILabelContent`, `CdsAiTreatment` |
| AISkeleton | `CdsAISkeletonIcon`, `CdsAISkeletonPlaceholder`, `CdsAISkeletonText` |
| ChatButton | `CdsChatButton` |

## Actions

| Family | Types |
|---|---|
| Button | `CdsButton` |
| ButtonSet | `CdsButtonSet` |
| ComboButton | `CdsComboButton` |
| CopyButton | `CdsCopyButton` |
| IconButton | `CdsIconButton` |
| Menu | `CdsMenu`, `CdsMenuItem`, `CdsMenuItemDivider`, `CdsMenuItemGroup`, `CdsMenuItemRadioGroup`, `CdsMenuItemSelectable` |
| MenuButton | `CdsMenuButton` |
| OverflowMenu | `CdsOverflowMenu` |

## Charts

Data shapes, axes and options are in [charts](charts.md). The shared types and the bases (`CdsChart`, `CdsCartesianChart`, `CdsThematicChart` for maps) are in `Charts/Core`.

| Family | Types |
|---|---|
| Core | `CdsChartPoint`, `CdsChartAxis`, `CdsChartThreshold`, `CdsChartExportingEventArgs`, `CdsChartSelectedGroupsChangedEventArgs` |
| AlluvialChart | `CdsAlluvialChart`, `CdsAlluvialLink`, `CdsAlluvialNode` |
| AreaChart | `CdsAreaChart` |
| BoxplotChart | `CdsBoxplotChart` |
| BubbleChart | `CdsBubbleChart` |
| BulletChart | `CdsBulletChart`, `CdsBulletChartItem` |
| ChoroplethChart | `CdsChoroplethChart`, `CdsGeoBounds` |
| CirclePackChart | `CdsCirclePackChart` |
| ComboChart | `CdsComboChart`, `CdsComboChartType` |
| ConnectionMap | `CdsConnectionMap`, `CdsGeoConnection` |
| DonutChart | `CdsDonutChart` |
| GaugeChart | `CdsGaugeChart` |
| GroupedBarChart | `CdsGroupedBarChart` |
| HeatmapChart | `CdsHeatmapChart` |
| Hierarchy | `CdsChartNode` |
| HistogramChart | `CdsHistogramChart` |
| LineChart | `CdsLineChart` |
| LollipopChart | `CdsLollipopChart` |
| MeterChart | `CdsMeterChart`, `CdsMeterStatusRange` |
| NetworkDiagram | `CdsNetworkDiagram`, `CdsNetworkEdge`, `CdsNetworkNode` |
| ParallelCoordinatesChart | `CdsParallelCoordinatesChart`, `CdsParallelDimension`, `CdsParallelFilter`, `CdsParallelRecord` |
| PieChart | `CdsPieChart` |
| ProportionalSymbolMap | `CdsGeoPoint`, `CdsProportionalSymbolMap` |
| RadarChart | `CdsRadarChart` |
| ScatterChart | `CdsScatterChart` |
| SimpleBarChart | `CdsSimpleBarChart` |
| SparklineChart | `CdsSparklineChart` |
| StackedAreaChart | `CdsStackedAreaChart` |
| StackedBarChart | `CdsStackedBarChart` |
| TreeChart | `CdsTreeChart` |
| TreemapChart | `CdsTreemapChart` |
| WordCloudChart | `CdsWordCloudChart` |

## Containers

| Family | Types |
|---|---|
| ExpandableTile | `CdsExpandableTile` |
| RadioTile | `CdsRadioTile`, `CdsTileGroup` |
| SelectableTile | `CdsSelectableTile` |
| Tile | `CdsClickableTile`, `CdsTile` |

## Data

| Family | Types |
|---|---|
| Accordion | `CdsAccordion`, `CdsAccordionItem` |
| CodeSnippet | `CdsCodeSnippet` |
| ContainedList | `CdsContainedList`, `CdsContainedListItem` |
| DataTable | `CdsDataTable`, `CdsDataTableBatchAction`, `CdsDataTableEmptyState`, `CdsDataTableHeader`, `CdsDataTableRowAction`, `CdsTable`, `CdsTableCell`, `CdsTableHead`, `CdsTableHeader`, `CdsTableRow` |
| DataTableSkeleton | `CdsDataTableSkeleton` |
| OrderedList | `CdsListItem`, `CdsOrderedList`, `CdsUnorderedList` |
| Pagination | `CdsPagination` |
| PaginationNav | `CdsPaginationNav` |
| StructuredList | `CdsStructuredList`, `CdsStructuredListColumn`, `CdsStructuredListRow` |
| TreeView | `CdsTreeNode`, `CdsTreeView` |

## Feedback

| Family | Types |
|---|---|
| ActionableNotification | `CdsActionableNotification` |
| BadgeIndicator | `CdsBadgeIndicator` |
| Callout | `CdsCallout` |
| DismissibleTag | `CdsDismissibleTag` |
| IconIndicator | `CdsIconIndicator` |
| InlineLoading | `CdsInlineLoading` |
| InlineNotification | `CdsInlineNotification` |
| Loading | `CdsLoading` |
| OperationalTag | `CdsOperationalTag` |
| ProgressBar | `CdsProgressBar` |
| ProgressIndicator | `CdsProgressIndicator`, `CdsProgressStep` |
| SelectableTag | `CdsSelectableTag` |
| ShapeIndicator | `CdsShapeIndicator` |
| SkeletonIcon | `CdsSkeletonIcon` |
| SkeletonPlaceholder | `CdsSkeletonPlaceholder` |
| SkeletonText | `CdsSkeletonText` |
| Tag | `CdsTag` |
| ToastNotification | `CdsToast`, `CdsToastNotification`, `CdsToastOptions` |

## FluidForms

| Family | Types |
|---|---|
| ComboBox | `CdsFluidComboBox` |
| DatePicker | `CdsFluidDatePicker` |
| Dropdown | `CdsFluidDropdown` |
| FluidForm | `CdsFluidForm`, `CdsFluidFormRow` |
| MultiSelect | `CdsFluidMultiSelect` |
| NumberInput | `CdsFluidNumberInput` |
| Search | `CdsFluidSearch` |
| Select | `CdsFluidSelect` |
| TextArea | `CdsFluidTextArea` |
| TextInput | `CdsFluidTextInput` |
| TimePicker | `CdsFluidTimePicker` |

## Forms

| Family | Types |
|---|---|
| Checkbox | `CdsCheckbox` |
| CheckboxGroup | `CdsCheckboxGroup` |
| ComboBox | `CdsComboBox` |
| DatePicker | `CdsDatePicker`, `CdsDatePickerSkeleton` |
| Dropdown | `CdsDropdown`, `CdsDropdownSkeleton` |
| FileUploader | `CdsFileUploader`, `CdsFileUploaderItem`, `CdsUploadedFile`, `CdsFileUploaderSkeleton` (also the MVVM local-file picker; see [file-uploader](file-uploader.md)) |
| Form | `CdsForm`, `CdsFormGroup`, `CdsFormItem`, `CdsFormLabel` |
| MultiSelect | `CdsMultiSelect` |
| NumberInput | `CdsNumberInput` |
| PasswordInput | `CdsPasswordInput` |
| RadioButtonGroup | `CdsRadioButton`, `CdsRadioButtonGroup` |
| Search | `CdsSearch` |
| Select | `CdsSelect`, `CdsSelectItem`, `CdsSelectItemGroup` |
| Slider | `CdsSlider` |
| TextArea | `CdsTextArea` |
| TextInput | `CdsTextInput` |
| TimePicker | `CdsTimePicker` |
| Toggle | `CdsToggle` |

## Layout

| Family | Types |
|---|---|
| AspectRatio | `CdsAspectRatio` |
| Grid | `CdsColumn`, `CdsGrid` |
| Heading | `CdsHeading`, `CdsSection` |
| Stack | `CdsStack` |

## Navigation

| Family | Types |
|---|---|
| Breadcrumb | `CdsBreadcrumb`, `CdsBreadcrumbItem` |
| ContentSwitcher | `CdsContentSwitcher`, `CdsSwitch` |
| HeaderPanel | `CdsHeaderPanel` |
| Link | `CdsLink`, `CdsLinkSpan` |
| PageHeader | `CdsPageHeader`, `CdsPageHeaderActions`, `CdsPageHeaderBreadcrumbBar`, `CdsPageHeaderContent`, `CdsPageHeaderContentText`, `CdsPageHeaderHeroImage`, `CdsPageHeaderTabBar` |
| Tabs | `CdsTab`, `CdsTabs` |
| UIShell | `CdsHeader`, `CdsHeaderGlobalAction`, `CdsHeaderGlobalBar`, `CdsHeaderMenu`, `CdsHeaderMenuButton`, `CdsHeaderMenuItem`, `CdsHeaderName`, `CdsHeaderNavigation`, `CdsHeaderSideNavItems`, `CdsSideNav`, `CdsSideNavDivider`, `CdsSideNavFooter`, `CdsSideNavHeader`, `CdsSideNavItems`, `CdsSideNavLink`, `CdsSideNavMenu`, `CdsSideNavMenuItem`, `CdsSkipToContent`, `CdsSwitcher`, `CdsSwitcherDivider`, `CdsSwitcherItem`, `CdsUIShell` |

## Overlays

| Family | Types |
|---|---|
| DefinitionTooltip | `CdsDefinitionTooltip` |
| Dialog | `CdsDialog` |
| Modal | `CdsComposedModal`, `CdsModal`, `CdsModalBody`, `CdsModalFooter`, `CdsModalHeader` |
| Popover | `CdsPopover` |
| Toggletip | `CdsToggletip`, `CdsToggletipActions` |
| Tooltip | `CdsTooltip` |

## Composed product patterns

`samples/Maui.Carbon.Gallery/Pages/Patterns/` demonstrates create forms, resource lists, settings, filtering, empty states, loading, notifications, confirmation, read-only/disabled states, overflow content and login. These are compositions to copy from the repository, not additional package controls. For an application with bindings, commands and navigation, see `samples/Maui.Carbon.Relay/`.
