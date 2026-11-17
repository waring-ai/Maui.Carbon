# Checkbox

A 16px box for choosing zero, one or several independent options. Checked and indeterminate fill with `icon-primary` and draw the mark in `icon-inverse`.

Hand-written from `packages/styles/scss/components/checkbox`. Left out: CheckboxSkeleton and the AI label decorator. Groups (legend, group-level helper, invalid, warn, read-only, horizontal) are the CheckboxGroup card.

**When to use**
- Selecting any number of options from a list, filters, batch selection in a DataTable, agreeing to terms.
- A "select all" parent over a set of children (the parent shows indeterminate when only some are checked).

**When not to use**
- Only one option may be chosen: use a RadioButtonGroup.
- The change applies immediately with no Save step: use a Toggle.
- Picking from a long list: use a MultiSelect.

**You provide:**
- `labelText` (required, short, under about three words where possible; long labels wrap under themselves). `hideLabel` keeps it only for screen readers (for example a table row).
- `checked` + `onChange(event, { checked, id })` (controlled) or `defaultChecked` (uncontrolled). `id` is generated if omitted.
- `indeterminate`: mixed state for a parent. You compute it from the children.
- `disabled`; `readOnly` (focusable and announced but cannot be toggled; outlined box with an `icon-primary` mark).
- `helperText`; `invalid` + `invalidText`; `warn` + `warnText` (invalid wins; both ignored while disabled or read-only).
- `title`, `className`; other props (`name`, `value`, `aria-*`) go to the `<input>`.
- Group related checkboxes in a CheckboxGroup (`legendText`, `helperText`, `orientation`).

**Example**
```jsx
const { Checkbox, CheckboxGroup } = window.Carbon;
<CheckboxGroup legendText="Email me when">
  <Checkbox id="ev-done" labelText="Deployment finished" checked={done} onChange={(e, { checked }) => setDone(checked)} />
  <Checkbox id="ev-fail" labelText="Deployment failed" defaultChecked />
</CheckboxGroup>
```

**Rules**
- Box: 1px `icon-primary` border, 2px radius (`radius-checkbox`), 10px between box and label. Focus is a 2px `focus` outline offset 1px from the box.
- Label is `body-compact-01` in `text-primary`; disabled uses `text-disabled` and `icon-disabled`; read-only uses an `icon-disabled` border with the mark in `icon-primary`.
- Invalid draws the box border in `support-error` with a `warning--filled` icon and `text-error` message; warn uses a `support-warning` triangle and a `text-primary` message.
- Stack checkboxes vertically, 8px apart; labels sit to the right of the box. Never truncate a label.
- A parent "select all" is checked when all children are, indeterminate when some are, unchecked when none are; checking it checks every child.

**Accessibility**
- A native `<input type="checkbox">` with a `<label for>`: Tab focuses, Space toggles, clicking the label toggles.
- Indeterminate sets the native `indeterminate` flag and `aria-checked="mixed"`.
- `readOnly` sets `aria-readonly` and cancels clicks and Space; invalid sets `aria-invalid`. Helper text or the message is linked with `aria-describedby`.
- Always pass a `labelText`, even when `hideLabel` is set; in a group, the CheckboxGroup `<fieldset>` / `<legend>` names the set.
