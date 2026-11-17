# Select

A native `<select>` styled as a Carbon field for choosing one option from a list in a form, so mobile and assistive technology get the platform picker.

Hand-written from `packages/styles/scss/components/select` and `packages/react/src/components/Select` (Carbon v11.99.0). `SelectItem`, `SelectItemGroup` and `SelectSkeleton` are exported alongside. Left out: the xs (24px) size, fluid variant (see FluidSelect), AI label / decorator slot and the `light` prop.

**When to use**
- One choice from three or more options in a form that will be submitted, especially where mobile use is common.

**When not to use**
- Two options, or a handful that fit on screen: use a RadioButtonGroup.
- Several choices, filtering or sorting page content, or rich option rows: use a Dropdown, MultiSelect or ComboBox (the native list cannot be styled or filtered).

**You provide:**
- `id` (required) and `labelText` (`hideLabel` hides it visually; `noLabel` renders none, so give your own accessible name).
- Children: `SelectItem` (`value`, `text`; optional `disabled`, `hidden`) or `SelectItemGroup` (`label`, optional `disabled`) wrapping items, or raw `<option>`s.
- `value` + `onChange(event)` (controlled) or `defaultValue`.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px); `inline` puts the label beside the control without a fill.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly` (blocks opening and changing, stays focusable). `invalid` and `warn` are ignored while disabled or read only.
- Also passed through: `name`, `required`, `autoComplete`, `title`, `onBlur`, `onFocus`, `onMouseDown`, `onKeyDown`, `className`.
- `SelectSkeleton`: `hideLabel`, `className`.

**Example**
```jsx
const { Select, SelectItem, SelectItemGroup } = window.Carbon;
<Select id="bucket-region" labelText="Region" defaultValue="us-south"
  helperText="Data stays in the region you choose">
  <SelectItemGroup label="Americas">
    <SelectItem value="us-south" text="Dallas (us-south)" />
    <SelectItem value="us-east" text="Washington DC (us-east)" />
  </SelectItemGroup>
  <SelectItemGroup label="Europe">
    <SelectItem value="eu-de" text="Frankfurt (eu-de)" />
  </SelectItemGroup>
</Select>
```

**Rules**
- `field-01` fill (`field-02` inside a Layer), 1px `border-strong-01` underline, hover `field-hover-01`, focus a 2px `focus` outline; the chevron is `icon-primary` (`icon-disabled` when disabled or read only).
- Padding reserves 3rem on the end side for the chevron and 4rem when the invalid or warning icon is shown; invalid draws a 2px `support-error` inset outline and a `text-error` message; warning shows the `support-warning` icon and a `text-primary` message. Messages replace the helper text.
- Inline has a transparent background and no underline, with the label to the left; use it in toolbars and table headers, not in stacked forms.
- Read only drops the fill and uses a `border-subtle-01` underline; disabled uses `text-disabled`.
- Copy: a one-to-three-word label naming what the list holds; order options alphabetically or by increasing value; start with an empty "Choose a ..." item only when a choice is required, otherwise preselect the common default.

**Accessibility**
- The label is linked to the `<select>` by `for`/`id`; the browser handles the keyboard (Space, Enter or arrows open the list; arrows move; Enter or Escape closes).
- `aria-describedby` points at the helper text, or at the error / warning message; invalid sets `aria-invalid`.
- Read only sets `aria-readonly`, keeps focus, and blocks mouse and arrow-key changes; disabled removes the field from the tab order.
