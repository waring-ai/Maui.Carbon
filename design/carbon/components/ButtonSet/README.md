# ButtonSet

Groups related buttons in a row (or column) with a hairline separator between them. Exports `ButtonSet`.

Hand-written from the `.cds--btn-set` rules in `packages/styles/scss/components/button/_button.scss` and `react/src/components/ButtonSet/ButtonSet.tsx` (Carbon v11.99.0). Left out: the loading-state override and the `cds--btn--disabled` class (the disabled separator is applied to `:disabled`).

**When to use**
- Two or three related actions that sit together: a form footer (Cancel, Create), a modal or side-panel footer, a tile's actions.
- `stacked` in narrow columns and side panels; `fluid` in modal, tearsheet and side-panel footers where buttons should fill and bleed to the edges.

**When not to use**
- More than three actions. Use a MenuButton, an OverflowMenu or a toolbar instead.
- Unrelated actions, or icon-only buttons. Lay those out with Stack and use IconButton.
- Mixing sizes or mixing buttons with and without icons inside one set.

**You provide:**
- `Button` children in visual order. In dialogs, side panels, progressive forms and wizards the primary goes last (on the right); in in-page forms it goes first (on the left), as Carbon's forms pattern specifies. When stacked, the primary is first.
- `stacked` (default `false`): a column.
- `fluid` (default `false`): right-aligned layout that fills its container, orders the buttons by kind (ghost, danger ghost, tertiary, secondary, danger, primary) whatever the child order, and stacks itself (primary first) when the container gets narrow.
- `className`; other attributes go on the wrapping `div`.

**Example**
```jsx
const { ButtonSet, Button } = window.Carbon;
<ButtonSet fluid>
  <Button kind="secondary" size="xl" onClick={close}>Cancel</Button>
  <Button kind="danger" size="xl" onClick={deleteKey}>Delete key</Button>
</ButtonSet>
```

**Rules**
- Each button is 100% of the set but at most 12.25rem (196px; 20rem for `cds--btn--expressive`). Heights follow the Button `size`; use one size in a set.
- The separator is a 1px box-shadow in `button-separator` on the leading edge of every button but the first (top edge when stacked, flipped under `dir="rtl"`). It is removed on the focused button and the one after it so the focus ring stays clean.
- Disabled buttons use `icon-on-color-disabled` for the vertical separator and `layer-selected-disabled` for the stacked one.
- Fluid mode: buttons are `flex: 0 1 25%` up to 14.5rem, ghost buttons grow to fill, and each button has an 11rem minimum. Stacking is decided by container queries on the set (`container-type: inline-size`), at 11rem per button up to four.
- One high-emphasis (primary or danger) button per set. Good pairs: primary with secondary, tertiary or ghost; avoid two primaries, secondary without a primary, and tertiary in fluid sets.
- Alignment: left-justified on full pages and in forms, right-justified (or full-width fluid) in dialogs, side panels, inline notifications and data tables.
- A ButtonSet inside a flex row shrinks unless you give it a width (`flex: 0 0 auto` or a fixed basis), because each button is `inline-size: 100%`.

**Accessibility**
- ButtonSet adds no role; the buttons are separate tab stops in DOM order, which matches the visual order (a fluid set re-sorts the rendered children by kind, so tab order follows what is shown).
- Add `role="group"` and an `aria-label` on the set when the group needs a name (for example, several sets on one page).
- Disabled buttons cannot be focused; explain why an action is unavailable in nearby text.
