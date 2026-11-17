# DismissibleTag

A tag with a close button, used for applied filters or chosen values that the user can remove.

Hand-written from `packages/styles/scss/components/tag` and `packages/react/src/components/Tag/DismissibleTag.tsx` (Carbon v11.99.0). Left out: the deprecated `slug` prop and AI-label resizing of `decorator`; `renderIcon` takes an icon file stem.

**When to use**
- Applied filters, search keywords or user-chosen values (labels on a resource, recipients) that the user can remove one by one.

**When not to use**
- Labels that must always stay: use a read-only `Tag`.
- An on/off choice from a set: use `SelectableTag`. A tag that opens more detail: use `OperationalTag`.
- Navigation to another page: use a `Link`. Do not give one tag several functions.

**You provide:** `text` (the label), `onClose` (remove the tag from your own state; the tag does not hide itself), and optionally `type` (gray default, red, magenta, purple, blue, cyan, teal, green, cool-gray, warm-gray, high-contrast, outline), `size` sm | md | lg (18/24/32px, default md), `disabled`, `renderIcon` (16px icon before the label, hidden at sm), `title` (close button label and tooltip, default "Dismiss"), `dismissTooltipLabel` (used instead when the label is truncated, default "Dismiss tag"), `dismissTooltipAlignment` (default bottom), `tagTitle` (native title of the label, default `text`), `decorator`, `id`, `className`.

**Example**
```jsx
const { DismissibleTag } = window.Carbon;
{filters.map(f => <DismissibleTag key={f.id} type="blue" text={f.label} title="Remove filter" onClose={() => removeFilter(f.id)} />)}
```

**Rules**
- Colors come from the `tag-background-*`, `tag-color-*` tokens; the close button hover circle uses `tag-hover-*`. Use color to separate categories (region, status, owner), not one color for everything.
- Close button is a circle as tall as the tag (18/24/32px) with a 16px close icon, 2px after the label (5px at sm); focus is an inset 1px `focus` ring (`focus-inverse` on high-contrast).
- Max width 13rem; long labels truncate with an ellipsis and keep the full text in the label's `title`. Never wrap a tag onto two lines.
- Sizes: sm for dense or inline spots (no icon), md by default, lg next to 32px-high controls.
- Disabled uses `layer-01` with `text-disabled` and an `icon-disabled` close icon; the button is disabled, not hidden.
- Content: a few words, under about 20 characters; "Key: value" works well for filters.
- In a filter bar, offer a "Clear filters" action next to the tags.

**Accessibility**
- Only the close button is focusable: Tab reaches it, Enter or Space removes the tag. Move focus to a sensible place (the next tag or the filter control) after removal.
- The close button's accessible name is `title` followed by the tag text ("Remove filter Region: us-east"), so a row of tags stays distinguishable; its tooltip shows `title` on hover and focus.
- Disabled tags are not operable and are exempt from contrast requirements.
