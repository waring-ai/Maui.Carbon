# IconButton

A square, icon-only button that always carries a tooltip label. Prefer it over a bare icon `Button` so the action is named for sighted users.

Hand-written from `packages/react/src/components/IconButton` and the icon-only parts of `packages/styles/scss/components/button` (Carbon v11.99.0). Left out: `autoAlign`, `href`/anchor rendering.

**When to use**
- Toolbars and dense layouts where a text label does not fit and the icon is standard and recognizable (download, settings, filter, refresh).
- A toggle such as a view switch or "pin", using `isSelected`.
- Global header actions with a `badgeCount` (notifications).

**When not to use**
- Actions whose icon is not instantly clear, or primary page actions: use a `Button` with text (and optional icon).
- Destructive actions: a danger button must have a visible label.
- Copying a value: use `CopyButton`, which adds the feedback.

**You provide:** `label` (required: the tooltip text and the accessible name) and the icon, either as `renderIcon="settings"` (a name from the Icons group, drawn at 16px) or as an icon element in `children`. Props: `kind` primary (default) | secondary | tertiary | ghost · `size` xs | sm | md | lg (default lg; square 24/32/40/48) · `align` (tooltip side, default top) · `disabled` · `isSelected` · `badgeCount` · `defaultOpen` / `open` · `enterDelayMs`, `leaveDelayMs` (100) · `closeOnActivation` (default true) · `highContrast` (default true), `dropShadow` · `wrapperClasses` (tooltip wrapper), `className`, `type` (default button), `onClick`. Other attributes (`id`, `aria-*`, `data-*`, handlers) go on the `<button>`.

**Example**
```jsx
const { IconButton } = window.Carbon;
<IconButton kind="ghost" label="Export as CSV" renderIcon="download" align="bottom" onClick={exportCsv} />
```

**Rules**
- It reuses the Button classes (`cds--btn`, `--icon-only`), so colors and focus match Button: primary `button-primary`, secondary `button-secondary`, tertiary 1px `button-tertiary` border, ghost transparent with `icon-primary`. Primary and ghost are the common kinds; in toolbars use ghost.
- The tooltip is the icon-tooltip form: dark surface, body-compact-01, 2px/16px padding, 8x4 caret, 8px offset.
- `isSelected` fills the button with `background-selected` and sets `aria-pressed`.
- `badgeCount` adds a BadgeIndicator inside the button (count, or a dot at 0) and wires it to `aria-describedby`; use only with `kind="ghost"` and `size="lg"`. It is hidden when disabled.
- A disabled button still shows the tooltip wrapper but ignores pointer events.
- Use the outline (not filled) icon variant, the same icon for the same action across the product, and a one- or two-word label that names the action.

**Accessibility**
- The tooltip text names the button (`aria-labelledby` to the tooltip), so `label` is required and should describe the action, not the icon ("Download logs", not "Arrow").
- Tooltip opens on hover and keyboard focus and closes on Escape; Enter or Space activates the button.
- When `isSelected` is defined the button exposes `aria-pressed` (true/false) so it reads as a toggle; pass `aria-pressed` yourself to override.
- The badge is announced through `aria-describedby`; for a dot, put the meaning in `label` ("Notifications, new activity") if it matters.
