# Dropdown

A select-like control: a field that opens a styled list of options and holds one selected item.

Hand-written from `packages/styles/scss/components/list-box` and `dropdown` (Carbon v11.99.0). The list-box CSS is shared with ComboBox and MultiSelect. `DropdownSkeleton` (also used for ComboBox and MultiSelect) is exported alongside. Left out: the xs (24px) size, `autoAlign` / floating placement and automatic upward flip, fluid (see FluidDropdown) and AI-label decorators, `light` (use a Layer instead), `renderSelectedItem`, `translateWithId`, Downshift passthrough props.

**When to use**
- Choosing one option from a predefined list in a form, modal or side panel, or to filter or sort page content (often as `type="inline"`). Works well where space is limited.

**When not to use**
- Two options: use a RadioButtonGroup (or a Toggle for on / off).
- Long lists where typing helps, or values not in the list: use a ComboBox.
- Several choices: use a MultiSelect.
- Plain form submission or heavy mobile use: a native Select is lighter. Do not nest dropdowns.

**You provide:**
- `items`: strings, or objects with `label` / `text` / `name` (`disabled: true` disables a row). `itemToString(item)` maps an item to text; `itemToElement` renders a rich row from an object item.
- `label` (the placeholder text shown while nothing is selected) and `titleText` (the visible label; `hideLabel` keeps it for screen readers only, or pass `aria-label` when there is no title).
- `selectedItem` + `onChange({ selectedItem })` (controlled) or `initialSelectedItem`. Objects match by identity or by `id`.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px); `type` `default` | `inline`; `direction` `bottom` (default) | `top`.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. `invalid` and `warn` are ignored while disabled or read only.
- `open` / `defaultOpen` / `onOpenChange(open)` to control the list (previews use `defaultOpen`); `id`, `className`.
- `DropdownSkeleton`: `size`, `hideLabel`, `className`.

**Example**
```jsx
const { Dropdown } = window.Carbon;
<Dropdown id="cluster-region" titleText="Region" label="Choose a region"
  items={regions} itemToString={(r) => (r ? r.label : '')}
  selectedItem={region} onChange={({ selectedItem }) => setRegion(selectedItem)}
  invalid={submitted && !region} invalidText="Choose a region to continue" />
```

**Rules**
- Field uses `field-01` (`field-02` inside a Layer; hover `field-hover-01`) with a 1px `border-strong-01` bottom border; the open list uses `layer-01` with `layer-hover-01` hover and `layer-selected-01` for the selected row, plus a check icon on the right. Rows are separated by 1px `border-subtle-01` rules.
- Menu rows match the field height; the menu shows about five and a half rows, then scrolls. It is positioned under the field (`position: absolute`), not portalled; give the container room or use `direction="top"`.
- Focus is a 2px `focus` outline inset on the field; the highlighted row has the same inset outline. Invalid draws a 2px `support-error` outline plus the error icon and a `text-error` message; warning shows the `support-warning` icon and a `text-primary` message.
- Inline drops the fill and underline and puts the label to the left; use it in toolbars and filter bars.
- Disabled uses `text-disabled` and `icon-disabled`, no bottom border; read only drops the fill and shows a `border-subtle-01` underline.
- Copy: a short one-line label; placeholder such as "Choose a region"; short option text in alphabetical or logical order (long text is truncated with an ellipsis and a `title` tooltip); no decorative icons.

**Accessibility**
- The field is a `<button role="combobox">` with `aria-haspopup="listbox"`, `aria-expanded`, `aria-controls` and `aria-activedescendant`; the list is `role="listbox"` with `role="option"` rows (`aria-selected`, `aria-disabled`). Label ids are unique per instance.
- Keyboard: Tab focuses; ArrowDown / ArrowUp open and move the highlight (skipping disabled rows); Home / End jump; Enter or Space opens or chooses; Escape or Tab closes without changing the selection; typing letters jumps to the next match.
- Read only sets `aria-disabled` and ignores input but stays focusable. Error and warning text is linked with `aria-describedby`.
