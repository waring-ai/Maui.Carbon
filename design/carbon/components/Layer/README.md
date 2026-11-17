# Layer

Re-maps the layer tokens for everything inside it so components that use the `-01` tokens step up to the next layer (level two, then three) without changing their own styles. Exports `Layer`, `LayerLevel` and `useLayer`.

Hand-written from `packages/styles/scss/_layer.scss` and `layer/_layer-sets.scss` (Carbon v11.99.0). Left out: the `cds--layer-one` reset is a no-op (see Rules), and Carbon's unprefixed alias tokens (`--cds-layer`, `--cds-field`) are not emitted. `useLayer()` is reconstructed from Carbon's `LayerContext`; it was not checked against source.

**When to use**
- A surface sits on another surface: fields, tiles, tags or a data table placed on a layer-01 panel, tile, modal or side panel. Wrap the content in a Layer so it contrasts with what it sits on.
- Components that you reuse on the page and inside panels: build them with the `-01` tokens and let the nearest Layer decide the level.

**When not to use**
- To change theme (light/dark). Use a Theme zone instead; a zone also resets layering to level one.
- For spacing or grouping. Use Stack, Grid or FormGroup; Layer adds no box model of its own.
- Deeper than three levels. Carbon stops at level three; redesign the nesting instead.

**You provide:**
- `children`.
- `level` (1, 2 or 3; `LayerLevel.one/two/three`): force a level instead of stepping one above the enclosing Layer.
- `withBackground` (default `false`): paint the level's `layer-background` token as the wrapper background.
- `as` (default `div`) and `className`; other attributes pass through to the element.
- `LayerLevel` is `{ one: 1, two: 2, three: 3 }`. `useLayer()` returns `{ level }`, the level of the nearest enclosing Layer (1 outside any Layer or directly inside a Theme zone).

**Example**
```jsx
const { Layer, Tile, TextInput } = window.Carbon;
<Tile>
  <p>Worker pool settings</p>
  <Layer>
    <TextInput id="pool-name" labelText="Pool name" defaultValue="default-pool" />
  </Layer>
</Tile>
```

**Rules**
- A Layer with no `level` steps one above the layer around it. On the page (level one) a Layer is level two; a Layer inside it is level three; deeper Layers stay at three. This is tracked with a React context.
- It works by overriding custom properties on the wrapper: `.cds--layer-two` sets `--layer-01: var(--layer-02)` and the matching `-hover`, `-active`, `-selected`, `-selected-hover`, `-accent`, `-accent-hover`, `-accent-active`, `--layer-background-01`, `--field-01`, `--field-hover-01`, `--border-subtle-01`, `--border-subtle-selected-01`, `--border-strong-01` and `--border-tile-01`. `.cds--layer-three` does the same with the `-03` tokens.
- In the light themes the layers alternate (white page, `layer-01` gray 10, `layer-02` white, `layer-03` gray 10); in the dark themes each level is one step lighter (g100: gray 90, 80, 70).
- Components never name `-02`/`-03` tokens; they use `-01` and get the right value from the nearest Layer. Do not hard-code a layer number in a component.
- Use one Layer per surface change: a Tile, field or Tag sitting on a layer-01 panel goes inside a Layer so it contrasts with the panel.
- `withBackground` fills with `layer-background-01` as remapped, so it matches the level.
- Level one cannot be restored inside a Layer (a custom property cannot reference itself); `level={1}` only labels the wrapper `cds--layer-one`. A Theme zone does reset to level one because it re-binds every token.

**Accessibility**
- Layer is purely visual: it renders a plain `div` (or `as`) with no role. Give the surface itself a landmark or heading if it is a region.
- Layering is what keeps fields and tiles at a visible contrast with their container; skipping a Layer can leave a field the same color as the panel behind it.
