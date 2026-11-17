# DataTable

Shows structured rows and columns that people scan, sort, search, select and act on. It is declarative: pass `headers` and `rows` and the table builds the toolbar, header, selection, expansion and row menus for you.

Hand-written from `packages/styles/scss/components/data-table` (base, action, expandable, sort) (Carbon v11.99.0). The overflow-menu column is built from a `rowActions` prop instead of a composed `OverflowMenu` cell, and the sticky header uses `position: sticky` on the header cells rather than Carbon's flex-row layout. Left out: inline editing, skeleton (use DataTableSkeleton), AI label and decorator columns, top-aligned cells, "select all across pages" in the batch bar, the expand-all header button.

**When to use**
- Many records with several attributes that people compare, sort, filter or act on in bulk: clusters, nodes, API keys, invoices.
- Showing all of a user's resources of one kind, or helping them find the one record a task needs.

**When not to use**
- A few label/value pairs or a short read-only list: use a StructuredList or ContainedList.
- Picking one option inside a form: use a RadioButtonGroup, Dropdown or RadioTile.
- Hierarchical data deeper than one level: use a TreeView.
- Spreadsheet-style editing or interactions more complex than rows and columns: build a dedicated view.

**You provide:**
- `headers` as `[{ key, header, align?, isSortable? }]` and `rows` as `[{ id, disabled?, <key>: value }]` (values are strings, numbers or nodes; `align: 'right'` right-aligns numbers; `disabled` disables the row's selection control).
- `size` xs | sm | md | lg | xl (default lg; rows 24/32/40/48/64px) · `useZebraStyles` · `isSortable` (all columns; `isSortable: false` on a header opts out).
- `selectable` true | 'multi' (checkboxes with indeterminate select-all) | 'radio' (single select) · `expandable` with `renderExpanded(row)` · `renderCell(value, row, header)`.
- `title`, `description` (above the toolbar; the title labels the table, the description describes it) · `aria-label` / `aria-labelledby` when there is no title · `id`, `className`, `style`.
- Toolbar: `search` (+ `searchPlaceholder`, `persistentSearch`), `batchActions` `[{ label, icon, onClick(selectedRows) }]` (multi-select only), `toolbarContent` (buttons at the end).
- Row menus: `rowActions` `[{ label, onClick(row), kind: 'danger', disabled, icon }]` or `(row) => [...]` adds a last column with an `OverflowMenu` per row; `rowActionsHeader` (hidden header text, default "Actions"); `overflowMenuOnHover` (default true: menus appear on row hover or focus; false keeps them visible).
- `stickyHeader`: the header row stays pinned while the body scrolls inside a 300px-high area.
- `emptyState`: `{ title, description, action }` or a node, shown when no rows match (or there are none).
- State is uncontrolled by default; control it with `selectedIds` / `defaultSelectedIds` / `onSelectionChange(ids)`, `expandedIds` / `defaultExpandedIds` / `onExpandChange(ids)`, `sortKey` + `sortDirection` / `defaultSortKey` + `defaultSortDirection` / `onSortChange(key, dir)`, `searchValue` / `defaultSearchValue` / `onSearchChange(value)`.
- **Primitives** for hand-built static tables, same styling: `Table` (`size`, `useZebraStyles`, `isSortable`, `stickyHeader`, `overflowMenuOnHover`, `contentClassName`), `TableHead`, `TableBody`, `TableRow` (`isSelected`), `TableHeader` (`isSortable`, `isSortHeader`, `sortDirection` NONE | ASC | DESC, `onClick`, `align`), `TableCell` (`align`).

**Example**
```jsx
const { DataTable, Button } = window.Carbon;
<DataTable
  title="Clusters"
  headers={[{ key: 'name', header: 'Name' }, { key: 'region', header: 'Region' }, { key: 'nodes', header: 'Nodes', align: 'right' }]}
  rows={clusters}
  isSortable selectable search searchPlaceholder="Filter clusters"
  batchActions={[{ label: 'Restart', icon: 'restart', onClick: restartClusters }]}
  rowActions={[{ label: 'View details', onClick: openCluster }, { label: 'Delete', kind: 'danger', onClick: confirmDelete }]}
  toolbarContent={<Button renderIcon="add" onClick={createCluster}>Create cluster</Button>}
  emptyState={{ title: 'No clusters yet', description: 'Create a cluster to deploy your first workload.' }}
/>
```

**Rules**
- Surfaces use the `-01` layer tokens: body `layer-01`, header row `layer-accent-01`, dividers `border-subtle-01`, hover `layer-hover-01`, selected `layer-selected-01` (hover `layer-selected-hover-01`). A parent `Layer` remaps them.
- Header text is 14px / 600 `text-primary`, cells 14px `text-secondary` (`text-primary` when hovered or selected). Left-align text, right-align numbers (`align: 'right'`); a right-aligned sortable header puts its label on the end edge with the arrow before it.
- Sort cycles none, ascending, descending. The active header gets `layer-selected-hover-01` and an arrow (rotated for descending); inactive sortable headers show the up-down arrows on hover and focus.
- Selection checkboxes sit in a 40px column; the header checkbox is indeterminate when some rows are selected. The batch bar replaces the toolbar on `background-brand` with `text-on-color`: a count, your actions, then Cancel.
- The toolbar search is a 48px magnifier that expands on focus (`persistentSearch` keeps it open); `Escape` clears it. In `xs` and `sm` tables the toolbar is 32px. Toolbar buttons should be `size="lg"` (48px) to fill it.
- The overflow column is 52px wide; the menu trigger matches the row height (xl uses lg) and opens flipped to the end edge. Use `kind: 'danger'` for destructive actions and put them last.
- Expanded child rows are indented 56px (88px with a checkbox column); with zebra a parent and its child share a stripe.
- Toolbar: at most five global actions (settings, export, filters, the primary action at the end); match its height to the rows (lg toolbar with lg / xl rows, sm with xs / sm). Use skeletons (DataTableSkeleton) rather than a spinner while rows load.
- Content: title and description in sentence case saying what the rows have in common; column headers one or two words.
- Put a `Pagination` directly after the table: its top border is dropped when it follows `.cds--data-table-container`.
- The empty state (`cds--data-table-empty`, not a Carbon class) is a centered block under the header row on `layer-01`: a short title, one sentence that says what to do, at most one action.

**Accessibility**
- Native `<table>` semantics; header cells have `scope="col"`, sortable headers are buttons inside the `th` and the `th` carries `aria-sort`.
- The table is labelled by the `title` (`aria-labelledby`) and described by `description`; without a title pass `aria-label`.
- Selection uses native checkboxes / radios labelled "Select all rows" / "Select row"; batch buttons are `tabindex=-1` and the bar is `aria-hidden` while nothing is selected.
- Expand buttons are real buttons with `aria-expanded`, `aria-controls` and the label "Expand current row" / "Collapse current row".
- Row menu triggers are named "Options for <first cell text>"; the menu supports arrow keys, Home/End and Escape (focus returns to the trigger). Hidden-until-hover menus become visible on keyboard focus.
- The toolbar is a `section` labelled "data table toolbar"; the search has `role="search"` and a visually hidden label from `searchPlaceholder`. Focus is a 2px `focus` outline inset on headers, search and toolbar items.
