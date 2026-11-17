# InlineLoading

A small spinner with status text for a short action in progress, usually in place of the button that started it, which settles to a success or error icon.

Hand-written from `packages/styles/scss/components/inline-loading` and `packages/react/src/components/InlineLoading` (Carbon v11.99.0). Left out: the draw-on checkmark animation (a static `checkmark--filled` is shown).

**When to use**
- Create, update, delete and save actions that take a moment: replace the primary button with it until the request returns.
- Refreshing a small piece of data such as a status, or checking a value inside a field (spinner only).

**When not to use**
- Whole pages or large regions: use skeletons or Loading.
- Measurable progress: use a ProgressBar.
- Several actions at once: show one indicator for the group.

**You provide:**
- `status` active | inactive | finished | error (default active) and `description` (the status text).
- `iconDescription` (accessible title of the icon; defaults to "loading", "not loading", "finished" or "error").
- `onSuccess` (called `successDelay` ms after `status` becomes finished, default 1500) so you can reset the form, close a modal or refetch.
- `aria-live` (default assertive) · `className`.

**Example**
```jsx
const { InlineLoading, Button } = window.Carbon;
{saving === 'idle'
  ? <Button onClick={save}>Save changes</Button>
  : <InlineLoading status={saving} description={{ active: 'Saving...', finished: 'Saved', error: 'Could not save' }[saving]}
      onSuccess={() => setSaving('idle')} />}
```

**Rules**
- Active shows the small Loading spinner (16px, `interactive`); finished shows `checkmark--filled` in `support-success`; error shows `error--filled` in `support-error`; inactive winds the spinner down to nothing and keeps the text.
- Layout is a flex row, minimum height 32px, 8px between icon and text; text is `label-02` in `text-secondary`. It takes the place and alignment of what it replaces.
- Disable the other actions of the form (Cancel, inputs) while active so people cannot resubmit.
- Copy: the active text names the action ("Saving..."), the finished text confirms it ("Saved"), the error text says it failed; follow an error with an InlineNotification or field error that says what to do.
- Without `onSuccess` the finished state stays until you change it.

**Accessibility**
- The container has `aria-live="assertive"` (set `aria-live="polite"` for background refreshes), so status text changes are announced.
- Icons have titles from `iconDescription` or the status; give a spinner-only instance an `iconDescription` that says what is happening.
- When the button is replaced, focus would be lost: move it to a sensible place (the next field, the dialog, or back to the restored button).
