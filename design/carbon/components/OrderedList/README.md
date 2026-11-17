# OrderedList

Numbered and bulleted lists, plain or nested, in the productive (`body-01`) or expressive (`body-02`) type size. Documents `OrderedList`, `UnorderedList` and `ListItem`.

Hand-written from `packages/styles/scss/components/list/_list.scss` and `react/src/components/{OrderedList,UnorderedList,ListItem}` (Carbon v11.99.0).

**When to use**
- `OrderedList` for steps or ranked items where order matters: setup instructions, a migration checklist.
- `UnorderedList` for a short set of related items of equal weight: prerequisites, what a plan includes.

**When not to use**
- Data people sort, filter or select, or anything with columns. Use a DataTable.
- Label and value pairs or simple hierarchy with dividers. Use a StructuredList or ContainedList.
- Layout or navigation. Use Stack, a Link list or the UI shell side nav.

**You provide:**
- `ListItem` children (any content; `className` and attributes pass to the `li`).
- A nested list goes inside a `ListItem` and takes `nested`.
- Both lists take `isExpressive` (default `false`) and `className`; `OrderedList` also takes `native` (default `false`). Other attributes go on the `ol` / `ul`.

**Example**
```jsx
const { OrderedList, ListItem } = window.Carbon;
<OrderedList>
  <ListItem>Install the CLI and log in</ListItem>
  <ListItem>Target your cluster
    <OrderedList nested><ListItem>List clusters</ListItem><ListItem>Download the kubeconfig</ListItem></OrderedList>
  </ListItem>
  <ListItem>Apply the deployment file</ListItem>
</OrderedList>
```

**Rules**
- Type is `body-01` (0.875rem, 1.42857, 0.16px) or `body-02` with `isExpressive` (1rem, 1.5); item text is `text-primary`. Margin and padding are reset. Use productive lists in product UI.
- `OrderedList` numbers with a CSS counter (`1.`, `2.`) placed 1.5rem (24px) before the item, outside the box; leave that space or pad the container. `native` uses the browser's `decimal` markers instead.
- `UnorderedList` uses an en dash placed 1rem before the item, with a `margin-inline-start` of 1rem at the top level. Nested lists use a small square, 0.75rem before the item.
- Nested lists are indented 2rem, their items get 0.25rem of start padding, and nested ordered lists use `lower-latin` (a., b.). Go no deeper than two levels.
- Keep items short and grammatically parallel; let long items wrap (they wrap under the text, not the marker), never truncate. Order ordered lists logically.

**Accessibility**
- Native `ol` / `ul` / `li` elements, so screen readers announce the list and its item count. Markers are CSS (`::before` counters or `::marker`).
- Lists are not interactive; links or buttons inside items keep their own keyboard behavior.
- Because the CSS sets `list-style: none`, Safari VoiceOver may not announce the counter-based list as a list; add `role="list"` to the `ol` / `ul` if that matters to your users.
