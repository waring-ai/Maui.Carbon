# Icon

Draws one of Carbon's own single-ink glyphs, copied from `packages/icons` and inlined as SVG, in `currentColor`.

The icon set here is a 78-file subset (71 names, see the preview and the Icons asset group); Carbon's full set is much larger and lives in `@carbon/icons`. New glyphs cannot be added from this system.

**When to use**
- Next to text to support it: a status beside its label, an icon in a Button, Link, Tag or Tab, a leading icon in a list row.
- Alone only inside a control that supplies a name: an icon-only Button or IconButton.

**When not to use**
- As the only carrier of meaning without text or an accessible name.
- For status with color: prefer IconIndicator or ShapeIndicator, which pair a glyph, a color and a label.
- As illustration or decoration in product UI; pictograms are for marketing surfaces.

**You provide:**
- `name`: a file stem from the Icons group, e.g. `"settings"`, `"error--filled"`, `"arrow--right"`. Unknown names render nothing.
- `size`: pixels, 16 (default) | 20 | 24 | 32. The closest source size is used (exact size if it exists, then 16, 20, 24, 32) and scaled.
- `title`: the accessible name for a meaningful standalone icon; omit for decorative icons (they get `aria-hidden`).
- `className`, `style` (set `color` to a token to color it).

**Example**
```jsx
const { Icon } = window.Carbon;
<span className="status"><Icon name="checkmark--filled" size={16} style={{ color: 'var(--support-success)' }} />Running</span>
```

**Rules**
- Icons are single-ink and inherit `currentColor`: use `icon-primary` (default), `icon-secondary`, `icon-interactive`, `icon-disabled`, `icon-inverse` on `background-inverse`, `icon-on-color` on saturated fills, or a `support-*` color for status.
- 16px is the default in controls and text; 20px for standalone status and headers; 24 and 32px for larger touch targets and empty states. Align icons to the text's center line.
- Use outline glyphs in controls and filled glyphs (`error--filled`, `checkmark--filled`, `warning--filled`) for status, always next to a text label.
- `add` and `close` ship at 32px only: `close` is drawn from its full 32 box (an 8px cross at 16px, as Carbon's 16px glyph), `add` from its centre 26x26. Other 32-only glyphs whose artwork fits the centre 24x24 are cropped to it when drawn smaller; the rest keep their full box so nothing is clipped.
- The warning glyphs draw the "!" as a cut-out, so it shows the surface behind.

**Accessibility**
- Without `title` the `<svg>` is `aria-hidden="true"` and `focusable="false"`: right for icons next to text.
- With `title` it gets `role="img"` and `aria-label`; use this only when the icon stands alone and carries meaning.
- Never make an `Icon` itself clickable; put it in a Button (with `iconDescription`) or Link.
- Status icons need a text label too; color and shape alone do not meet contrast and color-only rules.
