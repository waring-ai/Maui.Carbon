# FluidNumberInput

A numeric field with the label inside a 64px field and the decrement / increment steppers at the right, for fluid forms.

Hand-written from `packages/styles/scss/components/fluid-number-input` (on top of `number-input`) and `packages/react/src/components/FluidNumberInput` / `NumberInput` (Carbon v11.99.0). `FluidNumberInputSkeleton` is the loading placeholder. Left out: `type="text"` locale formatting and `validate`, AI label / decorator slot, `disableWheel`, toggletip label.

**When to use**
- A bounded number that users often nudge up or down (replicas, workers per zone, vCPU) inside a FluidForm.
- `hideSteppers` for numbers that are typed rather than stepped (ports, IDs with digits only).

**When not to use**
- Stacked or complex forms: use NumberInput. A wide range picked visually: use Slider. Values that are not really numbers (phone numbers, ZIP codes, account IDs with leading zeros): use FluidTextInput.

**You provide:**
- `label` (or the alias `labelText`), `id` (auto-generated), `name`, `placeholder`.
- Value: controlled `value` + `onChange(event, { value, direction })` or uncontrolled `defaultValue` (default `0`, or `''` with `allowEmpty`).
- `min`, `max`, `step` (default 1; decimals are kept to the step's precision). Stepping clamps to min/max; a typed value outside them is flagged invalid.
- `allowEmpty` (default false: an empty field is invalid), `hideSteppers`.
- States: `invalid` + `invalidText` (default "Number is not valid"), `warn` + `warnText`, `disabled`, `readOnly`.
- `incrementNumLabel` (default "Increment number"), `decrementNumLabel` (default "Decrement number"), `onClick(event, { value, direction })` after a stepper click, `onFocus`, `onBlur`, `onKeyDown`, `className`.

**Example**
```jsx
const { FluidNumberInput } = window.Carbon;
<FluidNumberInput id="workers" label="Workers per zone" value={n} min={1} max={20}
  onChange={(e, { value }) => setN(value)} invalidText="Enter 1 to 20 workers per zone" />
```

**Rules**
- Field: `field-01`, input padding 32px 80px 13px 16px, min height 64px, 1px `border-strong-01` bottom rule. Steppers are 40px tall starting 23px from the top, divided by a 1px `border-subtle-01` rule; their icons use `icon-primary` (`icon-disabled` when read-only or disabled).
- Focus: a 2px inset `focus` outline around the whole field while the input is focused.
- Invalid: 2px `support-error` outline, `border-subtle-01` divider at 63px, message row in `text-error` with the error icon 73px from the top / 16px from the right. Say the allowed range in the message ("Enter a value from 1 to 20").
- Warning: same layout in `text-primary` with the `support-warning` icon; no outline.
- Read only: transparent input, `border-subtle-01` rule, steppers inert. Disabled: `text-disabled`, `border-subtle-01` bottom rule. Invalid/warn are ignored in both.
- Put the unit in the label ("CPU per replica (vCPU)") since there is no helper text.

**Accessibility**
- Native `<input type="number">` tied to its `<label>`: ArrowUp/ArrowDown step by `step`, and `min`/`max`/`step` are exposed to assistive tech.
- The stepper buttons have `aria-label`s and `tabIndex=-1`, as in Carbon; keyboard users step with the arrow keys.
- Invalid sets `aria-invalid`; `aria-describedby` points at the error or warning text. Read only sets `aria-readonly` and stays focusable.
