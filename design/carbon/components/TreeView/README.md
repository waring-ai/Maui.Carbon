# TreeView

A hierarchical list of expandable nodes for navigating nested data such as folders, resource groups or documentation sections.

Hand-written from `packages/styles/scss/components/treeview` and `packages/react/src/components/TreeView` (Carbon v11.99.0). Long labels get a `title` attribute (native tooltip) instead of Carbon's truncation tooltip. Left out: link nodes (`href`), the controllable-expansion feature flag, type-ahead search, and a collapsed parent taking on the selected style of a hidden selected child.

**When to use**
- Navigating a hierarchy with several levels: folders and objects in a bucket, resource groups and their resources.
- Organizing a large set of items into nested categories.

**When not to use**
- Only one level of nesting: use an Accordion, a ContainedList or an expandable DataTable.
- Showing and hiding page content: use an Accordion.
- Primary product navigation: use the SideNav of the UI shell.

**You provide:**
- `TreeView` with a `label` (accessible name; `hideLabel` hides it visually) or `aria-label`, wrapping `TreeNode`s.
- `TreeView`: `size` xs | sm (24px or 32px rows, default sm) · `active` (id of the current node) · `selected` (array of ids) · `multiselect` (Ctrl/Cmd+click toggles) · `onSelect(event, node)` where `node.selected` is the new id list · `onActivate(node)` · `className`. `active` and `selected` seed the state and re-sync when their values change; clicks still move them.
- `TreeNode`: `label`, `id` (needed for `active` / `selected`; generated otherwise), `value`, `renderIcon` (icon file stem such as `"folder"`, `"document"`), `disabled`, `isExpanded` / `defaultIsExpanded`, `onToggle(event, { id, isExpanded, label, value })`, `onSelect(event, node)`, `className`, and nested `TreeNode`s as `children` (a node with children is a parent).

**Example**
```jsx
const { TreeView, TreeNode } = window.Carbon;
<TreeView label="Folders in logs-archive" active="m03" selected={['m03']} onSelect={(e, node) => openFolder(node.id)}>
  <TreeNode id="y2026" label="2026" renderIcon="folder" defaultIsExpanded>
    <TreeNode id="m03" label="03 (March)" renderIcon="folder" />
    <TreeNode id="m02" label="02 (February)" renderIcon="folder" />
  </TreeNode>
</TreeView>
```

**Rules**
- Rows sit on `layer-01` with `text-secondary`; hover is `layer-hover-01` + `text-primary`; selected is `layer-selected-01` (hover `layer-selected-hover-01`); the active node adds a 4px `interactive` bar at the start edge. Focus is a 2px inset `focus` outline on the row.
- Disabled nodes use `field-01` fill with `text-disabled` / `icon-disabled` and `cursor: not-allowed`.
- Indentation is computed from depth (16px per level plus the toggle width, 8px more per level when icons are used); the whole row stays full width for hover and selection.
- Clicking the caret only expands or collapses; clicking anywhere else on the row selects it.
- Icons: use them on every node or on none (folders for parents, documents for leaves is the common pairing).
- Labels are `body-compact-01` and truncate with an ellipsis. Branch labels summarize what they contain; leaf labels name the item.

**Accessibility**
- Markup is `ul[role=tree]` > `li[role=treeitem]` with `aria-expanded` (parents), `aria-selected`, `aria-current` (active) and `aria-disabled`, and `ul[role=group]` for children; `aria-multiselectable` is set with `multiselect`.
- Only one node is in the tab order (the active one, else the selected one, else the first).
- Keyboard: Up / Down move through visible nodes, Home / End jump to the first / last, Right expands a closed parent then moves to its first child, Left collapses an open parent or moves to the parent, Enter toggles a parent and selects, Space selects.
- Always give the tree a name with `label` (visually hidden with `hideLabel` if the context is clear).
