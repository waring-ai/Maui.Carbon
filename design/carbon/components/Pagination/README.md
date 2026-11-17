# Pagination

Lets people move through a long list: items-per-page select, "x–y of z items", a page select with "of N pages", and previous / next buttons, placed directly under a table or list.

Hand-written from `packages/styles/scss/components/pagination` (Carbon v11.99.0). The native `<select>` styling is implemented inside this component. Left out: the xs (24px) size shown on the Carbon website, the unstable pagination variant, skeleton, tooltips on the arrow buttons (they use `title`), RTL arrow flip.

**When to use**
- Under a DataTable, ContainedList or StructuredList whose rows come in pages, especially when people may change the page size.

**When not to use**
- Navigating pages of content such as search results or articles: use a PaginationNav.
- Fewer than about 25 items: show them all.
- Endless feeds where position does not matter: load more on scroll or with a "Load more" Button.
- Linear flows such as a multi-step form: use a ProgressIndicator with Back / Next Buttons.

**You provide:**
- `pageSizes` (required): numbers, or `{ text, value }`. `pageSize` (initial / synced page size, default the first size) and `page` (1-based, default 1); both re-sync when you change them, otherwise the component keeps its own state.
- `totalItems` · `onChange({ page, pageSize })`: slice your data from these values. Changing the page size returns to page 1.
- `size` sm | md | lg (default md; 32/40/48px).
- `pagesUnknown` (total not known: shows "page N" instead of the page select and keeps next enabled) · `isLastPage` (disables next when the total is unknown).
- `disabled` (everything), `pageInputDisabled`, `pageSizeInputDisabled`.
- Text: `itemsPerPageText` ("Items per page:"), `itemRangeText(min, max, total)` ("1–10 of 103 items"), `itemText(min, max)` (used when the total is unknown), `pageRangeText(current, total)` ("of 11 pages"), `pageText(page)` ("page 3"), `backwardText` ("Previous page"), `forwardText` ("Next page").
- `id`, `className`.

**Example**
```jsx
const { Pagination } = window.Carbon;
<Pagination
  page={page} pageSize={pageSize} pageSizes={[10, 20, 50]} totalItems={buckets.length}
  itemRangeText={(min, max, total) => `${min}–${max} of ${total} buckets`}
  onChange={({ page, pageSize }) => { setPage(page); setPageSize(pageSize); }}
/>
```

**Rules**
- The bar is `layer-01` with a 1px `border-subtle-01` top rule; dividers between the left group, page group and each arrow are 1px `border-subtle-01`. Directly after a DataTable the top rule is dropped.
- Selects are borderless, 14px, with a 16px chevron; hover is `layer-hover-01`. Disabled selects use `text-disabled`, disabled arrows `icon-disabled`.
- The range text is `text-secondary`; labels are `text-primary`. Name the items in the range text ("buckets", "events") when the list has a clear noun.
- It is container-responsive: below 42rem wide only the item range and the arrow buttons remain, so give it the full width of the list it controls.
- Use the same `size` as the table above it (md under md/lg tables, sm under xs/sm tables).

**Accessibility**
- Previous / next are `<button>`s with `aria-label` and `title` from `backwardText` / `forwardText`; they use the `disabled` attribute at the ends.
- When the last (or first) page is reached with an arrow, focus moves to the other arrow so keyboard users are not left on a disabled button.
- The items-per-page select is labelled by the visible "Items per page:" text; the page select has a visually hidden label "Page of N pages".
- Announce result changes yourself (for example, the table's caption or a live region) if the list reloads asynchronously.
