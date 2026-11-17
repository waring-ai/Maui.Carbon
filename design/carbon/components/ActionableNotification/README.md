# ActionableNotification

A notification with one action button, in a toast layout (stacked, 288px wide) or an inline layout (full width, content and action on one row from 672px up).

Hand-written from `packages/styles/scss/components/notification/_actionable-notification.scss` and `Notification.tsx` (Carbon v11.99.0). Reuses the status icons of `InlineNotification` and does not edit it. Left out: the warning icon's inner black glyph, focus-sentinel spans (Tab wrapping is done on keydown instead). The non-dismissible variant is the separate `Callout` card.

**When to use**
- The user should respond to the message with one related action, such as "View logs" or "Update card".
- In-page (inline) or floating (toast) messages that must stay until the user dismisses them.

**When not to use**
- No response needed: use `ToastNotification` or `InlineNotification`.
- More than one action, or a decision that blocks work: use a `Modal`.
- Information that must always stay visible and cannot be dismissed: use `Callout`.

**You provide:** `kind` error | success | warning | warning-alt | info | info-square (default error), `title`, `subtitle`, optional `caption`, `actionButtonLabel` and `onActionButtonClick`, `inline` (default false = toast layout), `lowContrast`, `hideCloseButton`, `onClose` (return `false` to keep it), `onCloseButtonClick`, `aria-label` for the close button (default `close notification`), `statusIconDescription`, `closeOnEscape` (default true), `hasFocus` (default true), `role` (default `alertdialog`), `onKeyDown`, `className`, `style`, children (after the caption).

**Example**
```jsx
const { ActionableNotification } = window.Carbon;
<ActionableNotification inline kind="warning" lowContrast role="status" title="Payment method expires soon"
  subtitle="The card ending 4242 expires on 30 Nov 2026." actionButtonLabel="Update card" onActionButtonClick={openBilling} />
```

**Rules**
- Inline layout uses a ghost small (32px) action button; toast layout uses a tertiary small one under the text, aligned with the title (left margin 50px). Both are `Button size="sm"`.
- High contrast: `background-inverse`, `text-inverse`, 3px left border and icon in `support-*-inverse`; the action button uses `link-inverse` (ghost) or the `notification-action-tertiary-inverse*` tokens (tertiary). Low contrast: `notification-background-*`, `text-primary`, `support-*`, the inline form also gets a 1px 40%-opacity outline in the kind colour, ghost hover is `notification-action-hover`.
- Inline max widths: 288px below 672px, then 608, 736 (1056px) and 832 (1584px); minimum 288px. Toast layout is always 288px with a `0 2px 6px` shadow in `shadow`.
- Title is `heading-compact-01`, subtitle and caption `body-compact-01`; in inline layout the title and subtitle share a line and wrap together. The 48px close button sits at the right (absolute at the top right in the toast layout).
- One action only, labelled with one or two words; it usually leads to a related page or flow. Keep the subtitle to two lines.
- It stays until dismissed, even in the toast layout (no timeout). Place inline ones at the top of the section they concern.

**Accessibility**
- Default `role="alertdialog"` (named by the title, else the subtitle): on mount focus moves to the action button and Tab wraps between the action and the close button. This interrupts screen reader and keyboard users, so use it only for messages that need attention now; pass `role="status"` (as the preview does) or `hasFocus={false}` to leave focus where it is.
- Escape closes the notification while focus is inside (`closeOnEscape`).
- The close button is named by `aria-label`; the status icon by `statusIconDescription`. Focus shows a 2px inset outline (`focus-inverse`, or `focus` when low contrast).
