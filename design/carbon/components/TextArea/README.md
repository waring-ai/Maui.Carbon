# TextArea

A multi-line text field for longer free-form input such as descriptions and comments, with an optional character or word counter.

Hand-written from `packages/styles/scss/components/text-area` and `packages/react/src/components/TextArea` (Carbon v11.99.0). `TextAreaSkeleton` is exported alongside. Left out: fluid variant (see FluidTextArea), AI label / decorator slot, the aria-live counter announcer.

**When to use**
- The answer is more than a few words and may run over several lines: a description, a change reason, a comment for reviewers.

**When not to use**
- Use a TextInput for a single short value (a name, a host, a port).
- Use a Select, Dropdown or RadioButtonGroup when the answer comes from a known list; free text invites errors.

**You provide:**
- `labelText` (required unless the context labels the field; `hideLabel` keeps it for screen readers only) and `id` (auto-generated if omitted).
- `value` + `onChange(event)` (controlled) or `defaultValue` (uncontrolled).
- `rows` (default 4), or `cols` for a fixed width (the field then stops stretching and cannot be resized).
- `enableCounter` with `maxCount`, and `counterMode` `character` (default) or `word`. Input over the limit is not accepted (native `maxLength` in character mode; extra words are ignored in word mode).
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. `invalid` and `warn` are ignored while disabled or read only; `invalid` wins over `warn`.
- Also passed through: `placeholder`, `name`, `required`, `onBlur`, `onFocus`, `onKeyDown`, `onClick`, `className`.
- `TextAreaSkeleton`: `hideLabel`, `className`.

**Example**
```jsx
const { TextArea } = window.Carbon;
<TextArea
  labelText="Justification"
  helperText="Say what the extra capacity is for and how long you need it"
  enableCounter maxCount={300} rows={4}
  value={reason} onChange={(e) => setReason(e.target.value)}
/>
```

**Rules**
- The field sits on `field-01` (`field-02` inside a Layer) with a 1px `border-strong-01` bottom border; focus is a 2px `focus` outline inset by 2px.
- Invalid draws a 2px `support-error` inset outline and a `support-error` icon at the top right; the message uses `text-error`. Warning uses the `support-warning` icon and a `text-primary` message. Only one of helper, error or warning text shows at a time, and they replace each other in the same place.
- Read only drops the fill and uses a `border-subtle-01` underline; disabled uses `text-disabled`, no underline and no resize handle.
- Minimum height 40px, minimum width 10rem, vertical resize only (none when `cols` is set). The counter sits right-aligned on the label row in `text-secondary`.
- Copy: short sentence-case label without a colon; helper text says what a good answer contains; never use the placeholder as the label. Pair the counter with a limit users can act on.

**Accessibility**
- The `<label>` is linked by `for`/`id`; `aria-describedby` points at the helper text and a visually hidden "Character limit n" / "Word limit n" note, or at the error / warning message when shown.
- Invalid sets `aria-invalid`; read only sets `readOnly` and `aria-readonly` and stays focusable; disabled removes the field from the tab order. Error and warning messages use `role="alert"`.
- The visible `n/max` counter is `aria-hidden`; changes to it are not announced.
