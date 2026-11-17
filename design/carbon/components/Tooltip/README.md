# Tooltip

A short text label that appears on hover or keyboard focus of its trigger. Use for non-interactive, plain-text help; use Toggletip when the content has links or buttons.

Hand-written from `packages/styles/scss/components/tooltip` and `packages/react/src/components/Tooltip` (Carbon v11.99.0), built on this bundle's own Popover CSS. Left out: `autoAlign` and drag handling. The definition-tooltip variant is the separate `DefinitionTooltip` card.

**When to use**
- Name a control that has no visible label, such as an icon-only button (prefer `IconButton`, which includes one).
- Add brief, non-essential context to a focusable element, or show the full text of a truncated value.

**When not to use**
- Critical or task-essential information: write it as visible helper text instead, since a tooltip is not persistent.
- Links, buttons or other interactive content: use `Toggletip`.
- Defining a term inside running text: use `DefinitionTooltip`.

**You provide:** exactly one focusable child as the trigger, and `label` (names the trigger via `aria-labelledby`) or `description` (describes it via `aria-describedby`). Optional `align` (default top; any Popover alignment), `enterDelayMs` (100) and `leaveDelayMs` (300), `defaultOpen` to start open, `open` to force it, `closeOnActivation` (default false), `highContrast` (default true), `dropShadow` (default false), `id` (of the content), `className`.

**Example**
```jsx
const { Tooltip } = window.Carbon;
<Tooltip label="Restart node" align="bottom">
  <button type="button" className="cds--btn cds--btn--ghost cds--btn--md cds--btn--icon-only">…</button>
</Tooltip>
```

**Rules**
- Opens on mouse enter (after the enter delay) and on keyboard focus; stays open while the pointer is over the trigger or the content; closes on leave, blur and Escape. Focus gained by a mouse press does not open it.
- Default surface is `background-inverse` with `text-inverse`; body-01 type, 16px padding, 288px max width (longer text wraps), 12px offset, 12x6 caret.
- Align defaults to `top`; choose the side and start/center/end alignment that keeps it on screen and off key content. The tooltip is inline (no portal), so avoid `overflow: hidden` ancestors.
- Content: brief and specific. One or two words to name an icon-only control ("Download logs"); full sentences in sentence case for descriptions. Do not repeat the trigger's visible label.

**Accessibility**
- The content has `role="tooltip"` and an id; the trigger gets `aria-labelledby` (with `label`) or `aria-describedby` (with `description`) pointing at it. `aria-hidden` is set on the content while closed.
- The trigger must be keyboard focusable (a button, link or `tabIndex={0}` element); Tab shows the tooltip, Escape dismisses it without moving focus.
- Keep the content plain text: it is never focusable, so anything interactive in it is unreachable.
