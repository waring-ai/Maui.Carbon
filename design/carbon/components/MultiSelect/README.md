# MultiSelect

A field that opens a checkbox list and holds several selected items, summarised by a count tag with a clear button; add `filterable` to type and narrow the list.

Hand-written from `packages/styles/scss/components/multiselect`, `list-box` and `tag` (Carbon v11.99.0), sharing the list-box CSS with Dropdown and ComboBox. Use `DropdownSkeleton` for its loading state. Left out: the xs (24px) size, `selectAll` (parent "All" checkbox), `sortItems` / `compareItems` (selected items move to the top but keep their order, unselected ones keep the order of `items`; Carbon also sorts alphabetically), `useTitleInItem`, `autoAlign`, fluid (see FluidMultiSelect) and AI-label decorators, `light`, `translateWithId`.

**When to use**
- Several values from a list: notification channels, roles, and filters with several criteria (status, region) in a filter bar.

**When not to use**
- Exactly one value: use a Dropdown, ComboBox or Select.
- Fewer than about five options with room to show them: use a CheckboxGroup.

**You provide:**
- `items` (strings, or objects with `label` / `text` / `name`; `disabled: true` disables a row) and `itemToString(item)`.
- `label` (field text, kept beside the count tag; you may change it to reflect the selection) and `titleText` (or `aria-label`; `hideLabel` keeps it for screen readers only).
- `selectedItems` + `onChange({ selectedItems })` (controlled) or `initialSelectedItems`.
- `selectionFeedback`: `top-after-reopen` (default; items selected when the menu opened are listed first), `top` (move to the top as they are chosen) or `fixed` (keep the order of `items`).
- `filterable` turns the field into a text input (`placeholder`, `shouldFilterItem({ item, itemToString, inputValue })`); the list stays open while choosing.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px); `type` `default` | `inline`; `direction` `bottom` | `top`.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. `invalid` and `warn` are ignored while disabled or read only.
- `open` / `defaultOpen` / `onOpenChange` (previews use `defaultOpen`); `id`, `className`.

**Example**
```jsx
const { MultiSelect } = window.Carbon;
<MultiSelect id="status-filter" type="inline" titleText="Status" label="All"
  items={['Succeeded', 'In progress', 'Failed']}
  selectedItems={statuses} onChange={({ selectedItems }) => setStatuses(selectedItems)} />
```

**Rules**
- The list is the shared list-box (field `field-01` / `field-02` on a Layer, menu on `layer-01`, hover `layer-hover-01`, 1px `border-subtle-01` dividers) with a Carbon checkbox per row; clicking anywhere on a row toggles it and the list stays open.
- The selection tag is a high-contrast filter tag (`background-inverse` / `text-inverse`) holding the count and an x that clears everything.
- Focus is a 2px inset `focus` outline around the field. Invalid: 2px `support-error` outline, error icon, `text-error` message; warning: `support-warning` icon, `text-primary` message. Disabled uses `text-disabled` / `icon-disabled`.
- Inline drops the fill and puts the label to the left; use it in filter bars, never for filterable lists in forms.
- Copy: short option text in alphabetical or logical order; in a filter bar, "All" as the empty label reads well.

**Accessibility**
- The field is a `role="combobox"` button (or input when filterable) with `aria-haspopup="listbox"`, `aria-expanded`, `aria-controls` and `aria-activedescendant`; rows are `role="option"` with `aria-selected` in an `aria-multiselectable` listbox. The row checkboxes are decorative.
- Keyboard: ArrowDown, Enter or Space open; arrows move; Enter or Space toggle the highlighted row (Space types when filterable); Home / End jump (not filterable); Escape or Tab close; Delete on the closed field clears every selection.
- The tag's clear control is not a tab stop; use Delete from the keyboard. Error and warning text is linked with `aria-describedby`.
