# PatternLogin

A two-step login page: the user ID first (with remember ID, create account and an alternative login), then the password, with inline validation and one generic error after a failed attempt.

Composed from this system's components following Carbon's login pattern. Uses "Log in" everywhere, as Carbon does, never "Sign in".

**When to use**
- The entry page of a product that authenticates people itself, especially when some accounts use company SSO: the first step's ID decides where to route them.
- Use a single page with both fields only when the system can't tell from the ID which method a person needs; then show the alternative login buttons from the start.

**Components used**
`TextInput`, `PasswordInput`, `Checkbox`, `Button`, `Link`, `InlineNotification`.

**Composition rules**
- Centered or left column about 4 grid columns wide (here 346px) on `background`; heading `heading-04` "Log in to Cloud console", then "Don't have an account? Create an account", then a hairline.
- Step 1: Email field, "Remember my user ID" checkbox directly under it, "Continue" primary button with the `arrow--right` icon, full width. Alternative logins follow after a hairline under an "Alternative login" label as tertiary buttons; never between the field and Continue, never above the form.
- Step 2: "Logging in as <ID>." with a "Not you?" link back to step 1, the password field, "Forgot password?" link under it, and "Log in" as the primary button.
- Default (not fluid) inputs and buttons when alternative logins are shown, so the primary button stays next to its field. 24px hairline spacing, 16px between field and checkbox, 24px before buttons.
- Validate on blur: empty fields and bad formats show inline errors that say how to fix them; clear the error once the value is valid.
- After a failed login, return to step 1 with the ID kept, the password cleared and focus on the ID field, and show an error `InlineNotification` with one generic message ("Your user ID or password is incorrect.") that doesn't reveal which part was wrong. Mention lockout rules if they apply.
- A "Need help? Contact support" line at the bottom.

**Accessibility**
- Each step is a `form` labelled by its heading; fields have visible labels and `autocomplete="username"` / `"current-password"` so password managers work.
- The password visibility toggle has an accessible name ("Show password"); the error notification uses `role="alert"`.
- Don't disable Continue or Log in while fields are empty; let the inline errors explain what is missing.

Known workarounds in the preview: the base `TextInput` passes neither `onBlur` nor `autoComplete`, so blur validation listens on a wrapping `div` and `autocomplete="username"` is set on the input after render.
