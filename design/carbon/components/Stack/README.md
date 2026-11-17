# Stack

Lays children out in one dimension with a consistent gap from the spacing scale. Exports `Stack`.

Hand-written from `packages/styles/scss/components/stack/_stack.scss` and `react/src/components/Stack/Stack.tsx` (Carbon v11.99.0).

**When to use**
- Even spacing between siblings in one direction: fields in a form section, a title above its content, a row of tags or buttons, items of a list.
- Instead of margins on the children, so components stay margin-free and the parent decides the rhythm.

**When not to use**
- Two-dimensional or column-aligned layout. Use Grid (or a CSS grid) instead.
- Content that has to wrap onto several lines (a long tag list). Stack never wraps; use flex-wrap.
- Buttons that belong together as one action group. Use ButtonSet, which adds the separators and ordering.

**You provide:**
- `children`.
- `orientation`: `vertical` (default, a CSS grid with row gap) or `horizontal` (an inline grid flowing in columns).
- `gap`: a spacing scale step from 1 to 13, or a CSS length string (`'3.5rem'`) for an arbitrary gap. No gap by default.
- `as` (default `div`; for example `ul`, or a component such as `Layer`), `className` and `style`; other attributes pass through.

**Example**
```jsx
const { Stack, TextInput, ButtonSet, Button } = window.Carbon;
<Stack gap={7}>
  <Stack gap={6}>
    <TextInput id="key-name" labelText="Name" helperText="Shown in audit logs" />
    <TextInput id="key-desc" labelText="Description (optional)" />
  </Stack>
  <ButtonSet><Button kind="secondary">Cancel</Button><Button>Create key</Button></ButtonSet>
</Stack>
```

**Rules**
- Steps map to `--spacing-01` to `--spacing-13`: 0.125, 0.25, 0.5, 0.75, 1, 1.5, 2, 2.5, 3, 4, 5, 6 and 10 rem (2 to 160px). The common gaps are 3 (8px) inside a group, 5 (16px) between related controls, 6 or 7 (24 or 32px) between form fields, and 9 (48px) between sections.
- Classes are `cds--stack-vertical` / `cds--stack-horizontal` plus `cds--stack-scale-N`, which sets `--cds-stack-gap`. A string `gap` sets that property inline.
- A horizontal stack is `inline-grid`, so it sizes to its content; put it in a block container to align it. Stack does not wrap.
- Smaller gaps read as related, larger gaps as separate. Stay on the scale; step down the scale at small breakpoints rather than inventing values.

**Accessibility**
- Stack is presentational: it adds no role. With `as="ul"` or `as="ol"` give the children `li` elements so the list semantics hold.
- Visual order equals DOM order, so keyboard and reading order stay in sync.
