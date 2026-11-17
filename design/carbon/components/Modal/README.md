# Modal

A dialog over a scrim that blocks the page until the user responds. `Modal` is the all-in-one form (heading, body, optional footer buttons); `ComposedModal` with `ModalHeader`, `ModalBody` and `ModalFooter` is the composable form of the same dialog.

Hand-written from `packages/styles/scss/components/modal` and `packages/react/src/components/Modal` / `ComposedModal` (Carbon v11.99.0). Left out: AI label/decorator slot, `ModalPresence`/dialog-element feature flags, the tooltip on the close button (a native `title` is used), `hasScrollingContent` overflow detection beyond a height check, `ModalWrapper`, the progress (multi-step) modal as a built-in (compose it with `ComposedModal` and your own footer buttons).

**When to use**
- Confirm a decision the user started, especially a destructive or irreversible one (`danger`).
- Ask for a small amount of input that blocks the current task, such as naming a new API key.
- Show urgent information the user must read before going on (passive modal).

**When not to use**
- Non-critical messages: use a `ToastNotification` or `InlineNotification`.
- Repeated tasks or long, complex flows: put them on the page or on a full page instead.
- Extra detail or controls next to a trigger: use a `Toggletip` or `Popover`.

**You provide (`Modal`):** `open`, `modalHeading`, optional `modalLabel` (small text above the heading), children for the body, and for a transactional modal `primaryButtonText`, `secondaryButtonText`, `onRequestSubmit` (primary button), `onRequestClose` (close button, Escape, secondary button; `onSecondarySubmit` replaces the secondary click). Options: `danger` (danger primary button, initial focus on the secondary one), `passiveModal` (no footer), `size` xs | sm | md | lg (default md), `secondaryButtons` (up to two secondary buttons: three-button footer), `primaryButtonDisabled`, `loadingStatus` inactive (default) | active | finished | error with `loadingDescription`, `loadingIconDescription`, `onLoadingSuccess` (an `InlineLoading` replaces the primary label and the footer buttons are disabled), `hasScrollingContent`, `isFullWidth`, `alert` (alertdialog role), `shouldSubmitOnEnter`, `preventCloseOnClickOutside`, `selectorPrimaryFocus`, `launcherButtonRef`, `closeButtonLabel` (default `Close`), `aria-label`, `className`, `onKeyDown`.

**You provide (`ComposedModal`):** `open`, `size`, `danger`, `onClose` (return `false` to stay open), `aria-label`, `containerClassName`, `preventCloseOnClickOutside`, `selectorPrimaryFocus`, `launcherButtonRef`; children `ModalHeader` (`title`, `label`, `iconDescription`, `buttonOnClick`), `ModalBody` (`hasForm` for form fields, `hasScrollingContent`, `aria-label`) and `ModalFooter` (`primaryButtonText`, `secondaryButtonText`, `onRequestSubmit`, `onRequestClose`, `secondaryButtons`, `primaryButtonDisabled`, `danger`, or custom `children`). The footer secondary button and the header close button close the modal through `ComposedModal`.

**Preview aid:** `inline` (alias `static`, plus `inlineHeight`, default `24rem`) renders the modal `position: absolute` inside a bordered, fixed-height, `position: relative` frame so it can be shown in a page or card. It also skips initial focus and the body scroll lock. Never use it in a product: a real modal is `position: fixed` over the whole viewport.

**Example**
```jsx
const { ComposedModal, ModalHeader, ModalBody, ModalFooter, TextInput } = window.Carbon;
<ComposedModal open={open} size="sm" onClose={() => setOpen(false)}>
  <ModalHeader label="Access" title="Create API key" />
  <ModalBody hasForm>
    <TextInput id="key-name" labelText="Key name" helperText="Say where the key is used, for example ci-deploy." />
  </ModalBody>
  <ModalFooter primaryButtonText="Create key" secondaryButtonText="Cancel" onRequestSubmit={createKey} />
</ComposedModal>
```

**Rules**
- Scrim is `overlay`; container is `layer-01` with a 1px `border-subtle-01`; fields in the body use the `-02` layer tokens (the body remaps the `-01` custom properties, as `Layer` does). Square corners, no shadow.
- Container width follows the breakpoints: below 672px full width, then 84% (md), 60% (lg), 48% (xlg) of the scrim; `xs` 48/32/24%, `sm` 60/42/36%, `lg` 96/84/72%. Max height 90/84% (less for xs and sm). Use xs and sm for short messages, md for most dialogs, lg for complex content such as a table; when the body scrolls a lot, move up a size or use a page.
- Header: label `label-01` in `text-secondary`, heading 20px/400 in `text-primary`, 16px padding, 48px reserved for the 48px close button. Body is `body-01` in `text-primary`, 16px side padding, 48px bottom padding. Footer is 64px high, buttons are 64px high, flex 50% each (25% each in the three-button form), label top-left aligned, no gaps.
- Header and footer stay fixed; only the body scrolls (vertically, never horizontally), with a fade at the bottom.
- Content: the heading is a short verb phrase that matches the button that opened the modal ("Create API key"). Button labels are specific verbs ("Delete bucket", not "OK"); the secondary button is on the left, primary on the right. In a three-button footer only the rightmost is primary.
- A transactional modal does not close on a scrim click; a passive one does, unless `preventCloseOnClickOutside`. The close icon closes without submitting. Validate input before closing and show errors inline; keep the modal open.
- For short waits after submit use `loadingStatus` on the primary button; for long ones show a `Loading` overlay.
- Use `danger` only for destructive confirmations.

**Accessibility**
- The container is `role="dialog"` (or `alertdialog` with `alert`, described by the body) with `aria-modal="true"`, named by the label/heading or `aria-label`.
- Initial focus: the `selectorPrimaryFocus` element (`[data-modal-primary-focus]` by default), else the secondary button of a danger modal, else the first enabled field in the body, else the primary (last enabled) footer button, else the close button.
- Tab and Shift+Tab wrap inside the dialog; Escape requests close; focus returns to `launcherButtonRef` or to the element focused before opening. The page behind does not scroll (`cds--body--with-modal-open`).
- A scrolling body becomes a focusable `role="region"` named like the dialog, so keyboard users can scroll it.
- Open modals only from a user action, never on page load. The close button is named by `closeButtonLabel` / `iconDescription`.
