# Theme

`Theme` is a theme zone: a wrapper that switches every color token in its subtree to the `white`, `g10`, `g90` or `g100` theme and paints `background` / `text-primary`. `GlobalTheme` records the app's theme, and `Carbon.useTheme()` reads the nearest one.

Hand-written from `packages/react/src/components/Theme` and `packages/styles/scss/_zone.scss` (Carbon v11.99.0). Here a zone sets `data-theme="<theme>"` (tokens.css re-binds all color tokens under that attribute) plus Carbon's `cds--<theme>` class. Left out: `usePrefersDarkScheme`.

**When to use**
- A region that needs a different theme from the page: a dark (g100) header band, an inverse (g90) side panel or banner on a light page, a light preview inside a dark app.
- Theming a whole app: `data-theme` on `<html>` plus `GlobalTheme` so components can ask `useTheme()`.

**When not to use**
- Single controls or small accents. Use the component's own kind (an inverse tooltip, a high-contrast tag), not a zone.
- Stepping up a surface within the same theme. Use Layer.
- More than one contrasting zone per view; mixing many themes breaks the hierarchy.

**You provide:**
- `Theme`: `theme` (`white` | `g10` | `g90` | `g100`; other values render no zone), `as` (element, default `div`), `className`, `children` and any attribute (`style`, `aria-label`).
- `GlobalTheme`: `theme` and a single child element (it receives the ref).
- `useTheme()` returns `{ theme, isDark }`; `isDark` is true for g90/g100. The default outside any Theme is `{ theme: 'white', isDark: false }`.

**Example**
```jsx
const { Theme, Tag, Button } = window.Carbon;
<Theme theme="g90" as="aside" aria-label="Active incident" className="incident-panel">
  <Tag type="red">Incident</Tag>
  <p>prod-us-2 is degraded</p>
  <Button kind="tertiary" size="sm">View incident</Button>
</Theme>
```

**Rules**
- A zone resets layering to level one (it renders a `Layer level={1}`, and `useLayer()` inside it reports 1): components inside use the zone's `layer-01` / `field-01`, whatever Layer it sits in.
- The zone sets `background-color: background` and `color: text-primary`. Add your own padding.
- `GlobalTheme` only provides context. Theme the page itself with `data-theme` on `<html>`.
- Pair themes: white or g10 for light pages, g90 or g100 for dark ones, plus at most one contrasting zone (g100 header on white, g90 panel on g10).
- Never hard-code theme colors inside a zone. Use tokens, so content re-themes when it moves.

**Accessibility**
- Every Carbon theme meets WCAG AA contrast for its token pairs; a zone keeps that as long as content inside uses tokens.
- A zone adds no role. Give a themed region a landmark (`as="header"`, `as="aside"`) and a name when users should be able to jump to it.
- Theme changes are visual only; do not rely on the zone's color alone to signal state (add text or an icon, like the Incident tag).
