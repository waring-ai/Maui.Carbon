# HeaderPanel

A 256px panel that slides in from the right edge under the 48px `Header`, opened by a `HeaderGlobalAction`; it holds a `Switcher` of product links or your own content such as notifications. Exports `HeaderPanel`, `Switcher`, `SwitcherItem` and `SwitcherDivider` (part of the UI shell, see `UIShell`).

Hand-written from `packages/styles/scss/components/ui-shell/{header-panel,switcher}` and `packages/react/src/components/UIShell/{HeaderPanel,Switcher,SwitcherItem,SwitcherDivider}.tsx` (Carbon v11.99.0). Left out: `SideNavSwitcher` and forwarded refs; `SwitcherItem` renders a plain `<a>` (or `element`) instead of the shell `Link` wrapper.

**When to use**
- System-level content tied to one header utility: the product switcher (always the far-right action, and only for switching products, offerings or accounts), notifications, help or a user profile panel.

**When not to use**
- For product navigation: use the `SideNav` (left panel) or `HeaderNavigation`.
- For a task with inputs and buttons: use a `Dialog` or a full page. For a few contextual options next to a control: use a `Popover` or `Menu`.

**You provide:**
- `HeaderPanel`: placed inside `Header` after the `HeaderGlobalBar` · `expanded` tied to state (or `defaultExpanded` for uncontrolled use) · `onHeaderPanelFocus` that closes it · `href` = `#<id>` of the action that opens it (focus returns there on Escape) · `addFocusListeners` (default true; `false` turns off blur/Escape closing) · `aria-label` for custom content · `className`, children.
- The opening `HeaderGlobalAction`: an `id`, `isActive` and `aria-expanded` while open, an `aria-label` that says what it opens, and a 20px icon (`switcher`, `notification`...).
- `Switcher`: `aria-label`, the same `expanded`, `SwitcherItem` and `SwitcherDivider` children.
- `SwitcherItem`: `href` · `isSelected` (current product) · `aria-label` · `tabIndex` (overrides the expanded-based default) · `target`, `rel`, `onClick`, `onKeyDown` · `element` · label as children.

**Example**
```jsx
const { Header, HeaderName, HeaderGlobalBar, HeaderGlobalAction, HeaderPanel, Switcher, SwitcherItem, SwitcherDivider, Icon } = window.Carbon;
<Header aria-label="Platform">
  <HeaderName prefix="IBM">Platform</HeaderName>
  <HeaderGlobalBar>
    <HeaderGlobalAction id="switcher-btn" aria-label={open ? 'Close switcher' : 'Open switcher'} aria-expanded={open} isActive={open} onClick={() => setOpen(!open)}>
      <Icon name="switcher" size={20} />
    </HeaderGlobalAction>
  </HeaderGlobalBar>
  <HeaderPanel expanded={open} onHeaderPanelFocus={() => setOpen(false)} href="#switcher-btn">
    <Switcher aria-label="Switch product" expanded={open}>
      <SwitcherItem href="/platform" isSelected>Platform</SwitcherItem>
      <SwitcherDivider />
      <SwitcherItem href="/storage">Storage</SwitcherItem>
    </Switcher>
  </HeaderPanel>
</Header>
```

**Rules**
- Panel: `position: fixed`, top 48px, bottom 0, flush right, z-index of the header; `layer-01` background, `text-secondary`; 0 wide when closed and 256px with 1px `border-subtle-01` side borders when expanded (scrolls vertically), animating width over `duration-fast-02` with `ease-exit-productive`. It floats over the page; it does not push content.
- Only one panel is open at a time. Its action shows the active state (`layer-01` fill and side borders) that flows into the panel.
- Closes on Escape, on focus leaving the panel, and, when its only child is a `Switcher`, on a click outside the panel and the header actions; all of these call `onHeaderPanelFocus`. Clicking the action again closes it too.
- Switcher items: 32px rows, heading-compact-01 (14px semibold) `text-secondary`, padding 6px 16px, first item 16px from the top. Hover `layer-hover-01` + `text-primary`, pressed `layer-active-01`, selected `layer-selected-01` + `text-primary` and `aria-current="page"`. Focus is a 2px inset `focus` outline.
- Dividers: 1px `border-subtle-01`, 224px wide, 8px above/below, 16px from each side. Use them to group items (current product, related products, all products), not between every item.
- Keep labels short (one line, they truncate); put the current product first and mark it `isSelected`. Custom content uses `layer-01` tokens; inside a `Layer` they remap.
- The panel is fixed-positioned: to embed it in a frame, give an ancestor `transform: translateZ(0)` and a height.

**Accessibility**
- Give the opening action `aria-expanded` and an `id`, and pass `href="#<id>"` so Escape returns focus to it; give custom panels an `aria-label`.
- `Switcher` is a `ul` with your `aria-label`; ArrowDown/ArrowUp move between items and wrap, skipping dividers. Items are in the tab order only while the Switcher is `expanded`; for custom content, set `tabIndex={-1}` on links while closed.
- `SwitcherItem` with `isSelected` sets `aria-current="page"`.
