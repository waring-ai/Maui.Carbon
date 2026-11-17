# PatternResourceList

A resource list page: UI shell header and side nav, page header with breadcrumb and description, a searchable, sortable, selectable data table with batch actions, a row overflow menu and pagination.

Composed from this system's components following Carbon's data table usage and the global header, search and common actions patterns.

**When to use**
- The main page for a resource type that people browse, search, sort and act on in bulk (clusters, buckets, API keys, users).
- Use a ContainedList or StructuredList instead for a short read-only list inside a card or panel; use PatternFiltering when people need several filter categories, not just search.

**Components used**
`Header`, `HeaderName`, `HeaderNavigation`, `HeaderMenuItem`, `HeaderGlobalBar`, `HeaderGlobalAction`, `SideNav` (`SideNavLink`, `SideNavMenu`, `SideNavMenuItem`, `SideNavDivider`), `PageHeader` (`PageHeaderBreadcrumbBar`, `PageHeaderContent`, `PageHeaderContentText`, `PageHeaderContentPageActions`), `Breadcrumb`, `DataTable`, `Link`, `IconIndicator`, `OverflowMenu`, `MenuItem`, `MenuItemDivider`, `Button`, `IconButton`, `Pagination`.

**Composition rules**
- Header: product name left, product links next to it, system utilities (search, notifications, account) right-justified as 20px icon buttons. The side nav marks the current page with `isActive`.
- Page header: breadcrumb to the parent, `Clusters` title, one-sentence description, page-level icon actions on the right.
- Table toolbar: search first, the primary "Create cluster" button last. When rows are selected the batch bar replaces the toolbar and shows the count, the bulk actions and Cancel.
- Name column links to the detail page; status uses an `IconIndicator` (icon plus word, never color alone); numbers are right-aligned; dates use one format.
- Row actions go in a last 40px column with an `OverflowMenu` (`flipped`), the destructive item last after a divider, unavailable items disabled rather than removed.
- Pagination sits directly under the table and states the range ("1-10 of 23 items"). Content padding 24px top, 32px sides.

**Accessibility**
- The header has `aria-label`; the side nav and header navigation each have their own `aria-label`; the main region has `id="main-content"` for a skip link.
- The table is named with `aria-label` (or its title). Each row checkbox and overflow menu gets a name that includes the row ("Actions for prod-eu-01"); the actions column header is visually hidden text.
- The search field keeps its visually hidden label; sortable headers are buttons that announce their sort state.

Known workarounds in the preview: the overflow column is built with `renderCell` (the published DataTable has no `rowActions` yet), its wrapper is set to `overflow: visible` so the open menu is not clipped, and right-aligned header labels need a CSS override. The batch Delete action has no icon because the icon set has no trash can.
