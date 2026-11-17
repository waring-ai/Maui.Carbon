# TextInput

Single-line text field for short, unique values such as names, IDs and addresses. A flat `field-01` rectangle with a 1px `border-strong-01` bottom border; no side or top border and no radius.

Hand-written from `packages/styles/scss/components/text-input`. Left out: TextInputSkeleton, the fluid variant (see FluidTextInput), the read-only "edit off" icon (the glyph is not in this icon subset), the AI label decorator and the `ControlledPasswordInput` (see PasswordInput).

**When to use**
- Free-form, single-line input that cannot be picked from a list: a cluster name, an email address, an API key label.

**When not to use**
- Several lines or more than a few words: use a TextArea.
- A choice from a known list: use a Dropdown, Select, ComboBox or RadioButtonGroup.
- Numbers with steppers: NumberInput. Secrets: PasswordInput. Searching: Search. Dates and times: DatePicker, TimePicker.

**You provide:**
- `labelText` (always: a visible label above the field, `label-01`, `text-secondary`); `hideLabel` hides it visually but keeps it as the accessible name.
- `helperText`: format or consequence, below the field (beside the label when `inline`); replaced by the invalid or warn message.
- `value` + `onChange` (controlled) or `defaultValue`; `placeholder`; `type` (default text); `id` (generated if omitted).
- `invalid` + `invalidText`: 2px `support-error` outline, `warning--filled` icon in `support-error`, message in `text-error`.
- `warn` + `warnText`: triangle icon in `support-warning`, message in `text-primary`; no outline. `invalid` wins over `warn`; both are ignored while `disabled` or `readOnly`.
- `readOnly`: transparent fill and `border-subtle-01` rule; the value can be focused, selected and copied. `disabled`: not focusable.
- `size` xs 24 | sm 32 | md 40 (default) | lg 48.
- `enableCounter` + `maxCount`: "n/max" at the end of the label row; input stops at `maxCount`.
- `inline`: label (and helper) to the start of the field, for dense settings rows.
- `className`; any other prop (`name`, `required`, `autoComplete`, `onBlur`, `aria-*`) goes to the `<input>`.

**Example**
```jsx
const { TextInput } = window.Carbon;
<TextInput
  id="cluster-name" labelText="Cluster name" placeholder="e.g. prod-eu-de-1"
  helperText="Lowercase letters, numbers and dashes."
  value={name} onChange={e => setName(e.target.value)}
  invalid={!valid} invalidText="Remove spaces and use lowercase letters."
/>
```

**Rules**
- On `layer-01` and above, fields must step to `field-02`: wrap the fields in a Layer (it remaps `field-01`, `field-hover-01` and `border-strong-01`).
- Focus is a 2px `focus` inset outline. Error and warning never rely on color alone: the icon and the message are required.
- Labels: sentence case, short, no colon. Helper text: full sentences with punctuation. Error text says what is wrong and how to fix it.
- Placeholder is `text-placeholder`, deliberately low contrast: an example of the format at most, never required instructions, never the label.
- Use one height per form; md beside 40px buttons, sm in tables and toolbars.
- Validate on blur or submit, not on every keystroke, unless the rule is simple and the message helps while typing.

**Accessibility**
- The `<label for>` names the input. Helper text, or the invalid / warn message when shown, is linked with `aria-describedby`; invalid sets `aria-invalid="true"`, read-only sets `aria-readonly`.
- The counter is `aria-hidden`; when the limit matters, say it in `helperText` ("Up to 60 characters.").
- Disabled fields are skipped by Tab and screen readers; use `readOnly` for values people still need to read and copy.
- If you hide the label, keep `labelText` meaningful: it is the only name the field has.
