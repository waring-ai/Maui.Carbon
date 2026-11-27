# Foundations

Carbon v11's visual language as Maui.Carbon exposes it. Every value below has a token; use the token, never the literal.

## Themes

Four themes share one set of token names: `White` (default) and `G10` are light, `G90` and `G100` are dark. Set the app theme with `cds:CdsResources Theme="…"` or `CdsThemeManager.SetTheme(CdsThemeName.G100)`, follow the system with `CdsThemeManager.FollowSystem()`, and switch one region with `cds:CdsTheme Theme="G90"` (themes nest).

- A product picks one light and one dark theme and uses them everywhere. White with G100 is the common pair; G10 suits dense, data-heavy light UI where tiles on white would wash out.
- A dark region inside a light page (a side panel, a code area, a hero) is a `CdsTheme` zone, not hand-picked dark colors.
- Design and check every screen in at least one light and one dark theme.

## Layers, not shadows

Depth is a step in fill color, never a drop shadow. The page is `CdsBackground`. Components always reference the `-01` keys (`CdsLayer01`, `CdsField01`, `CdsBorderSubtle01`, `CdsLayerHover01`…), and each enclosing `cds:CdsLayer` makes those keys resolve one level up:

| Where the content is | Level | `-01` keys resolve to |
|---|---|---|
| Directly on the page (outside any `CdsLayer`) | 1 | `Layer01`, `Field01`, `BorderSubtle01` |
| Inside one `CdsLayer` | 2 | `Layer02`, `Field02`, `BorderSubtle02` |
| Inside two (or more) | 3 | `Layer03`, `Field03`, `BorderSubtle03` |

- A surface on the page (a tile, a panel, a card-like group) fills with `{DynamicResource CdsLayer01}`. Put what sits on that surface inside a `CdsLayer`, so its fields, rows and hairlines take the next level's values and contrast with the surface. `CdsTile` does not step its content for you; the modal body (`CdsModalBody`) does.
- Three levels is the maximum. Needing a fourth means the hierarchy is too deep: flatten it or use a `CdsTheme` zone (which resets to level 1).
- `CdsLayer WithBackground="True"` paints the level's layer background behind its content when the region itself is the surface.
- Shadows exist only on floating surfaces (menus, popovers, toasts, the open dropdown), and the library draws them. Modals and side panels sit over the `CdsOverlay` scrim.

## Color

- **Interactive:** blue is the only brand hue and always means "you can act here": `CdsButtonPrimary`, `CdsLinkPrimary`, `CdsInteractive`, `CdsBorderInteractive`, `CdsFocus`. Do not use blue for decoration or emphasis.
- **Text:** `CdsText` `Tone`: `Primary` (content), `Secondary` (labels, secondary copy), `Helper` (helper text), `Error`, `OnColor` (on saturated fills), `Inverse` (on `CdsBackgroundInverse`), `Link`. `Placeholder` and `Disabled` are deliberately below 4.5:1: never essential information.
- **Icons:** `CdsIconPrimary`, `CdsIconSecondary`, `CdsIconInteractive`, `CdsIconDisabled`, `CdsIconOnColor`.
- **Status:** `CdsSupportError`, `CdsSupportSuccess`, `CdsSupportWarning`, `CdsSupportInfo`, and caution (`CdsSupportCautionMajor`/`Minor`). Always paired with an icon and a label (`CdsIconIndicator`, `CdsShapeIndicator`, notifications). Warning yellow is an icon fill only, never text (1.7:1 on white).
- **Categories:** the ten tag colors (`CdsTag Type="Blue|Cyan|Teal|Green|Magenta|Purple|Red|Gray|CoolGray|WarmGray"`) group items. A red tag does not mean "error"; status goes in an indicator.
- **Data:** chart colors come from the chart controls; see [data visualization](data-visualization.md).

## Typography

IBM Plex Sans for UI, Plex Mono for code, Plex Serif for quotations; weights 300, 400 and 600 only. Use `CdsText` with `TypeStyle`:

| Use | Style |
|---|---|
| Single-line UI text: buttons, menus, table cells, list rows | `BodyCompact01` (14/18, the default), `BodyCompact02` (16/22) |
| Paragraphs | `Body01` (14/20), `Body02` (16/24); keep lines under about 80 characters (`MaximumWidthRequest` ≈ 640) |
| Field labels, small metadata | `Label01`, `Label02` |
| Helper text | `HelperText01`, `HelperText02` |
| Headings inside components (tiles, panels, modals) | `HeadingCompact01`, `HeadingCompact02` |
| Page and section headings | `Heading03`…`Heading07`; `CdsHeading` maps outline level 1 to `Heading05`, 2 to `Heading04`, 3 to `Heading03`, 4 to `Heading02` |
| Expressive surfaces (onboarding, marketing-like landing pages) | `FluidHeading03`…`06`, `FluidParagraph01`, `FluidDisplay01`…`04` |
| Code | `Code01`, `Code02` |
| Legal and fine print | `Legal01`, `Legal02` |

- Productive (task) UI uses the productive styles; expressive styles are for a few deliberate moments, never in forms or tables.
- Hierarchy comes from size and weight steps in the scale, not color, italics, underline or all caps. Do not bold body text for emphasis in UI; use structure.
- Never set `FontSize` or `FontFamily` by hand on Carbon UI.

## Spacing and size

The 8px mini-unit. `CdsSpacing.Spacing01`…`Spacing13`: 2, 4, 8, 12, 16, 24, 32, 40, 48, 64, 80, 96, 160. `CdsStack Gap="n"` uses step *n*.

| Gap | Use |
|---|---|
| 2 to 4 (`Spacing01`/`02`) | inside compact controls, icon to text |
| 8 (`Spacing03`) | items within a group (checkboxes, tags, a label and its value) |
| 16 (`Spacing05`) | default component padding; related controls; fields in a dense side panel |
| 24 (`Spacing06`) | related groups; card and tile content padding; fields in modals |
| 32 (`Spacing07`) | form fields on a page; sections inside a component |
| 48 (`Spacing09`) | between page sections; before a form's buttons |
| 64+ | page-level sections, expressive layouts |

- Put gaps on the container (`CdsStack Gap`, grid spacing), not margins on children; components stay margin-free.
- Fluid spacing (`fluid-spacing-01`…`04`: 0, 2, 5 and 10% of the page width) is only for space between sections of expressive pages, never inside components: `CdsStack FluidGap="1..4"`, or the attached `cds:CdsFluidSpacing.Padding` / `Margin` whose thickness values are steps (`"3,0"` is `fluid-spacing-03` left and right). `CdsFluidSpacing.Resolve(step, width)` gives the value in code.
- Proximity carries meaning: related items closer, groups further apart. Use a consistent gap for one relationship across the product.
- Control heights: `CdsSizing.Small` 32, `Medium` 40 (default for inputs, tabs), `Large` 48 (default for buttons), `XLarge` 64, `X2Large` 80. Controls on one row share one size (`CdsSize`). Use `Sm` in dense tables and toolbars, `Md` in forms, `Lg`/`Xl` sparingly for prominent standalone actions.

## Grid and breakpoints

The 2x grid: `CdsGrid` with `CdsColumn` children (or the attached `CdsGrid.Span`).

| Breakpoint | From width | Columns | Outer margin |
|---|---|---|---|
| `Sm` | 0 (320 design minimum) | 4 | 0 |
| `Md` | 672 | 8 | 16 |
| `Lg` | 1056 | 16 | 16 |
| `Xlg` | 1312 | 16 | 16 |
| `Max` | 1584 | 16 | 24 |

- Columns have 16px padding on each side (32px gutters); text aligns to the padding edge. `CdsGrid Mode`: `Wide` (default, for text and most layouts), `Narrow` (content hangs into the gutter; type next to imagery), `Condensed` (1px gutters for tiles that read as one unit).
- Set spans per breakpoint (`SpanSm`, `SpanMd`, `SpanLg`…; a missing one inherits the smaller); `Span 0` hides a column at that size. Typical: full width on Sm, half on Md, a third or a quarter on Lg.
- Forms and reading content stay narrow even on wide screens (a form about 8 of 16 columns, or a fixed 576 to 672 width); tables and dashboards take the full width.
- Page body: put the content under a `CdsPageHeader` in one `CdsGrid FullWidth="True"` with a full-row `CdsColumn` (vertical padding only). The column box sits at the breakpoint margin plus 16, the same inset as the page header, so body and header line up at every breakpoint; a fixed horizontal padding does not.
- Grids nest: a `CdsGrid` inside a column (directly or deeper in it) is a subgrid. It has as many columns as the column spans, its columns line up with the page's, and it adds no outer margin. Its own `Mode` sets its gutters, so a narrow subgrid can sit in a wide grid. Don't cancel margins by hand with negative margins.
- Fixed boxes come from the size scale (8, 16, 24, 32, 48, 64, 80) and aspect ratios 1:1, 2:1, 2:3, 3:2, 4:3, 16:9 (`CdsAspectRatio`).
- `CdsBreakpointExtensions.FromWidth(width)` gives the breakpoint for custom adaptive layout. On phones, design for Sm first: one column, full-width controls, the primary action reachable.

Examples: the gallery's `Pages/Layout/` in the repository.

## Shape and borders

- Square by default: buttons, inputs, tiles, notifications, modals and data tables have no radius. Exceptions are built in (tags and toggles are pills, checkboxes have a 2px corner); do not add `CornerRadius` to Carbon surfaces.
- Inputs have no box border, only a bottom rule; that is intentional.
- Separate with 1px `CdsBorderSubtle01` hairlines or a layer step, never heavy borders or boxes around every group.

## Focus and interaction states

- Every interactive element shows a 2px `CdsFocus` ring (blue on light themes, white on dark). The library draws it; never hide it, and give custom interactive views one (build them on Carbon controls rather than raw gesture recognizers).
- Hover steps the fill one notch, pressed steps further, selected uses the selected token. Disabled uses the disabled tokens, not opacity on a whole region.
- Interactive targets are at least 24×24 (prefer the control's own size: 32/40/48).

## Motion

`CdsDuration`: `Fast01` 70 ms (button and toggle micro-interactions), `Fast02` 110 ms (fades), `Moderate01` 150 ms (small expansions), `Moderate02` 240 ms (expansions, toasts, modals), `Slow01` 400 ms (large expansions, important notifications), `Slow02` 700 ms (background dimming). The larger the change, the longer the duration.

`CdsEasing`: `Standard*` for elements that stay on screen, `Entrance*` for arriving, `Exit*` for leaving. `*Productive` is the default for task UI; `*Expressive` for occasional significant moments (a page opening, a primary action's result).

- Motion explains a change (where something came from or went); it is never decoration. Nothing autoplays, loops or flashes.
- Use `CdsMotion.AnimateAsync` for custom animation so reduced motion is respected (`CdsMotion.IsReducedMotion`); every state must be readable without the animation.

## Icons

- Carbon's single-ink set (`CdsIcon` / `CdsIcons`, names in `Generated/CdsIcons.g.cs`) at 16 (default), 20, 24 or 32. No emoji, no Unicode symbols, no other icon families.
- Icons support a label; they rarely replace one. An icon-only control is fine where the meaning is conventional (close, search, overflow, settings) and needs a name and tooltip (`CdsIconButton Label`).
- Status uses the filled glyphs next to a word. Keep one metaphor per action across the product.

## Imagery

Flat fills from the layer tokens. No gradients (except AI presence), textures, decorative illustration or stock photography in task UI; pictograms and photography belong to onboarding and marketing surfaces.

## AI presence

- Mark AI-generated or AI-recommended content, from one field to a whole page, with `CdsAILabel`, and give it explainability content (`CdsAILabelContent`): a short summary first, detail on demand.
- The AI treatment (soft gradient and glow, `CdsAISkeleton*` loading) identifies AI; never use it as decoration. An AI component behaves like its standard version.
- When the person edits AI content, it drops the AI styling and offers revert (`IsRevertActive`). Use `CdsChatButton` for prompts and quick actions in assistant surfaces.

Examples: the gallery's `Pages/AI/` in the repository.
