# Form patterns

Carbon's form, fluid style, login, settings and text toolbar patterns, with the Cds controls that build them. Start from [patterns](patterns.md) to pick one.

## Forms

Gallery: `PatternCreateFormPage`. Relay: `CreateIssueView`.

A form is a group of related inputs on a page, in a dialog or in a side panel, used to create, edit, configure, register or buy something.

**Respect the person**

- Ask only for what the task needs. Every field costs effort, and personal data has legal weight.
- Order fields the way people think about the task (first name before last name), with the required and important fields first. Group related fields under section headings (`CdsFormGroup` with a legend).
- Prefill what you already know or can detect, such as the country from the locale, today's date as a start date, or a common minimum quota. A default must be safe to leave unchanged.
- Reveal fields only when an earlier answer makes them relevant (progressive disclosure).
- Let people finish with the keyboard alone, and leave room for autofill and password managers: give each `CdsTextInput` and `CdsPasswordInput` the right `Keyboard` and keep login fields as real text entries.
- When the form opens, focus the first field that needs input.

**Where the form lives**

| Container | Use when | Rules |
|---|---|---|
| Dedicated page | Long, complex or multi-step requests (provisioning, orders) | Grid layout. Can hold the most complexity: sections, steps, side-nav sections. |
| Dialog (`CdsModal` with `HasForm`) | A short, infrequent, critical task, with fewer than about five inputs | No accordions or tabs inside, no links that leave the task, never a second dialog from this one. |
| Side panel | A frequent task where the person must still see what they edit (edit a table row), with more than about five inputs | No accordions or tabs. Use `CdsSidePanel` (with `Actions` for the footer buttons); in package versions without it, compose a fixed-width `CdsLayer` pane beside the content, or use a page. |

**Layout and spacing**

- One column. Two or three fields share a row only when they form one answer: first name, middle initial and last name; card number, expiry and security code; city, state and postal code.
- A form on a page is about 576 to 672 wide, or 8 of 16 grid columns. Field widths hint at the expected length (a postal code is narrow) while staying on the grid.
- The form heading is the largest text in the form: `Heading03` (productive heading 03) inside a container or dialog, larger when the form is the only thing on the page, with an optional one-line description. Section headings sit between the form heading and the labels in size.
- Inputs are `Md` (40 high) in product UI.
- Fields are 32 apart on a page (`CdsStack Gap="7"`) and 24 or 16 apart in dialogs and side panels. Space between groups and before the first field scales with that: 40 when fields are 32 apart, 32 when they are 24 apart. Leave 48 between the last field and the buttons.
- When side-by-side fields differ only in that one has helper text, align the inputs, not the labels.

**Labels, help and placeholders**

- Every input has a visible label (`Label`) of one to three words, in sentence case, without a colon. A label is not instructions.
- Labels sit above their inputs (Carbon's only arrangement). Checkbox and radio text sits to the right of the control, and groups stack vertically.
- **Helper text** (`HelperText`, below the label, always visible) carries what people need to know, such as a format or a limit. Keep it short and specific, never longer than the input is wide, and never use it in place of a label.
- **Tooltip help** uses the `Information` icon (an "i", not "?") with a `CdsToggletip` or `CdsTooltip`. It is for extra, nice-to-know explanation and is used sparingly. Never put essential information there.
- **Placeholder** text is a short, anonymized example (`YYYY-MM-DD`, "Search for networks or devices") that disappears on typing. It is never a label or a requirement. Put long rules such as password rules in helper text or a toggletip.
- **Required and optional:** mark the minority. If most fields are required, label the rest "(optional)". If most are optional, label the required ones "(required)" and never "(optional)". Be consistent across the product. Many optional fields deserve their own section.

**Choosing inputs** (see [choosing components](choosing-components.md))

- Text: `CdsTextInput` for a few words, `CdsPasswordInput` for secrets (passwords, PINs, card numbers), `CdsTextArea` for several lines.
- One of a few: `CdsRadioButtonGroup` with a sensible default selected, plus a "None" option when no choice is valid. Up to five options use radios or checkboxes. With more than five, use `CdsDropdown`, `CdsComboBox` (type-ahead) or `CdsMultiSelect`.
- Several of a few: `CdsCheckboxGroup`. A single opt-in uses one `CdsCheckbox`.
- `CdsToggle` applies immediately, so it belongs in settings that save on change and never in a form with a Save button. Its label names the attribute it controls.
- Bounded values: `CdsNumberInput`, `CdsSlider`, `CdsDatePicker` and `CdsTimePicker` accept only valid values, so they need little validation.

**Buttons**

- Buttons go at the bottom of the form, never pinned at the top of a page form. If actions must stay visible on a long page, use a pinned footer bar.
- The primary label names the task ("Create cluster", "Save changes"), never "Submit". The secondary is Cancel or Discard. Back is not a secondary button: the breadcrumb, the progress indicator and the platform back gesture provide it.
- In-page forms: left-aligned, primary first (on the left), then the secondary.
- Multi-step forms and wizards: right-aligned, primary last (on the right), because it moves forward.
- Dialogs and side panels: buttons fill the footer edge to edge, secondary on the left and primary on the right (`CdsModal` does this). If the labels are too long, stack them with the primary at the bottom.

**Validation**

- Validate a field when it loses focus, never on each keystroke. Set `IsInvalid` and an `InvalidText` that names the fix ("Password must be at least 16 characters"), and clear the error as soon as the value is valid.
- After a failed submit or a server-side error, keep every field error and add an error `CdsInlineNotification` at the top of the form that says how many fields need attention. Move focus to it or to the first invalid field.
- Disable the primary button until the form is valid only on very short forms. On longer forms the error and the button may not both be in view, so leave the primary enabled and validate on submit.
- While submitting, disable the primary button to prevent a double submit and show `CdsInlineLoading` (or the modal's `LoadingStatus`). Show progress for long work.

**Long forms**

- Split them into steps with `CdsProgressIndicator`. Each step holds related fields, people can go back to review, and saving progress is possible. Or split them into page sections with a side nav.
- Avoid accordion forms, and never use them in dialogs: people cannot tell whether a button applies to the section or to the whole form.
- Confirm before discarding unsaved changes ("Discard changes?", "Discard" / "Keep editing").

**Accessibility:** use visible labels and written format instructions, and state any requirements before the form starts. Group with `CdsFormGroup` (it names the group), make focus order follow the visual order, and mark required fields in the label text, not only with color.

## Fluid styles

Gallery: the FluidForms pages. Use `CdsFluidForm` with `CdsFluidTextInput`, `CdsFluidDropdown` and the other fluid inputs.

Fluid components bleed to the edges of their container and join into one unit. Their label sits inside the field, all inputs are 64 high, and there is no space between them.

- **Use them** for expressive or important moments, such as login, sign-up, a short request in a modal or a focused side panel. Use them in spacious layouts and attached to a compound component.
- **Do not use them** for complex, dense or multi-section productive forms, where people need white space between inputs. Do not use them inside accordions or similarly sectioned containers, where their edges clash with the dividers.
- Stack fluid inputs flush, with no gap, in one `CdsFluidForm`. Never mix fluid and default inputs in one form. Fluid inputs show no helper text, so put what people must know in the label, the placeholder or a toggletip.
- A fluid form always sits in a container (a modal, a tile, a panel), never floating on the page. A common hybrid is default inputs with fluid, full-width buttons at the bottom of a modal or side panel.
- Fluid buttons span 25, 50 or 100 percent of the container and hug its bottom edge. They are never placed mid-flow. If more actions follow below the main flow, use default buttons.
- Prefer buttons whose width spans grid columns over buttons sized to their text, and give related buttons (ghosts excepted) equal widths (`CdsEqualWidthRow`).

## Login

Gallery: `PatternLoginPage`.

Show login when someone opens the app signed out, after they sign out (confirm, then show login), or after an inactivity timeout (afterwards, return them to the page they were on).

**Anatomy**

- The title is "Log in" (not "Sign in"), optionally with the product name.
- The user ID field is first and is usually an email address. The password appears on a second step, because the flow depends on the ID.
- "Remember ID" (optional) is a `CdsCheckbox` under the ID field that says what it remembers.
- The primary button on the ID step is "Continue".
- Other links: "Forgot password?" (on the password step), "Create an account", alternative logins, and optionally "Need help?".

**Flow**

- **Progressive:** the ID step decides in the background whether the account uses single sign-on or a password, so the person never has to choose. Single sign-on goes to the organization's flow. A password account goes to the password step, which offers a way back to correct the ID, and "Forgot password?".
- If the system cannot tell which method applies, offer a button for each method up front, with default (not fluid) inputs and buttons.
- Keep the primary button closest to the ID input. Alternative login buttons never sit between the input and the primary button, never sit above the form, and never look more prominent than the preferred path.

**Errors**

- On blur or on Continue, an empty or badly formatted field gets an inline error ("Email is required", "Enter a valid email address"). It clears once the field is fixed.
- Do not say whether the ID exists until the password is submitted. Then show one generic error for either mistake ("Incorrect email or password. Try again.") in an error `CdsInlineNotification`, clear the password, and put focus back on the first field.

**Layout:** a centered fluid form on a plain or subtly textured background, or a split screen with brief, minimal content on the other side. Keep marketing links and calls to action out of the login region, because people miss the real actions among them. Keep positions fixed when the password step replaces the ID step so nothing jumps. Without Remember ID, drop the 24 above where it would be. Mark the login region (`CdsSemantics.GroupName`) so screen readers can jump to it, and make the whole page work by keyboard.

## Settings

Gallery: `PatternSettingsPage`. Relay: `SettingsView`.

- Group settings with `CdsTabs` or side-nav sections.
- Use either a form with Save for related fields that apply together, or `CdsToggle`s that apply immediately with saving feedback for independent switches. Never mix the two in one group.
- Show identifiers as read-only fields with `CdsCopyButton`.
- Destructive actions go last, in their own section, behind a confirmation (see common actions in [patterns](patterns.md#common-actions)).
- Let people control which non-critical notifications they receive.

## Text toolbar

No gallery page. Compose it from existing controls.

A horizontal toolbar above an editable text area, for writing and formatting text, attaching files or links, and finding words.

- **Groups, left to right:** actions (Undo, Redo, Cut, Copy, Paste), formatting (typeface, size, bold, italic, underline, color), paragraph (alignment, lists, indent), attach (file, link), then search. Include only the controls people need.
- **Controls:** basic actions are ghost `CdsIconButton`s with a `Label`, which becomes the tooltip and the accessible name. Typeface and size are `CdsDropdown`s. Text color and alignment open a `CdsMenuButton` or `CdsMenu` of choices. Separate groups with a 1px `CdsBorderSubtle01` rule.
- **Narrow widths:** move the controls that do not fit into a trailing `CdsOverflowMenu`, so the most-used ones stay visible.
- **Link:** the link button opens a small field in a `CdsPopover`. Pressing the link button again embeds the link, and the x or an outside click closes the field. Selecting the linked text reopens it for editing or removal.
- **Attach:** attached files list at the bottom left of the text area, each with an x to remove (`CdsFileUploader` items).
- **Search:** highlight matches in the text, show the count in a tag inside the search field with an x to clear it, and close search with its x or an outside click.
- **Saving:** offer "Save draft", or autosave and say when it last saved.
- **Accessibility:** name the toolbar as a group (`CdsSemantics.GroupName="Formatting"`). Focus enters on the first enabled control and moves left to right in reading order. Each icon button has a name, and toggle buttons show their state (`IsSelected`).
