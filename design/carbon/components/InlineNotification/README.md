# InlineNotification

An in-context message about the state of the page or of a task, shown near what it is about. Square, 48px minimum height, 3px colored start edge, status icon, a semibold title and a subtitle, and an optional close button.

Hand-written from `packages/styles/scss/components/notification/_inline-notification.scss` and `themes/src/component-tokens/notification`. The warning glyphs draw the "!" as a cut-out (Carbon fills it black). The `actions` slot is kept for compatibility; for a notification with an action, use ActionableNotification.

**When to use**
- Feedback on an action or a system status inside a task flow: a failed deployment, a quota nearly reached, a form-level error summary.
- Short messages of one or two sentences that should stay until the person dismisses them or fixes the cause.

**When not to use**
- Transient confirmation that can disappear on its own: use a ToastNotification.
- The person must act, or the message is long and needs a "View more": use an ActionableNotification.
- Contextual guidance that is part of the page and cannot be dismissed: use a Callout.
- Field-level errors alone: use the field's own `invalid` state (an inline notification may summarize them above the submit buttons).

**You provide:**
- `kind` error | success | warning | warning-alt | info (default) | info-square.
- `title` (the outcome, no period: "Deployment failed") and `subtitle` (text or a node, for example text with an inline Link: the reason and next step); `children` render after the subtitle.
- `lowContrast`; `hideCloseButton` (when the person must read or act on it).
- `onClose(event)`: return `false` to keep it open; `onCloseButtonClick(event)` fires on every close-button click.
- `role` status (default) | alert | log; `ariaLabel` (close button name, default "Close notification"); `statusIconDescription` (icon name, default "<kind> icon"); `actions` (a ghost Button); `className`.

**Example**
```jsx
const { InlineNotification, Link } = window.Carbon;
<InlineNotification
  kind="error" title="Deployment failed"
  subtitle={<>Image checkout-api:2.4 was not found. <Link href="/registry" inline>Open the registry</Link></>}
  onClose={() => setShown(false)}
/>
```

**Rules**
- High contrast (default) is `background-inverse` with `text-inverse` and `support-*-inverse` accents; low contrast uses `notification-background-<kind>` with `text-primary`, `support-<kind>` accents and a 1px border in the kind color. Low contrast is the safer default; do not mix both styles on one surface.
- Icons: `error--filled`, `checkmark--filled`, `warning--filled`, `warning--alt--filled`, `information--filled`, `information--square--filled`, 20px. The status is carried by the icon shape and the title, never by edge color alone.
- Title states the outcome; subtitle gives the reason and the next step in one or two short sentences and does not repeat the title. Errors always say what to do.
- Links inside use `link-inverse` on high contrast (applied automatically to Link) and `link-primary` on low contrast.
- Place it at the top of the content it concerns, or in a form just above the submit buttons; it fills the container width up to 38rem. Never stack more than three; summarize instead.
- Inline notifications never dismiss themselves.

**Accessibility**
- The container has `role="status"` (polite); use `role="alert"` only for urgent errors that must interrupt. Render the notification when the event happens so it is announced; the title and subtitle are read as one sentence, so write them to read naturally together.
- The status icon has `role="img"` and a name ("error icon" or `statusIconDescription`), so the kind is not conveyed by color only.
- The close button is a `<button>` named "Close notification"; Enter or Space activate it, and Escape closes the notification while focus is inside it. Move focus somewhere sensible if the notification held focus when it closed.
