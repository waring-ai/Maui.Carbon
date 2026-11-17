# ContentSwitcher

A segmented control for switching between two or more alternate views of the same content; one switch is always selected. Exports `ContentSwitcher`, `Switch` and `IconSwitch`.

Hand-written from `packages/styles/scss/components/content-switcher` (Carbon v11.99.0). `IconSwitch` uses the native `title` as its tooltip instead of Carbon's IconButton tooltip, and `lowContrast` is drawn with layer tokens (the `content-switcher-*` tokens are not in the token set). Left out: the deprecated `light` prop and a container-level `disabled` (disable every `Switch` instead).

**When to use**
- Toggle between views of the same or closely related content, one view at a time: chart and table, grid and list, or narrowing a set (All, Read, Unread; Last hour, 24 hours, 7 days).
- Often placed inside a tab to sort content one level further down.

**When not to use**
- To move between distinct sections or subpages: use `Tabs`.
- For an on/off or yes/no setting: use a `Toggle`. For an action: use a `Button`. To pick a form value: use a `RadioButtonGroup` or `Dropdown`.

**You provide:**
- `ContentSwitcher`: `size` `sm` | `md` | `lg` (default `md`) · `selectedIndex` (default 0; re-applied whenever it changes) · `onChange({ index, name, text })` · `selectionMode` `automatic` (default) | `manual` · `lowContrast` (default false) · `className`, `aria-label`.
- `Switch` children: `text` (the label; falls back to children) · `name` (returned in `onChange`) · `disabled`.
- Or all `IconSwitch` children for the icon-only variant: `text` (required: accessible name and tooltip) · `renderIcon` (16px icon file stem, an element as children, or a component) · `name` · `disabled`. Do not mix `Switch` and `IconSwitch`.

**Example**
```jsx
const { ContentSwitcher, Switch } = window.Carbon;
<ContentSwitcher size="sm" selectedIndex={range} onChange={(e) => setRange(e.index)} aria-label="Time range">
  <Switch name="1h" text="Last hour" />
  <Switch name="24h" text="24 hours" />
  <Switch name="7d" text="7 days" />
</ContentSwitcher>
```

**Rules**
- Heights are 32px (`sm`), 40px (`md`) and 48px (`lg`); the container has a 4px radius and a 1px `border-inverse` outline, and text switches share the width equally with 16px inline padding (14px at `lg`). Icon switches are squares of the control height and the group is only as wide as its switches.
- Unselected labels are `text-secondary` on transparent; hover and press use `layer-hover-01` with `text-primary`. A 1px `border-subtle-01` divider separates unselected neighbours and disappears next to the selected, hovered or focused switch.
- High contrast (default): the selected switch fills with `layer-selected-inverse` and uses `text-inverse` / `icon-inverse`, so it inverts in every theme; use it for top-level placement such as page headers. `lowContrast`: no outline, a `layer-01` track and the selected switch on `layer-02` with a `border-subtle-01` edge and `text-primary`; use it in cards, dialogs and next to primary or secondary buttons. When unsure, use high contrast.
- Focus is a 2px `focus` ring with a 1px `focus-inset` ring inside the switch.
- A disabled switch uses `text-disabled` and `border-disabled`; if the selected switch is disabled it fills with `button-disabled`. Any disabled switch turns the container outline `border-disabled`.
- Place the switcher above the content it controls, aligned left or right, never centered. The first switch is selected by default.
- Labels are two or three word nouns, not verbs (they must not read as buttons). Long labels truncate with an ellipsis and keep the full text in `title`; if labels still do not fit, use the icon-only variant.

**Accessibility**
- It is a `tablist` of `button`s with `role="tab"`; the selected switch has `aria-selected="true"` and `tabindex="0"`, the others `aria-selected="false"` and `tabindex="-1"`, so the switcher is a single Tab stop. Pass `aria-label` on the `ContentSwitcher` to name the group.
- Keyboard: Left and Right Arrow move focus, skip disabled switches and wrap; in `automatic` mode they also select, in `manual` mode Enter or Space selects the focused switch. Clicking anywhere on a switch selects it.
- `IconSwitch` sets `aria-label` and `title` from `text`; always provide it.
