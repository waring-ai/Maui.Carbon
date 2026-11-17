# ProgressBar

A horizontal bar showing how far a task has progressed, or that it is running when the amount is unknown.

Hand-written from `packages/styles/scss/components/progress-bar` and `packages/react/src/components/ProgressBar` (Carbon v11.99.0). Left out: high-contrast and RTL overrides.

**When to use**
- Long or unpredictable operations that can be measured: uploads, downloads, copies, exports, provisioning a known number of nodes.
- Processes of unknown length that should still show they are running (indeterminate), switching to a value once it is known.

**When not to use**
- Waits under about 5 seconds or tied to one action: use InlineLoading or Loading.
- Content that is loading into the page: use SkeletonText and SkeletonPlaceholder.
- Steps the user completes themselves: use a ProgressIndicator.

**You provide:**
- `label` (required; what is progressing) · `value` (0 to `max`; omit for the indeterminate animation) · `max` (default 100) · `helperText`.
- `status` active | finished | error (default active): finished forces a full `support-success` bar with a check icon; error a full `support-error` bar, an error icon and `text-error` helper text.
- `size` big | small (8px or 4px track, default big) · `type` default | inline | indented (inline puts the label beside the track; indented pads label and helper text 16px) · `hideLabel` · `className`.

**Example**
```jsx
const { ProgressBar } = window.Carbon;
<ProgressBar
  label="Uploading usage-march.csv"
  value={uploadedMb} max={100}
  status={failed ? 'error' : uploadedMb === 100 ? 'finished' : 'active'}
  helperText={failed ? 'Connection lost. Check your network and try again.' : `${uploadedMb} of 100 MB`}
/>
```

**Rules**
- Track is `layer-01` (inside a `Layer` it follows that layer, so the track stays visible on cards); the fill is `interactive` and scales from the start edge over 110ms (`duration-fast-02`).
- Label is `body-compact-01`, helper text `helper-text-01` in `text-secondary`, 8px gap to the track. Labels sit above the bar (beside it when inline), helper text below, never inside the bar.
- Sizes: big for pages, dialogs and full-width areas; small for cards, data tables and side panels. Types: default for pages and dialogs, inline in tables and lists, indented in side panels and dashboard cards.
- When inline and in error, the track is hidden and only the label and status icon remain.
- A determinate bar only moves forward and never resets. Keep the label the same throughout; status comes from color, icon and helper text. Error helper text must say what went wrong and what to do.
- Do not fake progress values; indeterminate bars show no numbers.

**Accessibility**
- The track carries `role="progressbar"` with `aria-valuemin/max/now` (omitted when indeterminate), `aria-busy` (until finished) and `aria-invalid` (error), labelled by the label and described by the helper text.
- With helper text, a visually hidden polite live region says "Loading" or "Done".
- Always pass a `label`, even when it is hidden with `hideLabel`.
- Status never relies on color alone: finished and error add icons. While a region is processing, disable only what is affected.
