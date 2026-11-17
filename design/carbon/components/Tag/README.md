# Tag

Labels and categorizes an object with a short pill. Tags are pills (`radius-tag`, 16px) in one of ten tint pairs, plus `high-contrast` and `outline`.

Hand-written from `packages/styles/scss/components/tag` and `themes/src/component-tokens/tag`. This is the read-only tag; the `filter` and `onClick` options are kept for compatibility, but the dedicated DismissibleTag, OperationalTag and SelectableTag cards cover those variants fully. Left out: TagSkeleton, the AI label decorator, start/middle truncation.

**When to use**
- To show categories, attributes or metadata of an item (region, environment, runtime) where several values must be told apart at a glance.
- In tables, lists, cards and detail pages next to the item they describe.

**When not to use**
- To navigate: use a Link. Tags are never links.
- For removable filters or user-entered keywords: use DismissibleTag. For toggling a filter on and off: SelectableTag. For opening a list of more tags: OperationalTag.
- For status (running, failed): use IconIndicator or ShapeIndicator, which pair color with a shape and text.

**You provide:**
- `children`: the label, a few words, under about 20 characters, sentence case unless it is a value such as `us-south`.
- `type` red | magenta | purple | blue | cyan | teal | green | gray (default) | cool-gray | warm-gray | high-contrast | outline.
- `size` sm (18px) | md (24px, default) | lg (32px).
- `renderIcon`: an optional decorative 16px icon before the label (icon file stem); not drawn at `sm`.
- `disabled`, `className`, plus native attributes.
- Compatibility: `filter` + `onClose` (+ `title`, the close button name, default "Clear filter") adds a close button; `onClick` renders an operational `<button>` with the `tag-hover-<color>` hover.

**Example**
```jsx
const { Tag } = window.Carbon;
<Tag type="purple">Kubernetes</Tag>
<Tag type="blue" size="sm">us-south</Tag>
<Tag type="teal" renderIcon="ai-generate">AI assisted</Tag>
```

**Rules**
- Each color is a pair: `tag-background-<c>` fill with `tag-color-<c>` text. All pairs meet 4.5:1 in every theme; never mix a background from one color with text from another.
- Use different colors for different categories and the same color for the same category everywhere. Color is decoration; the label carries the meaning.
- Labels never wrap: they truncate with an ellipsis at 13rem and the full text is the `title`.
- sm for dense rows (table cells), md by default, lg beside 32px controls or as a focal element. Space tags 8px apart.
- `outline` is the lowest emphasis, `high-contrast` the highest; disabled is `layer-01` with `text-disabled`.

**Accessibility**
- A read-only tag is a `<span>` with no role: it is read as text in place, so the label must make sense on its own.
- The `filter` close button is a `<button>` named by `title` ("Clear filter" by default); name it after the value ("Remove filter Region: eu-de").
- An `onClick` tag is a native `<button>` (Tab, Enter, Space).
- Disabled tags are exempt from contrast requirements; do not use disabled styling for information people need.
