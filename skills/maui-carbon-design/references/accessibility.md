# Accessibility

Carbon targets WCAG 2.1 AA. The library's controls ship roles, names, focus rings and keyboard support; the screen design still owns the parts below. Design them in from the start; they are hard to add later.

## Perceivable

- **Contrast.** Body text 4.5:1 against its surface, large text (24+, or 19+ semibold) 3:1, icons, control borders and the focus ring 3:1, in every theme. The Carbon token pairs meet this; custom colors or opacity usually break it. `Placeholder` and `Disabled` tones and warning yellow as a fill are deliberately low contrast: never essential information.
- **Never color alone.** Status pairs color with an icon and a word (`CdsIconIndicator`, `CdsShapeIndicator`, notifications). Errors show an icon and message, not only a red rule. Charts keep legends, labels and "Show as table". Links in running text are underlined (`CdsParagraph` with `CdsLink`).
- **Text resizes.** Layouts must survive larger text: let rows grow, wrap labels, avoid fixed heights on text containers.
- **Motion.** Nothing autoplays or flashes; reduced motion is honored by the library and by `CdsMotion.AnimateAsync` for custom animation.

## Operable

- **Keyboard.** Everything works with Tab, Shift+Tab, Enter, Space, Escape and the arrow keys. A composite control (tabs, radio group, menu, tree) is one Tab stop with arrows inside; the library does this, so do not add your own Tab stops inside them.
- **Focus order follows reading order.** Build the visual tree in reading order rather than reordering with layout tricks; do not use `TabIndex` to patch a confusing layout.
- **Overlays.** Modals trap focus while open and return it to the trigger on close. Set `CdsModal.LauncherElement` when the modal opens from code, and `InitialFocusElement` for the first field of a form modal (danger modals start on Cancel).
- **Targets.** At least 24×24; prefer the controls' own sizes (32/40/48), and keep dense `Sm` sizes for pointer-first desktop surfaces.
- **No time limits** on reading: errors and important messages stay until dismissed.

## Understandable

- **Every field has a visible label.** `HideLabel` only where context names the field visually (a table row, a search in a toolbar); the label still reaches screen readers.
- **Errors** name the fix and are tied to their field (the control's `InvalidText`). A failed submit summarizes at the top.
- **Plain language and a linear layout;** one primary action per view; consistent placement of navigation and actions across pages.

## Robust: names, structure and announcements

- **Headings form an outline.** One level-1 page title; no skipped levels. `CdsHeading` inside nested `CdsSection`s gets the level and style right; otherwise set `SemanticProperties.HeadingLevel` on the `CdsText`. A `CdsDataTable` title defaults to level 4; set `TitleHeadingLevel` to fit the page outline.
- **Icon-only controls have names:** `CdsIconButton Label` (also its tooltip), `CdsButton IconDescription` with `IsIconOnly`. Decorative icons have none.
- **Name groups without hiding them.** Use `cds:CdsSemantics.GroupName` (with `GroupKind`: `Group`, `Navigation`, `Banner`, `List`) on a layout that holds controls. Never put `SemanticProperties.Description` or `Hint` on a raw layout, border or scroll view with interactive children: on iOS and Mac Catalyst that turns the container into one element and VoiceOver cannot reach the controls inside. (On a Cds control, a Description is forwarded to its own hit target, so that is safe.)
- **Announce changes** that happen away from focus: result counts after filtering, "Saved", background refreshes. Use `CdsAnnouncer.Announce(text)` (polite by default; `CdsAnnouncePriority.Assertive` only for errors that block the person). `CdsInlineLoading` announces assertively by default; set `LiveMode="Polite"` for background work.
- **Rows out of view in a virtualized `CdsDataTable` are not in the accessibility tree;** give long tables search, sort and pagination so content is reachable.

## Test

- Keyboard only, through every flow.
- VoiceOver (Mac Catalyst, iOS) and TalkBack (Android) on the main flows; the library's own screen-reader behavior is verified from source and unit tests, not yet manually, so report anything odd.
- White and G100, at a narrow and a wide width, with larger text.
