# SelectableTag

A toggle tag the user can switch on and off, used for picking several options from a set of chips.

Hand-written from `packages/styles/scss/components/tag` and `packages/react/src/components/Tag/SelectableTag.tsx` (Carbon v11.99.0). `renderIcon` takes an icon file stem.

**When to use**
- Quick filters or multi-select choices shown as chips: regions, node types, statuses.

**When not to use**
- Removing an applied value: use `DismissibleTag`. Opening more detail: use `OperationalTag`.
- A choice inside a form with a submit: use `Checkbox` (several) or `RadioButton` (one).
- A single on/off setting: use `Toggle`.

**You provide:** `text`, and either `selected` + `onChange(selected)` (controlled) or `defaultSelected` (uncontrolled); optionally `onClick`, `size` sm | md | lg (18/24/32px, default md), `disabled`, `renderIcon` (hidden at sm), `id`, `className`. There is no color `type`.

**Example**
```jsx
const { SelectableTag } = window.Carbon;
<div role="group" aria-labelledby="region-label">
  {regions.map(r => <SelectableTag key={r} text={r} selected={sel.includes(r)} onChange={on => toggleRegion(r, on)} />)}
</div>
```

**Rules**
- Unselected: 1px `border-inverse` border, `layer-01` fill, `text-primary`; hover `layer-hover-01`.
- Selected: `layer-selected-inverse` fill with `text-inverse`; hover keeps the selected fill. The two states must stay clearly different.
- Focus is a 2px `focus` outline offset 1px.
- Disabled: `border-disabled` border, `layer-01` fill, `text-disabled`, both when selected and not.
- Truncated labels get a bottom tooltip with the full text. A group may wrap to a few lines, never more than five.

**Accessibility**
- Renders a `<button aria-pressed>`; Tab moves between tags, Enter and Space toggle.
- Wrap a set in an element with `role="group"` and a label naming what the choices filter.
- Disabled tags are not operable and are exempt from contrast requirements.
