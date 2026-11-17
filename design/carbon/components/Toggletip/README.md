# Toggletip

An information bubble that opens when its button is clicked and can hold interactive content (links, buttons). Exports `Toggletip`, `ToggletipButton`, `ToggletipContent`, `ToggletipActions`.

Hand-written from `packages/styles/scss/components/toggletip` and `packages/react/src/components/Toggletip` (Carbon v11.99.0). Left out: `ToggletipLabel` (use a plain span in `body-01` / `text-secondary`), `autoAlign` and RTL.

**When to use**
- Supplementary help next to a field label, table header or metric that may need a link ("Learn more") or a small action.
- Quick, in-context content that the user opens on purpose and that must stay open while they read or tab through it.

**When not to use**
- A plain hover/focus label: use `Tooltip` (or `IconButton`).
- Critical information or input the user needs to finish a task: put it on the page or in a `Modal`.
- A custom floating panel such as a filter: build it on `Popover`.

**You provide:** `<Toggletip>` containing `<ToggletipButton label="…"/>` and `<ToggletipContent>`. Content is usually `<p>` text and optionally `<ToggletipActions>` holding a `Link` on the start and a small `Button` on the end. `ToggletipButton` renders the 16px information icon unless you pass children; its `label` (default `Show information`) is the accessible name. Props on `Toggletip`: `align` (default bottom; any Popover alignment), `defaultOpen`, `as`, `className`. `ToggletipContent` and `ToggletipActions` take `className`.

**Example**
```jsx
const { Toggletip, ToggletipButton, ToggletipContent, ToggletipActions, Link, Button } = window.Carbon;
<Toggletip align="bottom-start">
  <ToggletipButton label="Show information about the upgrade window" />
  <ToggletipContent>
    <p>Control plane and node upgrades start only inside this window.</p>
    <ToggletipActions><Link href="/docs/upgrades">Learn more</Link><Button size="sm">Edit window</Button></ToggletipActions>
  </ToggletipContent>
</Toggletip>
```

**Rules**
- The button icon is `icon-secondary`, becoming `icon-primary` on hover and while open. Focus is a 2px `focus` outline.
- Surface is high contrast (`background-inverse`, `text-inverse`), 16px padding and row gap, 288px (18rem) max width, 13px offset with caret. Links inside use `link-inverse`; a button inside gets an inverse focus ring.
- Content: one to three short sentences; a link that adds context for the current task, at most one button. Never long text, forms or anything required.
- Pick an alignment that keeps the bubble on screen; it is inline (no portal), so avoid `overflow: hidden` ancestors.

**Accessibility**
- `ToggletipButton` is a real button with `aria-expanded`, `aria-controls` and, while open, `aria-describedby` pointing at the content. Give each one a specific `label` when several share a view.
- Enter or Space toggles it; Escape closes it and returns focus to the button. A click outside and focus leaving the bubble also close it.
- The content follows the button in DOM order, so Tab moves from the button into links and buttons inside, in a logical order.
