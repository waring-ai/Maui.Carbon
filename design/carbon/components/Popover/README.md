# Popover

A floating surface anchored to a trigger, with an optional caret. `Popover` is the wrapper, `PopoverContent` the surface; Tooltip, Toggletip and DefinitionTooltip are built on it.

Hand-written from `packages/styles/scss/components/popover` (Carbon v11.99.0). Left out: `autoAlign` (viewport-aware flipping), `isTabTip` (the tab-tip variant for toolbars), RTL overrides and the `Layer`-aware background token.

**When to use**
- A custom floating panel next to a trigger that holds interactive content: a filter panel, a list of extra items behind a "+3" tag, a small disclosure.
- Building your own overlay pattern when Toggletip or a menu component does not fit.

**When not to use**
- A short text label or hint: use `Tooltip` (or `IconButton`, which has one built in).
- Help text with a link or button next to a label: use `Toggletip`.
- Content wider than about four columns, or anything that would need a popover inside a popover: use a `Modal`.
- A list of commands or options: use `OverflowMenu`, `MenuButton` or `Dropdown`.

**You provide:** a trigger element (usually a `button`) followed by `<PopoverContent>` as children of `<Popover open>`. `open` is controlled: toggle it from your own state, and pass `onRequestClose` to be told about outside clicks and focus leaving. Give the trigger `aria-expanded` (and `aria-controls` if useful).

**Props:** `align` bottom (default) | bottom-start | bottom-end | top | top-start | top-end | left | left-start | left-end | right | right-start | right-end (the older -left/-right/-top/-bottom spellings still work) · `caret` (default true) · `dropShadow` (default true) · `highContrast` (default false) · `border` · `open` · `onRequestClose` · `as` (default `span`) · `className`; other attributes go on the container. `PopoverContent` takes `className` (on the surface) and passes other attributes (`id`, `role`, `aria-*`) to its outer span.

**Example**
```jsx
const { Popover, PopoverContent } = window.Carbon;
<Popover open={open} align="bottom-end" onRequestClose={() => setOpen(false)}>
  <button type="button" aria-expanded={open} onClick={() => setOpen(!open)}>Filter</button>
  <PopoverContent><div style={{ padding: 16 }}>Region and status filters</div></PopoverContent>
</Popover>
```

**Rules**
- Content is `layer-01` with `text-primary`, 2px radius, at most 23rem wide; `highContrast` swaps to `background-inverse` / `text-inverse`. Inside a `Layer` the surface follows the next layer token.
- The caret is 12x6px and pushes the content 10px off the trigger; without a caret the offset is 0. With a caret, the caret centre lines up with the trigger centre and start/end alignments shift the content to that side. Use the caret when the trigger has no visible pressed state; drop it when the trigger shows its own open state.
- Shadow is `drop-shadow(0 2px 2px)` in `shadow`, applied as a filter so the caret shares it. `border` adds a 1px `border-subtle-01` outline around content and caret.
- Content is `position: absolute` inside a `position: relative` inline-block container, z-index 6000. Keep the container free of `overflow: hidden` ancestors and choose an alignment that keeps the content on screen (no auto flipping here).
- A transparent bridge fills the gap between trigger and content so hover is not lost crossing it.
- Give the content its own padding; the surface has none. Content scrolls vertically if needed, never horizontally. Show one popover at a time.

**Accessibility**
- Popover sets no roles: the trigger must be a real button with `aria-expanded`, and the content needs a role only if it is one (for example `role="dialog"` with a label for a filter panel).
- Enter or Space on the trigger toggles it (your click handler). `onRequestClose` fires on a mousedown outside and when focus moves out of the container; add Escape handling yourself if the content is interactive.
- Keep interactive content in DOM order right after the trigger (as here) so Tab moves into it naturally.
