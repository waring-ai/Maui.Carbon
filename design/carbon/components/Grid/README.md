# Grid

The 16-column responsive layout grid. `Grid` and `Column` use CSS Grid; `FlexGrid`, `Row` and `Column` use the older flexbox implementation. Exports `Grid`, `Column`, `FlexGrid` and `Row`.

Hand-written from `packages/grid/scss/_css-grid.scss`, `_flex-grid.scss`, `_config.scss` and `react/src/components/Grid/` (Carbon v11.99.0). Left out: `ColumnHang`, `HideAtBreakpoint` and the `enable-css-grid` feature flag (Grid is always the CSS grid here; use FlexGrid for flex).

**When to use**
- Page and panel layout that must line up with the 2x Grid: page headers, filter and content columns, dashboards, card rows, two-column forms.
- Responsive layouts: give each Column a span per breakpoint instead of writing media queries.

**When not to use**
- Spacing items in a single row or column. Use Stack.
- Layout inside a component (a tile's content, a toolbar). Use flex or Stack; the grid is for page structure.
- New layouts that need FlexGrid. Prefer the CSS Grid; keep FlexGrid for existing flexbox pages.

**You provide:**
- `Column` children inside a `Grid` (or `Row`s of `Column`s inside a `FlexGrid`).
- `Column`: `sm`, `md`, `lg`, `xlg`, `max` and (CSS Grid only) `span`. A number spans that many columns, `true` is auto, `'25%' | '50%' | '75%' | '100%'` span a fraction, and an object `{ span, offset, start, end }` positions it (`start`/`end` are CSS Grid only; flex mode supports `span` and `offset`). `as`, `className`.
- `Grid`: `narrow`, `condensed`, `fullWidth`, `align` (`start` | `center` | `end`, default centred), `as`, `className`. A `Grid` inside a `Grid` becomes a subgrid on the parent's columns.
- `FlexGrid`: `narrow`, `condensed`, `fullWidth`, `as`, `className`. `Row`: `narrow`, `condensed`, `as`, `className`.

**Example**
```jsx
const { Grid, Column } = window.Carbon;
<Grid>
  <Column sm={4} md={8} lg={16}><h1>Clusters</h1></Column>
  <Column sm={4} md={8} lg={4}>{filters}</Column>
  <Column sm={4} md={8} lg={12}>{clusterTable}</Column>
</Grid>
```

**Rules**
- Breakpoints (min-width): sm 20rem (320px, 4 columns, 0 margin), md 42rem (672px, 8 columns, 1rem margin), lg 66rem (1056px, 16 columns), xlg 82rem (1312px), max 99rem (1584px, margin 1.5rem). The grid is centred and never wider than 99rem unless `fullWidth`.
- Modes: wide (default) has a 2rem gutter (1rem each side of a column) for most layouts and text; `narrow` removes the gutter before each column so content hangs 1rem into the gutter (type next to imagery); `condensed` uses a 1px gutter for tiles and data that read as one unit. Gutters are in `--cds-grid-gutter`, column count in `--cds-grid-columns`, page margin in `--cds-grid-margin`.
- Spans cascade upward: set `sm`, `md` and `lg` as needed; a breakpoint with no value inherits the smaller one. A span of 0 hides the column at that breakpoint.
- Classes follow Carbon: `cds--css-grid`, `cds--css-grid-column`, `cds--lg:col-span-4`, `cds--lg:col-start-5`; flex mode uses `cds--grid`, `cds--row`, `cds--col-lg-4`, `cds--offset-lg-4`.
- Put content inside columns, not on the grid; the column box is the gutter-free area in CSS Grid but includes the padding gutter in flex mode.
- Prefer the 2x Grid's aspect ratios (1:1, 2:1, 3:2, 4:3, 16:9) for boxes on the grid; see AspectRatio.
- The preview colors columns with `layer-accent-01` and a `border-interactive` outline; these are demo styles, not part of the component.

**Accessibility**
- Grid and Column render plain `div`s with no role. Use `as` (`main`, `section`, `aside`) to give regions landmark semantics.
- Keep DOM order equal to visual order; do not use `start`/`offset` to move content before something that comes earlier in the source.
- At every breakpoint content must reflow without horizontal scrolling (WCAG 1.4.10); stack columns at sm and md.
