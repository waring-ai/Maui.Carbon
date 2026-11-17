# FluidSelect

A native `<select>` with the label inside the 64px field and the chevron at the right, for fluid forms.

Hand-written from `packages/styles/scss/components/fluid-select` (on top of `select`) and `packages/react/src/components/FluidSelect` / `Select` (Carbon v11.99.0). `FluidSelectSkeleton` is the loading placeholder. Left out: AI label / decorator slot, toggletip label, inline mode, sizes.

**When to use**
- Choosing one value from a short, known list in a FluidForm, especially where native mobile pickers and plain form submission matter (region, plan, worker profile).

**When not to use**
- Stacked or complex forms: use Select. Two or three options that should all be visible: RadioButtonGroup. Lists that need rich rows or filtering: FluidDropdown or FluidComboBox. Several values: FluidMultiSelect.

**You provide:**
- `labelText` (required), `id` (auto-generated), `name`, `required`, `autoComplete`, `noLabel` (omits the label; avoid).
- Options as children: `SelectItem` / `SelectItemGroup` from the Select component (`{ value, text, disabled, hidden }`), or plain `<option>`s. For a placeholder use a disabled, hidden `SelectItem` with `value=""` ("Choose a region").
- Value: controlled `value` + `onChange(event)` or uncontrolled `defaultValue`.
- States: `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly` (value visible; mouse and Arrow/Space changes blocked). Invalid/warn are ignored when disabled or read-only.
- `onFocus`, `onBlur`, `onKeyDown`, `className`.

**Example**
```jsx
const { FluidSelect, SelectItem } = window.Carbon;
<FluidSelect id="plan" labelText="Billing plan" value={plan} onChange={e => setPlan(e.target.value)}>
  <SelectItem value="lite" text="Lite" />
  <SelectItem value="standard" text="Standard" />
  <SelectItem value="enterprise" text="Enterprise" />
</FluidSelect>
```

**Rules**
- 64px `field-01` box, select padding 32px 32px 13px 16px, `body-compact-01` value, label `label-01` `text-secondary` at 13px / 16px, 16px chevron 34px from the top and 12px from the right in `icon-primary`.
- Focus is a 2px inset `focus` outline around the whole input box; the select itself never draws an outline.
- Invalid: 2px `support-error` outline, `border-subtle-01` divider, message row in `text-error` with the error icon 73px from the top / 16px from the right.
- Warning: same row in `text-primary` with the `support-warning` icon; the 1px `border-strong-01` bottom rule moves under the message.
- Read only: transparent select, `border-subtle-01` rule, chevron in `icon-disabled`. Disabled: `text-disabled` label, value and chevron.
- Option text is short, sentence case, in a logical or alphabetical order; group long lists with `SelectItemGroup`.

**Accessibility**
- Native `<select>` with a `<label>`: the browser's own keyboard (Arrow keys, type-ahead, Alt+ArrowDown/Space to open) and mobile pickers work unchanged.
- Invalid sets `aria-invalid`; `aria-describedby` points at the message. Read only sets `aria-readonly` and stays focusable.
