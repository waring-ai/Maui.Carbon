# FluidTextArea

A multi-line text field with the label inside the field, for fluid forms; optional character or word counter at the top-right.

Hand-written from `packages/styles/scss/components/fluid-text-area` (on top of `text-area`) and `packages/react/src/components/FluidTextArea` / `TextArea` (Carbon v11.99.0). `FluidTextAreaSkeleton` is the loading placeholder. Left out: AI label / decorator slot, toggletip label, the aria-live counter announcer, `cols`, the modal `field-02` override.

**When to use**
- Longer free text that may span lines (descriptions, release notes, incident notes) inside a FluidForm or a fluid side panel.

**When not to use**
- One-line values: use FluidTextInput. Stacked or complex forms, or text that needs helper text below it: use TextArea.

**You provide:**
- `labelText` (required), `placeholder`, `rows` (default 4), `id` (auto-generated), `name`, `required`.
- Value: controlled `value` + `onChange(event)` or uncontrolled `defaultValue` (default `''`).
- `enableCounter` + `maxCount`: shows `n/max`. `counterMode` `'character'` (default; also sets `maxLength`, so typing stops at the limit) or `'word'` (counts words; does not block typing).
- States: `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly` (invalid/warn are ignored when disabled or read-only).
- `onFocus`, `onBlur`, `onClick`, `onKeyDown`, `className`.

**Example**
```jsx
const { FluidTextArea } = window.Carbon;
<FluidTextArea id="resolution" labelText="Resolution note" rows={3} enableCounter maxCount={280}
  placeholder="What fixed it, and what follow-up is needed" value={note} onChange={e => setNote(e.target.value)} />
```

**Rules**
- The wrapper paints `field-01`; the textarea starts 32px below the top (label at 13px) with 16px side and 13px bottom padding, min height 64px, `body-01` text, 1px `border-strong-01` bottom rule. Focus is a 2px inset `focus` outline on the whole field. The user can resize it vertically; content scrolls past the visible rows.
- Invalid: 2px `support-error` border around the field, message row (`text-error`, 8px 40px 8px 16px padding) under a `border-subtle-01` divider with the error icon at the right of the row; resize is turned off.
- Warning: same message row in `text-primary` with the `support-warning` icon, and the 1px `border-strong-01` bottom rule moves under the message.
- Read only: transparent textarea and a `border-subtle-01` bottom rule; disabled uses `text-disabled`, no rule, no resize.
- Fluid fields have no helper text. If there is a limit, show it with the counter rather than in the placeholder.
- Skeleton: `FluidTextAreaSkeleton` is a 128px `skeleton-background` box with a label bar and a value bar.

**Accessibility**
- The `<label>` is tied to the textarea with `htmlFor`.
- With a counter, a visually hidden "Character limit N" / "Word limit N" text is added to `aria-describedby`; the visible `n/max` is `aria-hidden`.
- Invalid sets `aria-invalid`; the error or warning message has `role="alert"` and is referenced by `aria-describedby`.
- Read only sets `aria-readonly` and stays focusable; disabled leaves the tab order.
- Tab moves focus out of the field (Tab is not captured for indentation).
