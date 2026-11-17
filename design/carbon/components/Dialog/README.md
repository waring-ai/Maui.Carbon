# Dialog

A dialog built on the native `<dialog>` element (Carbon v11.99 preview): `modal` opens it with `showModal()` over a backdrop and makes the page inert; otherwise it opens with `show()` as a non-modal panel that leaves the page usable.

Hand-written from `packages/styles/scss/components/dialog` and `packages/react/src/components/Dialog` (Carbon v11.99.0). Left out: the `enable-presence` animation variant and the AI-gradient `cds--dialog--danger` container styling. Added: an `inline` prop that lays a non-modal dialog out in the page flow (previews and docs) and a `size` prop on the Modal size scale (Carbon's preview Dialog has a single width).

**When to use**
- Modal: the user must respond before continuing a task they started (confirm a destructive or irreversible action, provide a missing value), or must see an urgent error about their current work.
- Non-modal: optional or repeatable tasks where the user still needs the page (find and replace, contextual help).
- Passive (no footer) shows information only; transactional has Cancel plus one primary action; danger is transactional with a destructive primary; acknowledgment has one button.

**When not to use**
- For non-critical or system-generated messages: use an `InlineNotification` or `ToastNotification`. For long or complex flows, or content that needs information from elsewhere: use a full page.
- For tasks people repeat often: let them complete the task on the page. Do not nest dialogs, open one without a user action, or fill it with tabs, accordions or large tables.
- `Modal` is the established Carbon modal; use `Dialog` when you want the native element (top layer, `::backdrop`, built-in focus trapping) or a non-modal panel.

**You provide:**
- `Dialog`: `open` (default false) and `onRequestClose` (the parent owns the state; called on backdrop click for modal dialogs and on Escape) · `modal` (default false) · `size` `xs` | `sm` | `md` (default) | `lg` · `role` `'alertdialog'` for interruptive confirmations · `ariaLabel` / `ariaLabelledBy` (defaults to the `DialogTitle` id) / `ariaDescribedBy` · `focusAfterCloseRef` · `onCancel` (sees Escape first; `preventDefault()` keeps it open) · `onClose`, `onClick` · `inline` · `className`.
- `DialogHeader` with `DialogSubtitle` (small label above the title), `DialogTitle` (`id` optional) and `DialogControls` holding a `DialogCloseButton` (`onClick`, `label` default "Close").
- `DialogBody` (`hasScrollingContent` to force the scrolling treatment; otherwise detected from overflow).
- `DialogFooter`: `primaryButtonText` (default "Save"), `secondaryButtonText` (default "Cancel"; `''` hides it) or `secondaryButtons` (two `{ buttonText, onClick }` for a three-button footer), `onRequestSubmit`, `onSecondarySubmit` (falls back to `onRequestClose`), `primaryButtonDisabled`, `danger`, `loadingStatus` `inactive` | `active` | `finished` | `error` with `loadingDescription`, `loadingIconDescription`, `onLoadingSuccess`, or your own buttons as children.

**Example**
```jsx
const { Dialog, DialogHeader, DialogSubtitle, DialogTitle, DialogControls, DialogCloseButton, DialogBody, DialogFooter } = window.Carbon;
<Dialog open={open} modal size="xs" role="alertdialog" onRequestClose={close}>
  <DialogHeader>
    <DialogSubtitle>logs-archive-prod</DialogSubtitle>
    <DialogTitle>Delete bucket?</DialogTitle>
    <DialogControls><DialogCloseButton onClick={close} /></DialogControls>
  </DialogHeader>
  <DialogBody><p>All 1,204 objects in the bucket are deleted. This cannot be undone.</p></DialogBody>
  <DialogFooter danger primaryButtonText="Delete" onRequestClose={close} onRequestSubmit={deleteBucket} />
</Dialog>
```

**Rules**
- Surface is `layer-01` with `text-primary`; the modal backdrop is `overlay`. Height is capped at 90vh / 84vh (from 42rem / 66rem).
- Sizes (share of the viewport at 42rem / 66rem / 82rem and up): xs 48 / 32 / 24%, sm 60 / 42 / 36%, md 84 / 60 / 48% (48rem wide), lg 96 / 84 / 72%. Inline they are 24, 36, 48 and 72rem. Use xs or sm for short messages and confirmations, md or lg for forms and data; if the body scrolls a lot, go up a size.
- Header: 16px padding, 48px right padding for the close button; subtitle is label-01 in `text-secondary`, title is heading-03 (20px).
- Body: body-01, padding 8px 16px 48px; content is wrapped in a Layer, so fields inside use the next layer's tokens. Text should take about 80% of the width. When it overflows, the body scrolls between a fixed header and footer and fades out at the bottom; it never scrolls horizontally.
- Footer is 64px high; buttons fill it (50% each, 25% each with three, a single button takes the right half) with the label at the top-left. Cancel on the left, the one primary action on the right.
- Close button: 48x48 ghost, 20px close icon, `layer-hover-01` on hover, 2px `focus` border on focus.
- Opens with a 240ms fade and 24px slide (`ease-entrance-expressive`); motion is removed for prefers-reduced-motion.
- Content: the title is a short phrase that matches the button or menu item that opened it ("Delete bucket?", "Rename cluster"); buttons are specific verbs ("Delete", "Rename"), never "OK" or "Done". Validate before closing: keep the dialog open and show the error on the field. While a submit runs, set `loadingStatus` (the primary is disabled and shows `InlineLoading`).

**Accessibility**
- The native `dialog` element supplies `role="dialog"` (or your `role="alertdialog"`); `modal` also makes the rest of the page inert and traps Tab inside. It is labelled by the `DialogTitle` (`aria-labelledby`) unless you pass `ariaLabel`.
- Escape fires `onRequestClose` (non-modal too); the close button must call it as well. Backdrop clicks close modal dialogs.
- Focus: on open the browser focuses the first focusable element; put the first field first. With `danger`, the secondary (Cancel) button takes focus so Enter cannot delete by accident. On close the browser returns focus to the element that opened a modal dialog; use `focusAfterCloseRef` to choose another target (it fires only when the dialog closes, not on a first render that starts closed).
- A scrolling body becomes a focusable `region` (`tabindex="0"`) so keyboard users can scroll it. The footer sets `aria-busy` while loading.
