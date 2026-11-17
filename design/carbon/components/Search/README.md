# Search

A text field for searching, with a magnifier icon, a clear button that appears once there is text, and an expandable form (`ExpandableSearch`) that collapses to just the icon.

Hand-written from `packages/styles/scss/components/search` and `packages/react/src/components/Search` (Carbon v11.99.0). `ExpandableSearch` (a `Search` with `expandable` set) and `SearchSkeleton` are exported alongside. Left out: the xs (24px) size, the standalone search button, fluid variant (see FluidSearch), a `renderIcon` component (it takes an icon name), and the tooltip on the collapsed magnifier.

**When to use**
- Finding items in a large or complex set: a resource list, a data table toolbar, a whole console.
- `ExpandableSearch` in toolbars where space is tight and search is secondary.

**When not to use**
- Small sets people can scan in one view; show the list instead.
- Choosing one value for a form field; use a ComboBox (search plus selection) or a Dropdown.

**You provide:**
- `labelText` (required; visually hidden, it names the input) and `placeholder` (default "Search").
- `value` + `onChange(event)` or `defaultValue`. The clear button and Escape call `onChange` with an empty value, then `onClear()`, and return focus to the input.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px). Match the height of controls beside it (`lg` in a table toolbar).
- `disabled`, `closeButtonLabelText` (default "Clear search input"), `renderIcon` (icon name for the magnifier, default `search`).
- Expandable: `expandable` (or use `ExpandableSearch`), `isExpanded` (controlled) or `defaultExpanded`, `onExpand(event)`.
- Also passed through: `id`, `name`, `type` (default `search`), `autoComplete` (default `off`), `onKeyDown`, `onBlur`, `onFocus`, `className`.
- `SearchSkeleton`: `size` (or legacy `small`), `className`.

**Example**
```jsx
const { Search, ExpandableSearch } = window.Carbon;
<Search size="lg" labelText="Search clusters" placeholder="Search by name or region"
  value={query} onChange={(e) => setQuery(e.target.value)} />
<ExpandableSearch labelText="Search API keys" placeholder="Search API keys" />
```

**Rules**
- `field-01` fill (`field-02` inside a Layer) with a 1px `border-strong-01` underline; hover `field-hover-01`; focus is a 2px `focus` outline. The magnifier is `icon-secondary` (`icon-primary` when collapsed).
- The clear button is a square the height of the field, `icon-primary`, hover `field-hover-01`, and hidden while the field is empty.
- A collapsed expandable search is transparent, one control tall and wide, with a `background-hover` hover; it expands to fill its container and collapses again on blur when empty.
- Disabled uses `text-disabled` and `icon-on-color-disabled` and hides the placeholder.
- Copy: the placeholder hints at what can be found ("Search by name or region"); keep one search per region and filter results as people type or on Enter.

**Accessibility**
- The wrapper has `role="search"` named by the placeholder; the input has a visually hidden `<label>`.
- Escape clears the text; the clear button is a `<button>` with an `aria-label`, skipped by Tab while the field is empty.
- Expandable: the magnifier is a focusable `role="button"` with `aria-expanded` and `aria-controls`; Enter or Space expands and focuses the input; Escape on an empty field collapses and returns focus to the magnifier. The input is out of the tab order while collapsed.
