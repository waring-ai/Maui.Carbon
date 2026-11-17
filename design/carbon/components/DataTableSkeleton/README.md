# DataTableSkeleton

A loading placeholder in the shape of a data table: optional title block, toolbar, header row and n body rows of pulsing bars.

Hand-written from `packages/styles/scss/components/data-table/skeleton/_data-table-skeleton.scss` and `packages/react/src/components/DataTableSkeleton` (Carbon v11.99.0), on top of the DataTable styles. `compact` only adds Carbon's legacy class, which has no v11 styles.

**When to use**
- While the rows of a `DataTable` load for the first time, in the place the table will appear.

**When not to use**
- Reloading or paging an already-visible table: keep the rows and show `InlineLoading` or `Loading`.
- Lists, cards or text: use `SkeletonText`, `SkeletonPlaceholder` and `SkeletonIcon`.
- Long operations that need progress: use `ProgressBar`.

**You provide:** `rowCount` (default 5), `columnCount` (default 5), optional `headers` (`[{ key, header }]`, real labels instead of header bars), `showHeader` (default true), `showToolbar` (default true), `zebra`, `compact`, `className`; other attributes (such as `aria-label`) go on the `<table>`.

**Example**
```jsx
const { DataTableSkeleton } = window.Carbon;
{loading ? <DataTableSkeleton showHeader={false} rowCount={4} columnCount={5}
  headers={[{ key: 'name', header: 'Name' }, { key: 'status', header: 'Status' }, { key: 'region', header: 'Region' }, { key: 'nodes', header: 'Nodes' }, { key: 'created', header: 'Created' }]} /> : <ClustersTable />}
```

**Rules**
- Bars are 64x16px (`skeleton-background` with an animated `skeleton-element` sweep); header bars invert the two tokens.
- Title bar 120x24px, description bar 160x16px 8px below; toolbar holds a 150px button skeleton on the right in a 48px `layer-01` bar.
- Rows are 48px with `border-subtle-01` dividers; zebra alternates `layer-accent-01`.
- Match `columnCount`, `headers` and approximately `rowCount` to the table that will replace it, so the layout does not shift. Keep real page titles and headers visible when they are known.
- The animation stops under `prefers-reduced-motion`.

**Accessibility**
- The skeleton is a real `<table>` of empty cells. Give it an `aria-label` such as "Clusters (loading)" or set `aria-busy="true"` on the region that will hold the table, and announce completion with the loaded content.
- The bars carry no text and are not focusable; the toolbar skeleton has `aria-label="data table toolbar"`.
