# SkeletonText

An animated grey bar standing in for a line, heading or paragraph of text while it loads.

Hand-written from `packages/styles/scss/components/skeleton-styles` and `utilities/_skeleton.scss` (Carbon v11.99.0). Paragraph line widths are a fixed repeating pattern (100/85/95/75/90%) instead of Carbon's random widths, so output is stable.

**When to use**
- The first load of a page or region, for text that comes from data: names, values, descriptions in cards, lists and tables.
- Together with SkeletonPlaceholder for containers such as charts and tiles.

**When not to use**
- A user action is processing: use InlineLoading or Loading.
- Progress can be measured: use a ProgressBar.
- Labels and headings that are not data: render them right away. Never use it for toasts, menus, dropdown items or modals themselves.

**You provide:** nothing required.
- `width` (CSS length or percent, default `100%`; in paragraph mode a non-100% width applies to every line).
- `heading` for the taller 24px heading bar.
- `paragraph` with `lineCount` (default 3) for several lines.
- `className`.

**Example**
```jsx
const { SkeletonText } = window.Carbon;
<div aria-busy="true" aria-label="Loading cluster summary">
  <SkeletonText heading width="55%" />
  <SkeletonText paragraph lineCount={2} />
</div>
```

**Rules**
- Bar height 16px (24px for headings), 8px bottom margin, background `skeleton-background` with a `skeleton-element` shape sweeping across it in a 3s loop (`cds--skeleton` keyframes: scaleX in from the start, out to the end).
- Size skeletons like the content they replace and keep the region the same size when it loads, so nothing shifts.
- Show skeletons for a few seconds at most; replace them as content arrives (progressive loading).
- No pointer or focus behaviour; hover and focus never change it. `prefers-reduced-motion` stops the sweep.
- Do not mix with spinners in the same region.

**Accessibility**
- Skeletons are decorative `<p>` elements with no text, so screen readers skip them.
- Mark the loading region with `aria-busy="true"` (and an `aria-label` that says what is loading), then remove it when the content is in; announce failures with a notification.
