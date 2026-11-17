# FluidComboBox

Combo box for fluid forms: a text input with the label inside the top of a 64px field that filters a list of options as the user types.

Hand-written from `packages/styles/scss/components/fluid-list-box` and `fluid-combo-box` (Carbon v11.99.0), layered on the ComboBox list-box (`list-box`, `combo-box`). It renders `ComboBox` with the `cds--list-box__wrapper--fluid` wrapper classes, as Carbon does. `FluidComboBoxSkeleton` is the loading placeholder. Left out: AI-label/decorator slot, toggletip labels, `typeahead` autocomplete, `translateWithId`. Helper text and `size` are dropped for fluid inputs.

**When to use**
- Picking one value from a long list where typing is faster than scrolling (container images, users, instance profiles), inside a FluidForm.
- When the user may enter a value that is not in the list (`allowCustomValue`).

**When not to use**
- Regular stacked forms: use ComboBox. Short lists that are quicker to scan than to type: FluidDropdown or FluidSelect. Several values: FluidMultiSelect with `isFilterable`. Free text with no list: FluidTextInput.

**You provide:**
- `titleText` (label inside the field; required), `items`, `placeholder` (e.g. "Type to filter images").
- Selection: controlled `selectedItem` + `onChange({ selectedItem })`, or uncontrolled `initialSelectedItem`. `initialInputValue` pre-fills the text.
- `onInputChange(text)`, `shouldFilterItem({ item, itemToString, inputValue })` (default: case-insensitive "contains"), `allowCustomValue` (default false: when the menu closes, unmatched text reverts to the selected item), `itemToString`, `itemToElement`, `isCondensed` (40px rows; default 64px), `direction`, `open` / `defaultOpen` / `onOpenChange`, `id`, `className`.
- States: `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. Invalid/warn are ignored when disabled or read-only.
- `FluidComboBoxSkeleton`: `className`.

**Example**
```jsx
const { FluidComboBox } = window.Carbon;
<FluidComboBox id="image" titleText="Container image" placeholder="Type to filter images"
  items={['nginx:1.27', 'node:22-alpine', 'postgres:16', 'python:3.12-slim']} allowCustomValue
  selectedItem={image} onChange={({ selectedItem }) => setImage(selectedItem)} />
```

**Rules**
- The input fills the 64px field: padding 33px top, 13px bottom, 16px left, 64px right; `field-01` with a 1px `border-strong-01` bottom rule. Label is `label-01` in `text-secondary` 13px from the top.
- The clear button (16px close icon, 24px target) sits 41px from the right and 10px from the bottom; the chevron 14px from the bottom. Clear appears only when there is text.
- The menu starts 1px below the field; rows are 64px (text in the top 40px) or 40px with `isCondensed`, max 220px tall, `layer-01`, rendered inline.
- Invalid: 2px `support-error` outline around field and message, input bottom rule transparent, `border-subtle-01` divider, error icon 16px from the right of the message. Warning: `support-warning` icon, `text-primary` message, `border-strong-01` rule under it. Name the typed value and the fix in the error ("No image named ngnix. Choose one from the list").
- Focus is a 2px inset `focus` outline on the input. Disabled uses `text-disabled` / `icon-disabled`; read only keeps the text readable and blocks typing and opening.
- Skeleton: a 64px `skeleton-background` field with a label bar and a value bar.

**Accessibility**
- Keyboard and ARIA come from ComboBox: the input has `role="combobox"`, `aria-autocomplete="list"`, `aria-expanded` and `aria-activedescendant`, labelled by the `titleText` label; the menu is `role="listbox"`.
- ArrowUp/ArrowDown open and move, Enter selects the highlighted option, Escape closes (or clears when closed); the clear button has an accessible name.
- Invalid/warning text is referenced by `aria-describedby`.
