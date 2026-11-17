# OverflowMenu

An icon-only button (three dots) that opens a Menu of secondary actions for a row, card or toolbar.

Hand-written from `packages/styles/scss/components/overflow-menu` and `packages/react/src/components/OverflowMenu` (Carbon v11.99.0). The popup is the shared Menu (as in Carbon's menu-based OverflowMenu) rather than the legacy `overflow-menu-options` list, and the tooltip is replaced with a `title` attribute. Left out: floating-ui placement and `autoAlign`, `menuOffset`, `menuOptionsClass`, `light`, tooltip `align`/`tooltipAlignment`.

**When to use**
- A table row, card or tile has more actions than fit, typically more than three icon buttons, and the actions affect only that item.
- Inside a `Breadcrumb` (as a `BreadcrumbItem` child) to hold the hidden middle levels of a long trail.

**When not to use**
- For the only action in a context: show a `Button` or `IconButton` instead.
- For page-level actions: use a `MenuButton` or `ComboButton`. For checkboxes, radio buttons or fields in a table toolbar: use a `Popover`.

**You provide:**
- `MenuItem` / `MenuItemDivider` children and `iconDescription` (accessible name and `title`, default "Options"; `label` and `aria-label` are accepted as aliases). Name it after the object: "Actions for ci-deploy".
- `size` `xs` | `sm` | `md` | `lg` (default `md`; the trigger is a 24/32/40/48px square and the menu items use the same height) · `renderIcon` `"overflow-menu--vertical"` (default) | `"overflow-menu--horizontal"` | an element or component · `flipped` (align the menu's end edge to the trigger's end edge) · `direction` `bottom` (default) | `top` · `disabled`.
- `open` (controlled) or `defaultOpen`, `onOpen`, `onClose` · `className`, `style`.

**Example**
```jsx
const { OverflowMenu, MenuItem, MenuItemDivider } = window.Carbon;
<OverflowMenu size="lg" flipped iconDescription="Actions for ci-deploy">
  <MenuItem label="Rename" onClick={rename} />
  <MenuItem label="Rotate key" onClick={rotate} />
  <MenuItemDivider />
  <MenuItem label="Delete API key" kind="danger" onClick={confirmDelete} />
</OverflowMenu>
```

**Rules**
- Trigger is transparent with an `icon-primary` glyph; hover is `background-hover`. While open it takes `layer-01` and the menu shadow so it reads as part of the menu. Ghost style only; keep the standard three-dot icon.
- Match the size to the row: `sm` for 32px rows, `md` for 40px, `lg` for 48px rows. Use `flipped` when the trigger sits at the end of a row or card so the menu stays inside it.
- Focus is a 2px `focus` outline inset on the trigger. Disabled uses `icon-disabled` and `cursor: not-allowed`.
- Keep the menu to about 12 items; order by frequency, put destructive items last after a divider with `kind="danger"`, and disable (rather than hide) actions that are only temporarily unavailable.

**Accessibility**
- The trigger is a `button` with `aria-label` and `title` from `iconDescription`, `aria-haspopup="menu"`, `aria-expanded` and, while open, `aria-controls`; the menu is labelled with the same text.
- Keyboard: one Tab stop. Enter or Space opens the menu and focuses the first item; ArrowUp/ArrowDown move, Enter or Space activates and closes, Escape closes and returns focus to the trigger. Click outside or Tab away also closes.
- Each instance needs a unique `iconDescription` when several sit on one page (one per table row).
