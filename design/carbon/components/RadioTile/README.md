# RadioTile

A tile that acts as a radio button, grouped by `TileGroup` so exactly one can be chosen. Exports `RadioTile` and `TileGroup`.

Hand-written from `packages/styles/scss/components/tile/_tile.scss`, `react/src/components/RadioTile/RadioTile.tsx` and `TileGroup/TileGroup.tsx` (Carbon v11.99.0). Left out: `hasRoundedCorners` and the `enable-v12-tile-radio-icons` radio glyphs (the check mark glyph is used). The AI treatment with an AILabel `decorator` uses the AILabel card's approximated AI tokens.

**When to use**
- One choice among a few options that each need a description, price or spec: plans, regions, node sizes, operating systems.
- Wizard and create flows where the choice is the main decision on the step.

**When not to use**
- Any number of choices. Use SelectableTile instead.
- Short labels with no detail, or more than about six options. Use a RadioButtonGroup, Dropdown or Select.
- Navigation. Use a clickable Tile.

**You provide:**
- A `TileGroup` with `name` (required), optional `legend`, `defaultSelected` (uncontrolled) or `valueSelected` (controlled), `onChange(value, name, event)`, `disabled`, `required` and `className`.
- `RadioTile` children, each with a `value` (required) and content; optional `id`, `disabled`, `decorator` (usually an AILabel, `size="mini"`), `tabIndex`, `className`. `light` is deprecated (use a Layer).
- TileGroup injects `name`, `checked`, `required` and `onChange` into every RadioTile it finds, including ones nested in layout elements (a grid `div`). A RadioTile used alone takes `checked`, `name` and `onChange` itself.

**Example**
```jsx
const { TileGroup, RadioTile } = window.Carbon;
<TileGroup name="plan" legend="Plan" valueSelected={plan} onChange={setPlan}>
  <div className="plan-grid">
    <RadioTile value="dev">Development: 1 zone, no SLA</RadioTile>
    <RadioTile value="std">Standard: 3 zones, 99.9% SLA</RadioTile>
  </div>
</TileGroup>
```

**Rules**
- Markup is `fieldset.cds--tile-group` with an optional `legend.cds--label`; each tile is a visually hidden native `input[type=radio]` followed by a `label.cds--tile.cds--tile--selectable.cds--tile--radio`.
- Fill `layer-01`, hover `layer-hover-01`. The check mark glyph (16px, `checkmark--filled`) is hidden until selected, then `icon-primary` with a 1px `layer-selected-inverse` border.
- Focus on the input draws a 2px `focus` outline inset 2px on the tile. A disabled tile uses `text-disabled`, `icon-disabled` and `border-disabled` when selected.
- TileGroup `disabled` disables the fieldset and each tile here (Carbon only disables the inputs).
- Do not pre-select unless there is a sensible default; give tiles in a group equal heights and on a layer-01 panel wrap the group in a Layer.

**Accessibility**
- Native radios: the group is one tab stop; arrow keys move and select, Space selects the focused tile, Tab leaves the group. With nothing selected, Tab lands on the first tile.
- The fieldset is named by its `legend`; always give one (it can be visually short, like "Plan").
- Each radio is labelled by its tile content. Keep the title first; add `aria-describedby` on a RadioTile for extra detail.
- A decorator AILabel inside the tile is its own button; clicking it does not select the tile.
