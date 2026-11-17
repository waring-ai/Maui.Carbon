# PatternSettings

An account settings page: tabs for setting areas, a form section that saves on submit, toggles that apply immediately with save feedback, read-only identifiers and a separated destructive action.

Composed from this system's components. Carbon has no dedicated settings pattern; this follows its forms, toggle, read-only states, loading and common actions guidance.

**When to use**
- Pages where people review and change the configuration of an account, project or resource.
- Use PatternCreateForm for creating a resource, and PatternConfirmation for the dialog behind the destructive action.

**Components used**
`Breadcrumb`, `Heading`, `Tabs`, `Form`, `Stack`, `TextInput` (including `readOnly`), `Select`, `Toggle`, `InlineLoading`, `Button`.

**Composition rules**
- Tabs split large settings areas (General, Notifications, Access, Billing); each tab holds sections with a `heading-03` title and a one-line description that says how changes apply.
- Sections are separated by a 1px `border-subtle-01` rule with 32px above and below; the content column is at most 640px wide.
- Fields that are edited together save together: "Save changes" (primary) and "Cancel" (secondary) under the section, left-aligned, both disabled until something changes. While saving, `InlineLoading` takes the button's place, then "Changes saved".
- A toggle takes effect immediately, so it never sits inside a form with a Save button. Show "Saving..." then "Saved" under it with `InlineLoading`; on failure revert the toggle and show an error.
- Identifiers people need to copy (account ID) are read-only fields, not plain text, so they stay focusable and selectable; their helper text says why they can't change.
- Destructive actions sit last, in their own section on `layer-01`, with a `danger--tertiary` button that opens a confirmation, never acting directly.

**Accessibility**
- Each section is a `section` labelled by its heading; the Save form is labelled the same way.
- Each toggle keeps its `labelText` (visually hidden here because the row title shows it), so the switch is named.
- Save feedback is announced by `InlineLoading`'s live region; don't move focus after saving.

Known workarounds in the preview: the base `TextInput` sets the native `readonly` attribute but not Carbon's `cds--text-input-wrapper--readonly` class, so the preview applies the read-only look with a CSS rule.
