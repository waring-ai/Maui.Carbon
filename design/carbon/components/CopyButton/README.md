# CopyButton

An icon button with the copy glyph that shows a "Copied!" tooltip after it is clicked. It does not touch the clipboard itself.

Hand-written from `packages/styles/scss/components/copy-button` and `packages/react/src/components/CopyButton` (Carbon v11.99.0), composed from this bundle's IconButton. Left out: `autoAlign` and the fade keyframes (the tooltip simply switches label and hides after the timeout).

**When to use**
- Next to a single value the user is likely to paste elsewhere: an ID, key, endpoint URL or token.

**When not to use**
- Blocks of code or commands: use `CodeSnippet`, which has its own copy button.
- Any action other than copying: use `IconButton`.

**You provide:** `onClick` that writes to the clipboard (for example `navigator.clipboard.writeText(text)`). Optional `iconDescription` (default "Copy to clipboard"), `feedback` (default "Copied!"), `feedbackTimeout` (default 2000 ms), `align` (default bottom), `size` sm | md | lg (default md; square 32/40/48), `disabled`, `defaultCopied` to render in the feedback state, `defaultOpen` / `open` for the tooltip, `className`.

**Example**
```jsx
const { CopyButton } = window.Carbon;
<CopyButton size="sm" align="right" iconDescription="Copy key ID" onClick={() => navigator.clipboard.writeText(keyId)} />
```

**Rules**
- Fill is `layer-01`, `layer-hover-01` on hover, `layer-active-01` when pressed; the icon is `icon-primary` (`icon-disabled` when disabled). There is no border and focus is a 2px inset `focus` outline.
- Clicking switches the tooltip label to the feedback text and keeps the tooltip open for `feedbackTimeout`, then it reverts, whether the pointer is over it or not.
- Size it to the row it sits in: `sm` inside a 32px field or value row, `md` (default) on its own.
- It sits on a layer (`layer-01`); inside a `Layer` it steps to the next layer's tokens.

**Accessibility**
- The tooltip text is the accessible name, so keep `iconDescription` specific when several copy buttons share a view ("Copy key ID", "Copy endpoint").
- The feedback text is also written to a polite live region, so screen readers announce "Copied!" after activation.
- Enter or Space activates it; the tooltip shows on keyboard focus.
