# Tile

A flat container on a `layer-01` fill with 16px padding that groups related content; `ClickableTile` makes the whole tile one link or button. Static by default; with `href` or `onClick` it becomes clickable (hover `layer-hover-01`, pressed `layer-active-01`, trailing arrow).

Hand-written from `packages/styles/scss/components/tile`. This card covers the base `Tile` and `ClickableTile` (both exported). The other tile variants are their own cards: **SelectableTile** (single or multi select with a checkbox mark), **RadioTile** (one choice from a TileGroup) and **ExpandableTile** (shows more content below the fold). Left out: the v12 tile-border and selection-icon feature flags, rounded corners and the AI label decorator.

**When to use**
- Group a small amount of related information or a summary metric: usage, a plan, a service.
- `ClickableTile` when the whole tile leads to one place or starts one action (a dashboard card that opens details).

**When not to use**
- Choosing options: use SelectableTile, RadioTile, or a RadioButtonGroup / CheckboxGroup.
- Hiding long content: use ExpandableTile or an Accordion.
- Secondary information, actions or messages that pop over the page: use a Modal, Popover or Toggletip.
- Plain navigation inside text: use a Link.

**You provide:**
- `Tile`: `children`, `className`, and for compatibility `href` (link tile with a 20px `arrow--right`) or `onClick` (button tile), `disabled`.
- `ClickableTile`: `children`, `href` (renders `<a>`, with `target` / `rel`) or `onClick` (renders `<button>`), `renderIcon` (icon file stem drawn bottom-right at 20px in `icon-interactive`; default `"arrow--right"`, `"launch"` for external destinations, `null` for none), `disabled` (non-interactive, `aria-disabled`), `className`. Other props pass through.

**Example**
```jsx
const { Tile, ClickableTile } = window.Carbon;
<Tile><h4>Storage used</h4><p>4.2 TB of 10 TB in 48 buckets.</p></Tile>
<ClickableTile href="/clusters"><h4>View clusters</h4><p>12 clusters in 4 regions.</p></ClickableTile>
<ClickableTile href="https://docs.example.com" target="_blank" renderIcon="launch">API reference</ClickableTile>
```

**Rules**
- Tiles are square, with no shadow or border. Separation comes from the layer step: a tile inside a `layer-01` container is wrapped in a Layer so it uses `layer-02`.
- Min size is 4rem x 8rem; padding 16px; a ClickableTile reserves 48px at the bottom for its 20px icon.
- Focus is a 2px inset `focus` outline. Disabled clickable tiles use `text-disabled` and an `icon-disabled` icon.
- A clickable tile has exactly one action: no buttons, links or other interactive elements inside it. A base tile may hold its own links (bottom left) or a full-width button.
- Keep tiles in one group the same variant and the same height.

**Accessibility**
- `ClickableTile` is a native `<a href>` or `<button>`, so Tab focuses it and Enter (Space for buttons) activates it. Its accessible name is all of its text, so keep the content short and lead with the destination ("View clusters").
- The trailing icon is decorative (`aria-hidden`). For `target="_blank"` say that it opens a new tab in the text or with `aria-label`; `rel="noopener noreferrer"` is added for you.
- A static `Tile` is a plain `<div>`: give it a heading inside when it starts a new section.
