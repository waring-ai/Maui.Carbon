# ShapeIndicator

A small colored shape plus a label, used for severity and lifecycle where the shape distinguishes levels without relying on hue.

Hand-written from `packages/styles/scss/components/shape-indicator` and `packages/react/src/components/ShapeIndicator` (Carbon v11.99.0). Shapes are redrawn inside the component (the bundled icon set does not include them); their exact outlines approximate Carbon's.

**When to use**
- Severity or priority (critical, high, medium, low) and lifecycle states (stable, draft, incomplete) in dense tables, lists and diagrams where scanning many rows matters.

**When not to use**
- Health or process state that needs more attention and has room for a glyph: use `IconIndicator`.
- New or unread items on an icon button: use `BadgeIndicator`.
- Categories: use `Tag`.

**You provide:** `kind` and `label`. Optional `textSize` 12 (default) or 14 (the shape is always 16px), `className`.

**Kinds and tokens:** failed, critical, high `status-red` (octagon with x, hexagon, triangle) · medium `status-orange` with a `status-orange-outline` border (diamond) · low `status-yellow` with a `status-yellow-outline` ring (inverted triangle) · cautious `status-yellow` with `status-yellow-outline` border (triangle) · undefined `status-purple` (diamond) · stable `status-green` (circle) · informative `status-blue` (inverted triangle) · incomplete `status-blue` (half-filled circle) · draft `status-gray` (ring).

**Example**
```jsx
const { ShapeIndicator } = window.Carbon;
<ShapeIndicator kind="critical" label="Critical" textSize={14} />
```

**Rules**
- Label is `text-secondary`: helper-text-01 (12px) or body-compact-01 (14px); 8px between shape and label. Use 14 to match 14px row text in tables.
- Orange and yellow shapes get a darker outline token so they hold 3:1 contrast on light surfaces.
- Pair severity shapes consistently across a product (a given shape always means the same level).

**Accessibility**
- The shape is decorative (`aria-hidden`); the label is the accessible text and is required.
- Shape and color together distinguish the levels, so the indicator works for users who cannot tell the hues apart; never drop the label to save space.
