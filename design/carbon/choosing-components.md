# Choosing components

Several components here do similar jobs. Pick by the question in the left column; each component's README has the full rules.

## Actions

| Need | Use | Not |
| --- | --- | --- |
| The one main action on a view | `Button kind="primary"` (one per view) | A second primary; use `secondary` or `tertiary` |
| Go to another page or place | `Link` (inline in text, or standalone) | A button styled as a link |
| An action with no room for a label (toolbars, table rows) | `IconButton` with `label` (shows a tooltip) | An icon without a name; a danger action (danger buttons always have text) |
| Two or three related actions together | `ButtonSet` | More than three; put the rest in a `MenuButton` or `OverflowMenu` |
| One action with a menu of variants ("Create", "Create from template") | `ComboButton` | A `MenuButton` when there is a clear default action |
| A labelled button that opens a list of actions | `MenuButton` | A `Dropdown`, which selects a value |
| Secondary actions on a row or card | `OverflowMenu` | Hiding the primary action in it |
| Right-click actions on an area | `Menu` opened at the pointer | The only way to reach an action |
| Copy a value | `CopyButton`, or `CodeSnippet` for code | A plain button without feedback |

## Selecting values

| Need | Use |
| --- | --- |
| One of 2 to 5 options, all visible | `RadioButtonGroup` (or `RadioTile` in a `TileGroup` when options need a description) |
| One of many options | `Dropdown`; `Select` when a native control is preferred (mobile, very long lists); `ComboBox` when people type to find or add a value |
| Several of many options | `MultiSelect` (`filterable` for long lists); `CheckboxGroup` when there are few and all should be visible; `SelectableTile`s for rich choices |
| A setting that takes effect immediately | `Toggle` (never inside a form that has a Save button: use a `Checkbox` there) |
| Agree to or choose one independent option | `Checkbox` |
| A number | `NumberInput`; `Slider` (with its input) when the range matters more than precision |
| A date or range | `DatePicker` (`single`, `range`, or `simple` for typed dates); `TimePicker` for a time |
| Switch between views of the same content | `ContentSwitcher` for 2 to 5 equal views; `Tabs` for separate sections of a page |
| Filter by category | `SelectableTag`s for a few quick filters; `MultiSelect` or a filter panel for many (see Patterns) |

## Text entry

| Need | Use |
| --- | --- |
| A single line | `TextInput`; `PasswordInput` for secrets; `Search` for searching a set |
| Several lines | `TextArea` (with `enableCounter` when there is a limit) |
| Dense, edge-to-edge forms (side panels, data-heavy pages) | The `Fluid*` inputs inside `FluidForm`; never mix fluid and default inputs in one form |

## Messages and feedback

| Need | Use |
| --- | --- |
| Result of an action the person is looking at (form errors, page-level state) | `InlineNotification`, placed above the related content |
| Confirmation or system event that doesn't need a response | `ToastNotification` (top right; a timeout only for low-severity success messages) |
| A message with one follow-up action | `ActionableNotification` |
| Information to read before acting, that should stay | `Callout` |
| A decision that blocks progress, or a destructive confirmation | `Modal` (danger kind for destructive), or `Dialog` |
| Status of a resource in a table or list | `IconIndicator` or `ShapeIndicator` with a label; `BadgeIndicator` for counts on an icon button |
| Categorize or label items | `Tag` (read-only); `DismissibleTag` for applied filters; `OperationalTag` for a tag that opens more detail |

## Extra information

| Need | Use |
| --- | --- |
| Name an icon-only control on hover and focus | `Tooltip` (built into `IconButton`) |
| Define a term inline | `DefinitionTooltip` |
| Extra information that may contain a link or button | `Toggletip` (opens on click) |
| A small surface of controls anchored to a trigger (filters, settings) | `Popover` |
| Long secondary detail in the page | `Accordion`, `ExpandableTile`, or "Show more" (see Patterns: overflow content) |

## Loading

| Need | Use |
| --- | --- |
| First load of a page or region | `SkeletonText`, `SkeletonPlaceholder`, `DataTableSkeleton`, the `*Skeleton` variants of inputs (`AISkeleton` for AI content) |
| An action started by a button | `InlineLoading` in place of the button row |
| Work with a known amount (uploads, exports) | `ProgressBar`; `ProgressIndicator` for steps in a flow |
| A blocking action that must finish before anything else | `Loading` with an overlay, sparingly |

## Lists and data

| Need | Use |
| --- | --- |
| Records to scan, sort, select and act on | `DataTable` (with `Pagination`) |
| A few rows of read-only facts or a choice of plans | `StructuredList` |
| A simple list with actions in a panel or tile | `ContainedList` |
| Hierarchy (folders, resources) | `TreeView` |
| Ordered steps or plain bullets in content | `OrderedList` / `UnorderedList` |
| Page-level navigation between pages | `UIShell` side nav and header, `Breadcrumb`, `PaginationNav` for numbered pages of content |
