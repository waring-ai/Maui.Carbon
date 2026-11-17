# CodeSnippet

Displays code or a command, inline in text, as a single copyable line, or as a multi-line block that can expand.

Hand-written from `packages/styles/scss/components/code-snippet`, `copy-button` and `packages/react/src/components/CodeSnippet` (Carbon v11.99.0). Left out: the skeleton variant, syntax highlighting, and the full popover alignment set (only bottom, bottom-start, bottom-end, top, top-start, top-end, left, right). The standalone copy button is the separate `CopyButton` component.

**When to use**
- Commands and code people will copy: CLI steps, API calls, configuration files, keys and IDs in documentation and setup flows.
- Highlighting a command, variable or file name inside a sentence (`type="inline"`).

**When not to use**
- People need to edit the value: use a TextInput or TextArea (snippets are read-only).
- Prose or ordinary values: use plain text; a value that only needs a copy action can use `CopyButton` beside it.

**You provide:**
- The code as `children` (a string; newlines are kept in `multi`). `copyText` replaces what is copied (otherwise the rendered text).
- `type` inline | single | multi (default single).
- `disabled` · `hideCopyButton` · `wrapText` (multi) · `light` (kept for API parity; the fill follows layer tokens, so wrap it in a `Layer` instead).
- Copy feedback: `feedback` (default "Copied!") shown for `feedbackTimeout` ms (default 2000), `copyButtonDescription` (tooltip and name of the copy button, default "Copy to clipboard"), `align` of the tooltip (default bottom-end), `onClick(event)` after a copy.
- Multi-line limits: `minCollapsedNumberOfRows` 3 / `maxCollapsedNumberOfRows` 15 / `minExpandedNumberOfRows` 16 / `maxExpandedNumberOfRows` 0 (unlimited) · `showMoreText` / `showLessText` ("Show more" / "Show less").
- `aria-label` (name of the scrolling code area; for inline, of the copy button) · `className`.

**Example**
```jsx
const { CodeSnippet } = window.Carbon;
<CodeSnippet type="single" aria-label="Log in command" feedback="Copied to clipboard">
  cloud login --sso --region us-east
</CodeSnippet>
```

**Rules**
- Fill is `layer-01` (remapped inside a `Layer`), text `text-primary`, `code-01` mono (12px/16px lines, 0.32px tracking); hover `layer-hover-01`, active `layer-active-01`.
- Inline is a button with a 4px radius and 8px side padding inside the text flow; do not let it wrap over lines. The copied tooltip uses `background-inverse` / `text-inverse`.
- Single is 40px high, max width 768px, with a 40px copy button at the end and a 32px fade mask on overflowing text, which scrolls sideways.
- Multi pads 16px, puts a 32px copy button 8px from the top end, and shows a ghost "Show more" button only when the code is taller than `maxCollapsedNumberOfRows` lines; the chevron flips when expanded. Without Show more, long samples scroll inside the box.
- Disabled uses `text-disabled` / `icon-disabled`, removes the container from the tab order and blocks copying.
- Content: copied code must run as is (no prompts like `$`, correct syntax); keep feedback and button text short and action-based. Align the snippet box, not its text, with neighbouring form fields.

**Accessibility**
- The code area is `role="textbox"`, `aria-readonly`, `aria-multiline` for multi, focusable (2px `focus` outline) so keyboard users can scroll it; the fade mask is removed while focused. Name it with `aria-label`.
- The copy button is a `<button>` named by `copyButtonDescription`; Enter / Space copy, focus stays on the button, and the feedback text sits in a `role="status"` element so it is announced.
- Inline snippets are buttons named by `aria-label` and described by the code.
- Show more / Show less is a `<button>` operated with Enter / Space.
