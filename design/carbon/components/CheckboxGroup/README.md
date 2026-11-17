# CheckboxGroup

A `<fieldset class="cds--checkbox-group">` that groups Checkboxes under one legend and owns the group's helper, invalid and warning messages.

Hand-written from `packages/styles/scss/components/checkbox` (`cds--checkbox-group*`) and `packages/react/src/components/CheckboxGroup` (Carbon v11.99.0). Left out: the deprecated `slug` prop and AI-label sizing of `decorator`; Carbon passes `invalid` / `readOnly` / `warn` down to each Checkbox, here the group styles them through its own classes instead.

**When to use**
- Several independent options where the user can pick none, one or many: notification channels, alert types, add-ons.
- A parent "select all" checkbox with children (the parent shows indeterminate when only some are checked).

**When not to use**
- Exactly one choice. Use RadioButtonGroup.
- A setting that takes effect immediately. Use Toggle.
- Options that need a description or price. Use SelectableTile.

**You provide:**
- `legendText` (required) and `Carbon.Checkbox` children (each with `id`, `labelText`, `checked`/`defaultChecked`, `indeterminate`).
- `orientation`: `vertical` (default) or `horizontal`.
- `helperText`; `invalid` + `invalidText`; `warn` + `warnText`; `readOnly`; `disabled`.
- `legendId`, `decorator` (a node after the legend, usually an AILabel `size="mini"`), `className`; other attributes go on the fieldset.

**Example**
```jsx
const { CheckboxGroup, Checkbox } = window.Carbon;
<CheckboxGroup legendText="Notify me by" helperText="You can change this later in your profile"
  invalid={!anyChecked} invalidText="Select at least one channel">
  <Checkbox id="ch-email" labelText="Email" defaultChecked />
  <Checkbox id="ch-slack" labelText="Slack" />
</CheckboxGroup>
```

**Rules**
- The legend is `label-01` in `text-secondary`; boxes stack 8px apart, or sit in a wrapping row with 16px (`spacing-05`) between them when horizontal. Prefer vertical; leave 32px before the next form component.
- Invalid: every box border turns `support-error`, and the message shows a 16px `warning--filled` icon in `support-error` and text in `text-error`, indented 8px. Warning uses the warning-alt triangle in `support-warning` with `text-primary` text. Helper text is hidden while either message shows.
- Read-only: boxes get `icon-disabled` borders, checked boxes are transparent with a `text-primary` checkmark, and clicks or Space don't toggle them. Use read-only for values the user may review but not change; disabled for options that do not apply.
- Disabled sets the native fieldset `disabled`: every checkbox, the legend and the helper text use `text-disabled` / `icon-disabled`.
- Write the legend as the question ("Notify me by"); keep labels to a few words, don't repeat the legend in them, and let long labels wrap rather than truncate.
- Select all: the parent is checked when all children are, indeterminate when some are, unchecked when none; indent the children 24px.

**Accessibility**
- The fieldset is labelled by its legend (`aria-labelledby`) and described by the helper text; the invalid or warning message replaces the helper text.
- Tab and Shift+Tab move between checkboxes; Space toggles the focused one. Both the box and its label are click targets.
- Read-only sets `aria-readonly` on the fieldset and keeps the boxes focusable so their state can still be read.
- Skip the legend only when the group sits inside a component that already labels it, and then pass `aria-labelledby`.
