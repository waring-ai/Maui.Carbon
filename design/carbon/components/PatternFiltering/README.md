# PatternFiltering

Narrowing a resource list with search and filters: a left filter panel that updates results instantly, a result count, removable applied-filter tags and Clear filters, plus a toolbar flyout that applies a batch of filters together.

Composed from this system's components following Carbon's filtering and search patterns.

**When to use**
- Instant filters in a panel: several categories over a data set that responds quickly; people explore by toggling options.
- Batch filters in a flyout: complex combinations or slow queries, where results should change only when the person chooses Apply filters.
- Use the DataTable's own search alone when people only look things up by name (see PatternResourceList).

**Components used**
`Breadcrumb`, `Heading`, `Accordion`, `AccordionItem`, `Checkbox`, `RadioButtonGroup`, `Search`, `DismissibleTag`, `Button`, `DataTable`, `IconIndicator`, `Link`, `Popover`, `PopoverContent`, `IconButton`.

**Composition rules**
- Several categories go vertically in a 256px panel on the left (or horizontally above the data), never inside one menu. Each category is an accordion item whose title shows how many options are on ("Region (2)").
- Multiselect categories use checkboxes with the matching count; single-select categories use radio buttons with an "Any" default.
- Every category has its own clear ("Clear region"); the panel header has "Clear all". Clearing returns to the starting state.
- Above the results: search, then a row with the result count ("Showing 5 of 14 clusters"), one `DismissibleTag` per applied filter ("Region: Dallas") and a Clear filters ghost button. Always show the count, including zero.
- No matches: replace the table with an empty state that names the cause and offers Clear filters (see PatternEmptyStates).
- Batch flyout: a filter `IconButton` in the toolbar opens a `Popover` (bottom-end) with the categories and a full-width footer, "Reset" (secondary) left and "Apply filters" (primary) right. When it is closed and filters are on, a tag next to it shows the number ("3 filters") and clears them without reopening.
- Search placeholder hints at what is searched ("Search clusters by name"); the search has no visible label.

**Accessibility**
- The filter panel is an `aside` labelled by its "Filters" heading; each category's options sit in a `fieldset` whose legend is the category (visually hidden where the accordion title already shows it).
- The result count is a polite live region, so screen readers hear the new count after each change; focus stays on the option that was toggled.
- Each filter tag's remove button is named for the filter ("Remove filter Region: Dallas"). The flyout trigger has an accessible name and `aria-expanded`; Escape and outside clicks close it.

Known workaround in the preview: `IconButton` drops `aria-expanded`, so the flyout trigger is a native button with the icon-only ghost button classes and an `aria-label`.
