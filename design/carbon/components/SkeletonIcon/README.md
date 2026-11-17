# SkeletonIcon

A 16x16px pulsing placeholder shown where an icon will appear once content loads.

Hand-written from `packages/styles/scss/components/skeleton-styles/_skeleton-styles.scss` and `packages/react/src/components/SkeletonIcon` (Carbon v11.99.0).

**When to use**
- In a loading layout, where an icon (a resource type, a status glyph) will appear, next to `SkeletonText` for its label.

**When not to use**
- Images, thumbnails or larger blocks: use `SkeletonPlaceholder`.
- Whole tables: use `DataTableSkeleton`.
- Short waits after a user action: use `InlineLoading` or `Loading`.

**You provide:** optionally `className` or `style` to size it for 20/24/32px icons; other attributes go on the `<div>`.

**Example**
```jsx
const { SkeletonIcon, SkeletonText } = window.Carbon;
<div className="row"><SkeletonIcon /><SkeletonText width="60%" /></div>
```

**Rules**
- `skeleton-background` fill with an animated `skeleton-element` sweep; no border, no pointer events.
- Inline-block, so it lines up with text and other skeletons. Size it to the icon it stands for so nothing shifts when content arrives.
- Animation stops under `prefers-reduced-motion`.
- Use with SkeletonText/SkeletonPlaceholder in the same loading layout; do not mix skeletons with real content in one item.

**Accessibility**
- Purely visual: no role, no text, not focusable. Mark the loading region with `aria-busy="true"` (or a visually hidden "Loading" message) so assistive tech knows content is on its way.
