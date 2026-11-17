# PageHeader

The top-of-page header from Carbon v11.99 (preview): a breadcrumb bar, the page title with optional icon and contextual actions, page actions, subtitle and body text, and a tab bar with tags, all on `layer-01` and aligned to the 16-column grid.

Hand-written from `packages/styles/scss/components/page-header` and `packages/react/src/components/PageHeader` (Carbon v11.99.0). Left out: collapsing page actions into an overflow MenuButton when space runs out (`menuButtonLabel` is accepted but unused), the "+N" tag overflow popover, and the DefinitionTooltip on a clipped title (a native `title` tooltip is used instead).

**When to use**
- At the top of the main content of every page under the `UIShell`, to state what the page is, where it sits (breadcrumb) and what can be done with it (page actions), and to host page-level `Tabs`.
- Pick only the parts the page needs: a title alone for top-level pages, add the breadcrumb bar for nested pages, the tab bar when the page has sections, a hero image for landing pages.

**When not to use**
- More than once per page, or inside a card, panel or `Dialog`: use a `Heading` there.
- For the global product bar (name, product navigation, utilities): that is the `UIShell` `Header`.

**You provide:** a `PageHeader` (`className`) holding, in order:
- `PageHeaderBreadcrumbBar`: children (a `Breadcrumb`, usually `size="sm"`) · `renderIcon` (16px icon before it) · `contentActions` (e.g. a md `Button`) · `pageActions` (usually md ghost `IconButton`s) · `border` (default true) · `contentActionsFlush`, `pageActionsFlush` (default false).
- `PageHeaderContent`: `title` (required string) · `renderIcon` (32px icon) · `contextualActions` (e.g. a lg `Tag` for status) · `pageActions` (a `PageHeaderContentPageActions`) · children (usually `PageHeaderContentText`).
- `PageHeaderContentPageActions`: `actions`, an array of `{ id, body, onClick }` rendered in order or any node; wrap even a single button in it so it right-aligns.
- `PageHeaderContentText`: `subtitle`, children = body text.
- `PageHeaderTabBar`: `tabs: [{ label, disabled, id, panelId }]` with `selectedIndex` / `defaultSelectedIndex` (0) and `onChange({ selectedIndex })`, or a tab strip as children · `ariaLabel` (default "Page tabs") · `tags: [{ type, text, size, id }]` aligned to the end.
- `PageHeaderHeroImage`: wraps an image in a 2x1 box (3x2 below 66rem); put it in a `Grid` `Column` beside a column holding the breadcrumb bar and content.
- `renderIcon` takes an icon file stem (`"virtual-machine"`), an element or a component. Render the tab panels below the header and switch them from `onChange`.

**Example**
```jsx
const { PageHeader, PageHeaderBreadcrumbBar, PageHeaderContent, PageHeaderContentPageActions, PageHeaderContentText, PageHeaderTabBar, Breadcrumb, BreadcrumbItem, Button, Tag } = window.Carbon;
<PageHeader>
  <PageHeaderBreadcrumbBar>
    <Breadcrumb size="sm"><BreadcrumbItem href="/compute">Compute</BreadcrumbItem><BreadcrumbItem href="/compute/servers">Virtual servers</BreadcrumbItem></Breadcrumb>
  </PageHeaderBreadcrumbBar>
  <PageHeaderContent title="dal-vm-01" renderIcon="virtual-machine" contextualActions={<Tag type="green" size="lg">Running</Tag>}
    pageActions={<PageHeaderContentPageActions actions={[{ id: 'snap', body: <Button size="md" renderIcon="add">Create snapshot</Button> }]} />}>
    <PageHeaderContentText subtitle="Checkout service, US storefront">Backups run nightly at 02:00 UTC.</PageHeaderContentText>
  </PageHeaderContent>
  <PageHeaderTabBar tabs={[{ label: 'Overview' }, { label: 'Storage volumes' }]} selectedIndex={tab} onChange={(e) => setTab(e.selectedIndex)} />
</PageHeader>
```

**Rules**
- Background `layer-01` with a 1px `border-subtle-01` bottom border; the breadcrumb bar is 40px high with its own `border-subtle-01` border (turn it off with `border={false}` when nothing follows the bar).
- Content area: 24px top and bottom padding. Title is heading-04 (28px/400), clamped to 2 lines and 640px wide (1 line when `contextualActions` are present); subtitle is heading-03 (20px), body is body-01 (14px) up to 640px.
- Title row is a grid: title block on the left, page actions right-aligned with a 16px gap; below 42rem the actions stack under the title and left-align.
- Use md (40px) buttons in the breadcrumb bar and page actions; at most one primary action, others ghost icon-only with a label (tooltip).
- The tab bar pulls 16px left so tab labels align with the title; tags sit at the end of the tab row with a 64px gap from the tabs.
- Titles are the name of the object or page in sentence case (a resource name stays as typed); the breadcrumb ends with the parent level, never repeats the title.

**Accessibility**
- The title renders as an `h4` (as in Carbon); keep it the only page-level title and make the heading levels around it consistent. A clipped title gets a native `title` tooltip with the full text.
- Tabs are a `tablist` (`aria-label` from `ariaLabel`) of `role="tab"` buttons with `aria-selected` and `aria-controls` (pass `panelId`); only the selected tab is in the tab order. Left/Right move and select (skipping disabled tabs), Home/End jump.
- Icon-only page actions need a `label` on `IconButton` (its accessible name and tooltip). Decorative `renderIcon`s are hidden from assistive technology.
