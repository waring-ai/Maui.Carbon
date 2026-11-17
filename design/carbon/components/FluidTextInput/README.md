# FluidTextInput

A single-line text field whose label sits inside the 64px field (top-left, 13px from the top), for dense edge-to-edge fluid forms; `isPassword` turns it into the fluid password input with a show/hide toggle.

Hand-written from `packages/styles/scss/components/fluid-text-input` (on top of `text-input`) and `packages/react/src/components/FluidTextInput` / `TextInput` / `PasswordInput` (Carbon v11.99.0). `FluidTextInputSkeleton` is the loading placeholder. Left out: AI label / decorator slot, toggletip label, the aria-live counter announcer, inline mode, the password tooltip position/alignment props (the tooltip opens below the toggle).

**When to use**
- Free-form, single-line entry that cannot be picked from a list (names, hostnames, API keys) inside a FluidForm, a fluid modal or a toolbar.
- `isPassword` for secrets that should be masked by default.

**When not to use**
- Regular stacked forms, dense or complex forms, or when you need helper text under the field: use TextInput (or PasswordInput).
- Multi-line text: use FluidTextArea. Numbers with steppers: FluidNumberInput. A choice from a known list: FluidSelect, FluidDropdown or FluidComboBox.

**You provide:**
- `labelText` (required), `placeholder`, `id` (auto-generated if omitted), `name`, `type` (default `'text'`), `autoComplete`, `pattern`, `required`.
- Value: controlled `value` + `onChange(event)` or uncontrolled `defaultValue` (default `''`).
- `enableCounter` + `maxCount`: `n/max` at the top-right and `maxLength` on the input (not for passwords).
- States: `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly` (default all `false`; invalid/warn are ignored when disabled or read-only, and warn is ignored when invalid).
- Password: `isPassword`, `showPasswordLabel` (default "Show password"), `hidePasswordLabel` (default "Hide password"), `onTogglePasswordVisibility(event)`.
- `onClick`, `onFocus`, `onBlur`, `onKeyDown`, `className`.

**Example**
```jsx
const { FluidTextInput } = window.Carbon;
<FluidTextInput id="cluster-name" labelText="Cluster name" value={name} onChange={e => setName(e.target.value)}
  enableCounter maxCount={32} invalid={!valid} invalidText="Use lowercase letters, numbers and hyphens" />
```

**Rules**
- Field: `field-01` fill (hover `field-hover-01`), 1px `border-strong-01` bottom border, padding 32px 16px 13px, min height 64px; label is `label-01` in `text-secondary`, value `body-compact-01` in `text-primary`. Focus is a 2px inset `focus` outline.
- No helper text in fluid inputs; put guidance in the label or a toggletip. Keep labels short, sentence case, no colon; a long label scrolls with the field instead of wrapping.
- Placeholder is an example or hint only ("e.g. prod-us-south-01"), never the label and never essential rules such as password requirements.
- Invalid: the message row appears inside the field under a 1px `border-subtle-01` divider (16px side inset), text `text-error`, with the `support-error` icon at the right of the row; the whole field gets a 2px `support-error` outline (becomes `focus` while focused). The field grows to fit the message.
- Warning: same message row in `text-primary` with the `support-warning` triangle; the 1px `border-strong-01` bottom rule moves below the message.
- Error text says what is wrong and how to fix it ("Use lowercase letters, numbers and hyphens"); remove it as soon as the value is valid.
- Read only: transparent input on the field fill, `border-subtle-01` bottom rule, `text-secondary` value. Disabled: `text-disabled` label and value, no bottom rule.
- Password: 32x32 toggle at 26px from the top, 8px from the right.
- Skeleton: `FluidTextInputSkeleton` is a 64px `skeleton-background` box with a label bar and a value bar.

**Accessibility**
- The visible `<label>` is tied to the input with `htmlFor`; never ship without `labelText`.
- Invalid sets `aria-invalid="true"`, and `aria-describedby` points at the error or warning message so it is read with the field.
- Read-only stays focusable and readable by screen readers; disabled is removed from the tab order.
- The password toggle is a `<button>` with an `aria-label` that switches between "Show password" and "Hide password"; Tab reaches it after the input, Enter/Space toggles.
- The counter is visual only; announce limits in the error text if the user reaches them.
