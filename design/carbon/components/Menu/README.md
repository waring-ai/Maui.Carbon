# Menu

A floating list of actions: items, dividers, groups, selectable items, radio groups and nested submenus. The same implementation powers MenuButton, OverflowMenu and ComboButton.

Hand-written from `packages/styles/scss/components/menu` and `packages/react/src/components/Menu` (Carbon v11.99.0). Left out: floating-ui auto-flip/collision handling, rendering into a portal, `useContextMenu`/`ContextMenu`, typeahead, RTL mirroring, `legacyAutoalign`, `mode`.

**When to use**
- Hide less frequent or advanced actions to keep a surface clean, and for right-click context menus on a selection (a row, an object).
- Offer shortcuts and settings that experienced users toggle often (selectable items, a radio group of sort orders).
- Attached to a trigger, use `MenuButton` (equal actions), `ComboButton` (one main action plus alternatives) or `OverflowMenu` (row and card actions) rather than positioning a `Menu` yourself.

**When not to use**
- To pick a form value or filter a list: use a `Dropdown`, `ComboBox` or `MultiSelect`.
- For many or complex inputs (checkbox groups, fields): use a `Popover` or a `Toggletip`.

**You provide:**
- `Menu`: `label` (required, aria-label) · `open` (default false; a closed root menu renders nothing) · `onClose` (Escape, item chosen, focus leaves) · `onOpen` · `size` `xs` | `sm` | `md` | `lg` (24/32/40/48px items, default `sm`; submenus inherit it) · `x`, `y` (px from the nearest positioned ancestor, or `[x1, x2]` ranges of which the first value is used) · `fixed` (viewport coordinates) · `menuAlignment` `top` | `top-start` | `top-end` | `bottom` | `bottom-start` | `bottom-end` (place it against a `position: relative` wrapper) · `border` · `backgroundToken` `layer` (default) | `background` · `containerRef`, `returnFocusRef` · `id`, `className`, `style`.
- `MenuItem`: `label` (required) · `onClick` · `shortcut` (text at the end) · `renderIcon` (icon file stem such as `"edit"`, an element or a component) · `kind` `default` | `danger` · `disabled` · `children` (MenuItems, makes it a submenu) · `defaultSubmenuOpen` (documentation aid, not in Carbon).
- `MenuItemSelectable`: `label`, `selected` / `defaultSelected` (false), `onChange(checked)`, `disabled`, `shortcut`, `renderIcon`.
- `MenuItemRadioGroup`: `label` (aria-label), `items`, `itemToString` (default: the string or `item.label`), `selectedItem` / `defaultSelectedItem`, `onChange(item)`.
- `MenuItemGroup` (`label`, MenuItem children) and `MenuItemDivider`.

**Example**
```jsx
const { Menu, MenuItem, MenuItemDivider } = window.Carbon;
<div style={{ position: 'relative' }} onContextMenu={openAt}>
  <Menu label="Object actions" open={open} x={pos.x} y={pos.y} onClose={() => setOpen(false)}>
    <MenuItem label="Download" renderIcon="download" shortcut="Ctrl+D" onClick={download} />
    <MenuItem label="Share link" renderIcon="share" onClick={share} />
    <MenuItemDivider />
    <MenuItem label="Delete object" kind="danger" onClick={confirmDelete} />
  </Menu>
</div>
```

**Rules**
- Surface is `layer-01` with a `0 2px 6px var(--shadow)` shadow and 4px block padding; width fits the labels between 10rem and 18rem (12rem minimum when any item has an icon), longer labels truncate with an ellipsis and keep the full text as `title`.
- Item heights match the trigger: xs 24, sm 32, md 40, lg 48px. xs is for dense rows only, not for buttons.
- Items use `text-secondary` and switch to `text-primary` on `layer-hover-01`; disabled items use `text-disabled` and keep a plain `layer-01` background on hover. Hide actions a user can never perform instead of disabling them.
- `kind="danger"` items turn `button-danger-primary` with `text-on-color` on hover and focus. Use it only for destructive actions, last in the list after a divider.
- Selectable and radio items show a 16px checkmark in a leading column; icons get their own 16px column.
- Order items by frequency or by the sequence of the task; group long lists with `MenuItemDivider`. Keep context and overflow menus to about 12 items and menu buttons to fewer than 5. Use one level of submenu at most.
- Choosing an item closes the root menu (via `onClose`), including selectable and radio items.

**Accessibility**
- Roles: `menu` (with `aria-label` from `label`), `menuitem`, `menuitemcheckbox` and `menuitemradio` with `aria-checked`, `group` with `aria-label`, `separator`. Disabled items get `aria-disabled`; submenu items get `aria-haspopup="menu"` and `aria-expanded`.
- Keyboard: ArrowUp/ArrowDown move and wrap (skipping disabled items), Home/End jump, Enter or Space activates, ArrowRight opens a submenu and focuses its first item, ArrowLeft or Escape closes it and refocuses the parent item, Escape on the root calls `onClose` and returns focus to `returnFocusRef` (or the element focused before opening). Items are not tab stops (`tabindex="-1"`); Tab leaves and closes the menu.
- Opening a root menu after first render focuses its first item. Submenus also open on hover after 100ms.
- Focus is a 2px inset `focus` outline on the item. Group labels are accessible names only; they are not drawn.
