# FluidForm

A `<form class="cds--form cds--form--fluid">` wrapper for fluid inputs: fields with the label inside a 64px box, stacked flush with no gaps so the form reads as one block.

Hand-written from `packages/react/src/components/FluidForm` and the `cds--form--fluid` rules in `packages/styles/scss/components/form` (Carbon v11.99.0), with layout guidance from the Carbon website (fluid styles, form, forms pattern). Left out: the `FormContext` that makes plain `TextInput`/`Select` render fluid; use the `Fluid*` components inside it instead. The form adds no grid of its own: you lay out rows yourself (see Rules).

**When to use**
- Expressive or featured moments with a short, simple task: a create panel, a modal, a sign-up or billing form, a toolbar of filters.
- Inside compound containers (side panel, modal, tile) where the fields should read as one block that hugs the container edges.

**When not to use**
- Long, complex or multi-section forms, hyper-productive tasks and dense layouts: use Form with the default inputs (TextInput, Select, Dropdown, DatePicker...) instead.
- When fields need helper text under them or space between them: fluid inputs have neither. Use the default inputs.
- Inside an Accordion, or for a single field on its own: fluid components never stand alone.

**You provide:** `children`: FluidTextInput, FluidTextArea, FluidNumberInput, FluidSearch, FluidSelect, FluidTimePicker, FluidDropdown, FluidComboBox, FluidMultiSelect, FluidDatePicker, and fluid (full-bleed) Buttons at the bottom. `className`. Any other `<form>` attribute (`onSubmit`, `aria-label`, `noValidate`, `id`) passes through to the `<form>`.

**Example**
```jsx
const { FluidForm, FluidTextInput, FluidSelect, SelectItem, FluidTextArea, Button } = window.Carbon;
<FluidForm aria-label="Create deployment" onSubmit={createDeployment}>
  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', columnGap: 1, background: 'var(--border-strong-01)' }}>
    <FluidTextInput id="name" labelText="Deployment name" placeholder="e.g. payments-api" />
    <FluidSelect id="region" labelText="Region" defaultValue="us-south">
      <SelectItem value="us-south" text="Dallas (us-south)" />
      <SelectItem value="eu-de" text="Frankfurt (eu-de)" />
    </FluidSelect>
  </div>
  <FluidTextArea id="notes" labelText="Description (optional)" rows={2} />
  <div style={{ display: 'flex' }}>
    <Button kind="secondary" size="xl" style={{ flex: 1, maxInlineSize: 'none' }}>Cancel</Button>
    <Button kind="primary" size="xl" type="submit" style={{ flex: 1, maxInlineSize: 'none' }}>Create deployment</Button>
  </div>
</FluidForm>
```

**Rules**
- Stack fluid fields flush: 0px between rows. Each field paints `field-01` and keeps its own 1px `border-strong-01` bottom rule, which separates stacked rows.
- Side-by-side fields: Carbon separates fluid columns with a 1px line (not the 32px gutter of default forms). Put the row in a grid with `column-gap: 1px` on a `border-strong-01` background (as in the preview) so the 3:1 divider shows between fields. Fields in a row must share one height, so keep validation messages to one line where you can.
- Use one or two (at most three closely related) fields per row; size them to the expected content. Only FluidTimePicker draws its own internal dividers.
- Fluid fields have no helper text. Put format and guidance in the label ("Description (optional)") or a toggletip; validation messages render inside the field, under a `border-subtle-01` divider.
- Do not mix fluid and default (label-above) inputs in the same form. The accepted hybrid is default inputs with fluid buttons in a contained form (modal, side panel).
- Fluid buttons hug the bottom edge at 25%, 50% or 100% of the container width, size `xl`, primary on the right; in side panels and modals use two 50% buttons.
- Mark the minority: if most fields are required, mark only the optional ones "(optional)". Validate a field on blur; in short forms keep the primary button disabled until the form is valid.

**Accessibility**
- Renders a native `<form>`: Enter in a text field submits, and `onSubmit` receives the event (call `preventDefault` for client-side handling).
- Give the form an accessible name (`aria-label` or `aria-labelledby` pointing at the visible heading) when the page has more than one form.
- Every field needs its visible `labelText`/`titleText`; the label inside the field is a real `<label>` (or `aria-labelledby`) for each component.
- Because there is no helper text, any guidance you move into a toggletip must also be reachable by keyboard and screen readers.
