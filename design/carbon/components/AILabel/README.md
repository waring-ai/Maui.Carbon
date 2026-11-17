# AILabel

Marks content or a field as AI-generated: a small "AI" button that opens an explainability callout (a toggletip with the AI gradient). `AILabelContent` is the callout and `AILabelActions` its bottom toolbar.

Hand-written from `packages/styles/scss/components/slug` (`ai-label` re-exports it), `utilities/_ai-gradient.scss` and `packages/react/src/components/AILabel` (Carbon v11.99.0). Left out: `autoAlign` (floating-ui flipping), the `alignmentAxisOffset` for small sizes, the deprecated `.cds--slug` class names, and the in-field `translate(0.5rem, -50%)` Carbon applies to the revert state (it assumes the label is absolutely positioned inside an input). The Carbon `ai-*` theme tokens are not in this bundle: the gradient, borders and caret use `highlight`, `tag-background-blue`, `border-interactive` and `background` instead, and the callout's two faint outer drop shadows are omitted. The form inputs in this bundle (TextInput, Select, NumberInput...) have no `decorator` slot and no AI field styling, so an AILabel cannot be placed inside a field here; the decorator works on SelectableTile, RadioTile, ExpandableTile, CheckboxGroup and a Tile with `cds--tile--decorator`.

**When to use**
- Anything an AI generated or suggested: a value, a summary, a recommendation, a column or a whole region.
- As the single entry point to explainability: the callout says what the AI did, how confident it is and where the data came from.

**When not to use**
- As decoration or a generic info button. Use a Toggletip or Tooltip for non-AI help.
- To trigger AI actions (regenerate, ask). Use a Button or IconButton with its own icon.
- With changed behavior (hover-only, links). It always opens the same click-to-open callout.

**You provide:**
- `children`: an `AILabelContent` holding the explanation, optionally ending with `AILabelActions` (ghost `IconButton`s and at most one primary `Button`).
- `size`: `mini` | `2xs` | `xs` | `sm` | `md` | `lg` | `xl` (16/20/24/32/40/48/64px, default `xs`).
- `kind`: `default` | `inline` (default `default`); `textLabel` adds text after "AI" in the inline kind (bordered "inline with content" look); `aiTextLabel` is its deprecated alias.
- `aiText` (default "AI"), `aria-label` (default "Show information"; the button is named "AI Show information"); `slugLabel` is its deprecated alias.
- `align` places the callout (default `bottom`); `defaultOpen`, or controlled `open` + `onOpenChange(open)`.
- `revertActive` swaps the button for a ghost revert `IconButton` (undo icon, tooltip and name `revertLabel`, default "Revert to AI input") that calls `onRevertClick(event)`.
- `className`; the ref is forwarded to the wrapper.

**Example**
```jsx
const { AILabel, AILabelContent, SelectableTile } = window.Carbon;
<SelectableTile id="pool-mem" decorator={
  <AILabel size="mini" aria-label="Why this is suggested">
    <AILabelContent><p>Suggested because two nodes ran out of memory last week.</p></AILabelContent>
  </AILabel>}>
  Memory-optimized pool
</SelectableTile>
```

**Rules**
- The default button is a square with a 1px `border-inverse` border, `text-primary` 600-weight text; hover fills with `border-inverse` and flips text to `text-inverse`; focus is a 1px `focus` border plus an inset 1px `focus` ring. mini and 2xs get a 24px invisible hit area.
- Sizes: `mini` on tiles and other containers, `xs` (default) beside fields and table headers, larger sizes only next to large type. Beside a ghost icon button use one size smaller than the button.
- Inline kind: a 4px (8px at lg/xl) `icon-primary` square before "AI", no box; hover shows an `icon-secondary` border and `text-secondary` text. Use inline only at sm, md or lg, matched to the adjacent text; the text is 12px at sm and below, 16px at lg.
- Callout: 8px radius, 1px gradient border (light blue to `border-interactive`), `background` fill fading to a blue tint at the bottom, `text-primary` text, 280px minimum width, 320px maximum content width, 24px padding with 80px at the bottom to leave room for the toolbar. Structure it as overview, supporting details, sources, then actions.
- `AILabelActions` is docked to the bottom edge, right-aligned, with no gaps; the primary button sits last with a 7px bottom-right radius.
- Placement: top-right of containers with a 1rem margin, never flush; on a legend or label, right after the text; leftmost in a group of icons. Mirror for right-to-left.
- As a decorator a container takes the AI gradient, gradient border and a soft shadow (`cds--tile--decorator`). When the user edits the AI value, remove the AI styling and show `revertActive`; reverting restores the value, the label and the styling.
- The label is never disabled, even in a disabled or read-only context.

**Accessibility**
- The trigger is a `button` with `aria-expanded` and `aria-controls` pointing at the callout; its name is "{aiText} {aria-label}" (or "{aiText} {textLabel}" for inline with text). Write `aria-label` for what it explains ("Why bx2.8x32").
- Enter or Space (or a click) toggles; Escape closes and returns focus to the button; clicking outside or tabbing out of the label closes it. Focus moves into the callout's actions with Tab.
- The revert button is an IconButton whose `revertLabel` is both its tooltip and accessible name.
- Inside a SelectableTile, clicks and keys on the label do not toggle the tile.
