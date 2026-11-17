# Link

Navigates to another page, site, section or address. `link-primary` text; underline on hover (always underlined when `inline`), `link-primary-hover` on hover, `text-primary` while pressed, and `link-visited` once visited when `visited` is set.

Hand-written from `packages/styles/scss/components/link`.

**When to use**
- Going somewhere: another page in the console, an external document, a section of this page, an email address.
- Standalone links after content ("View all buckets") and inline links inside running text.

**When not to use**
- Doing something (saving, deleting, changing what is shown): use a Button, or a ghost Button for low emphasis.
- A whole card that navigates: use a ClickableTile. Breadcrumb trails: Breadcrumb. Images as links: use a tile instead.

**You provide:**
- `children` (meaningful text naming the destination, not "click here" or "read more") and `href`. `target` and `rel` (`rel` defaults to "noopener noreferrer" for `_blank`).
- `size` sm (12/16) | md (14/18, default) | lg (16/22).
- `inline`: underlined, `display: inline` so it wraps with the paragraph; never shows an icon.
- `renderIcon`: icon file stem after the text: `"arrow--right"` for internal destinations, `"launch"` for external ones.
- `visited`: opt in to `link-visited` for visited destinations (off by default; use sparingly).
- `disabled`: renders without `href`, `role="link"`, `aria-disabled="true"`, `text-disabled`.
- `onClick`, `className`, and native attributes (`aria-describedby`, `id`, `download`).

**Example**
```jsx
const { Link } = window.Carbon;
<p>Volumes are kept for 7 days, see <Link href="/docs/retention" inline>volume retention</Link> to change this.</p>
<Link href="https://status.example.com" target="_blank" renderIcon="launch">Service status page</Link>
```

**Rules**
- Use `inline` links in running text so they are distinguishable without color; inline links take the paragraph's size. Standalone links match the body size of the page and have no underline until hover.
- The icon matches the text color, sits 8px after the text and is 16px; use the same icon for the same kind of destination everywhere.
- On inverse surfaces (high-contrast notifications, toggletips) links switch to `link-inverse`, `link-inverse-hover` and `link-inverse-visited`.
- Focus is a 1px `focus` outline.
- Do not use the same text for different destinations on one page; keep the number of inline links low.

**Accessibility**
- A native `<a href>`: Tab focuses it and Enter follows it. Do not use a link for an action; if you must, it also has to respond to Space.
- Disabled links have no `href` (they leave the tab order) and expose `aria-disabled="true"`; explain nearby why the destination is unavailable.
- Icons are decorative (`aria-hidden`); for `target="_blank"` tell people the link opens a new tab (in the text, or with `aria-describedby`).
