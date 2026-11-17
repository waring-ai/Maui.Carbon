# IconIndicator

A status glyph followed by a text label. The glyph's shape carries the status, so it stays readable without color.

Hand-written from `packages/styles/scss/components/icon-indicator` and `packages/react/src/components/IconIndicator` (Carbon v11.99.0). Glyphs are drawn inside the component because the bundled icon set does not include them; they follow Carbon's shapes but are redrawn, not copied.

**When to use**
- System health and the state of objects or processes where attention may be needed: a status column in a table, task lists, dashboards, notifications.

**When not to use**
- Dense lists, severity or lifecycle levels in small spaces: use `ShapeIndicator`.
- New or unread items on an icon button: use `BadgeIndicator`.
- Categories or attributes rather than state: use `Tag`. Messages: use `InlineNotification`.
- Nothing needs attention: plain text is enough. Avoid more than five or six different indicators in one view.

**You provide:** `kind` and `label`. Optional `size` 16 (default) or 20, `className`.

**Kinds and tokens:** failed `status-red` (circle, x) · caution-major `status-orange` (inverted triangle, !) · caution-minor `status-yellow` (triangle, !) · undefined `status-purple` (diamond, ?) · succeeded `status-green` (circle, check) · normal `status-blue` (outlined circle, check) · in-progress `status-blue` (half-filled circle) · incomplete `status-blue` (half circle, dashed outline) · not-started `status-gray` (dashed circle) · pending `status-gray` (circle, three dots) · unknown `status-gray` (circle, ?) · informative `status-blue` (square, !).

**Example**
```jsx
const { IconIndicator } = window.Carbon;
<IconIndicator kind="caution-major" label="Degraded: 2 of 4 pods ready" />
```

**Rules**
- The label is `text-secondary`: body-compact-01 with the 16px glyph, body-compact-02 with the 20px glyph. The glyph is 8px from the label.
- Color meaning: red error/danger, orange serious warning, yellow warning, green success or normal, blue in progress or passive information, gray not started or draft, purple undefined or outlier.
- The marks inside the caution, undefined and unknown glyphs are cut-outs, so they show the surface behind (Carbon paints the caution marks black; no black token exists).
- Label says the state in the product's words ("Deploying (3 of 5 pods)"), and the same kind always maps to the same meaning across the product.

**Accessibility**
- The glyph is decorative (`aria-hidden`); the visible label is the accessible text, so the label is required.
- Color is never the only cue: each kind has its own shape and symbol, and the label repeats the state.
- Place the indicator next to the object it describes (in the same table row or list item).
