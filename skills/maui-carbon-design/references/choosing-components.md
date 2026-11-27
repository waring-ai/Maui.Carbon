# Choosing components

Pick by the need in the left column. "Not" lists the usual wrong choice. For each control's properties, see the maui-carbon skill's catalog.

## Actions

| Need | Use | Not |
|---|---|---|
| The one main action on a view | `CdsButton` (`Kind="Primary"`, the default), one per view | A second primary; use `Secondary`, `Tertiary` or `Ghost` |
| A destructive action | `CdsButton Kind="Danger"` (or `DangerTertiary`/`DangerGhost` when not the main action), always with text | An icon-only delete button |
| Go to another page or place | `CdsLink` (inline in text via `CdsParagraph`, or standalone) | A button styled as a link |
| An action with no room for a label (toolbars, table rows) | `CdsIconButton` with `Label` (it becomes the tooltip and the accessible name) | An unnamed icon |
| Two or three related actions together | `CdsButtonSet` | More than three; put the rest in a `CdsMenuButton` or `CdsOverflowMenu` |
| One action with variants ("Create", "Create from template") | `CdsComboButton` | A `CdsMenuButton` when there is a clear default |
| A labelled button that opens a list of actions | `CdsMenuButton` | A `CdsDropdown`, which selects a value |
| Secondary actions on a row or card | `CdsOverflowMenu` | Hiding the primary action in it |
| Context-menu actions on an area | `CdsMenu` at the pointer | The only way to reach an action |
| Copy a value | `CdsCopyButton`; `CdsCodeSnippet` for code and commands | A plain button with no feedback |

Button emphasis, from most to least: Primary, Secondary, Tertiary, Ghost. A view has one primary; a group pairs it with a secondary (or ghost for low-emphasis actions such as "Cancel" in a toolbar).

## Selecting values

| Need | Use |
|---|---|
| One of 2 to 5 options, all visible | `CdsRadioButtonGroup`; `CdsRadioTile`s in a `CdsTileGroup` when options need a description |
| One of many options | `CdsDropdown`; `CdsSelect` when a native picker is better (phones, very long lists); `CdsComboBox` when people type to find a value |
| Add one or many resources from a long or nested list (users to a group, a cluster to a project) | `CdsSingleAddSelect` / `CdsMultiAddSelect` (search, filters, a selection sidebar) |
| Several of many options | `CdsMultiSelect`; `CdsCheckboxGroup` (`CdsCheckbox`es) when there are few and all should show; `CdsSelectableTile`s for rich choices |
| A setting that applies immediately | `CdsToggle`. Never inside a form with a Save button: use a `CdsCheckbox` there |
| Agree to, or turn on, one independent option | `CdsCheckbox` |
| A number | `CdsNumberInput`; `CdsSlider` (with its input) when the range matters more than precision |
| A date, range or time | `CdsDatePicker` (single, range, or simple for typed dates); `CdsTimePicker` |
| A file to read or upload | `CdsFileUploader` |
| Switch between views of the same content | `CdsContentSwitcher` for 2 to 5 equal views |
| Move between separate sections of a page | `CdsTabs` |
| Quick filters by category | `CdsSelectableTag`s for a few; `CdsMultiSelect` or a filter panel for many |

## Text entry

| Need | Use |
|---|---|
| A single line | `CdsTextInput`; `CdsPasswordInput` for secrets; `CdsSearch` to search a set |
| Several lines | `CdsTextArea` (`EnableCounter` with `MaxCount` when there is a limit) |
| Dense, edge-to-edge forms (side panels, data-heavy pages) | Fluid inputs (`CdsFluidTextInput`…) inside `CdsFluidForm`. Never mix fluid and default inputs in one form; fluid inputs show no helper text |

## Messages and feedback

| Need | Use |
|---|---|
| Result of what the person is looking at (form errors, page-level state) | `CdsInlineNotification`, above the content it is about |
| A confirmation or system event that needs no response | `CdsToastNotification` / `CdsToast.Show` (top right; time out only low-severity success) |
| A message with one follow-up action | `CdsActionableNotification` |
| Information to read before acting, that stays | `CdsCallout` |
| A decision that blocks progress, or a destructive confirmation | `CdsModal` (`IsDanger` for destructive) or `CdsDialog IsModal="True"`; `CdsComposedModal` for custom structure |
| Delete or remove a resource, with the friction its impact needs | `CdsRemoveModal` (it can make the person type the resource name) |
| A common product task in a dialog | `CdsCreateModal` (create a simple resource), `CdsExportModal`, `CdsImportModal`, `CdsAPIKeyModal`, `CdsAboutModal` |
| An optional tool or help used beside the page (find and replace) | `CdsDialog` (non-modal, the default) |
| Create, edit or view details of something while the page stays in view | `CdsSidePanel`: slides over the page by default, `IncludeOverlay` to block it, `SlideIn` to push it aside |
| A substantial task that is too big for a modal (a long form, a multi-step flow, reviewing many records) | `CdsTearsheet` (wide, with optional tabs and an influencer column); `CdsTearsheetNarrow` for a focused task |
| Create something in steps | `CdsCreateTearsheet` over the page, `CdsCreateFullPage` for a large setup on its own page; one step: `CdsCreateTearsheetNarrow`, `CdsCreateSidePanel` or a modal |
| A product- or account-wide message (maintenance, billing) | A high-contrast `CdsInlineNotification` directly under the header (Carbon has no banner component) |
| A history of system notifications | `CdsNotificationsPanel` opened from a `CdsHeaderGlobalAction` with a `CdsBadgeIndicator` (in package versions without it, a `CdsHeaderPanel`) |
| Status of a resource in a table or list | `CdsIconIndicator` or `CdsShapeIndicator` with a label; `CdsBadgeIndicator` (`CdsIconButton BadgeCount`) for counts |
| Categorize or label items | `CdsTag` (read-only); `CdsDismissibleTag` for applied filters; `CdsOperationalTag` for a tag that opens detail |

## Extra information

| Need | Use |
|---|---|
| Name an icon-only control on hover and focus | `CdsTooltip` (built into `CdsIconButton`) |
| Define a term inline | `CdsDefinitionTooltip` |
| Extra information that may hold a link or button | `CdsToggletip` (opens on click) |
| A small surface of controls anchored to a trigger (filters, settings) | `CdsPopover` |
| Long secondary detail in the page | `CdsAccordion`, `CdsExpandableTile`, or "Show more" |

## Loading

| Need | Use |
|---|---|
| First load of a page or region | `CdsSkeletonText`, `CdsSkeletonPlaceholder`, `CdsSkeletonIcon`, `CdsDataTableSkeleton`, chart `IsLoading`; `CdsAISkeleton*` for AI content |
| An action started by a button | `CdsInlineLoading` in place of the button row (`Active`, `Finished`, `Error`); `CdsModal LoadingStatus` in a modal |
| Work of known size (uploads, exports) | `CdsProgressBar`; `CdsProgressIndicator` for steps in a flow |
| A blocking action that must finish first | `CdsLoading` with an overlay, sparingly |

## Lists, data and navigation

| Need | Use |
|---|---|
| Records to scan, sort, select and act on | `CdsDataTable` (with `CdsPagination` for large sets) |
| A few rows of read-only facts, or a choice of plans | `CdsStructuredList` |
| A simple list with actions in a panel or tile | `CdsContainedList` |
| Hierarchy (folders, resources) | `CdsTreeView` |
| Ordered steps or plain bullets in content | `CdsOrderedList` / `CdsUnorderedList` |
| A group of related information on a surface | `CdsTile`; `CdsClickableTile` when the whole tile navigates |
| App frame and global navigation | `CdsUIShell` (header, side nav, header panels) |
| Where am I in a hierarchy | `CdsBreadcrumb` (in `CdsPageHeader`) |
| Numbered pages of content | `CdsPaginationNav` |

## Charts

See [data visualization](data-visualization.md). A single headline number is not a chart: show it as a large `CdsText` figure in a `CdsTile`, with a `CdsSparklineChart` if the trend matters.
