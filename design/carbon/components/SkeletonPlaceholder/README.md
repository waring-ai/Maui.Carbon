# SkeletonPlaceholder

An animated grey block standing in for a chart, image, tile or any non-text content while it loads.

Hand-written from `packages/styles/scss/components/skeleton-styles` and `utilities/_skeleton.scss` (Carbon v11.99.0).

**When to use**
- The first load of containers and visual content: charts, thumbnails, avatars, tiles on a dashboard.
- Paired with SkeletonText inside the same card so the whole layout appears at once.

**When not to use**
- Text that is loading: use SkeletonText.
- A user action is processing: use InlineLoading or Loading. Progress is measurable: use a ProgressBar.
- Buttons, inputs and other action components: render them (disabled if needed) instead.

**You provide:** nothing required; it is 100px by 100px by default. Size it with `style` (`width`, `height`) or a `className` that sets `inline-size` / `block-size` so it matches the content it will become.

**Example**
```jsx
const { SkeletonPlaceholder, SkeletonText } = window.Carbon;
<div aria-busy="true" aria-label="Loading CPU usage">
  <h4>CPU usage, last 24 h</h4>
  <SkeletonPlaceholder style={{ width: '100%', height: 120 }} />
  <SkeletonText width="60%" />
</div>
```

**Rules**
- Background is `skeleton-background` with a `skeleton-element` sweep in a 3s loop (same `cds--skeleton` keyframes as SkeletonText); square corners.
- Keep proportions equal to the final content so the layout does not jump when it loads. Not every element needs one: a fixed-size empty box is fine for an image.
- Not interactive: pointer events are disabled and hover / focus change nothing. `prefers-reduced-motion` stops the sweep.

**Accessibility**
- The block is an empty decorative `div`; screen readers ignore it.
- Set `aria-busy="true"` (and an `aria-label` naming what is loading) on the region, and remove it once content arrives.
