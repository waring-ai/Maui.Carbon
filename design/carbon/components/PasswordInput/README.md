# PasswordInput

A single-line text field that masks its value, with a button to show or hide the characters.

Hand-written from `packages/styles/scss/components/text-input` (password rules) and `packages/react/src/components/TextInput/PasswordInput.tsx` (Carbon v11.99.0). Left out: inline layout, fluid variant, tooltip placement props (the toggle tooltip is always below, end-aligned), the `light` prop and a skeleton (Carbon reuses `TextInputSkeleton`, which this system does not export).

**When to use**
- Private values users type: passwords, API keys, tokens, webhook secrets.

**When not to use**
- Use a TextInput for anything that is not secret; masking slows people down.
- Do not use it to display a stored secret users only need to copy; show a masked value with a copy action instead.

**You provide:**
- `labelText` (`hideLabel` keeps it for screen readers only) and `id` (auto-generated if omitted).
- `value` + `onChange` or `defaultValue`. `type` `password` (default) or `text` to start revealed.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px).
- Toggle: `showPasswordLabel` (default "Show password") and `hidePasswordLabel` (default "Hide password") name the button and fill its tooltip; `onTogglePasswordVisibility(event)` fires on each toggle.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. `invalid` and `warn` are ignored while disabled or read only.
- Also passed through: `placeholder`, `name`, `autoComplete` (use `new-password` / `current-password`), `onBlur`, `onFocus`, `className`.

**Example**
```jsx
const { PasswordInput } = window.Carbon;
<PasswordInput
  id="db-password" labelText="Password" autoComplete="new-password"
  helperText="At least 12 characters, with a number and a symbol"
  value={pw} onChange={(e) => setPw(e.target.value)}
/>
```

**Rules**
- Uses the TextInput surface: `field-01` fill (`field-02` inside a Layer), 1px `border-strong-01` underline, 2px `focus` outline, `support-error` inset outline when invalid.
- The toggle is as wide as the field is tall (32 / 40 / 48px), uses `icon-primary` (`view` / `view--off`), and its tooltip is `background-inverse` with `text-inverse`; it shows on hover and keyboard focus.
- Text padding on the end side reserves room for the toggle (2.5rem at md) and for the invalid or warning icon (4rem total).
- Copy: put the format rules (length, character types) in `helperText` before people type; never echo the password in helper or error text.

**Accessibility**
- The toggle is a real `<button>` reachable with Tab and activated with Enter or Space; its `aria-label` switches between the show and hide labels. The visual tooltip is `aria-hidden`.
- `aria-describedby` points at the helper text, or at the error / warning message; invalid sets `aria-invalid`. Disabled also disables the toggle.
