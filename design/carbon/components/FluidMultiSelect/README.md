# FluidMultiSelect

Multi-select for fluid forms: a 64px field with the label inside it that opens a list of checkbox options and shows how many are chosen in a dismissible count tag.

Hand-written from `packages/styles/scss/components/fluid-list-box` and `fluid-multiselect` (Carbon v11.99.0), layered on the MultiSelect list-box (`list-box`, `multiselect`, `combo-box`). It renders `MultiSelect` (or its filterable mode) with the `cds--list-box__wrapper--fluid` wrapper classes, as Carbon does. `FluidMultiSelectSkeleton` is the loading placeholder. Left out: AI-label/decorator slot, select-all, `sortItems`/`compareItems`, `selectionFeedback`, toggletip labels, `translateWithId`. Helper text and `size` are dropped for fluid inputs.

**When to use**
- Choosing several options from a list in a FluidForm (zones, roles), or filtering page content by several values from a fluid filter bar.
- `isFilterable` when the list is long enough that typing helps.

**When not to use**
- Regular stacked forms: use MultiSelect. A few options that should all stay visible: a CheckboxGroup. Exactly one value: FluidDropdown or FluidComboBox.

**You provide:**
- `titleText` (label inside the field; required), `label` (field text, e.g. "Choose zones"; also the filter placeholder unless `placeholder` is set), `items`.
- Selection: controlled `selectedItems` + `onChange({ selectedItems })`, or uncontrolled `initialSelectedItems`.
- `isFilterable` (alias `filterable`), `placeholder`, `itemToString`, `isCondensed` (40px rows; default 64px), `direction`, `open` / `defaultOpen` / `onOpenChange`, `id`, `className`.
- States: `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. Invalid/warn are ignored when disabled or read-only.
- `FluidMultiSelectSkeleton`: `className`.

**Example**
```jsx
const { FluidMultiSelect } = window.Carbon;
<FluidMultiSelect id="status" titleText="Status" label="All statuses"
  items={[{ id: 'running', text: 'Running' }, { id: 'pending', text: 'Pending' }, { id: 'failed', text: 'Failed' }]}
  selectedItems={statuses} onChange={({ selectedItems }) => setStatuses(selectedItems)} />
```

**Rules**
- Field is `field-01`, 64px, 1px `border-strong-01` bottom rule; label `label-01` / `text-secondary` 13px from the top. The count tag is a high-contrast filter tag pushed down 20px to sit on the value line; its close button clears every selection. Chevron sits 12px from the right.
- Rows hold a checkbox and the option text; they are 64px (content in the top 40px) or 40px with `isCondensed`. The list stays open while toggling. Menu max 220px, `layer-01`, inline (filterable menu starts 1px below the field).
- Invalid: 2px `support-error` outline around field and message, `border-subtle-01` divider, error icon at the right of the message. Warning: `support-warning` icon, `text-primary` message, `border-strong-01` rule under it.
- Focus is a 2px inset `focus` outline on the field (filterable: on the whole box). Disabled uses `text-disabled` / `icon-disabled`, including the tag; read only shows the tag but blocks opening and clearing.
- Content: options are short nouns in a logical or alphabetical order; use the field text to say what an empty selection means ("All statuses").
- Skeleton: a 64px `skeleton-background` field with a label bar and a value bar.

**Accessibility**
- The field is a `role="combobox"` (filterable: the input) with `aria-haspopup="listbox"` and `aria-expanded`; the menu is `role="listbox"` with `aria-multiselectable="true"` and `aria-selected` rows.
- ArrowUp/ArrowDown open and move, Enter (and Space when not filterable) toggles the highlighted option, Home/End jump, Escape or Tab closes. The count tag's clear button has an accessible name.
- Invalid/warning text is referenced by `aria-describedby`.
