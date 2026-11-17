# DefinitionTooltip

A term in running text with a dotted underline that shows a short definition in a tooltip when focused, clicked or (optionally) hovered.

Hand-written from `packages/styles/scss/components/tooltip/_tooltip.scss` (`definition-tooltip` mixin) and `packages/react/src/components/Tooltip/DefinitionTooltip.tsx` (Carbon v11.99.0), built on Popover. Left out: `autoAlign` (no collision flipping). The deprecated `tooltipText` still works as an alias of `definition`.

**When to use**
- Define a term or abbreviation inside running text, a UI label or a table header ("GB-hours", "egress") without leaving the page.

**When not to use**
- Labels for icon-only controls: use `IconButton` (or `Tooltip`).
- Content with links or buttons: use `Toggletip`.
- Information the user needs to complete the task: write it on the page.

**You provide:** `children` (the term), `definition` (the text shown), optionally `align` (default bottom; top, left, right and their -start/-end variants), `openOnHover`, `defaultOpen`, `id` (of the tooltip), `className` (on the popover container), `triggerClassName`, `onKeyDown`; other attributes go on the trigger button.

**Example**
```jsx
const { DefinitionTooltip } = window.Carbon;
<p>Billing is based on <DefinitionTooltip definition="Memory allocated multiplied by hours running." openOnHover>GB-hours</DefinitionTooltip>.</p>
```

**Rules**
- The term is a `<button>` inheriting the surrounding font, `text-primary`, with a 1px dotted `border-strong-01` underline that turns `border-interactive` on hover and focus.
- Tooltip is high contrast (`background-inverse`, `text-inverse`), no shadow, 8px 16px padding, max 176px wide, `body-01` text, start-aligned even inside right-aligned cells.
- Opens on focus and on click (mouse down toggles); with `openOnHover` also on hover. Escape, blur and mouse leave close it.
- Content: one short sentence in sentence case; do not repeat the term.

**Accessibility**
- The trigger is a native button, so it is in the tab order; it has `aria-describedby` and `aria-controls` pointing at the tooltip and `aria-expanded`.
- Keyboard: Tab to the term opens the definition, Escape closes it without moving focus.
- Keep the definition plain text; it is not focusable.
