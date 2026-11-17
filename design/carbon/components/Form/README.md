# Form

The structural pieces of a Carbon form: `Form` (`<form class="cds--form">`), `FormGroup` (a `<fieldset class="cds--fieldset">` whose `<legend class="cds--label">` names a set of related fields), `FormItem` (the `.cds--form-item` column every input sits in) and `FormLabel` (a bare `cds--label`). They add structure and labelling only; the fields themselves are TextInput, Select, Checkbox, etc.

Hand-written from `packages/styles/scss/components/form` and `packages/react/src/components/{Form,FormGroup,FormLabel,FormItem}` (Carbon v11.99.0). Left out: the fluid form (`cds--form--fluid`; see FluidForm), skeleton labels, and the legacy `cds--tooltip` label styles.

**When to use**
- Any set of inputs the user submits: create and edit flows, settings, sign-up.
- `FormGroup` for related fields that share one question or heading (location, billing contact, zones); `FormItem` + `FormLabel` to wrap a custom control so it lines up with Carbon fields.

**When not to use**
- A single search or filter field that applies instantly. Use Search on its own, without a form submit.
- Changes that apply immediately (toggles in a settings list). Use Toggle without a Save button.
- Expressive, featured forms with labels inside the field. Use FluidForm.

**You provide:**
- `Form`: `children`, `onSubmit` and any native form attribute (`noValidate`, `aria-label` or `aria-labelledby` when the page has several forms), `className`.
- `FormGroup`: `legendText` (required, any node), `children`, `disabled` (native fieldset disable, so every nested control is disabled), `invalid` (sets `data-invalid`), `message` + `messageText` (a note under the group, wired with `aria-describedby`), `legendId`, `className`.
- `FormItem`: `children`, `className`.
- `FormLabel`: `children` and `id` (the id of the control it labels; rendered as `htmlFor`), `className`.

**Example**
```jsx
const { Form, FormGroup, Stack, TextInput, Select, SelectItem, ButtonSet, Button } = window.Carbon;
<Form aria-labelledby="create-title" onSubmit={createCluster}>
  <Stack gap={7}>
    <FormGroup legendText="Location">
      <Select id="region" labelText="Region" defaultValue="eu-de">
        <SelectItem value="eu-de" text="eu-de (Frankfurt)" />
      </Select>
    </FormGroup>
    <TextInput id="name" labelText="Cluster name" helperText="Lowercase letters, numbers and dashes" />
    <ButtonSet><Button kind="secondary">Cancel</Button><Button type="submit">Create cluster</Button></ButtonSet>
  </Stack>
</Form>
```

**Rules**
- Anatomy: an optional header (title and one line of context), the body of labelled fields, then a footer with the actions. Put the primary action last, with a secondary Cancel before it, left-aligned on full pages.
- Legend and labels use `label-01` (12px/16px, 0.32px) in `text-secondary`, 8px (`spacing-03`) above the control; `FormLabel` drops that margin (`cds--label--no-margin`).
- Stack fields 32px apart (`Stack gap={7}`, 24px when tight) and sections 48px (`gap={9}`). Two-column rows align to the grid with a 32px gap; the pair grows together when one shows an error.
- Every field has a visible label in sentence case, without a colon. Helper text under the field gives format rules; it is replaced by the error or warning message.
- Mark whichever is rarer: "(optional)" when most fields are required, "(required)" when most are optional. Be consistent across the product.
- Errors block submission and say how to fix the value; warnings flag a risk but allow submitting. Validate on submit (and on blur for format rules), and keep the message until the value is fixed.
- The FormGroup message is `helper-text-01` in `text-helper`, or `text-error` when the group is `invalid`. Carbon renders this element without styling; the colors here are an addition. Inside a disabled fieldset, labels, helper text and the message use `text-disabled`.
- Use one FormGroup per related set; don't nest fieldsets for visual spacing. Autofilled inputs keep the `field-01` background and `text-primary` text.

**Accessibility**
- `FormGroup` is a native `fieldset` labelled by its `legend` (`aria-labelledby`) and described by its message, so screen readers announce the group name before each field.
- Every control needs a programmatic label: the field components render one; with a custom control use `FormLabel id="<control id>"` (or `aria-label`).
- Give the form an accessible name with `aria-labelledby` pointing at its heading when there is more than one form on the page.
- On a failed submit, move focus to the first invalid field (or to an error summary) so keyboard users find the problem.
