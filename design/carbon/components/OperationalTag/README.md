# OperationalTag

A clickable tag that triggers an action, such as opening a popover with more detail or the full list behind a "+3" tag.

Hand-written from `packages/styles/scss/components/tag` and `packages/react/src/components/Tag/OperationalTag.tsx` (Carbon v11.99.0). `renderIcon` takes an icon file stem.

**When to use**
- Reveal related tags or detail in place: a "+3" overflow tag that opens a `Popover`, a tag that opens a `Modal` with the full list.

**When not to use**
- Navigation to another page or tab: use a `Link`.
- Removing a value: use `DismissibleTag`. Toggling a choice: use `SelectableTag`. Plain labels: use `Tag`.

**You provide:** `text`, `onClick`, and optionally `type` (gray default, red, magenta, purple, blue, cyan, teal, green, cool-gray, warm-gray; no high-contrast or outline), `size` sm | md | lg (18/24/32px, default md), `disabled`, `renderIcon` (16px icon before the label, hidden at sm), `id`, `className`. Other attributes (`aria-expanded`, `aria-controls`, `data-*`) go on the button.

**Example**
```jsx
const { Popover, PopoverContent, OperationalTag } = window.Carbon;
<Popover open={open} align="bottom-start" onRequestClose={() => setOpen(false)}>
  <OperationalTag text="+3" aria-expanded={open} onClick={() => setOpen(!open)} />
  <PopoverContent>…all labels…</PopoverContent>
</Popover>
```

**Rules**
- Renders a `<button>` with a 1px `tag-border-*` border on `tag-background-*` and `tag-color-*` text; hover switches the fill to `tag-hover-*`.
- Focus is a 2px `focus` outline offset 1px.
- Disabled: 1px `border-disabled` border, `layer-01` fill, `text-disabled`, `not-allowed` cursor, no hover change.
- lg tags have 16px side padding; sm and md have 8px. Max width 13rem. Label text in `label-01` (12px/16px, 0.32px).
- When the label is truncated, a bottom tooltip shows the full text.

**Accessibility**
- It is a native button: Tab focuses it, Enter or Space runs `onClick`.
- When it opens something, pass `aria-expanded` (and `aria-controls`) so the state is announced, and keep the visible text in the accessible name.
- Disabled tags are not operable and are exempt from contrast requirements.
