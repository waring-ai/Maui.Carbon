# Patterns

Carbon's answers to recurring product problems, composed from Cds controls. Each section names a page that implements it in the library's public repository; read it at the tag of the installed package version and copy its structure:

- Gallery: `https://raw.githubusercontent.com/waring-ai/Maui.Carbon/v<version>/samples/Maui.Carbon.Gallery/Pages/Patterns/<Name>.xaml`
- Relay (the same patterns inside a full MVVM app): `…/samples/Maui.Carbon.Relay/Views/<Name>.xaml`

## Page anatomy

- `CdsUIShell` header and side nav for global navigation; the side nav marks the current page.
- A page header (`CdsPageHeader`): breadcrumb when the page is below the top level, the title (the page's only level-1 heading), an optional one-line description, page actions at the end (the primary one last in that row), tabs when the page has sections.
- Content on the grid, sections separated by 48. Page-level messages (`CdsInlineNotification`) go at the top of the content, under the header.
- Keep the layout stable across states: skeletons, empty states and errors take the place of the content, not a different page.

## Create and edit forms

Gallery: `PatternCreateFormPage`. Relay: `CreateIssueView`.

- One column by default, about 576 to 672 wide (or 8 of 16 grid columns). Put two fields side by side only when they belong together (city and postal code, start and end date).
- A form heading, then `CdsFormGroup`s with a legend or section heading for related fields. Order fields as people think about the task, the required and important ones first.
- Inputs are `Md` (40). Space fields 32 apart on a page (`CdsStack Gap="7"`), 24 or 16 in modals and side panels; leave 48 before the buttons.
- Buttons at the bottom. On a page the primary is first, on the left, followed by the secondary ("Cancel"). In modals, side panels, wizards and progressive forms the primary is last, on the right, and the buttons fill the footer width.
- Mark the minority: if most fields are required, label the rest "(optional)"; if most are optional, label the required ones "(required)".
- Validate a field when it loses focus. On a failed submit, show a summary `CdsInlineNotification` (Kind Error) at the top naming how many fields need attention, keep every field error, and move focus to the summary or the first invalid field. Clear each error as soon as it is fixed.
- Disable the primary button only on very short forms until they are valid, and while submitting (with `CdsInlineLoading` or the modal's `LoadingStatus`).
- Long forms: split into steps with `CdsProgressIndicator`, or into sections with a side nav. Confirm discarding unsaved changes.

## Resource lists

Gallery: `PatternResourceListPage`. Relay: `IssuesView`, `ProjectsView`.

- Page header with breadcrumb and description, then a `CdsDataTable`: title, toolbar search, the primary "Create …" action at the toolbar end, batch actions on selection, a row `CdsOverflowMenu` for per-row actions, `CdsPagination` (or virtualized scrolling) for large sets.
- The name column links to the resource's details page. Status columns use `CdsIconIndicator` with a word. Dates, sizes and counts are right-aligned when they are compared down the column.
- Default sort is the most useful order (often most recent); show the sort.
- No data yet: the empty state replaces the rows (see empty states) and offers the create action.

## Details pages

- Page header with the resource name, its status (indicator or tag), and its actions (primary last; destructive ones in the overflow menu or a separate danger section).
- Facts in a `CdsStructuredList` or a two-column label/value grid (labels `Tone="Secondary"`); identifiers read-only with a `CdsCopyButton`.
- Related collections in `CdsTabs` sections or tables below.

## Settings

Gallery: `PatternSettingsPage`. Relay: `SettingsView`.

- Group settings with `CdsTabs` or side-nav sections.
- Either a form with Save for related fields that apply together, or `CdsToggle`s that apply immediately (with saving feedback) for independent switches. Never mix the two in one group.
- Identifiers are read-only fields with `CdsCopyButton`. Destructive actions go last, in their own section, behind a confirmation.

## Filtering and search

Gallery: `PatternFilteringPage`.

- Search narrows by text; filters narrow by attributes. Instant filters (checkboxes in a side panel) update results at once; batch filters (a popover or panel with Apply and Reset) apply together.
- Show the result count (announce it with `CdsAnnouncer`), applied filters as `CdsDismissibleTag`s with "Clear filters", and an empty state with a way back when nothing matches.

## Empty states

Gallery: `PatternEmptyStatesPage`.

Say why the area is empty and what to do next:

- **First use:** what this area is for, and the primary create action.
- **No results:** what was searched or filtered, and "Clear search" / "Clear filters".
- **Error:** what failed, in plain words, and "Try again".
- **No permission:** who can grant access.

Keep the surrounding layout (and a table's headers once it has had data) so the page does not jump when data arrives. Keep empty-state copy short: a heading (`Heading03` or `HeadingCompact02`), one sentence, one action.

## Loading

Gallery: `PatternLoadingPage`.

- First load: skeletons in the shape of the content (`CdsSkeletonText`, `CdsSkeletonPlaceholder`, `CdsDataTableSkeleton`, chart `IsLoading`).
- An action from a button: `CdsInlineLoading` in the button's place, going Active, then Finished or Error.
- Measurable work: `CdsProgressBar` with a label and the amount done.
- Blocking: `CdsLoading` with an overlay, only when nothing else may happen meanwhile.
- Do not show a spinner for anything under about a second; do not replace content that is already on screen with a skeleton on refresh.

## Notifications

Gallery: `PatternNotificationsPage`.

- Inline next to what it is about; toast for events away from the person's focus; actionable when there is one follow-up; callout for persistent information in content; modal only when the person must decide before continuing.
- Title states the outcome, subtitle gives the reason and next step. High-severity messages stay until dismissed; only low-severity success toasts time out.
- Use the low-contrast inline variant (`IsLowContrast`) inside content, the high-contrast one for page-level or global messages. Do not stack several notifications about the same thing.

## Dialogs and confirmation

Gallery: `PatternConfirmationPage`.

- Match the friction to the impact. Low (reversible): act at once and offer Undo in a toast. Moderate: a danger `CdsModal` (`IsDanger`) whose title and button name the object and the action ("Delete bucket logs-eu", button "Delete bucket"). High (irreversible, wide impact): ask the person to type the resource name to enable the button.
- Discarding unsaved work uses a transactional modal ("Discard changes?", "Discard" / "Keep editing").
- The dismissive action ("Cancel") never does anything destructive; in danger modals focus starts on it.
- Modals are for short, focused tasks. Use `CdsModal Size` `Xs`/`Sm` for confirmations, `Md`/`Lg` for forms; a long or multi-step task belongs on a page.
- Passive modals (information only, `IsPassive`) have no buttons, only close.

## Read-only and disabled

Gallery: `PatternReadOnlyDisabledPage`.

- **Read-only:** the value matters but this person cannot change it. It stays focusable, full contrast and copyable (`IsReadOnly`).
- **Disabled:** the control does not apply right now. It is skipped by Tab and dimmed, so say why nearby (helper text, a notification, a tooltip on the reason).
- Hide actions a person can never take rather than disabling them forever.

## Overflow content

Gallery: `PatternOverflowContentPage`.

- Truncate at the end by default (the front for paths, the middle for IDs), always with the full text reachable through a `CdsTooltip` or the details page. Never truncate button labels.
- Extra actions go into a `CdsOverflowMenu`, extra tags into a "+n" `CdsOperationalTag`, long text behind "Show more", long lists behind "Load more" or `CdsPagination`.

## Login

Gallery: `PatternLoginPage`.

- Two steps: user ID (with "Remember ID" and alternative login), then password.
- Validate inline on blur; after a failed attempt show one generic error that does not reveal which part was wrong. Put recovery and create-account links next to the step they belong to.

## Dashboards

Relay: `DashboardView`, `ReportsView`. See also [data visualization](data-visualization.md).

- The most important figure or chart top left with the most space; key numbers first as large figures in tiles, then trends, then detail.
- Tiles on a `CdsGrid` (`Condensed` mode for tiles that read as one unit), equal heights per row, one unit system and one color per dataset across every chart.
- Each tile has a heading and, where useful, a link to the detail behind it.
