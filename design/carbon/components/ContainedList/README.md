# ContainedList

A list with a header and optional actions, for groups of similar items inside a panel, card or popover; items can be clickable or carry their own action.

Hand-written from `packages/styles/scss/components/contained-list` (Carbon v11.99.0). A `Search` passed as a child is lifted out of the list and pinned under the header; an `ExpandableSearch` can go in `action`. Left out: tag auto-sizing in items, the sticky-header option.

**When to use**
- Small spaces and disclosures (side panels, cards, popovers) that list items of the same structure: API keys, members, recent deployments.
- Rows that need inline actions (dismiss, download) or that open something when clicked.

**When not to use**
- Several columns with headers, sorting or bulk actions: use a DataTable.
- Comparing records across columns or picking one of them: use a StructuredList.
- Content nested more than one level: use a TreeView or Accordion.

**You provide:**
- `ContainedListItem` children. `label` (the header text, which also names the list) and `action` (usually an icon-only ghost `Button`, or an `ExpandableSearch`).
- `kind` on-page | disclosed (default on-page) · `size` sm | md | lg | xl (default lg; header and one-line item height 32/40/48/64px) · `isInset` (dividers inset by the padding).
- A `Search` child (with `labelText`, `value`, `onChange`) for a persistent filter under the header.
- `aria-label` / `aria-labelledby` when there is no `label` · `className`.
- `ContainedListItem`: `onClick` (turns the item into a button), `disabled`, `renderIcon` (icon file stem, e.g. `"document"`), `action` (an icon-only ghost `Button` at the end), `className`.

**Example**
```jsx
const { ContainedList, ContainedListItem, Button } = window.Carbon;
<ContainedList
  label="API keys"
  action={<Button hasIconOnly kind="ghost" renderIcon="add" iconDescription="Create API key" onClick={createKey} />}
>
  {keys.map((k) => (
    <ContainedListItem key={k.id} renderIcon="document"
      action={<Button hasIconOnly kind="ghost" renderIcon="close" iconDescription={`Revoke ${k.name}`} onClick={() => revoke(k)} />}>
      {k.name}
    </ContainedListItem>
  ))}
</ContainedList>
```

**Rules**
- `on-page`: header is 14px / 600 `text-primary` on `background` with a `border-subtle-01` rule; for persistent areas such as cards and sidebars. `disclosed`: 32px header on `layer-01`, 12px `text-secondary` label; for popovers and other elevated layers.
- Items are 14px `text-primary`, min height from `size`, 1rem side padding; dividers are 1px `border-subtle-01`. Use extended dividers by default and `isInset` when the list sits next to other divided components.
- Clickable items hover to `layer-hover-01`, press to `layer-active-01`, disabled uses `text-disabled`; focus is a 2px inset `focus` outline.
- Actions are absolutely placed at the top end and capped at the list's height (header action up to xl, item action up to lg); item text reserves 5rem at the end when an action is present.
- Icons are decorative and `icon-primary`; use them on every item or none.
- Lists span the full width of their container; stack on-page lists 16px apart, disclosed lists flush.
- Content: title of one to three words in sentence case; items concise and similar in structure, no multi-paragraph text.

**Accessibility**
- The list is a `<ul role="list">` labelled by its header (`aria-labelledby`), or by `aria-label` when there is no header.
- Clickable items are real `<button>`s (Enter / Space activate, `disabled` removes them from the tab order); item and header actions are separate tab stops after the item.
- Give every icon-only action an `iconDescription` that names the item ("Download usage-march.csv").
- The pinned `Search` keeps its own visually hidden label (`labelText`); Escape clears it.
