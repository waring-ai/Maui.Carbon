# AISkeleton

Loading placeholders for content an AI is generating: `AISkeletonText`, `AISkeletonPlaceholder` and `AISkeletonIcon` are the regular skeletons with a blue tint, rounded corners and a fast sliding shimmer.

Hand-written from `packages/styles/scss/components/skeleton-styles/_ai-skeleton-styles.scss` and `packages/react/src/components/AISkeleton` (Carbon v11.99.0). `AISkeletonText` and `AISkeletonPlaceholder` render the bundle's `SkeletonText` / `SkeletonPlaceholder` with the `--ai` modifier class. The Carbon `ai-skeleton-background` / `ai-skeleton-element-background` tokens are not in this bundle: the fill uses `tag-background-blue` (exact in white/g10, a brighter blue than Carbon in g90/g100) and the shimmer is `interactive` at 50% opacity.

**When to use**
- While an AI result is being generated: a summary, a recommendation, a generated chart or icon.
- In a container that already carries an AILabel, so the user knows what is loading.

**When not to use**
- Ordinary page or data loading. Use SkeletonText, SkeletonPlaceholder or SkeletonIcon.
- Short waits after a user action on a control (saving, submitting). Use InlineLoading.
- Indeterminate long jobs with progress to report. Use a ProgressBar or Loading.

**You provide:**
- `AISkeletonText`: `paragraph` with `lineCount` (default 3) for several lines of varying width, `width` (default `100%`), `heading` for a 24px line, `className`.
- `AISkeletonPlaceholder`: size it with `style` or `className` (default 100 x 100px).
- `AISkeletonIcon`: 16 x 16px by default; resize with `style`; `className`.

**Example**
```jsx
const { AISkeletonText, AISkeletonIcon } = window.Carbon;
<div aria-busy="true" aria-label="Generating health summary">
  <AISkeletonIcon style={{ width: 24, height: 24 }} />
  <AISkeletonText heading width="60%" />
  <AISkeletonText paragraph lineCount={3} />
</div>
```

**Rules**
- Text lines are 16px tall (24px heading) with a 16px (fully rounded) radius and 8px between lines; placeholders have an 8px radius; icons a 2px radius.
- The shimmer is a transparent to blue to transparent gradient sliding left to right every 1250ms, ease-in-out; it stops under `prefers-reduced-motion`. In forced-colors mode the fill becomes `CanvasText` with a `Canvas` shimmer.
- Use AI skeletons only where the pending content is AI-generated; use the plain skeletons elsewhere. Match the shape and size of the content that will replace them so the layout does not jump.

**Accessibility**
- Skeletons are decorative and have no text. Put `aria-busy="true"` on the region that is loading and remove it when the content arrives, or announce progress with a live status message ("Generating summary").
- Do not move focus into a skeleton; keep focus where the user started the request.
