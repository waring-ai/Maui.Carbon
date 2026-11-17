# BadgeIndicator

A red dot or count pill pinned to the top-right corner of its parent, used on icon buttons for unread or pending items.

Hand-written from `packages/styles/scss/components/badge-indicator` and `packages/react/src/components/BadgeIndicator` (Carbon v11.99.0).

**When to use**
- Signal new activity on a header or toolbar icon button: a count when the number matters (unread notifications), a dot when it does not.

**When not to use**
- Status of an object: use `IconIndicator` or `ShapeIndicator`.
- Labels or counts inside content (a "+3" next to tags, a count in a tab): use a `Tag` or text.

**You provide:** an optional `count`. Omit it (or pass 0 through `IconButton`'s `badgeCount`) for a plain dot. Counts above 999 show `999+`. Optional `id`, `className`. The parent must be `position: relative` (a Button already is); the badge is absolutely positioned at `inset-block-start: 0; inset-inline-end: 0`. Usually you set `IconButton badgeCount` instead of rendering it yourself.

**Example**
```jsx
const { IconButton } = window.Carbon;
<IconButton kind="ghost" label="Notifications" renderIcon="notification" badgeCount={3} />
```

**Rules**
- Fill is `support-error`, text `text-on-color`, label-01 type, fully rounded; dot is 8px, the pill grows with its text up to 16px tall.
- Offset from the corner is 8px for a dot and 4px for a count (also 4px on `sm` buttons).
- The count form is meant for 48px (lg) ghost icon buttons in the header; the dot also suits toolbar buttons. One badge per control, and use them sparingly.

**Accessibility**
- The badge is plain text with no role. Tie it to its control with `aria-describedby` (IconButton does this with the badge's `id`) so the count is announced after the button name.
- A dot has no text: if the new state matters, say it in the button label ("Activity, new items").
