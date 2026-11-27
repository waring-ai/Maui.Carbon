# Feedback and overlay patterns

Carbon's patterns for dialogs, disclosures, notifications, status, loading, empty states, and disabled and read-only states, with the Cds controls that build them. Start from [patterns](patterns.md) to pick one.

## Dialogs

Gallery: `PatternConfirmationPage`.

A dialog asks for input or gives urgent information without taking the person off the page. Dialogs interrupt, so use them sparingly: people who see too many dismiss them without reading, including the one that matters.

**When to use one:** to focus attention on a short task, collect a few inputs, confirm a decision, or show information the current work depends on.

**When not to:** for content unrelated to the current workflow, for complex or large amounts of data, to rebuild a page inside a dialog, or when the person did not trigger it. A background or system event is a toast, never a dialog. A dialog may follow from a person's action indirectly, such as closing with unsaved changes.

**Modal or non-modal**

- **Modal** (`CdsModal`, `CdsComposedModal`, or `CdsDialog` with `IsModal="True"`) blocks the page until it is completed or dismissed. Use it for input that the workflow needs before it can continue, for urgent information about the current work, and for confirmations. It suits short, infrequent tasks. A task people repeat often belongs on the page itself.
- **Non-modal** (`CdsDialog` with `IsModal="False"`, the default) leaves the page usable. Use it for optional help or a tool people use alongside the content, such as find and replace or in-context help. Never use it when a response is required.
- Never open a modal from a modal. If a task needs its own confirmation, it does not belong in a modal. Never make a modal full screen: content that needs more than `Lg` belongs on a page. Never use a modal when the person must consult information behind it.

**Variants** (Carbon's modal variants, as `CdsModal` builds them)

| Variant | Use | Buttons | Dismissed by |
|---|---|---|---|
| Passive (`IsPassive`) | Information only | None | x, Escape, outside click |
| Transactional | A decision or short task | Secondary "Cancel", then the primary | Primary (completes), Cancel (undoes changes), x, Escape |
| Acknowledgment | The person must confirm they have seen it | One primary (`PrimaryButtonText` only) | Primary, x, Escape |
| Progress | A few steps before closing | Ghost "Cancel" on the left, "Back" secondary and "Next" primary on the right (`CdsComposedModal` with `CdsProgressIndicator`) | As transactional. On the last step, Next names the final action. |
| Danger (`IsDanger`) | Destructive confirmation | "Cancel", then a danger primary that names the action | As transactional. Focus starts on Cancel. |

Set `PreventCloseOnClickOutside="True"` on anything but passive modals, so an outside click does not lose the person's input.

**Buttons and content**

- The title says the task ("Delete bucket logs-eu?", "Invite members"). An optional `Label` above the title gives context.
- Buttons name the action with a verb ("Add", "Delete", "Save"), never "OK" or "Done", except the single button of an acknowledgment. There is one primary button, last on the right, and Cancel is the outermost left. Three buttons share the right side, and only the rightmost is primary. If all three are equally important, all three are secondary.
- Sizes: `Xs`/`Sm` for confirmations, `Md`/`Lg` for forms. When content is taller than the modal, only the body scrolls (`HasScrollingContent`), and the header and footer stay fixed.
- Good contents are form inputs, a `CdsContentSwitcher`, a `CdsStructuredList`, a few selectable tiles, or a dropdown. Avoid accordions and tabs, which hide content, and avoid links that leave the task. Avoid data tables. If a table is unavoidable, allow selection only, with no batch actions or editing.

**Behavior**

- When the dialog opens, focus moves to the first input (`InitialFocusElement` when it is not the first control). Never focus the primary button when there are inputs to fill. Focus stays in a modal until it closes, then returns to the control that opened it (`LauncherElement`). Tab moves through the body, then the primary, the secondary and the close button. Escape closes it.
- Validate before closing. Invalid fields keep the dialog open with inline errors (on blur, as in [forms](patterns-forms.md#forms)), and a server failure shows an error `CdsInlineNotification` in the body.
- While submitting, set `LoadingStatus` (which disables the primary and covers the body). Work that takes longer than a few seconds closes the dialog and reports progress on the page.

## Disclosures

A disclosure opens extra information or controls from a trigger the person activates, in a popover anchored to it. Unlike a tooltip, it can contain interactive controls.

- **Use it for** profile menus, table settings, sort and filter menus, combo buttons and menu buttons, and extra detail about part of the UI.
- **Do not use it for** critical information or required input (use a modal), anything that would open on its own, or content wider than six grid columns.
- **Rules:** only the person opens and closes it. Only one is open at a time. Never nest them (side-opening submenus in a `CdsMenu` are fine). Never hide critical information in one. Keep the content short: it is not a screen takeover.
- **Opening and closing:** a click or Enter/Space on the trigger opens it. A click on the trigger again, a click outside, Escape, a close x (top right, in empty space, never in line with other controls), or tabbing out closes it.
- **Spacing:** a heading sits directly above the body or is separated by a divider. Keep at least 16 between groups of controls, and 16 before a footer.

**Common disclosures in Maui.Carbon**

- **Profile menu:** a `CdsHeaderGlobalAction` (the `UserAvatar` icon, or a `CdsUserAvatar` with initials) opens a `CdsHeaderPanel`. It shows the name or handle, the active account, plan or region, a link to edit the profile, settings, and "Log out". Add only what helps. Relay's `AccountPanel` shows this.
- **Settings or filter menu:** a ghost `CdsIconButton` (`Settings` or `Filter`, with a `Label`) opens a `CdsPopover`. Focus moves to the first control inside. Use checkboxes, radios or dropdowns, then a "Reset" / "Apply" `CdsButtonSet` when changes cannot apply live. Apply closes the popover and updates the results.
- **Combo button** (`CdsComboButton`): the most-used action is the default button, and the other actions are in its menu, with dividers between groups. The labels are text only, with no icons on the default button or the menu items. Down arrow on the menu trigger opens the menu.
- **Toggletip** (`CdsToggletip`): extra explanation that may hold a link or button, opened on click.

## Notifications

Gallery: `PatternNotificationsPage`. Relay: `NotificationsPanel`.

Notifications must be relevant (tied to what the person is doing), timely (critical ones seen at once) and informative (what happened and what to do next). Send only what is necessary, and keep each one in the part of the UI it is about, or people learn to ignore them.

**Status** (`Kind`): Info for extra information, Success when a task finished as expected, Warning for something undesirable or unexpected, and Error for a failure that may block progress until it is fixed. Each kind has a fixed color and icon, which are never changed.

**Pick the type by how disruptive the message should be**

| Type | Use for | Lifetime | Cds |
|---|---|---|---|
| Inline | Feedback on a task in a specific area: form errors, an upload problem | Stays until resolved or dismissed | `CdsInlineNotification` |
| Toast | System events away from the person's focus: export ready, connection restored | Low-severity success may time out. Everything else stays until dismissed | `CdsToast.Show` / `CdsToastNotification` |
| Actionable | A message with one follow-up action | Stays until acted on or dismissed | `CdsActionableNotification` (inline: one ghost button; toast: one tertiary button) |
| Callout | Guidance people must read before they act | Always shown with the page, never dismissed | `CdsCallout` |
| Banner | A product-wide or account-wide message (maintenance, billing) | Stays until dismissed or resolved, possibly across sessions | Maui.Carbon has none: use a high-contrast `CdsInlineNotification` at the top of the content, directly under the header |
| Notification panel | A history of system notifications to review | Opened and closed by the person | `CdsNotificationsPanel`, opened from a `CdsHeaderGlobalAction` with a `CdsBadgeIndicator` (older package versions: a `CdsHeaderPanel`) |
| Modal | A critical message that must interrupt the task now | Stays until the person responds | `CdsModal` (`IsAlert`) |

**Rules by type**

- **Inline:** place it next to what it is about (at the top or bottom of a form), never over content. Keep it to two lines or fewer. Use low-contrast (`IsLowContrast`) in content and high-contrast for page-level or urgent messages, and pick one style per type across the product. Pair form-level notices with field errors.
- **Toast:** appears at the top right with a fixed width, and new toasts stack on top. Keep it to three lines (dropping the `Caption` timestamp leaves room for a third). Only low-severity success toasts whose information is available elsewhere time out (`Timeout` about 5000). Never time out errors, warnings, actionable toasts or anything critical: some people need longer to read.
- **Actionable:** one action, labelled in two words or fewer, that leads to a related flow without blocking the current one. It takes focus when it appears, which disrupts keyboard and screen-reader users, so use it only when that is worth it. Leave out the close button only when the person must read it or act on it.
- **Callout:** Info or Warning only, never Success or Error. Place it next to the action, field or data it affects. Use at most one per page, and only for information that must not be missed. Leave out the title when it repeats the body.
- **Banner:** one at a time. It scrolls with the page and is not sticky.
- **Panel:** list notifications newest first, optionally grouped by source or urgency. Clear the badge once the notifications are seen, never resend one the person has not interacted with, and let people choose which notifications they get.
- **Modal:** one at a time, only when the task must stop now, with clear steps to resolve it.
- Never stack several notifications about the same thing. Match disruptiveness to urgency.

**Wording:** the title states the outcome and the subtitle gives the reason and the next step, in plain words without jargon. Task notifications need little background. System notifications need enough context to make sense out of context. Always leave a next step when action is needed. See [content](content.md).

**Accessibility:** `Role` is `Status` for polite messages and `Alert` for errors that need attention now. Toasts and inline messages that appear later are announced by the controls. Announce other changes with `CdsAnnouncer`.

## Status indicators

Status indicators show severity through shape, color and symbol, so people can see at a glance what needs attention.

| Variant | Use | Cds |
|---|---|---|
| Icon indicator | Status that needs the most attention, where there is room: tables, notifications, task lists, dashboard tiles | `CdsIconIndicator` with `Kind` and `Label` |
| Shape indicator | Scanning many items in little space, or a product-defined set (priority, lifecycle) | `CdsShapeIndicator` with `Kind` and `Label` |
| Status icon | One glyph for an object's operational status (fatal, critical, major and minor warning, normal, in progress, running, pending, unknown) beside its label | `CdsStatusIcon` with `Kind` and `IconDescription` |
| Badge with a number | The count of new items matters | `CdsBadgeIndicator` (`Count`, `MaxCount`), or `CdsIconButton BadgeCount` |
| Badge without a number | New items exist, and the count does not matter | `CdsBadgeIndicator` as a dot on a toolbar icon button |
| Differential | Changes in many statistics (finance, metrics) | Compose an `ArrowUp`/`CaretUp` icon and a signed value (see [dashboards](patterns-data.md#dashboards)) |

- **Icon indicators** report system health: running, failed, finished. **Shape indicators** are a secondary, product-defined set that need not imply urgency.
- **Severity:** define high attention (act now: errors, failures, destructive results), medium (no immediate action: in progress, acknowledged) and low (something is ready, or changed since the last visit) for the product, and use them consistently.
- **Rolled-up status** takes the highest severity among its parts: green, yellow and red together show as red.
- **Restraint:** skip the indicator when nothing needs action, and use plain text instead. More than five or six kinds in one view overwhelms.
- **Never color alone:** use at least two of color, shape and symbol, and always a text label. A shape always has a label. When the label alone is unclear (a number, a code), add a column heading or extra text that explains it.
- **Layout:** the icon or shape comes before the label, and icons and labels are left-aligned in a column, so different label lengths never push the icons out of line. Do not use one shape in different colors to mean different things in the same experience.
- **Sizes:** an icon indicator is 16 with 14 text, or 20 with 16 text. A shape indicator is 16 with 14 text, or 12 text in tight or secondary use. The controls set these, so do not resize them.
- **Badges:** a numbered badge shows at most three characters, ending in "+" when capped ("99+"), and belongs on a 48 header action. Clear it when the person has seen the items.
- **Differentials:** the sign, arrow or caret is required and color is optional. Up is green and down is red, unless the data says otherwise (for example, a falling error rate is good).
- Status colors are the support tokens (`CdsSupportError`, `CdsSupportSuccess`, `CdsSupportWarning`, `CdsSupportCautionMajor`, `CdsSupportInfo`), never hex values and never tag colors.

## Loading

Gallery: `PatternLoadingPage`.

Loading feedback shows that the app is working, not frozen, and makes the wait feel shorter.

- **First load: skeletons** in the shape of the content (`CdsSkeletonText`, `CdsSkeletonPlaceholder`, `CdsDataTableSkeleton`, `CdsDropdownSkeleton` and the other control skeletons, chart `IsLoading`), for a few seconds at most, replaced as the content arrives. Use them only for containers and data (tiles, structured lists, tables, cards). Buttons, inputs, checkboxes and toggles usually need none. Never use skeletons for toasts, overflow menus, dropdown items, modals or loaders. Content inside a modal may use them, but the modal itself never does.
- **Progressive loading:** load in batches. Load the structure first (skeleton containers, static text, skeleton text for data), then data text, images, controls and content below the fold. Something without a skeleton keeps its final size as blank space, so nothing jumps. Use this for slow pages that pull from several sources (dashboards) and for table refreshes after a filter change on large sets.
- **An action started by a button:** `CdsInlineLoading` in place of the button row, going from Active to Finished or Error. In a modal, use `LoadingStatus`.
- **Measurable work:** `CdsProgressBar` with a label and the amount done. Use `CdsProgressIndicator` for steps.
- **Blocking work:** `CdsLoading` with an overlay, only when the whole app must wait (saving a submission, a few seconds of processing). For work that takes minutes, let people continue and notify them when it finishes.
- A loading indicator shows no progress, so anything longer than a moment or two needs a progress bar. Do not show a spinner for anything under about a second. Do not replace content already on screen with a skeleton on refresh.
- **Long lists:** "Load more" fetches the next batch.
- **Accessibility:** announce loading, completion, a stall or a failure (`CdsAnnouncer`, or the controls' own descriptions such as `LoadingDescription`).

## Empty states

Gallery: `PatternEmptyStatesPage`.

An empty state appears where data would be (a page, table, tile, dashboard or panel) when there is none. It says what would be here and what to do next, so the person is never at a dead end.

**Control:** `CdsEmptyState` (title, subtitle, action button and link, sm or lg illustration), or a preset that draws the matching illustration in light and dark: `CdsNoDataEmptyState`, `CdsNotFoundEmptyState` (no results), `CdsErrorEmptyState`, `CdsUnauthorizedEmptyState`, `CdsNoTagsEmptyState`, `CdsNotificationsEmptyState`. For a whole page the person cannot open, use `CdsFullPageError` (403, 404 or custom). In package versions without these controls, compose the anatomy below from `CdsText` and `CdsButton`.

**Anatomy:** an optional image, a short title (prefer the positive form: "Start by adding data assets", not "You don't have any data assets"), one or two sentences of body text, and then the action. The action can be a primary button, a link in the text, or a pointer to the control that does it. An optional secondary link, such as the documentation, goes below.

**Types**

- **No data (first use, or everything was deleted):** say what will appear and how to add it, and offer the create action. Choose the single most important next step and use the person's words, not internal terms.
- **After an action:** for no search or filter results, name what was searched and offer "Clear search" or "Clear filters". When something finished and nothing needs attention ("No alerts triggered"), a short title is enough.
- **Errors:** say what is wrong in plain words, with no codes, and give a way forward. For no permission, say who can grant access or how to request it. For a system problem, say what is blocking and where to look (an activity log). When configuration is required, give the first step. For something unsupported, say what is supported. Be direct and never joke, and keep any image sensitive to the seriousness.

**Placement and layout**

- The empty state replaces the element it stands for. For a table that has never had data, Carbon replaces the whole table, headers and footer included, so screen readers do not announce an empty table. Note that `CdsDataTable`'s `EmptyState`/`EmptyContent` keeps the title, toolbar and column headers. For no results, the empty state replaces the results while the search and filter controls stay usable.
- Left-align the block. Center it as a block in large spaces, with the image above it for wide images or beside it for tall ones. In a small tile, center the image above the left-aligned text so it does not read as skippable. Use text only where space is tight.
- When several empty states can show at once (dashboard tiles), use tertiary buttons so they do not compete as several primaries, and prefer text-only states, because repeated illustrations lose their effect.
- Keep the page frame and a table's toolbar in place so the page does not jump when data arrives. The copy is a heading (`Heading03` or `HeadingCompact02`), one sentence and one action.
- Illustrations are decorative and hidden from screen readers (`SemanticProperties.Description` unset, `AutomationProperties.IsInAccessibleTree="False"`), and they never hold information.
- For a product's main resource, a richer first-use state can add short in-line documentation, an optional guided tour, or starter content. Keep a basic empty state as the fallback.

## Disabled, read-only and hidden

Gallery: `PatternReadOnlyDisabledPage`.

| State | When | Behavior |
|---|---|---|
| Disabled (`IsEnabled="False"`) | Temporarily unavailable until the person does something or a dependency resolves (fill the form, pick an option) | Dimmed, skipped by Tab, not read by screen readers, no hover. It returns to enabled once the condition is met, and never disappears meanwhile. |
| Read-only (`IsReadOnly`) | The value matters but cannot be changed now: a process holds it, someone else has it locked, or the person may view but not edit | Full-contrast text (4.5:1), focusable and copyable, keyboard-navigable but not operable. Same layout as enabled, with a transparent field background, quieter borders and `CdsIconDisabled` chevrons. |
| Hidden | The person lacks permission to see or do it | Removed from the UI. It appears only once permission is granted ("Add member" shown only to owners). |

- Read-only is not a softer disabled, and disabled is not a way to show values. If people need to read it, it is read-only. If it is waiting on their action, it is disabled. Do not switch disabled controls to read-only because the view is read-only: some controls stay disabled there.
- Read-only applies only to controls that have an editable state. Static facts are plain text (`CdsText`, a `CdsStructuredList`). When the enabled content was an instruction ("Choose a region"), the read-only content states the value instead ("No region selected").
- Say why something is disabled, next to it, in helper text or a tooltip on the reason. When a disabled item blocks the main action or several items, add a warning `CdsInlineNotification` that says how to enable it.
- Never fake a disabled look with `Opacity` or custom colors: the controls draw Carbon's disabled tokens themselves.
- Hide actions a person can never take rather than leaving them disabled forever.
