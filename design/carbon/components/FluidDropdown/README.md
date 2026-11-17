# FluidDropdown

Dropdown for fluid forms: the label sits inside the top of a 64px field and the selected item (or placeholder) sits below it.

Hand-written from `packages/styles/scss/components/fluid-list-box` and `fluid-dropdown` (Carbon v11.99.0), layered on the Dropdown list-box (`list-box`, `dropdown`). It renders `Dropdown` with the `cds--list-box__wrapper--fluid` wrapper classes, as Carbon does. `FluidDropdownSkeleton` is the loading placeholder. Left out: AI-label/decorator slot, toggletip labels, `translateWithId`. Helper text, `size`, `hideLabel` and `type="inline"` are dropped (Carbon drops them for fluid inputs too).

**When to use**
- Choosing one option from a predefined list in a FluidForm, a fluid side panel or a fluid toolbar, when the list is short enough to scan (roughly 3 to 15 options).

**When not to use**
- Regular stacked forms: use Dropdown. Two options: a Toggle or RadioButtonGroup. Long lists, or when users may type a value: FluidComboBox. Several values: FluidMultiSelect. Plain form submission and native mobile pickers matter more than styling: FluidSelect.

**You provide:**
- `titleText` (the label drawn inside the field; required), `label` (placeholder text while nothing is selected, e.g. "Choose a storage class"), `items` (strings, or objects with `label`/`text`/`name`; `disabled: true` disables a row).
- Selection: controlled `selectedItem` + `onChange({ selectedItem })`, or uncontrolled `initialSelectedItem`.
- `itemToString`, `itemToElement` (rich rows), `isCondensed` (40px menu rows; default 64px), `direction` (`'bottom'` default or `'top'`), `open` / `defaultOpen` / `onOpenChange(open)`, `id`, `className`.
- States: `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. Invalid/warn are ignored when disabled or read-only.
- `FluidDropdownSkeleton`: `className`.

**Example**
```jsx
const { FluidDropdown } = window.Carbon;
<FluidDropdown id="storage-class" titleText="Storage class" label="Choose a storage class"
  items={['Smart Tier', 'Standard', 'Vault', 'Cold Vault']}
  selectedItem={storageClass} onChange={({ selectedItem }) => setStorageClass(selectedItem)} />
```

**Rules**
- Field is `field-01` (hover `field-hover-01`), at least 64px tall, with a 1px `border-strong-01` bottom rule. Label is `label-01` in `text-secondary`, 13px from the top and 16px from the left; the value is `body-compact-01` starting 33px from the top. Chevron is 16px, 16px from the right edge.
- Menu rows are 64px (`spacing-10`) with the option text in the top 40px and the check icon 20px from the row top, as in Carbon; `isCondensed` gives 40px rows so more options show without scrolling. The menu is at most 220px tall, uses `layer-01` and renders inline under the field (`position: absolute`), not portalled.
- The wrapper fills the height of its parent (`block-size: 100%`) so fluid inputs in one row line up; wrap it if you do not want that.
- Invalid: a 2px `support-error` outline wraps the field and the message row, the bottom rule goes transparent, a 1px `border-subtle-01` divider (inset 16px) separates field and message, and the error icon sits at the right of the message (`padding: 8px 64px 8px 16px`, `text-error`). Warning uses the same row with a `support-warning` icon, `text-primary` text and a `border-strong-01` rule under the message.
- Focus is the field's 2px inset `focus` outline. Disabled uses `text-disabled` / `icon-disabled` and drops the bottom rule. Read only keeps the value readable and the chevron in `icon-disabled`; the menu does not open.
- Content: short label on one line; placeholder is an interaction cue ("Choose an option"), never key information. Keep option text brief, in a logical or alphabetical order, without icons; long options truncate with an ellipsis.
- Skeleton: a 64px `skeleton-background` field with a label bar and a value bar.

**Accessibility**
- Keyboard and ARIA come from Dropdown: the field is a `role="combobox"` button with `aria-haspopup="listbox"` and `aria-expanded`, labelled by the `titleText` label; the menu is `role="listbox"` with `aria-activedescendant` and `aria-selected` rows.
- Tab focuses the field; ArrowUp/ArrowDown open and move, Home/End jump, Enter/Space select, Escape closes, typing letters jumps to matching options. Disabled rows are skipped.
- Invalid/warning text is referenced by `aria-describedby`. Disabled is not focusable; read only stays focusable.
