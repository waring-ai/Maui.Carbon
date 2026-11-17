# SelectableTile

A tile that toggles a selected state, announced as a checkbox, for picking any number of cards from a set. Exports `SelectableTile`.

Hand-written from `packages/styles/scss/components/tile/_tile.scss` and `react/src/components/Tile/Tile.tsx` (Carbon v11.99.0). Left out: `hasRoundedCorners` and the experimental tile-contrast border. The AI treatment with an AILabel `decorator` uses the AILabel card's approximated AI tokens.

**When to use**
- Multi-select choices that need more than a label: add-ons, plans or features with a description, price or status.
- A small set of options (usually 2 to 8) laid out in a grid.

**When not to use**
- Exactly one choice. Use RadioTile in a TileGroup instead.
- Simple short options. Use a CheckboxGroup.
- Navigation. Use a clickable Tile or a Link; do not mix tile variants in one group.

**You provide:**
- `children`: the tile content (a title, a line of description, optionally a price).
- `selected` (default `false`): initial state, synced when the prop changes; `onChange(event, selected, id)` to control it.
- `disabled`, `id` (generated when omitted), `title`, `tabIndex` (default 0).
- `decorator`: a node pinned top-right beside the check box, usually an AILabel (`size="mini"`); it adds `cds--tile--decorator`, the AI gradient and 4.5rem end padding.
- `onClick`, `onKeyDown`, `className`; `light` is deprecated (use a Layer). Other attributes go on the tile.

**Example**
```jsx
const { SelectableTile } = window.Carbon;
<SelectableTile id="addon-logs" selected={addons.logs} onChange={(e, on) => setAddon('logs', on)}>
  <p>Log analysis</p>
  <p>Search and retain logs for 30 days</p>
</SelectableTile>
```

**Rules**
- Markup is `div.cds--tile.cds--tile--selectable[role=checkbox][aria-checked]` with `tabindex=0` (removed when disabled), a 16px checkbox glyph 1rem from the top-right, and the content in a `label`; the tile has `padding-inline-end: 3rem` reserved for the glyph. Padding is 1rem; keep tiles at least 4rem tall and 8rem wide.
- Fill is `layer-01` (so it follows a `Layer`), hover `layer-hover-01`. The border is 1px transparent; selected becomes `layer-selected-inverse`.
- Checkbox glyph is `icon-secondary` unselected, `icon-primary` selected, `icon-disabled` when disabled; it is always visible.
- Focus is a 2px `focus` outline inset by 2px. Disabled uses `text-disabled`, `not-allowed`, no hover, and `border-disabled` if selected.
- Clicks and keys inside the `decorator` (the AILabel and its callout) do not toggle the tile.
- On a layer-01 panel, wrap the tiles in a Layer. Lay a group out in a grid with equal heights and summarize the selection (count, total) near the action.

**Accessibility**
- Each tile is its own tab stop with `role="checkbox"` and `aria-checked`; Space or Enter toggles it, as does a click anywhere on the tile.
- The accessible name comes from the tile's text, so put the title first and keep it short. Add `aria-label` when the content alone is ambiguous.
- Wrap a set in an element with `role="group"` and an `aria-label` (or a heading it is labelled by) so the choices are announced together.
- A disabled tile gets `aria-disabled` and leaves the tab order; say nearby why it is unavailable.
