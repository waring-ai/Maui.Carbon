# NumberInput

A numeric field with decrement and increment steppers, constrained by `min`, `max` and `step`.

Hand-written from `packages/styles/scss/components/number-input` and `packages/react/src/components/NumberInput` (Carbon v11.99.0). `NumberInputSkeleton` is exported alongside. Left out: `type="text"` locale formatting and parsing (`locale`, `formatOptions`), `validate`, `disableWheel`, fluid variant (see FluidNumberInput), AI label / decorator slot.

**When to use**
- A whole or decimal number that users usually nudge by small amounts: replicas, worker nodes, a timeout, a CPU limit.

**When not to use**
- Use a Slider when users pick a position in a wide range and the exact value matters less.
- Use a TextInput for long or exact numbers that are typed, not stepped (prices, account IDs, phone numbers).

**You provide:**
- `label` (or its alias `labelText`; `hideLabel` keeps it for screen readers only) and `id` (auto-generated if omitted).
- `min`, `max`, `step` (default 1) and either `value` + `onChange` or `defaultValue` (default 0, or empty with `allowEmpty`). `onChange(event, { value, direction })` fires for typing and for stepper clicks; `onClick(event, { value, direction })` fires for stepper clicks only. Steppers clamp to `min`/`max` and keep the decimal precision of `step` and the current value.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px).
- States: `helperText`, `invalid` + `invalidText` (default "Number is not valid"), `warn` + `warnText`, `disabled`, `readOnly`. A value outside `min`/`max`, or an empty field without `allowEmpty`, is invalid automatically. `invalid` and `warn` are ignored while disabled or read only.
- `hideSteppers`, `allowEmpty`, `incrementNumLabel` / `decrementNumLabel` (stepper names, default "Increment number" / "Decrement number").
- Also passed through: `name`, `placeholder`, `onBlur`, `onKeyDown`, `onKeyUp`, `className`.
- `NumberInputSkeleton`: `hideLabel`, `className`.

**Example**
```jsx
const { NumberInput } = window.Carbon;
<NumberInput
  id="worker-count" label="Worker nodes" min={1} max={10} value={workers}
  helperText="Between 1 and 10" invalidText="Enter a number from 1 to 10"
  onChange={(e, { value }) => setWorkers(value)}
/>
```

**Rules**
- Fill is `field-01` (`field-02` inside a Layer), 1px `border-strong-01` underline, focus a 2px `focus` outline; the stepper buttons take a `focus` top and bottom border while the input is focused.
- Invalid: `support-error` outline and icon, `text-error` message. Warning: `support-warning` icon, `text-primary` message. They replace the helper text.
- Steppers are as wide as the field is tall (32 / 40 / 48px), separated by a 1px `border-subtle-01` divider.
- Read only dims the steppers to `icon-disabled` and drops the fill; disabled uses `text-disabled` and `icon-disabled`.
- Copy: always set a sensible default value (usually 0 or 1) and state the allowed range or unit in the label or `helperText`, not only in the error.

**Accessibility**
- Renders a native `<input type="number">` linked to its `<label>`; ArrowUp / ArrowDown change the value natively.
- The steppers are buttons with accessible names but are not in the tab order (`tabIndex=-1`); keyboard users type or use the arrow keys.
- `aria-describedby` points at the helper text, or at the error / warning message when shown; invalid sets `aria-invalid`; read only stays focusable with `aria-readonly`.
