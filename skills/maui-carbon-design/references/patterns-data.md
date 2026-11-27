# Data patterns

Carbon's patterns for finding, reading and acting on collections of data, with the Cds controls that build them. Start from [patterns](patterns.md) to pick one.

## Resource lists

Gallery: `PatternResourceListPage`. Relay: `IssuesView`, `ProjectsView`.

- A page header with a breadcrumb and a description, then a `CdsDataTable` with a title, toolbar search, and the primary "Create …" action at the end of the toolbar. Selecting rows shows the batch actions. Each row has a `CdsOverflowMenu` for its own actions. Large sets use `CdsPagination` or virtualized scrolling.
- The name column links to the resource's details page. Status columns use `CdsIconIndicator` or `CdsShapeIndicator` with a word (see [status indicators](patterns-feedback.md#status-indicators)). Dates, sizes and counts are right-aligned when they are compared down the column.
- The default sort is the most useful order (often the most recent), and the table shows it.
- No data yet: see [empty states](patterns-feedback.md#empty-states). The empty state offers the create action.
- Deleting rows follows the friction rules in [common actions](patterns.md#common-actions). Batch deletion of several items is at least moderate impact.

## Details pages

Relay: `IssueDetailView`.

- The page header holds the resource name, its status (indicator or tag) and its actions, with the primary last. Destructive actions go in the overflow menu or a separate danger section.
- Show facts in a `CdsStructuredList` or a two-column label/value grid with labels `Tone="Secondary"`. Identifiers are read-only, with a `CdsCopyButton`.
- Related collections go in `CdsTabs` sections or in tables below.
- The breadcrumb leads back to the list. Peer resources (previous, next) are local navigation on this page, not entries in the side nav.

## Search

Gallery: `PatternFilteringPage` and the Search component page.

Search finds content by words. It works at three scopes: global (the whole product, from the header), page (one page's content), or component (one table or list).

- **When not to use it:** a small set that fits in one view is easier to scan than to search.
- **Placement:** put search where people expect it for its scope: header search for global, above the content for page, and the table toolbar for a table (`CdsDataTable IsSearchable`). Align a default search with the text edges of the page. A fluid search (`CdsFluidSearch`) hangs to the edge like the other fluid components around it.
- **Sizes:** `Md` (40) by default, `Sm` (32) or `Xs` (24) in dense layouts, `Lg` (48) for prominent page-level search. Use one size across a layout. The width fits the context and has no fixed minimum or maximum.
- **Placeholder:** say what can be searched ("Search for networks or devices"), and keep it short.
- **Behavior:** `CdsSearch` shows a clear (x) button once there is text. Enter submits (`SearchCommand`) and Escape clears. The clear button is the next tab stop, and Space or Enter activates it. An expandable search (`IsExpandable`) is an icon until activated, then moves focus into the field.
- **Results:** announce the outcome ("25 results", "No results for 'staging-ap'") with `CdsAnnouncer`. Suggestions and type-ahead go in a `CdsComboBox`, not a custom popup. When nothing matches, show the no-results empty state with "Clear search", and keep the search field so people can change the query.
- **Accessibility:** the field is named "Search" (its label, even when hidden) and the clear button "Clear search input" (`CloseButtonLabel`).

## Filtering

Gallery: `PatternFilteringPage`.

Filtering narrows a set by its attributes, where search narrows it by text. Pick the selection method from the data:

| Method | Behaves like | Cds controls |
|---|---|---|
| Single selection: one value of one attribute | Radio buttons | `CdsDropdown` (inline or default), `CdsRadioButtonGroup` |
| Multiselect: several values of one attribute | Checkboxes | `CdsMultiSelect`, `CdsCheckboxGroup`, `CdsSelectableTag`s for a few quick filters |
| Several categories (size, color, price) | One group per category | Groups stacked in a left-hand panel, or in a row above the data. Never several categories inside one menu or dropdown. |

**Instant or batch**

- **Instant updates:** results change on every selection. Use this when people filter on one category or make one choice, and the data returns fast.
- **Batch updates:** people pick several filters and then press "Apply filters", and the results refresh once. Use this for selections across several categories, and whenever the data is slow to return. A `CdsPopover` or panel holds the choices with a "Reset" / "Apply" button set (see [disclosures](patterns-feedback.md#disclosures)).

**Starting state and reset**

- A category starts with all options cleared when people usually look for one value, or all selected when they usually exclude one or two. Categories can differ.
- When filters are hidden in a popover, panel or menu, the closed trigger shows that filters apply (at least the count, for example "Filters (3)") and lets people clear them without reopening it.
- Each category can be cleared in one action back to its default. With several categories, one "Clear filters" clears them all.
- Show the applied filters as `CdsDismissibleTag`s above the results, show and announce the result count (`CdsAnnouncer`), and when nothing matches show an empty state with "Clear filters".

## Overflow content

Gallery: `PatternOverflowContentPage`.

**Truncation** fits static text and links into a fixed space (`CdsText` with `MaxLines` and `LineBreakMode`). The ellipsis always stands for three or more characters, and the full text stays reachable through a `CdsTooltip`, a copy button or the details page.

- End truncation (`TailTruncation`) is the default, for names and descriptions: `12345…`.
- Front truncation (`HeadTruncation`) is for paths and strings that continue from earlier: `…56789`.
- Middle truncation is for IDs and values that differ at both ends: `1234…4321`.
- Truncate breadcrumbs, pagination, long URLs, descriptions and long item names.
- **Never truncate** page headers, titles, labels, button text, error or validation messages, or notifications. Let them wrap.
- An ellipsis on its own (collapsed breadcrumb items, `CdsBreadcrumb MaxVisibleItems`) opens a menu of the hidden items, not a tooltip.

**More content**

- Long text goes behind "Show more", with an optional "Show less", rather than in a fixed-height scroll area or under a fade.
- Long lists use "Load more" when loading is costly, or `CdsPagination`.
- Extra actions go in a `CdsOverflowMenu`, and extra tags in a "+n" `CdsOperationalTag`.

## Dashboards

Relay: `DashboardView`, `ReportsView`. See also [data visualization](data-visualization.md).

- Put the most important figure or chart at the top left with the most space. Key numbers come first as large figures in tiles, then trends, then detail.
- Lay tiles on a `CdsGrid` (`Condensed` mode for tiles that read as one unit) with equal heights per row. Use one unit system and one color per dataset across every chart.
- Each tile has a heading and, where useful, a link to the detail behind it.
- Load each tile on its own: skeletons and chart `IsLoading` first, then content as each source returns (see [loading](patterns-feedback.md#loading)). One failed source shows an error empty state in its own tile, not a page error. With several such states, use tertiary buttons and text-only states.
- Show changes with a differential indicator: an `ArrowUp`/`ArrowDown` or `CaretUp`/`CaretDown` icon and a signed number. Green (`CdsSupportSuccess`) for good and red (`CdsSupportError`) for bad are optional, but the sign or arrow is required.
