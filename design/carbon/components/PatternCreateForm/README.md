# PatternCreateForm

A full-page create flow for one resource: breadcrumb, page title, grouped fields, a server error summary, inline field errors and warnings, a cost summary, and the submit row.

Composed from this system's components following Carbon's forms pattern (carbondesignsystem.com/building-blocks/core/patterns/forms). The left-aligned, primary-first button order follows that page's rule for in-page forms.

**When to use**
- Creating a complex resource (a cluster, a VPC, a database) that needs more than about five inputs or sections; a dedicated page leaves room for grouping and a summary.
- Use a Modal instead for fewer than five inputs on a critical, infrequent task, and a side panel (HeaderPanel) for repeated tasks where people need to see the page behind.
- Use a ProgressIndicator with steps when the form is long enough to split.

**Components used**
`Breadcrumb`, `Heading`, `Form`, `FormGroup`, `Stack`, `InlineNotification`, `TextInput`, `Select`, `Dropdown`, `TextArea`, `RadioButtonGroup`, `NumberInput`, `Toggle`, `Checkbox`, `ButtonSet`, `Button`.

**Composition rules**
- One column of fields, max about 576px wide (8 grid columns). Put two fields side by side only when they are closely related (region and version); 16px (`spacing-05`) between them.
- Group fields under `heading-03` section titles in `FormGroup`s; 24px (`spacing-06`, `Stack gap={6}`) between fields, 32px (`spacing-07`) between groups, 48px (`spacing-09`) from the last field to the buttons.
- Labels are visible, sentence case, one to three words, no colon. When most fields are required, mark only the optional ones "(optional)". Helper text sits under the field and says what the value is for or its format; placeholders never replace labels.
- Validate a field when it loses focus. Error text names what is wrong and how to fix it, with an example ("For example, prod-eu-01"). A warning (`warn`) is for valid but risky values (fewer than 3 nodes).
- After a failed submit or a server error, show an error `InlineNotification` at the top of the form ("Fix 1 error before you continue.") in addition to the inline field errors; remove both once fixed.
- Buttons at the bottom, left-aligned, primary first: "Create cluster" then "Cancel". Name the primary for the outcome, never "Submit". Disable it while the request runs (see PatternLoading).
- A cost or summary panel on `layer-01`, 272px wide, sticky at the top, updates as fields change.

**Accessibility**
- Each `FormGroup` is a `fieldset` with a `legend`; every input has a visible label tied by `id`.
- Invalid fields set `aria-invalid` and point at their message with `aria-describedby`; move focus to the error summary or the first invalid field after a failed submit.
- Focus the first field when the page opens. Keep DOM order equal to visual order (form before the summary aside, which has an `aria-label`).
