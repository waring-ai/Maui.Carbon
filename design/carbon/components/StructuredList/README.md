# StructuredList

Shows simple tabular information in rows and cells without table chrome; the selectable variant lets people pick one row.

Hand-written from `packages/styles/scss/components/structured-list` (Carbon v11.99.0). Left out: skeleton, the v12 visible-icons flag (radio glyphs in the enabled state, on the start edge).

**When to use**
- Browsing a small group of related records, or comparing options side by side: node flavors, plans, label / value details of a resource.
- Picking exactly one option where each option needs a few columns of explanation (`selection`).

**When not to use**
- Sorting, searching, multi-select, nested or many columns: use a DataTable.
- Small or confined spaces such as a side panel or popover: use a ContainedList.
- A short single-choice question in a form: use a RadioButtonGroup or RadioTile.

**You provide:**
- Either `columns` (header labels) and `rows` (`[{ id, cells: [...], disabled? }]`), or compose the parts: `StructuredListHead` > `StructuredListRow head` > `StructuredListCell head`, then `StructuredListBody` > `StructuredListRow` > `StructuredListCell`.
- `aria-label` on the list (default "Structured list section"; always pass a real one).
- `selection` (a radio per row): `selectedInitialRow` (row id) or controlled `selectedRow`, and `onSelectionChange(rowId)`.
- `isCondensed` (8px cell padding) · `isFlush` (no start padding; ignored with `selection`) · `noWrap` (on the list for `rows`, or on a `StructuredListCell`) · `className`.
- `StructuredListRow`: `id` (needed for selection), `disabled` (cannot be selected, dimmed), `label` (accessible name of the row's radio, default "Select row"), `onClick`.

**Example**
```jsx
const { StructuredList } = window.Carbon;
<StructuredList
  aria-label="Choose a worker node flavor" selection selectedInitialRow="bx2-4x16"
  onSelectionChange={setFlavor}
  columns={['Flavor', 'vCPU / memory', 'Best for']}
  rows={[
    { id: 'bx2-4x16', cells: ['bx2.4x16', '4 vCPU / 16 GB', 'General web apps and APIs'] },
    { id: 'mx2-8x64', cells: ['mx2.8x64', '8 vCPU / 64 GB', 'In-memory caches'], disabled: true },
  ]}
/>
```

**Rules**
- Background is `layer-01`; rows are separated by 1px `border-subtle-01`; header cells are 14px / 600 `text-primary`, body cells 14px `text-secondary`.
- Cell padding is 1rem top and 1.5rem bottom (0.5rem all round when condensed), 1rem at the row start; flush removes the start padding so text lines up with content above (hang alignment is the default).
- Selectable rows hover to `layer-hover-01` and select to `layer-selected-01` with `text-primary`; the leading 16px radio glyph is `icon-primary`. Disabled rows use `text-disabled` / `icon-disabled` and `cursor: not-allowed`.
- Content: sentence case, column headers one or two words, at most three short paragraphs per cell. Long text wraps inside its cell.
- Only one row can be selected; for multiple selection use a DataTable.

**Accessibility**
- Roles are `table`, `rowgroup`, `row`, `columnheader` and `cell`; the list is named by `aria-label`.
- Each selectable row holds a visually hidden native radio (one `name` per list) named by the row `label`, so Tab reaches the group and Up / Down arrows move the selection; Space selects. The focused row shows a 2px `focus` outline.
- Disabled rows set `aria-disabled` and disable their radio.
- Do not put interactive controls inside selectable rows: the whole row is the click target.
