# PatternReadOnlyDisabled

The difference between read-only and disabled, shown for every main input side by side, and in a page where a viewer sees settings read-only and a temporarily unavailable action is disabled.

Composed from this system's components following Carbon's read-only states and disabled states patterns.

**When to use**
- Read-only: the value matters and people need to read, select or copy it, but they can't change it: their role doesn't allow it, the resource is locked, or a process owns the value.
- Disabled: the control is temporarily unavailable until something else happens (a field depends on another, an operation is in progress). Add an inline warning notification explaining how to enable it when it blocks the main action.
- Hidden: the person can never use it with their permissions (Delete for a Viewer). Don't show it disabled.
- Don't turn a disabled field into read-only or the reverse, and don't use read-only styling for static text that never had an editable state.

**Components used**
`TextInput`, `TextArea`, `NumberInput`, `Select`, `Dropdown`, `ComboBox`, `DatePicker`, `Slider`, `CheckboxGroup`, `Checkbox`, `RadioButtonGroup`, `RadioButton`, `Toggle`, `InlineNotification`, `Button`.

**Composition rules**
- Keep the enabled layout and spacing in both states, so switching state never moves anything.
- Read-only: transparent field, `border-subtle-01` underline, value in `text-primary` (4.5:1), steppers, chevrons and calendar icons in `icon-disabled`, no hover. Checkboxes and radios keep their selection with `icon-disabled` outlines. A prompt such as "Choose a version" becomes the value or "None".
- Disabled: `text-disabled` and `icon-disabled` for everything, no hover, `not-allowed` cursor. Never put information people need only in a disabled field.
- Say why once, near the top, in an info `InlineNotification` ("You have Viewer access... ask for the Editor role") instead of repeating it on every field.
- A disabled primary action gets a warning `InlineNotification` next to it saying what it is waiting for and when it comes back.

**Accessibility**
- Read-only fields stay in the tab order and are announced with their values (native `readonly`, or `aria-readonly` on custom controls); screen reader users can read and copy them.
- Disabled fields leave the tab order and are skipped or announced as unavailable, and their contrast is below 4.5:1 by design, which is why essential content must not be disabled.

Known workarounds in the preview: the base `TextInput` doesn't add Carbon's read-only class, so a CSS rule supplies the read-only look; the base `Toggle` has no `readOnly` prop, so the read-only toggle is a controlled toggle that ignores changes, styled with Carbon's read-only look (it isn't announced as read-only).
