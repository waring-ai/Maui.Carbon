# PatternOverflowContent

Handling content that doesn't fit: end, front and mid-line truncation with the full text still reachable, truncated table cells with tooltips, an overflow menu for extra actions, a +n tag for extra labels, Show more for long text and Load more for long lists.

Composed from this system's components following Carbon's overflow content pattern.

**When to use**
- Truncate static text or links that can exceed their container: long resource names, paths, URLs, IDs, descriptions, breadcrumbs.
- Don't truncate page titles, headings, field labels, error or validation messages, or notifications: let them wrap.
- Use Show more when the hidden content is substantial and people may want it; Load more when fetching everything at once would be slow.

**Components used**
`CopyButton`, `DataTable`, `Link`, `Tooltip`, `OverflowMenu`, `MenuItem`, `MenuItemDivider`, `Button`, `Tag`, `OperationalTag`, `Popover`, `PopoverContent`, `ContainedList`, `ContainedListItem`.

**Composition rules**
- End truncation (`cds--text-truncate--end`) for names and descriptions; front truncation (`cds--text-truncate--front`) for paths where the end identifies the item; mid-line truncation (script: keep the start and end) for IDs that differ at both ends. The ellipsis replaces at least 3 characters.
- The element needs a set width (a fixed column, `max-width: 100%`) for the ellipsis to appear. Truncated plain text gets a `title` with the full value; truncated links get a `Tooltip` so the value also appears on keyboard focus; IDs get a `CopyButton` that copies the full value.
- Tables: fixed column widths with `table-layout: fixed`, truncate the long columns, keep numbers and status untruncated.
- Overflow menu: keep the one or two main actions visible as buttons and move the rest into a `flipped` `OverflowMenu` at the end of the row, destructive items last after a divider.
- Tags: show the first few, then an operational "+n" tag that opens the rest in a `Popover`.
- Show more: clamp to 3 lines, then a ghost "Show more" button that becomes "Show less". Don't use fades or inner scrolling instead.
- Load more: state the count ("Showing 4 of 24") and fetch the next batch with a ghost "Load more" button at the end of the list.

**Accessibility**
- Truncation is visual only: the full text stays in the DOM, so screen readers read it all.
- `title` tooltips don't appear on keyboard focus, so anything interactive and truncated needs a `Tooltip` or another way to see the full value.
- Show more sets `aria-expanded` and `aria-controls` on its button; the +n tag sets `aria-expanded` and has a name that says what it opens ("Show 4 more labels"). Overflow menus need an `iconDescription` naming the object.

Known workarounds in the preview: the bundle ships `cds--text-truncate--end` only inside the UI shell and no `cds--text-truncate--front`, so the preview defines both utilities with Carbon's declarations; the base `Button` drops `aria-expanded` and `aria-controls`, so Show more is a native button with the ghost button classes.
