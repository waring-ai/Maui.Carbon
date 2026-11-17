# Breadcrumb

Shows where the current page sits in a hierarchy as a row of links separated by slashes. Exports `Breadcrumb` and `BreadcrumbItem`.

Hand-written from `packages/styles/scss/components/breadcrumb` (Carbon v11.99.0). The overflow crumb is an `OverflowMenu` child of a `BreadcrumbItem`, drawn as a link-colored horizontal ellipsis with the shared Menu as its popup (Carbon's `breadcrumb-menu-options` caret styling is not reproduced). Left out: the skeleton state and automatic collapsing by available width (you decide which levels go into the overflow).

**When to use**
- Products with hierarchical content more than two levels deep, at the top of a page (or inside a `PageHeader`) to show the location and give one-click access to parent levels.
- Location-based trails reflect the information architecture; path-based trails reflect the steps the user took and must be generated dynamically. Pick one kind and use it across the product.

**When not to use**
- As primary navigation (use the `UIShell` header and side nav) or in products with one level of navigation.
- For the steps of a task or wizard: use a `ProgressIndicator`. For sibling views of the same level: use `Tabs`.

**You provide:**
- `Breadcrumb`: `aria-label` (default "Breadcrumb") · `size` `md` (default) | `sm` · `noTrailingSlash` (default false) · `className` (on the `nav`) · `BreadcrumbItem` children.
- `BreadcrumbItem`: `href` and a string child (a link; without `href` it renders as text) · `isCurrentPage` (default false) or `aria-current` · `className`. A non-string child such as a router link is cloned with the `cds--link` class. An `OverflowMenu` child becomes the overflow crumb: it defaults to `size="sm"`, `renderIcon="overflow-menu--horizontal"` and `iconDescription` "Show more breadcrumbs"; give it one `MenuItem` per hidden level whose `onClick` navigates there.

**Example**
```jsx
const { Breadcrumb, BreadcrumbItem, OverflowMenu, MenuItem } = window.Carbon;
<Breadcrumb noTrailingSlash>
  <BreadcrumbItem href="/">Dashboard</BreadcrumbItem>
  <BreadcrumbItem>
    <OverflowMenu iconDescription="Show 3 more levels">
      <MenuItem label="Kubernetes" onClick={() => go('/kubernetes')} />
      <MenuItem label="Clusters" onClick={() => go('/kubernetes/clusters')} />
      <MenuItem label="prod-us-south" onClick={() => go('/kubernetes/clusters/prod-us-south')} />
    </OverflowMenu>
  </BreadcrumbItem>
  <BreadcrumbItem href="/kubernetes/clusters/prod-us-south/pools">Worker pools</BreadcrumbItem>
</Breadcrumb>
```

**Rules**
- Type is `body-compact-01` (14px) for `md`, `label-01` (12px) for `sm`; use `sm` in page headers and dense areas, `md` at the top of pages without a page header. The separator `/` is `text-primary` with 8px (`md`) or 4px (`sm`) spacing on each side.
- Links use `link-primary`, `link-primary-hover` on hover and `text-primary` while pressed; visited crumbs stay `link-primary`. Focus is the 1px `focus` outline of Link.
- By default leave the current page out: the trail ends with its parent and the trailing slash leads into the page title. Include the current page only when the page has no visible title; it is then the last item, not a link (omit `href`), `text-primary`, with `aria-current`. Use `noTrailingSlash` when no title follows.
- Long trails: keep the first level and the last two levels, and collapse the middle levels into the overflow crumb. The ellipsis is `link-primary` (`link-primary-hover` with a 1px underline on hover). Breadcrumbs should not wrap to a second line; on small screens collapse more levels into the overflow first.
- Labels are short and match the titles of the pages they link to, from the highest level down.

**Accessibility**
- Renders `nav` with `aria-label` ("Breadcrumb" by default) and an `ol` of `li` items, so screen readers announce a navigation landmark and the number of levels.
- Separators are CSS pseudo-elements, not text, so they are not read or focusable.
- Tab moves between links and the overflow button, Enter follows a link. The overflow button has `aria-label` "Show more breadcrumbs" (or your `iconDescription`), `aria-haspopup="menu"` and `aria-expanded`; its menu follows the Menu keyboard rules.
- Mark the current page with `isCurrentPage` (sets `aria-current`) or pass `aria-current="page"`.
