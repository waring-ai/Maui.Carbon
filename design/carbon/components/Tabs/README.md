# Tabs

Switches between related views of the same context without leaving the page. Line tabs (40px) sit on a 2px `border-subtle-00` rule with a 2px `border-interactive` underline on the selected tab; contained tabs (48px) are `layer-accent-01` blocks, the selected one in `layer-01` with a 2px top edge.

Hand-written from `packages/styles/scss/components/tabs`. Takes an `items` array instead of Carbon's `TabList` / `Tab` / `TabPanels` composition. Left out: vertical tabs (`TabsVertical`), the overflow scroll buttons (the tab row scrolls, keeping the focused tab in view), sm / lg size props, the hover tooltip on icon-only tabs (they use `title`) and the AI label decorator.

**When to use**
- Grouping related content into a few peer sections on one page, in a modal, a side panel or a card: a cluster's Overview, Nodes, Networking, Access.

**When not to use**
- Switching the format or filter of the same content (list vs. grid, last hour vs. last day): use a ContentSwitcher.
- Steps in a sequence: use a ProgressIndicator.
- Content people need to compare side by side, or primary site navigation: use the page layout or UIShell side nav.

**You provide:**
- `items`: `[{ label, content, disabled?, renderIcon?, secondaryLabel? }]`. `renderIcon` is pinned after the label (or is the whole tab when `iconOnly`); `secondaryLabel` adds a second line and makes contained tabs tall (64px).
- `selectedIndex` + `onChange({ selectedIndex })` for controlled tabs; `selectedIndex` alone or `defaultSelectedIndex` sets the initial tab.
- `contained`; `fullWidth` (contained only: tabs share the width equally; auto width below 42rem).
- `activation` automatic (default: arrows select) | manual (arrows move focus, Enter / Space select; use when a panel is slow to load).
- `iconOnly` with `iconSize` default (40px) | lg (48px): every item needs a `renderIcon`; `label` becomes the tab's name and `title`.
- `dismissable` + `onTabCloseRequest(index)`: a close icon on each tab and the Delete key on a focused tab; you remove the item and update the selection.
- `ariaLabel` (names the tablist; default "Tabs"), `className`.

**Example**
```jsx
const { Tabs } = window.Carbon;
<Tabs
  ariaLabel="Cluster sections" selectedIndex={tab} onChange={({ selectedIndex }) => setTab(selectedIndex)}
  items={[
    { label: 'Overview', content: <ClusterOverview /> },
    { label: 'Nodes', content: <NodeTable /> },
    { label: 'Billing', disabled: true },
  ]}
/>
```

**Rules**
- Selected label is `text-primary` semibold; unselected `text-secondary`; disabled `text-disabled` (contained: `button-disabled` fill with `text-on-color-disabled`). Hover steps the rule to `border-strong-01` (line) or the fill to `layer-accent-hover-01` (contained).
- Labels are one or two words, sentence case; a tab is 80-240px wide and labels truncate with an ellipsis. Use at least two tabs (one is allowed when dismissable) and about six at most per row; full-width contained tabs work best with four or fewer.
- Line tabs are auto-width; only contained tabs may be full width, and only contained tabs carry secondary labels.
- Do not mix tabs with and without icons in one set. Icon-only tabs need instantly recognizable glyphs.
- Dismissable tabs are for user-created content (open files, saved queries), never for navigation or critical sections.
- Panels hold content for one tab only; the other panels are hidden, not unmounted, and the first tab is selected by default.

**Accessibility**
- `role="tablist"` (named by `ariaLabel`) with `<button role="tab">` children carrying `aria-selected` and `aria-controls`; each panel is `role="tabpanel"` with `aria-labelledby` and `tabindex="0"`.
- Roving tab index: only the current tab is in the tab order, so Tab moves from the tab row into the panel. Left / Right move between enabled tabs (wrapping), Home / End jump to the first / last enabled tab; disabled tabs are skipped.
- With `activation="manual"`, arrows only move focus and Enter / Space select. With `dismissable`, Delete requests closing the focused tab; the close icon is a pointer target only.
- Icon-only tabs get `aria-label` from `label`.
