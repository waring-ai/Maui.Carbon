# PatternEmptyStates

Empty states for four situations: first use, no search results, data that failed to load, and no permission; each explains why the area is empty and gives the next step.

Composed from tokens, `Heading`-level type classes, `Icon`, `Button` and `Link`, following Carbon's empty states pattern. Carbon has no empty-state component and this system has no pictograms, so a 32px `Icon` in `icon-secondary` stands in for the illustration.

**When to use**
- Any container that would otherwise render blank: a table with no rows, a dashboard tile whose data failed, a page the person can't see.
- Use an InlineNotification instead when the area still has content and you only need to report a problem about it.

**Components used**
`DataTable` (its `emptyState` slot), `Tile`, `Icon`, `Button`, `Link`.

**Composition rules**
- Order: optional icon, title (`heading-03`), body (`body-01`, `text-secondary`, max about 48 characters per line), then actions. Left-align the block; 48px padding in large areas, 32px/16px in tiles.
- 24px below the icon, 8px between title and body, 24px before the actions; actions sit in a row with 24px between a button and a link.
- Replace the structure the content would have used: hide table headers and footers so screen readers don't read an empty grid. In a search result, keep the toolbar so people can change the query.
- Copy: a positive, specific title ("Create your first bucket"), one or two sentences on why and what to do, one main action. Name the query in no-results titles and offer a way back ("Clear search"). Errors say what failed and offer a retry; permission states name the missing role and who grants it. Never a dead end, never jokes.
- Use a primary button only for the single first-use action on a page; when several empty states can show at once, use tertiary or secondary buttons so they don't compete.

**Accessibility**
- The title is a real heading at the right level for the page.
- An error empty state that appears after the page loaded can use `role="alert"`; a static one needs no live region.
- Icons are decorative (`aria-hidden`); the text carries the meaning. Links that open another site say so with the `launch` icon.
