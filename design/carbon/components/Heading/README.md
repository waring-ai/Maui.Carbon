# Heading

`Heading` renders the right `h1`-`h6` for its place in the document: each enclosing `Section` adds one level, so content can be moved or reused without hand-picking heading tags.

Hand-written from `packages/react/src/components/Heading` (Carbon v11.99.0). Like Carbon, the component has no styles of its own. This bundle adds Carbon's `cds--type-<style>` type classes (from `@carbon/type`) so you can style a heading by class.

**When to use**
- Reusable components that contain a heading (cards, panels, widgets) and appear at different depths of different pages.
- Page structure built from nested regions, so the outline stays correct when sections move.

**When not to use**
- To make text big or bold. Use a type class on a `p` or `span`; headings are for structure.
- Labels of form controls or table headers. Use the field's label, a FormGroup legend or the DataTable header.

**You provide:**
- `Heading`: `children`, `className` and any heading attribute (`id`, `style`); the ref is forwarded.
- `Section`: `children`, `as` (element, default `section`), `level` (1-6, to force the level for its subtree instead of parent + 1), `className` and any attribute; the ref is forwarded.

**Example**
```jsx
const { Section, Heading } = window.Carbon;
function NodePoolCard({ name, desc }) {
  return <Section as="div"><Heading className="cds--type-heading-compact-01">{name}</Heading><p>{desc}</p></Section>;
}
<>
  <Heading className="cds--type-heading-04">prod-eu-1</Heading>{/* h1 */}
  <Section>
    <Heading className="cds--type-heading-02">Worker pools</Heading>{/* h2 */}
    <NodePoolCard name="default" desc="3 nodes per zone" />{/* its Heading is h3 */}
  </Section>
</>
```

**Rules**
- A Heading outside any Section is `h1`. Each nested Section adds 1, up to `h6`.
- Give the heading a type style by its role using productive headings: `heading-05` (2rem/400) for page titles, `heading-04` (1.75rem/400), `heading-03` (1.25rem/400), `heading-02` (1rem/600), `heading-01` (0.875rem/600), and `heading-compact-01` / `-02` in dense UI. Use the class `cds--type-heading-04` etc., or write the values literally.
- Headings use `text-primary`; supporting copy under them uses `body-01` in `text-secondary`. Keep them in sentence case.
- Don't skip levels for size. Pick the level from the structure and change the look with a type class.
- Use `as="div"` on a Section when the extra `<section>` element is not wanted.

**Accessibility**
- Screen reader users navigate by heading level, so the outline must be logical: one `h1` per page, then no skipped levels. Section nesting gives you that automatically.
- A `<section>` is only a landmark when it has an accessible name; add `aria-labelledby` pointing at its Heading's `id` for regions users should jump to, or use `as="div"`.
- Levels stop at `h6`; restructure content that would need more.
