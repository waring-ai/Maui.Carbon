# Text

A polymorphic text element (`span` by default) that writes a `dir` attribute so mixed left-to-right and right-to-left strings render correctly. Carbon's labels (FormLabel, legends) render through it.

Hand-written from `packages/react/src/components/Text` (`Text`, `TextDirection`; Carbon v11.99.0). Left out: the `Legend` and other `createTextComponent` shortcuts (use `as="legend"`).

**When to use**
- User-generated or translated strings that may be in a right-to-left script: resource names, descriptions, comments, labels.
- An app-wide direction policy: wrap a subtree in `TextDirection` and decide `ltr` / `rtl` per string with `getTextDirection`.

**When not to use**
- Setting the direction of a whole page or layout. Put `dir` on `<html>` (or a container) instead.
- Typography. Text adds no styles; use a type class or the parent's styles.

**You provide:**
- `Text`: `children`, `as` (element or component, default `span`), `dir` (`auto` default, `ltr`, `rtl`) and any attribute (`className`, `id`...); the ref is forwarded.
- `TextDirection`: wraps a subtree and takes `dir` (default `auto`) and `getTextDirection(text)`, which returns `ltr` / `rtl` / `auto` for each nested Text's string children.

**Example**
```jsx
const { Text } = window.Carbon;
<td><Text as="div">{bucket.name}</Text></td>
<td><Text as="div">{bucket.description}</Text></td>
```

**Rules**
- A top-level Text always writes `dir` (default `auto`). A nested Text writes `dir` only when it differs from its parent's, or when the parent is `auto`.
- With `dir="auto"` the browser takes the direction from the first strong character, so "גיבוי: 14 עותקים!" keeps its punctuation on the correct side. A block element (`as="div"` or `p`) also aligns to the start of that direction; a `span` only isolates its text.
- Set `ltr` / `rtl` only when you know the language; use `auto` for anything a user typed.
- Text adds no typography. Pair it with a type class (`cds--type-body-01`, `cds--type-label-01`...) or the parent's styles.

**Accessibility**
- A correct `dir` keeps punctuation, numbers and mixed scripts in reading order for everyone, and lets screen readers voice the text in the right order (WCAG 1.3.2 meaningful sequence).
- Text keeps the semantics of the element you pass in `as`; choose a semantic element (`p`, `label`, `legend`) where it matters.
- Set the language with `lang` as well as `dir` when the string's language is known, so it is pronounced correctly.
