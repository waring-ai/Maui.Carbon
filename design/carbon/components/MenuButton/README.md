# MenuButton

A button with a chevron that opens a Menu of related actions beneath it.

Hand-written from `packages/styles/scss/components/menu-button` and `packages/react/src/components/MenuButton` (Carbon v11.99.0). Left out: floating-ui flip/auto-placement, `menuTarget` portals, the `tabIndex` prop. The container is `display: inline-block; position: relative` here (Carbon uses `display: contents` plus a fixed-position popup).

**When to use**
- A page header or toolbar has several distinct actions of equal weight that affect the whole page or a large object (a cluster, a canvas, a whole data table), and there is no room to show them all as buttons.
- Use a `ComboButton` instead when one of the actions is clearly the most common; use an `OverflowMenu` for actions on a single row or card.

**When not to use**
- For one or two actions: show `Button`s. To choose a value in a form, use a `Dropdown`.
- Do not repeat a shared verb in the label and every item ("Create" with "Create cluster", "Create bucket"); label the button with the verb's object or use "Actions".

**You provide:**
- `label` (required; button text and the menu's accessible name) and `MenuItem` / `MenuItemDivider` / `MenuItemGroup` children.
- `kind` `primary` (default) | `tertiary` | `ghost` (`secondary` also renders, but Carbon documents only the other three) · `size` `xs` | `sm` | `md` | `lg` (default `lg`; 24/32/40/48px, the menu items use the same height) · `menuAlignment` `bottom` (default) | `bottom-start` | `bottom-end` | `top` | `top-start` | `top-end` · `disabled`.
- `open` (controlled) or `defaultOpen`, `onOpen`, `onClose` · `menuBorder`, `menuBackgroundToken` `layer` | `background` · `className`, `style`.

**Example**
```jsx
const { MenuButton, MenuItem, MenuItemDivider } = window.Carbon;
<MenuButton label="Actions" kind="tertiary" size="md">
  <MenuItem label="Restart cluster" onClick={restart} />
  <MenuItem label="Scale worker pool" onClick={openScaleDialog} />
  <MenuItemDivider />
  <MenuItem label="Delete cluster" kind="danger" onClick={confirmDelete} />
</MenuButton>
```

**Rules**
- Uses the Button classes (`button-primary`, `button-tertiary`, ghost uses `link-primary`) with the 16px chevron in the icon slot; it rotates 180deg while open (`duration-fast-02`).
- Non-ghost triggers are at least 12rem wide; ghost triggers keep their natural width so the chevron stays next to the label.
- With `bottom` or `top` the menu is never narrower than the trigger and grows to fit longer labels (up to 18rem); the `-start`/`-end` alignments use the menu's own width (10rem to 18rem) aligned to that edge of the button.
- Labels name the action or say that the button opens a list ("Actions", "Export"); never "Menu". Keep fewer than 5 items; put destructive items last, after a divider, with `kind="danger"`.
- Menu items follow the Menu rules (layer-01 surface, danger items in `button-danger-primary`).

**Accessibility**
- The trigger is a `button` with `aria-haspopup="menu"`, `aria-expanded` and, while open, `aria-controls` pointing to the `menu`, whose accessible name is the `label`.
- Keyboard: Tab reaches the button, Enter or Space opens the menu and focuses its first item, ArrowUp/ArrowDown move, Enter or Space activates an item and closes the menu, Escape closes it and returns focus to the button. Clicking outside or tabbing away closes it.
- Disabled uses the native `disabled` attribute, so the button leaves the tab order.
