# ComboBox

A text field with a list of options that filters as you type, for choosing one value from a long list; selecting an option fills the field and shows a clear button.

Hand-written from `packages/styles/scss/components/combo-box` and `list-box` (Carbon v11.99.0), sharing the list-box CSS with Dropdown and MultiSelect. Use `DropdownSkeleton` for its loading state. Left out: the xs (24px) size, `typeahead` autocompletion, `autoAlign`, fluid (see FluidComboBox) and AI-label decorators, `light` (use a Layer), `translateWithId`, Downshift passthrough props. There is no inline variant (as in Carbon).

**When to use**
- One value from a long list, or a list from a database, where typing a few letters is faster than scrolling: people, zones, images.
- With `allowCustomValue`, when people may enter a value that is not listed.

**When not to use**
- Short fixed lists where typing does not help: use a Dropdown or Select.
- Several values: use a MultiSelect with `filterable`.
- Free text with no list behind it: use a TextInput or Search.

**You provide:**
- `items` (strings, or objects with `label` / `text` / `name`; `disabled: true` disables a row), `itemToString(item)`, optional `itemToElement` for rich rows.
- `titleText` (or `aria-label`; `hideLabel` keeps it for screen readers only) and `placeholder`.
- `selectedItem` + `onChange({ selectedItem })` (controlled) or `initialSelectedItem`; `initialInputValue`; `onInputChange(text)` reports typing.
- Filtering: the default is a case-insensitive "contains" match; replace it with `shouldFilterItem({ item, itemToString, inputValue })`. When the text equals the selected item, the full list shows.
- `allowCustomValue`: typed text that matches nothing is kept, and Enter calls `onChange({ selectedItem: null, inputValue })`.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px); `direction` `bottom` | `top`.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. `invalid` and `warn` are ignored while disabled or read only.
- `open` / `defaultOpen` / `onOpenChange` (previews use `defaultOpen`); `id`, `className`.

**Example**
```jsx
const { ComboBox } = window.Carbon;
<ComboBox id="member" titleText="Person" placeholder="Type a name or email"
  items={users} itemToString={(u) => (u ? `${u.name} (${u.email})` : '')}
  selectedItem={person} onChange={({ selectedItem }) => setPerson(selectedItem)} />
```

**Rules**
- The same list-box as Dropdown (`field-01` / `field-02` on a Layer, `border-strong-01` underline, menu on `layer-01`) with a text input inside; focus is the input's 2px `focus` outline.
- The first match is highlighted as you type. The clear button (`icon-primary`, 24px target) appears whenever there is text and sits left of the chevron; clearing sets `selectedItem` to null and refocuses the input. Invalid and warning icons sit left of the clear button.
- Invalid: `support-error` outline, error icon, `text-error` message; warning: `support-warning` icon, `text-primary` message. Read only drops the fill; disabled uses `text-disabled`.
- Copy: the placeholder says what to type ("Type a name or email"), never replaces the label; keep option text short and unique so the filter works.

**Accessibility**
- The input has `role="combobox"`, `aria-autocomplete="list"`, `aria-haspopup="listbox"`, `aria-expanded`, `aria-controls` and `aria-activedescendant`; options are `role="option"` with `aria-selected` / `aria-disabled`.
- Keyboard: typing filters and opens; ArrowDown / ArrowUp open and move; Enter chooses the highlighted row (or opens the list when it is closed); Escape closes, or clears the text when already closed; Tab closes. Space types a space, it never selects.
- The clear and chevron controls are not tab stops (the input handles the keyboard). Error and warning text is linked with `aria-describedby`.
