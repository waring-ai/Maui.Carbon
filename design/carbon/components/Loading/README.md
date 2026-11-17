# Loading

A spinner that shows the app or a region is busy, optionally covering it with a dimmed overlay.

Hand-written from `packages/styles/scss/components/loading` and `packages/react/src/components/Loading` (Carbon v11.99.0).

**When to use**
- A user action is processing and the page or a key region must wait: saving, recalculating, applying a change (expected wait over about 3 seconds).
- The small spinner next to a label for a localized, background refresh.

**When not to use**
- Content loading into the page for the first time: use SkeletonText and SkeletonPlaceholder.
- An action tied to one button or field: use InlineLoading.
- Measurable progress: use a ProgressBar. Never show several spinners at once.

**You provide:**
- `active` (default true; false winds the spinner down and stops it) · `small` for the 16px spinner · `withOverlay` (default true) · `description` (the accessible title, default "loading") · `id` · `className`.
- Without `withOverlay` it renders only the spinner so you can place it inside a region.

**Example**
```jsx
const { Loading } = window.Carbon;
{recalculating && <Loading description="Recalculating billing summary" />}
<Loading small withOverlay={false} description="Refreshing metrics" />
```

**Rules**
- Large spinner is 88px, 10-unit `interactive` arc (81% of the ring) rotating once every 690ms; small is 16px with a 48% arc over a `layer-accent-01` track.
- Large is for full-page takeovers, sections, modals and tiles: center it and use the overlay. Small is for inline use next to the trigger: no overlay, and disable related actions while it runs.
- The overlay is `position: fixed`, covers the viewport at `z-index` 8000 and fills with `overlay`; it does not trap focus, so disable the content beneath it yourself. To cover only one region, give the region `position: relative` and a transform so the fixed overlay is contained.
- A short label ("Loading data...") may sit near a small spinner; the large spinner carries no visible text.
- `prefers-reduced-motion` removes the stroke animation.

**Accessibility**
- The spinner is `aria-live="assertive"` with `aria-atomic` while active and `off` when stopped; the SVG has a `<title>` from `description`, so name it after what is loading.
- Set `aria-busy="true"` on the region it covers and disable controls under the overlay; move focus back to a sensible place when loading ends.
