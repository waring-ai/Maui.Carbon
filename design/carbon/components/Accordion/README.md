# Accordion

A vertical list of headings that each expand to reveal a panel of content, for hiding secondary detail until the reader asks for it.

Hand-written from `packages/styles/scss/components/accordion` and `packages/react/src/components/Accordion` (Carbon v11.99.0). Left out: `renderToggle`/`renderExpando`, `onAnimationEnd` handling, the skeleton variant, RTL overrides, and wrapping the heading button in an `h1`-`h6` element.

**When to use**
- Related sections that people only need some of: FAQs, settings groups, details of a resource.
- Shortening a long page or fitting content into a small space such as a side panel.

**When not to use**
- People will read all the content anyway: show it on the page, the extra click only slows them down.
- Hierarchies deeper than one level: use a TreeView.
- Switching between peer views of the same object: use Tabs or a ContentSwitcher.

**You provide:**
- `Accordion` wrapping one or more `AccordionItem`s.
- `Accordion`: `align` end | start (default end: chevron at the end edge; start puts it before the title) · `size` sm | md | lg (heading height 32/40/48px, default md) · `isFlush` (no outer padding, only with `align="end"`) · `disabled` (all items; an item's own `disabled` wins) · `ordered` (renders an `<ol>`) · `className`.
- `AccordionItem`: `title` (the heading, string or node) and the panel as `children` · `open` (sets the expanded state and re-syncs when changed) · `onHeadingClick({ isOpen, event })` · `onClick(event)` · `disabled` · `aria-label` (name of the heading button when the title is not text) · `className`.

**Example**
```jsx
const { Accordion, AccordionItem } = window.Carbon;
<Accordion>
  <AccordionItem title="How is node usage billed?" open>
    <p>Worker nodes are billed per hour from the moment they are provisioned until they are deleted.</p>
  </AccordionItem>
  <AccordionItem title="Can I change a cluster's region?">
    <p>No. Create a cluster in the new region, move your workloads, then delete the old cluster.</p>
  </AccordionItem>
</Accordion>
```

**Rules**
- End alignment is the default and keeps titles aligned with the rest of the page; use `align="start"` only for tree-like content and use one alignment per page. Use `isFlush` in narrow spaces so text and dividers line up with neighbouring components.
- The chevron is `chevron--right` rotated: down when closed, up when open (`icon-primary`, 150ms `duration-fast-02` standard easing).
- Dividers are 1px `border-subtle-01`; hover fills the heading with `layer-hover-01` and tints the neighbouring dividers the same. Focus is a 2px `focus` ring drawn with an inset box shadow.
- Open panels get 8px top and 24px bottom padding; the content is inset 16px, with 48px (>=480px) or 25% (>=640px) of end padding to keep line length short. Several items can be open at once.
- Disabled headings use `text-disabled` and `icon-disabled` on a transparent fill, no hover, `cursor: not-allowed`.
- Titles are `body-01` (14px), short and descriptive, one line; no interactive controls in the heading. Panels may hold text, subheadings and form fields but should not scroll on their own or horizontally.

**Accessibility**
- The list is a `ul` (or `ol`); each heading is a `<button>` with `aria-expanded` and `aria-controls` pointing at its panel.
- Keyboard: Tab / Shift+Tab move between headings, Enter or Space toggles, Escape closes the open item.
- Disabled items use the `disabled` attribute, so they leave the tab order and report collapsed.
- Pass `aria-label` on an item whose `title` is not plain text.
