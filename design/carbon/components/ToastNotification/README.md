# ToastNotification

A transient message that floats above the page to report the result of an action. It has a status icon, title, subtitle, optional caption and a close button, and can close itself after a timeout.

Hand-written from `packages/styles/scss/components/notification/_toast-notification.scss` and `Notification.tsx` (Carbon v11.99.0). Left out: placement and stacking (position it yourself, usually fixed top-right in a column with 8px gaps), slide-in motion, the dev warning against interactive children, the warning icon's inner black glyph (the exclamation mark shows the background through).

**When to use**
- Confirm or report the outcome of something the user did or a background process finished ("Deployment complete", "Backup failed").
- Time-based, non-modal news that does not need a response.

**When not to use**
- The user must act on the message: use `ActionableNotification` (one action) or a `Modal` (a decision).
- The message belongs to a section, form or task on the page: use `InlineNotification`.
- Information that must stay visible with the content: use `Callout`.

**You provide:** `kind` error | success | warning | warning-alt | info | info-square (default error), `title`, `subtitle`, optional `caption` (such as a timestamp), `lowContrast`, `timeout` in milliseconds (0, the default, never closes), `hideCloseButton`, `onClose` (button or timeout; return `false` to keep it), `onCloseButtonClick`, `aria-label` for the close button (default `close notification`), `statusIconDescription` (default `<kind> icon`), `role` (default `status`), `className`, `style`. Children render under the caption; keep them to text and links.

**Example**
```jsx
const { ToastNotification } = window.Carbon;
<ToastNotification kind="success" title="Deployment complete" subtitle="checkout-api v2.4.1 is live in prod-us-east." caption="2 minutes ago" timeout={5000} onClose={() => removeToast(id)} />
```

**Rules**
- High contrast: `background-inverse` with `text-inverse`, a 3px left border and icon in the `support-*-inverse` colour of the kind. Low contrast: `notification-background-*` fills, `text-primary`, and `support-*` colours for border and icon. Pick one contrast style for all toasts in a product; low contrast when unsure.
- Fixed 288px wide (352px from 1584px up), height from content, `0 2px 6px` shadow in `shadow`. 3px border plus 13px padding, 16px icon margin, 16px vertical padding.
- Title is `heading-compact-01`; subtitle and caption are `body-compact-01`; caption sits under the subtitle.
- Placement: top right of the viewport, newest on top, stacked with an 8px (`spacing-03`) gap.
- Content: short title without end punctuation that says what happened (an error title says what failed); one or two short sentences of subtitle that do not repeat the title, and for errors say what to do. Use the same timestamp format everywhere.
- Auto-dismiss (`timeout`, about 5 seconds) only messages that can also be found elsewhere; do not auto-dismiss errors. Always keep the close button unless the toast closes itself.
- Pick one kind per meaning: error for failures, success for completion, warning for risk, info for neutral news.

**Accessibility**
- `role="status"` (polite live region) by default, so screen readers announce it without moving focus; pass `role="alert"` only for urgent errors.
- The status icon carries `statusIconDescription` as its accessible name; the close button is named by `aria-label`.
- No focus trap and no interactive content except links: a toast that times out must never hold an action the user needs.
- The 48px close button shows a 2px inset `focus-inverse` outline on focus (`focus` when low contrast).
