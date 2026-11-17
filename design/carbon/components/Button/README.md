# Button

Triggers an action. Use one `primary` button per view for the main action, `secondary` or `tertiary` for supporting actions, `ghost` for low-emphasis actions and `danger*` for destructive ones.

Hand-written from `packages/styles/scss/components/button` (Carbon v11.99.0). Left out: the hover/focus tooltip on icon-only buttons (the `iconDescription` is used as a native `title` instead; use IconButton for a real tooltip), `tooltipPosition` / `tooltipAlignment`, ButtonSkeleton, and the `as` prop.

**When to use**
- To start an action on the current page: save, create, deploy, delete, open a dialog.
- Primary for the one main action; secondary only next to a primary for the negative choice (Cancel, Back); tertiary alone or beside a primary; ghost for supplementary actions and toolbars; danger for destructive steps.

**When not to use**
- Navigating to another page or site: use a Link (a `href` Button is for actions that happen to be URLs, such as a download or an external console).
- More than two or three related actions: group them in a MenuButton, ComboButton or OverflowMenu.
- A row of buttons that belong together: wrap them in a ButtonSet so they share width and spacing.
- An icon-only action that needs a tooltip: use IconButton.

**You provide:**
- `children`: the label, sentence case, verb + noun ("Create cluster"); Done, Close, Cancel, Add and Delete may stand alone.
- `onClick`, or `href` to render an `<a>` (a disabled `href` button renders a disabled `<button>`). `type` button (default) | submit | reset.
- `kind` primary (default) | secondary | tertiary | ghost | danger | danger--primary (alias of danger) | danger--tertiary | danger--ghost.
- `size` xs | sm | md | lg (default) | xl | 2xl: 24 / 32 / 40 / 48 / 64 / 80px.
- `isExpressive`: 16px (`body-compact-02`) label and a 20px icon; with `size="lg"` it is Carbon's "large expressive" button.
- `renderIcon`: an icon file stem from the Icons group (`"add"`, `"arrow--right"`, `"launch"`); 16px at the end of the label (20px when expressive).
- `hasIconOnly` + `iconDescription` (required: the accessible name and `title`). `isSelected` (ghost icon-only only): selected fill and `aria-pressed`.
- `disabled`, `className`, plus any native attribute (`aria-*`, `id`, `form`, `name`) passed through.

**Example**
```jsx
const { Button } = window.Carbon;
<Button kind="primary" renderIcon="add" onClick={createCluster}>Create cluster</Button>
<Button kind="ghost" hasIconOnly renderIcon="star" iconDescription="Favorite" isSelected={fav} onClick={toggleFav} />
```

**Rules**
- Filled buttons put `text-on-color` on `button-primary`, `button-secondary`, `button-danger-primary`; tertiary uses a 1px `button-tertiary` border and text; ghost uses `link-primary` text with an `icon-primary` icon; danger tertiary and danger ghost use `button-danger-secondary`.
- Left-align the label and keep it to one line; a button is at most 20rem wide. The icon is pinned 16px from the end edge (8px after the label in ghost). Do not center labels.
- Match the button height to neighbouring fields (md beside 40px inputs) and never mix sizes inside a group. xl and 2xl put the label at the top of the button.
- Focus is a 1px `focus` border plus an inset 1px `focus` ring and a 2px `background` inner ring.
- Disabled is the `button-disabled` fill with `text-on-color-disabled` (tertiary and ghost: transparent with `text-disabled`); do not lower opacity. While an action runs, disable the button and show an InlineLoading.
- Selected ghost icon-only buttons use `background-selected` (`background-selected-hover` on hover).
- Danger buttons always have a visible label (never icon-only) and sit behind a confirmation for irreversible actions.
- Buttons are square: no border radius.

**Accessibility**
- Renders a native `<button type="button">` (or `<a href>`), so Tab focuses it and Enter / Space activate it (links: Enter).
- Icon-only buttons render the glyph `aria-hidden` and a visually hidden `iconDescription` as the name; give every icon-only button an `iconDescription`.
- `isSelected` on a ghost icon-only button sets `aria-pressed` (true / false) so it reads as a toggle button; keep the description constant ("Favorite"), do not change it with the state.
- Do not put a Button inside another interactive element (a clickable tile, a link).
