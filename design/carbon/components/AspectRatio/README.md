# AspectRatio

A box that keeps a fixed width-to-height ratio at any width. Exports `AspectRatio`.

Hand-written from `packages/styles/scss/components/aspect-ratio/_aspect-ratio.scss` and `react/src/components/AspectRatio/AspectRatio.tsx` (Carbon v11.99.0).

**When to use**
- Media and placeholder areas that must line up in a row whatever the source size: tile thumbnails, video, charts, map previews.
- Fluid boxes on the grid; the 2x Grid recommends 1:1, 2:1, 2:3, 3:2, 4:3 and 16:9.

**When not to use**
- Content-driven boxes (text cards, forms). Let them size to their content; a fixed ratio clips or leaves gaps.
- A fixed pixel size. Set width and height directly.
- Replacing the native `aspect-ratio` CSS property in your own styles; AspectRatio exists to match Carbon markup.

**You provide:**
- `ratio` (default `1x1`; also `16x9`, `9x16`, `2x1`, `1x2`, `4x3`, `3x4`, `3x2`, `2x3`).
- `children`: the content, positioned to fill the box (see Rules).
- `as` (default `div`; for example `figure`, `section`), `className`; other attributes pass through.

**Example**
```jsx
const { AspectRatio } = window.Carbon;
<AspectRatio ratio="16x9">
  <img src={thumbnailUrl} alt="Cluster topology for prod-eu-1" style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover' }} />
</AspectRatio>
```

**Rules**
- The box is as wide as its container; set a width on a wrapper (or a grid Column) to size it. Height follows width.
- Height comes from a floated `::before` with `padding-block-start` as a percentage of the width (`16x9` is 56.25%, `4x3` 75%, `1x2` 200%); `::after` clears it. The element itself is `position: relative`.
- Children flow beside the float, so to fill the box position the child absolutely (`position: absolute; inset: 0`), as the preview does, and use `object-fit: cover` on images.
- AspectRatio adds no background, border or padding; color it with layer tokens yourself.

**Accessibility**
- No role is added. Give images real `alt` text (or `alt=""` when decorative) and use `as="figure"` with a caption when the media needs one.
- Content clipped by the ratio is still in the DOM; do not hide meaningful text inside a box that is too small for it.
