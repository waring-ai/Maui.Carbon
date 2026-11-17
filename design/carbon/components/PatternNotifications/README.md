# PatternNotifications

How to choose between inline, toast, actionable, banner, callout and modal messages, with each shown where it belongs on a cloud console page.

Composed from this system's components following Carbon's notifications pattern. Carbon's banner and notification panel have no dedicated component here: the banner is an inline `ActionableNotification` spanning the content area, and a notification panel can be built in a `HeaderPanel`.

**When to use**
- Task-generated feedback (the person just did something): `InlineNotification` next to the form or section, a `ToastNotification` for outcomes elsewhere on the page, or an `ActionableNotification` when one follow-up action (Undo, Retry) helps.
- System-generated events: toasts, a banner for account-level messages, or a notification panel opened from the header.
- `Callout`: guidance that is part of the content, shown before the person acts; never dismissible, no success or error kind.
- `Modal`: only when the person must decide or acknowledge before continuing (session expiry). Never more than one.

**Components used**
`StructuredList`, `InlineNotification`, `ToastNotification`, `ActionableNotification`, `Callout`, `Modal`, `TextInput`, `Select`, `ButtonSet`, `Button`, `Link`, `DataTable`, `Layer`, `Stack`.

**Composition rules**
- Status kinds: info (blue, information icon), success (green, checkmark), warning (yellow, warning icon), error (red, error icon). Always icon plus words; yellow is never text.
- Two-part copy: a title that states the outcome ("Export ready", "Payment method expires soon.") and a subtitle with the reason and what to do next. Toasts stay within three lines.
- Inline notifications sit at the top of the form or section they are about, full width of it, 24px above the first field; they don't cover content.
- Toasts stack top-right under the header, fixed width, 8px apart, newest first. A toast without actions may auto-dismiss; one with an action stays until acted on or closed. Never time out errors.
- A banner sits under the header at the top of the content area, scrolls with the page, one at a time, and stays until dismissed.
- Modal footer: the safe or secondary action left, the primary right ("Log out", "Stay logged in").
- Send notifications only when needed: one message per event, no duplicates in a toast and inline.

**Accessibility**
- Inline and toast notifications use `role="status"` (polite); use `role="alert"` only for errors that need immediate attention. Actionable notifications move focus to themselves only when `hasFocus` is set, so reserve that for messages the person must act on.
- Every close button has an accessible name; the action button names the action ("Undo", "Update card").
- Give people enough time: anything with an action, and any error, must not auto-dismiss. A modal traps focus and returns it to the trigger when closed.
