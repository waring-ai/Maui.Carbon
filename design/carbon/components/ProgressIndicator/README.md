# ProgressIndicator

Shows the steps of a multi-step flow and where the user is in it, horizontally or vertically.

Hand-written from `packages/styles/scss/components/progress-indicator` and `packages/react/src/components/ProgressIndicator` (Carbon v11.99.0). Left out: the overflow tooltip on long labels (the full label is in the `title` attribute), Left / Right arrow-key movement between steps, and the skeleton variant. `ProgressStep` is exported with it and documented here.

**When to use**
- Linear flows of three or more steps that must be done in order: creating a cluster, onboarding, generating an API key.
- Alongside Back / Next buttons, validating each step before moving on.

**When not to use**
- Fewer than three steps, steps that can be done in any order, or a step count that changes with the answers: use headings or Tabs.
- Showing how much of a task has run: use a ProgressBar.

**You provide:**
- `ProgressIndicator` wrapping `ProgressStep`s, with `currentIndex` (zero-based; earlier steps become complete, that one is current, later ones incomplete).
- `onChange(index)` makes steps clickable (except the current and disabled ones) · `vertical` · `spaceEqually` (horizontal only; steps share the width) · `className`.
- `ProgressStep`: `label` (required), `secondaryLabel` (small text under the label, e.g. "Optional" or the error hint), `description` (icon title), `invalid`, `disabled`, `complete` (force the complete state on a later step), `className`.

**Example**
```jsx
const { ProgressIndicator, ProgressStep } = window.Carbon;
<ProgressIndicator vertical currentIndex={step} onChange={(i) => i < step && setStep(i)}>
  <ProgressStep label="Choose plan" />
  <ProgressStep label="Configure nodes" />
  <ProgressStep label="Networking" invalid={subnetError} secondaryLabel={subnetError ? 'Fix the subnet' : undefined} />
  <ProgressStep label="Add-ons" secondaryLabel="Optional" />
  <ProgressStep label="Review" />
</ProgressIndicator>
```

**Rules**
- Step states and icons: complete `checkmark--outline` (`interactive`), current `incomplete` (`interactive`), incomplete `circle-dash` (`icon-primary`), invalid `warning` (`support-error`), disabled `circle-dash` in `icon-disabled` with `text-disabled` label.
- The connector is a 2px line along the top edge of each horizontal step (1px along the start edge when vertical): `interactive` for complete and current, `border-subtle-01` for incomplete.
- Labels are `body-compact-01` (max 88px wide horizontally, ellipsized; 160px vertically, wrapping); hover on a clickable label underlines it in `link-primary-hover`. Secondary labels are `label-01` `text-secondary` and wrap.
- Prefer vertical when there is room (full pages, side panels, tearsheets); horizontal suits modals and narrow headers.
- Labels are one or two words, ideally verb + noun ("Choose plan"); avoid vague words like "Processing". Mark optional steps and say how to fix an invalid one in `secondaryLabel`.

**Accessibility**
- The indicator is a `ul`; each step is an `li` holding a `<button>` with a visually hidden status word (Complete, Current, Incomplete, Invalid) after the label.
- Steps are tabbable only when `onChange` is set; the current step is never tabbable. Enter / Space activate a step. Focus draws a 1px `focus` outline around the label.
- Disabled steps use the `disabled` attribute, so they are skipped by Tab and cannot be clicked.
- `description` sets the icon's accessible title; keep it consistent with the visible label.
