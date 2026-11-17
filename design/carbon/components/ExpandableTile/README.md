# ExpandableTile

A tile that shows a summary and reveals more content when expanded. Exports `ExpandableTile`, `TileAboveTheFoldContent` and `TileBelowTheFoldContent`.

Hand-written from `packages/styles/scss/components/tile/_tile.scss` and `react/src/components/Tile/Tile.tsx` (Carbon v11.99.0). Left out: `hasRoundedCorners`, `tileMaxHeight`/`tilePadding` overrides and the experimental tile-contrast border. The label-01 style of the visible `tileCollapsedLabel` / `tileExpandedLabel` text is reconstructed, not checked against source.

**When to use**
- A list of cards where each has a short summary and longer details or actions: clusters, certificates, policies.
- Hiding secondary detail to keep a dense page scannable.

**When not to use**
- Content most users need right away. Show it in a plain Tile.
- Several related sections that expand one at a time, or long stacked content. Use an Accordion.
- Revealing actions, confirmations or notifications. Use a Modal, Popover or notification instead.

**You provide:**
- Exactly two children: `TileAboveTheFoldContent` (always visible) then `TileBelowTheFoldContent` (shown when expanded).
- `expanded` (default `false`): initial state, synced when the prop changes.
- `tileCollapsedIconText` / `tileExpandedIconText`: accessible name of the toggle (defaults "Interact to expand Tile" / "Interact to collapse Tile"); write them for the tile ("Show details for prod-eu-1").
- `tileCollapsedLabel` / `tileExpandedLabel`: optional visible text beside the chevron (non-interactive variant).
- `decorator`: a node pinned top-right, usually an AILabel; it makes the tile the interactive variant and adds the AI gradient.
- `onClick`, `onKeyUp`, `tabIndex`, `className`; `light` is deprecated (use a Layer).

**Example**
```jsx
const { ExpandableTile, TileAboveTheFoldContent, TileBelowTheFoldContent, Link } = window.Carbon;
<ExpandableTile tileCollapsedIconText="Show certificate details" tileExpandedIconText="Hide certificate details">
  <TileAboveTheFoldContent><p>TLS certificate</p><p>Expires in 12 days</p><Link href="/certs">Renew now</Link></TileAboveTheFoldContent>
  <TileBelowTheFoldContent><p>Issued for api.example.com and *.example.com.</p></TileBelowTheFoldContent>
</ExpandableTile>
```

**Rules**
- If neither fold contains interactive content (links, buttons, inputs) the whole tile is a `button[aria-expanded]` with a non-interactive chevron. If it does, the tile is a `div` with `cds--tile--expandable--interactive`, no hover, and a separate 3rem chevron button with `aria-expanded`/`aria-controls`.
- Collapsed height is the measured above-the-fold height plus the tile padding, set as `max-height` and animated over `duration-moderate-01` (`duration-fast-02` when expanding). The tile clips (`overflow: hidden`) until expanded.
- Below-the-fold content fades with opacity and visibility (`duration-fast-02`, productive easing). The chevron sits in a 3rem box at the bottom-right and rotates 180 degrees; keep content clear of that corner. Visible labels are `label-01` in `text-secondary`, 0.5rem before the chevron.
- Fill `layer-01`, hover `layer-hover-01`; chevron `icon-primary`; focus is a 2px `focus` outline inset 2px on the tile (or the chevron button). On a layer-01 panel, wrap it in a Layer.

**Accessibility**
- Non-interactive variant: the tile is one button; Enter or Space toggles it and `aria-expanded` reports the state.
- Interactive variant: Tab reaches the links and buttons inside in order, then the chevron button; Enter or Space on the chevron toggles. Collapsed below-the-fold content is `visibility: hidden`, so it is skipped by Tab and screen readers.
- The chevron's name is `tileCollapsedIconText` / `tileExpandedIconText`; make it specific when several tiles share a page.
