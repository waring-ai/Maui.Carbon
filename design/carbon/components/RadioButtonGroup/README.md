# RadioButtonGroup

A set of radio buttons (`RadioButtonGroup` plus `RadioButton`) for choosing exactly one of a few visible options.

Hand-written from `packages/styles/scss/components/radio-button` and `packages/react/src/components/{RadioButtonGroup,RadioButton}` (Carbon v11.99.0). `RadioButton` and `RadioButtonSkeleton` are exported alongside. Left out: AI label / decorator slot, per-radio `invalid` / `warn` messages, and the deprecated top / bottom label positions.

**When to use**
- Mutually exclusive choices where seeing every option helps: a plan, a cluster type, an update policy. Works in forms, side panels, modals and as a filter.

**When not to use**
- Several options can be chosen: use a CheckboxGroup.
- An on / off setting: use a Toggle (or a single Checkbox in a form).
- Long lists or little space: use a Select or Dropdown.
- Options that need prices, descriptions or links: use RadioTile.

**You provide:**
- `name` (required) and `RadioButton` children, each with `value` and `labelText` (and an `id`, auto-generated if omitted).
- `legendText`: the group label, rendered as the `<legend>` of a `<fieldset>`.
- `valueSelected` (controlled) or `defaultSelected`; `onChange(selection, name, event)`.
- `orientation` `horizontal` (default) | `vertical`; `labelPosition` `right` (default) | `left`.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly` (selection cannot change), `required`. `invalid` and `warn` are ignored while disabled or read only; the helper text is hidden while invalid, warning or disabled.
- `className`.
- `RadioButton` on its own: `labelText`, `value`, `name`, `checked` / `defaultChecked`, `onChange(value, name, event)`, `labelPosition`, `hideLabel`, `disabled`, `readOnly`, `required`, `id`, `className`.
- `RadioButtonSkeleton`: `className` (one circle and label bar; stack several).

**Example**
```jsx
const { RadioButtonGroup, RadioButton } = window.Carbon;
<RadioButtonGroup legendText="Cluster type" name="cluster-type" orientation="vertical"
  valueSelected={type} onChange={(value) => setType(value)}>
  <RadioButton id="ct-single" value="single" labelText="Single zone" />
  <RadioButton id="ct-multi" value="multi" labelText="Multizone" />
</RadioButtonGroup>
```

**Rules**
- The circle is 18px with a 1px `icon-primary` ring and an `icon-primary` dot at half size when selected; focus is a 2px `focus` outline around the circle.
- Invalid turns the rings `support-error` and shows the error icon and a `text-error` message under the group; warning shows the `support-warning` icon and a `text-primary` message. Read only uses an `icon-disabled` ring; disabled uses `icon-disabled` and `text-disabled`.
- Horizontal radios sit 16px apart; vertical radios stack with a small gap. Prefer vertical: it scans faster, and long labels wrap under themselves, top-aligned.
- Label text is `body-compact-01` in `text-primary`; helper text is `text-helper`.
- Copy: labels under about three words, sentence case, never truncated. Nothing is preselected unless you pass a value; preselect only a safe default, and add a "None" option when people need a way out.

**Accessibility**
- A `<fieldset>` with a `<legend>` names the group; each radio is a native `<input type="radio">` linked to its label.
- The group is one tab stop: arrow keys move and select, Tab leaves (native behavior). With nothing selected, Tab lands on the first radio.
- `aria-describedby` on the fieldset points at the helper text or the error / warning message; `required` is set on every radio; read only sets `aria-readonly` on the fieldset.
