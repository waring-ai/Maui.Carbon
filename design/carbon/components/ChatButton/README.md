# ChatButton

A rounded (pill) button for chat and conversational UIs, plus the outlined "quick action" variant for suggested prompts.

Hand-written from `packages/styles/scss/components/chat-button` and `packages/react/src/components/ChatButton` (Carbon v11.99.0); same markup and colors as `Button` (`cds--btn` classes) with the `cds--chat-btn` modifiers. Left out: `ChatButtonSkeleton`. The Carbon `chat-button-*` tokens are aliases, used here directly: `link-primary`, `background-hover`, `link-primary-hover`, `background-active`, `background-selected`, `text-secondary`. The icon set here has no "send" glyph; the preview uses `arrow--up`.

**When to use**
- Actions inside a chat or AI assistant surface: send, stop generating, attach, regenerate.
- `isQuickAction` for suggested prompts or follow-ups the user can pick instead of typing.

**When not to use**
- Anywhere outside chat and AI surfaces. Use Button; pills do not match the rest of Carbon.
- Icon-only actions. Use IconButton.
- Filters or persistent selections. Use SelectableTag or a ContentSwitcher.

**You provide:**
- `children` (a short label, verb first) and `onClick`.
- `kind`: `primary` | `secondary` | `tertiary` | `ghost` | `danger` (default `primary`).
- `size`: `sm` | `md` | `lg` (32/40/48px, default `lg`; any other value logs an error and falls back to `lg`).
- `renderIcon`: icon name shown after the label; `disabled`; `type` (default `button`).
- `isQuickAction` forces ghost + sm and draws the outlined quick-action pill; `isSelected` marks the chosen quick action.
- `className`; other attributes (`id`, `aria-*`, `title`) go on the `<button>`; the ref is forwarded.

**Example**
```jsx
const { ChatButton } = window.Carbon;
<ChatButton isQuickAction isSelected={prompt === 'nodes'} onClick={() => ask('Show nodes not ready')}>Show nodes not ready</ChatButton>
<ChatButton renderIcon="arrow--up" disabled={!message} onClick={send}>Send</ChatButton>
```

**Rules**
- Radius is 24px at lg, 20px at md, 16px at sm. Without an icon the end padding is 15px, so the label is centered in the pill.
- Quick action: 1px `link-primary` border and text on a transparent fill; hover and active drop the border and fill with `background-hover` / `background-active` and `link-primary-hover` text; focus is a `focus` border plus an inset 1px `focus` ring.
- Selected quick action: no border, `background-selected` fill, `text-secondary` text (also when disabled). Disabled: `button-disabled` border and text.
- Offer a few short quick actions (2 to 4) after an assistant message and mark at most one selected; keep one primary action (Send) per composer.

**Accessibility**
- A native `button`: Tab focuses it, Enter or Space activates it, `disabled` removes it from the tab order.
- `isSelected` is visual only. If a quick action stays selected, also set `aria-pressed` on it so the state is announced.
- Group quick actions in an element with `role="group"` and an `aria-label` ("Suggested prompts"). Give an icon-only meaning in the label text, not the icon.
