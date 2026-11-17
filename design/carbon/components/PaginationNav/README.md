# PaginationNav

Numbered page buttons with previous / next arrows, for navigating pages of content (search results, articles) rather than table rows; pages that do not fit collapse into an overflow select.

Hand-written from `packages/styles/scss/components/pagination-nav` and `packages/react/src/components/PaginationNav` (Carbon v11.99.0). Left out: tooltips on the arrows (they use `title`), the small-screen media query (use `itemsShown`), RTL.

**When to use**
- Paging through whole pages or sections of content: search results, docs, release notes, a gallery of templates.
- People benefit from seeing how many pages there are and jumping to a specific one.

**When not to use**
- Under a DataTable or list whose page size people can change: use a Pagination.
- A linear flow such as a multi-step form: use a ProgressIndicator with Back / Next Buttons.
- Only one or two pages: show everything, or a single "Load more" Button.

**You provide:**
- `totalItems` (the number of pages), `page` (0-based current page; re-syncs when you change it, otherwise the component keeps its own state) and `onChange(pageIndex)`.
- `size` sm | md | lg (default lg; 32/40/48px).
- `itemsShown` (default 10): how many page slots are visible including the first, last and overflow selects; md allows 4 or 5, sm 4 to 7, lg at least 4.
- `loop` (previous / next wrap around at the ends instead of disabling) · `disableOverflow` (the "..." is shown but not selectable).
- `aria-label` (default "pagination") · `className`.

**Example**
```jsx
const { PaginationNav } = window.Carbon;
<PaginationNav
  aria-label="Search results pages"
  totalItems={14} page={page} itemsShown={7}
  onChange={(index) => loadResults(index)}
/>
```

**Rules**
- Items are `text-primary` 14px; hover is `background-hover`; the active page is 600 weight with a 1rem wide `border-interactive` bar at its bottom.
- Arrows are square buttons the size of the nav height; disabled items use `text-secondary` at 50%.
- The first and last page are always shown. Hidden ranges become a "..." native select whose options are the hidden page numbers; a range of one page shows that page directly. Never start or end the series with "...".
- Place it below the content it pages, preferably aligned with the content's start or end edge, with the same size as nearby controls.
- Focus is a 2px inset `focus` outline.

**Accessibility**
- The component is a `nav` with an `aria-label` (default "pagination"); pass a specific one ("Search results pages") when the page has more than one nav.
- Page buttons are real buttons with a visually hidden "Page" prefix; the active page has `aria-current="page"` and "Active, Page".
- Previous / next are buttons labelled "Previous" / "Next" that use the `disabled` attribute at the ends (unless `loop`).
- The overflow selects are labelled "Select Page number"; a visually hidden polite live region announces "Page N of M" after each change.
- Keyboard: Tab moves between buttons, Enter or Space activates; in the overflow select the arrow keys pick a page.
