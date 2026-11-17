# Toggle

Immediately switches a single setting on or off. A 48x24 pill track with an 18px knob; `toggle-off` track when off, `support-success` when on. The `sm` size is 32x16 with a 10px knob and a check mark in the on state.

Hand-written from `packages/styles/scss/components/toggle`. Left out: ToggleSkeleton and the v12 8px label-spacing feature flag (the label sits 16px above the md switch, 8px above sm).

**When to use**
- A binary setting that takes effect as soon as it is switched and is easy to reverse: auto-scaling, public endpoint, email alerts.

**When not to use**
- The change waits for a Save or Submit step, or is one of several options in a form: use a Checkbox.
- More than two options: use a RadioButtonGroup or Dropdown.
- An action with consequences that needs confirmation (delete, restart): use a Button.

**You provide:**
- `labelText`: the setting's name, above the switch (required for md, optional for sm when another visible element names it through `aria-labelledby`).
- `labelA` / `labelB`: state text beside the switch, defaults "Off" / "On"; adjectives, three words or fewer ("Enabled", "Muted").
- `hideLabel`: hides `labelText` above the switch (it stays the accessible name) and shows it beside the switch instead of the state text.
- `toggled` + `onToggle(next)` (controlled) or `defaultToggled`; `onClick(event)` also fires.
- `size` md (default) | sm; `disabled`; `readOnly` (focusable, announced, does not switch; outlined track with a `text-primary` knob and no check mark).
- `id`, `className`; other props (`aria-*`) go to the switch button.

**Example**
```jsx
const { Toggle } = window.Carbon;
<Toggle id="autoscale" labelText="Auto-scaling" labelA="Off" labelB="On"
  toggled={autoscale} onToggle={next => setAutoscale(next)} />
```

**Rules**
- A toggle takes effect immediately; if the change needs a Save step, use a Checkbox instead.
- The state text beside the switch, not the track color, tells the state to people who cannot distinguish green from gray; on md always show it (the sm check mark covers the on state where space is tight).
- Keep `labelText` constant and change only the state text; never rewrite the label to describe the state.
- Track is `radius-toggle` (12px); knob `radius-circle`. Focus is a 2px `focus` outline offset 1px around the track. Disabled uses `button-disabled` and `text-on-color-disabled`.
- `toggle-off` meets 3:1 against `background` in every theme.

**Accessibility**
- A `<button role="switch" aria-checked>`; Tab focuses it, Enter and Space switch it, and clicking the label switches it too.
- It is named by `labelText` through `aria-labelledby` (or your own `aria-labelledby` / `aria-label` when there is no `labelText`); the state text is `aria-hidden` because `aria-checked` already announces on/off.
- `readOnly` sets `aria-readonly` and ignores clicks; `disabled` removes it from the tab order.
