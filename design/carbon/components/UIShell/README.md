# UIShell

The application frame: a 48px `Header` fixed to the top with the product name, primary navigation and global actions, and a `SideNav` for deep navigation that is either fully expanded (256px) or a 48px icon rail. Exports `Header`, `HeaderName`, `HeaderNavigation`, `HeaderMenuItem`, `HeaderMenu`, `HeaderGlobalBar`, `HeaderGlobalAction`, `HeaderMenuButton`, `HeaderPanel`, `Switcher`, `SwitcherItem`, `SwitcherDivider`, `SideNav`, `SideNavHeader`, `SideNavItems`, `HeaderSideNavItems`, `SideNavLink`, `SideNavMenu`, `SideNavMenuItem`, `SideNavDivider`, `SideNavFooter` and `SkipToContent` (see also the `HeaderPanel` card).

Hand-written from `packages/styles/scss/components/ui-shell/{header,header-panel,switcher,side-nav,content}` (Carbon v11.99.0). v11.99 ships no styles for `SideNavHeader`/`SideNavFooter`; theirs follow the v10 shell (48px rows) on v11 tokens. Left out: `SideNavSwitcher`, `SideNavDetails`, `HeaderContainer`, the tooltip on `HeaderGlobalAction` (the label is the native `title`), and the open/close delay timers on the rail.

**When to use**
- Every product page: the `Header` names the product and holds product navigation and global utilities, the `SideNav` (left panel) holds secondary navigation when there are more than five destinations or people switch between them often, and a `HeaderPanel` (right panel) holds the switcher or a utility's content.
- Pick the header type that fits: name only, name plus `HeaderNavigation` links and `HeaderMenu`s, name plus `HeaderGlobalAction` utilities, or a side nav driven by `HeaderMenuButton`.

**When not to use**
- For in-page sections: use `Tabs` or a `ContentSwitcher`. For a page title, breadcrumb and page actions: use a `PageHeader` under the shell.
- Do not build a third navigation tier in the side nav (no `SideNavMenu` inside a `SideNavMenu`); move deeper levels into tabs on the page.

**You provide:** a `Header` with `HeaderMenuButton`, `HeaderName` (`prefix` + product name), an optional `HeaderNavigation` of `HeaderMenuItem`s (mark the current one with `isActive`) and `HeaderMenu`s (`menuLinkName`, `aria-label`, `HeaderMenuItem` children; `isActive` marks the section, `defaultExpanded`/`expanded` + `onToggle` control it) and a `HeaderGlobalBar` of `HeaderGlobalAction`s whose child is a 20px `Icon` and whose `aria-label` names the action. A `HeaderPanel` (`expanded`, `onHeaderPanelFocus` to close it, `href` = `#id` of its action for focus return) placed inside `Header` holds a `Switcher` (`aria-label`, `expanded`) of `SwitcherItem`s (`href`, `isSelected`) and `SwitcherDivider`s. Add a `SideNav` with `aria-label` containing `SideNavItems`; items are `SideNavLink` (`renderIcon` takes an icon file stem, `isActive` marks the current page), `SideNavMenu` (`title`, optional `renderIcon`, `defaultExpanded`) with `SideNavMenuItem` children, and `SideNavDivider`. Put `HeaderSideNavItems` (`hasDivider`) first in `SideNavItems` to repeat the header links below lg; add `SideNavHeader` (`renderIcon`, label) before and `SideNavFooter` (`expanded`, `onToggle`) after the items. Control it with `expanded` (or `defaultExpanded`) and `onToggle`, use `isRail` for the collapsed rail. Put the page in an element with class `cds--content` after the side nav so it gets the 48px/256px offsets. Put `SkipToContent` (`href`, default `#main-content`; children, default "Skip to main content"; `tabIndex`) first inside `Header`.
Other props: `SideNav` `isFixedNav`, `isChildOfHeader` (default true), `isPersistent` (default true), `addFocusListeners` / `addMouseListeners` (default true), `onOverlayClick`, `onSideNavBlur`, `href` (Escape target); `SideNavLink` and `SideNavMenu` `large` (48px rows), `isActive`, `tabIndex`, `isSideNavExpanded`; `SideNavMenuItem` `href`, `isActive`, `as`/`element`; `HeaderMenuButton` `isActive`, `isCollapsible`, `renderMenuIcon`, `renderCloseIcon`; `HeaderName` `href`, `prefix` (default "IBM", `''` hides it), `element`; `HeaderMenuItem` `href`, `isActive`, `aria-current`, `element`, `tabIndex`.

**Example**
```jsx
const { Header, SkipToContent, HeaderMenuButton, HeaderName, HeaderNavigation, HeaderMenuItem, HeaderGlobalBar, HeaderGlobalAction, SideNav, SideNavItems, SideNavLink, SideNavMenu, SideNavMenuItem, Icon } = window.Carbon;
<>
  <Header aria-label="Platform">
    <SkipToContent href="#main-content" />
    <HeaderMenuButton aria-label={open ? 'Close menu' : 'Open menu'} isActive={open} onClick={() => setOpen(!open)} />
    <HeaderName href="/" prefix="IBM">Platform</HeaderName>
    <HeaderNavigation aria-label="Platform">
      <HeaderMenuItem href="/catalog" isActive>Catalog</HeaderMenuItem>
      <HeaderMenuItem href="/billing">Billing</HeaderMenuItem>
    </HeaderNavigation>
    <HeaderGlobalBar>
      <HeaderGlobalAction aria-label="Notifications"><Icon name="notification" size={20} /></HeaderGlobalAction>
    </HeaderGlobalBar>
  </Header>
  <SideNav aria-label="Side navigation" expanded={open} onOverlayClick={() => setOpen(false)}>
    <SideNavItems>
      <SideNavLink href="/" renderIcon="dashboard">Dashboard</SideNavLink>
      <SideNavMenu title="Infrastructure" renderIcon="folder" defaultExpanded>
        <SideNavMenuItem href="/clusters">Clusters</SideNavMenuItem>
        <SideNavMenuItem href="/buckets" isActive>Buckets</SideNavMenuItem>
      </SideNavMenu>
    </SideNavItems>
  </SideNav>
  <main id="main-content" className="cds--content" tabIndex={-1}>...</main>
</>
```

**Rules**
- The header and side nav use the shell tokens of the theme, not an inverse bar: `background`, hover `background-hover`, pressed `background-active`, selected `background-selected`, text `text-primary`/`text-secondary`, icons `icon-secondary` going to `icon-primary`, header bottom border and dividers `border-subtle-00`. In v11.99 the shell follows the theme (white and g10 are light, g90 and g100 are dark); there are no `$shell-header-*` tokens.
- Header height is 48px; action buttons are 48x48 with a 1px transparent border that becomes `focus` on focus. `isActive` actions fill with `layer-01` and get `border-subtle-00` side borders.
- Current header link: `text-primary` plus a 3px `border-interactive` underline. Current side-nav link: `background-selected`, 3px `border-interactive` bar on the start edge, semibold `text-primary`.
- Side nav rows are 32px (48px with `large`); icons are 16px with 24px of space before the label; sub-menu items are indented to 32px (72px when the group has an icon). The chevron rotates 180deg when open.
- `HeaderMenu`: the trigger is an `<a aria-haspopup="menu" aria-expanded>` with a 16px chevron (`icon-secondary`, 8px gap) that rotates 180deg when open. Open, the trigger fills with `layer-01` and a 200px `layer-01` list with a `0 4px 8px` `shadow` drops below it; items are 48px, hover `layer-hover-01`, active `layer-active-01`, current `layer-selected-01` with a 3px `border-interactive` bar on the start edge. Enter/Space/click toggle, ArrowDown opens and focuses the first item, ArrowUp/ArrowDown/Home/End move between items, Escape closes and refocuses the trigger, focus leaving the menu closes it. The trigger shows the current underline while closed if a child is active.
- `HeaderPanel`: `position: fixed`, 48px from the top, flush right, `layer-01` with `text-secondary`; 0 wide when closed, 256px with 1px `border-subtle-01` side borders when `expanded`, width animating over `duration-fast-02` with the productive exit curve. Escape, blur and (when its child is a `Switcher`) a click outside the panel and header actions call `onHeaderPanelFocus`. Mark its action `isActive` and `aria-expanded` while open.
- `Switcher`: items are 32px rows of heading-compact-01 `text-secondary` with 6px/16px padding, 16px top gap; hover `layer-hover-01` + `text-primary`, active `layer-active-01`, selected `layer-selected-01` + `text-primary` (`aria-current="page"`), focus a 2px inset `focus` outline. Dividers are 1px `border-subtle-01`, 224px wide with 8px/16px margins. ArrowUp/ArrowDown move between items and wrap; items are tabbable only while `expanded`.
- `HeaderSideNavItems` is hidden from lg up; below it shows header links as 32px heading-compact-01 side-nav rows (sub-menu items 400 weight, indented 68px) with a 1px `border-subtle-01` divider 16px below when `hasDivider`.
- `SideNavHeader` is a 48px row with a 16px icon and semibold label over a `border-subtle-00` rule; `SideNavFooter` is a 48px row (top `border-subtle-00`) with a full-width toggle: 20px chevron when collapsed, close when expanded, hover `background-hover`, 2px inset `focus` outline; its label is visually hidden and repeated as `title`.
- Rail: 48px wide showing icons only; hover, click or focus-in expands it to 256px, mouse-out, blur or Escape collapses it. When the nav is collapsed (not rail) its links are removed from the tab order.
- Below the lg breakpoint (1056px) `HeaderNavigation` is hidden and the side nav is 0px wide until expanded, with a full-screen `overlay` behind it; `HeaderMenuButton` is hidden from lg up unless `isCollapsible`.
- Focus on side-nav rows is a 2px inset `focus` outline; on header links a 2px `focus` border.
- Header content: a brief product name (IBM products prefix it with "IBM"); header links are unique nouns that describe their destination, not verbs, and do not open new tabs. `HeaderMenu` labels only open their menu; they never link to a page. Utilities sit right-aligned without gaps, the switcher always last.
- Use `aria-current="page"` or `isActive`, never both styles for different pages. Do not nest `SideNavMenu` inside `SideNavMenu`.
- `Header` and `SideNav` are `position: fixed`; to embed them in a frame, give an ancestor `transform: translateZ(0)` and set a height.

**Accessibility**
- `Header` renders a `header` (banner landmark) and `HeaderNavigation` and `SideNav` render `nav` landmarks: give each an `aria-label`, unique when there are several navigation regions. Keep DOM order equal to visual order (product items left, global utilities right).
- `SkipToContent` (`href` default `#main-content`, text default "Skip to main content") must be the first focusable element; it stays visually hidden until focused, then shows as a 48px bar with a 4px `focus` border over the header. Give the target `main` an `id` and `tabIndex={-1}`.
- `HeaderGlobalAction` and `HeaderMenuButton` are icon buttons: their `aria-label` is the accessible name and the native `title`. Utilities that open a panel get `aria-expanded` and `isActive` while it is open.
- Current pages: `isActive` or `aria-current="page"` on `HeaderMenuItem`, `SideNavLink` and `SideNavMenuItem`. `SideNavMenu` and `HeaderMenu` triggers expose `aria-expanded`; `HeaderMenu` follows the menu-button keys (Enter/Space toggle, ArrowDown opens and focuses the first item, arrows/Home/End move, Escape closes and refocuses the trigger).
- Escape inside the side nav collapses it (and navigates to `href` when set); a collapsed (non-rail) nav removes its links from the tab order.
