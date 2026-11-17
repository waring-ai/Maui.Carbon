# Patterns

Patterns are Carbon's answers to recurring product problems. Each one below has a pattern card (group **Patterns**) that composes the real components into a full surface; copy its structure, then read the card's README for the composition rules.

## Forms (`PatternCreateForm`)

- One column by default; put fields side by side only when they belong together (city and postal code). Give the form a heading and each `FormGroup` a legend.
- Inputs are 40px (`md`). Space fields 32px apart on a page, 24px or 16px in side panels and modals; leave 48px before the buttons.
- Buttons sit at the bottom. On an in-page form the primary is first, on the left; in dialogs, side panels, wizards and progressive forms the primary is last, on the right, and buttons bleed full width in dialogs and side panels.
- Validate a field when it loses focus; after a failed submit, summarize in an `InlineNotification` and keep the field errors. Disable the primary button only on short forms until they are valid, and always while submitting.

## Resource lists (`PatternResourceList`)

- UI shell header and side nav, `PageHeader` with breadcrumb and description, then a `DataTable` with toolbar search, the primary "Create" action at the toolbar end, batch actions on selection, a row `OverflowMenu`, and `Pagination`.
- Status columns use `IconIndicator` with a word; names link to the resource's details page.

## Settings (`PatternSettings`)

- Group settings with `Tabs` or side nav sections. Use a form with Save for related fields that apply together, and `Toggle`s that apply immediately (with saving feedback) for independent switches; never mix the two in one group.
- Identifiers are read-only fields with a `CopyButton`; destructive actions go last, in their own section, behind a confirmation.

## Filtering and search (`PatternFiltering`)

- Search narrows by text; filters narrow by attributes. Instant filters (checkboxes in a side panel) update results at once; batch filters (a flyout with Apply and Reset) apply together.
- Show the result count in a live region, applied filters as `DismissibleTag`s with a "Clear filters" action, and an empty state with a way back when nothing matches.

## Empty states (`PatternEmptyStates`)

- Say why the area is empty and what to do next: first use (explain the value and offer the primary create action), no results (offer "Clear search" or "Clear filters"), error (say what failed and offer "Try again"), no permission (say who can grant access).
- Keep table headers hidden when a table has never had data; keep the surrounding layout so the page doesn't jump when data arrives.

## Loading (`PatternLoading`)

- First load: skeletons in the shape of the content. An action from a button: `InlineLoading` in the button's place with active, finished and error states. Measurable work: `ProgressBar`. A blocking action: `Loading` with an overlay, only when nothing else may happen meanwhile.

## Notifications (`PatternNotifications`)

- Inline next to what it is about; toast for events away from the person's focus; actionable when there is one follow-up; callout for persistent information in content; modal only when the person must decide before continuing.
- High-severity messages stay until dismissed. Title states the outcome, subtitle the reason and next step.

## Dialogs and confirmation (`PatternConfirmation`)

- Match the friction to the impact: low impact acts immediately with Undo in a toast; moderate impact uses a danger `Modal` naming the object ("Delete bucket logs-eu"); high impact asks the person to type the resource name. Discarding unsaved work uses a transactional modal ("Discard changes").
- The dismissive action ("Cancel") never does anything destructive; in danger modals focus starts on it.

## Read-only and disabled (`PatternReadOnlyDisabled`)

- Read-only: the value is meaningful but can't be changed by this person; it stays focusable, readable at full contrast and copyable. Disabled: the control doesn't apply right now; it is skipped by Tab and dimmed, so say why nearby (a notification or helper text).
- Hide actions a person can never take rather than disabling them forever.

## Overflow content (`PatternOverflowContent`)

- Truncate at the end by default (front for paths, middle for IDs), always with the full text reachable through a `Tooltip` or the details page. Extra actions go into an `OverflowMenu`, extra tags into a "+n" `OperationalTag`, long text behind "Show more", long lists behind "Load more" or `Pagination`.

## Login (`PatternLogin`)

- Two steps: user ID (with "Remember ID" and alternative login) then password. Validate inline on blur; after a failed attempt show one generic error that doesn't reveal which part was wrong. Put recovery and create-account links next to the step they belong to.
