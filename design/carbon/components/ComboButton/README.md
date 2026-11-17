# ComboButton

A split button: a primary action beside a chevron button that opens a Menu of related actions.

Hand-written from `packages/styles/scss/components/combo-button` and `packages/react/src/components/ComboButton` (Carbon v11.99.0). Left out: floating-ui flip/hide handling, `translateWithId`, and the tooltip on the chevron (it uses a `title` attribute). Only the primary button kind exists, as in Carbon.

**When to use**
- A page header has one action that is used far more often than its alternatives (Deploy, with Deploy to staging and Schedule deployment in the menu) and space is limited.

**When not to use**
- When all actions are equally important: use a `MenuButton`. For row or card actions: use an `OverflowMenu`.
- When there is room to show two or three `Button`s, show them; a ComboButton hides the alternatives.

**You provide:**
- `label` (required) and `onClick` for the primary action; `MenuItem` / `MenuItemDivider` children for the additional actions.
- `size` `sm` | `md` | `lg` (default `lg`; 32/40/48px, the menu items use the same height; `xs` also renders at 24px but is not a Carbon size) · `menuAlignment` `bottom` (default) | `bottom-start` | `bottom-end` | `top` | `top-start` | `top-end` · `disabled`.
- `open` (controlled) or `defaultOpen`, `onOpen`, `onClose` · `className`, `style`.

**Example**
```jsx
const { ComboButton, MenuItem, MenuItemDivider } = window.Carbon;
<ComboButton label="Deploy" size="md" onClick={deploy}>
  <MenuItem label="Deploy to staging" onClick={deployStaging} />
  <MenuItem label="Schedule deployment" onClick={openScheduler} />
  <MenuItemDivider />
  <MenuItem label="Roll back to v2.13.0" kind="danger" onClick={confirmRollback} />
</ComboButton>
```

**Rules**
- Both halves use `button-primary` (hover/active tokens as Button) with `text-on-color`; a 1px gap shows the page background between them as the separator.
- The primary action is 10rem wide in total at minimum (label button min 111px, max 239px, label truncated with an ellipsis); the chevron button is a square of the chosen size and its icon rotates 180deg while open.
- With `bottom`/`top` the menu is at least as wide as the two buttons together and grows to fit its labels (up to 18rem).
- The primary action must be the most common choice and must not repeat in the menu; use `kind="danger"` only for destructive alternatives.
- Disabled disables both halves.

**Accessibility**
- Two Tab stops: the primary button, then the chevron button. Enter or Space runs the primary action or opens the menu.
- The chevron button has `aria-label` and `title` "Additional actions", `aria-haspopup="menu"`, `aria-expanded` and, while open, `aria-controls` pointing to the menu (labelled "Additional actions").
- Keyboard and focus inside the menu are those of Menu: the first item takes focus on open, Escape returns focus to the chevron button.
