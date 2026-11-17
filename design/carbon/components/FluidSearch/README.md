# FluidSearch

A search field for fluid forms and filter bars: visible label inside a 64px field, magnifier at the bottom-right and a clear button next to it.

Hand-written from `packages/styles/scss/components/fluid-search` (on top of `search`) and `packages/react/src/components/FluidSearch` / `Search` (Carbon v11.99.0). `FluidSearchSkeleton` is the loading placeholder. Left out: expandable mode, custom `renderIcon`. Carbon's FluidSearch has no invalid, warning or read-only state, so neither does this one.

**When to use**
- Filtering a list or table from a fluid toolbar, a fluid filter bar next to FluidMultiSelect / FluidDropdown, or a side panel.

**When not to use**
- Page or table toolbars in productive UIs: use Search (with its sizes and expandable mode). Picking one value from a list: use FluidComboBox, which also filters.

**You provide:**
- `labelText` (required, shown in the field), `placeholder` (default `''`), `id` (auto-generated), `name`.
- Value: controlled `value` + `onChange(event)` or uncontrolled `defaultValue` (default `''`). `onClear()` fires after the clear button or Escape empties the field (`onChange` also fires with an empty value).
- `disabled`, `closeButtonLabelText` (default "Clear search input"), `role` (default `searchbox`), `type` (default `text`), `autoComplete` (default `off`), `onKeyDown`, `onFocus`, `onBlur`, `className`.

**Example**
```jsx
const { FluidSearch } = window.Carbon;
<FluidSearch id="bucket-search" labelText="Search buckets" placeholder="Filter by bucket name"
  value={query} onChange={e => setQuery(e.target.value)} />
```

**Rules**
- 64px `field-01` box with a 1px `border-strong-01` bottom rule; input padding 32px 88px 13px 16px; label `label-01` in `text-secondary` at 13px / 16px.
- The magnifier sits 16px from the right and 13px from the bottom in `icon-secondary`; the 40x40 clear button sits 48px from the right on the bottom edge and only appears when there is text. Its bottom line follows the field: 1px `border-strong-01`, or 2px `focus` while the input is focused.
- Focus is a 2px inset `focus` outline. Filter results as the user types; show an empty-state message when nothing matches.
- Disabled: `text-disabled` label, `border-subtle-01` bottom rule, `icon-disabled` magnifier, clear button disabled.
- Keep the label meaningful ("Search services"), not only the placeholder.

**Accessibility**
- The wrapper has `role="search"` (named by the placeholder); the input has `role="searchbox"` and a `<label>`.
- Escape clears the text and keeps focus in the field; the clear button has an `aria-label` and is removed from the tab order while hidden. After a click on it, focus returns to the input.
