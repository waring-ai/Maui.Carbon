# Slider

A draggable thumb on a track for choosing a number (or, with `twoHandles`, a range) between a minimum and maximum, with an editable number box.

Hand-written from `packages/styles/scss/components/slider` and `packages/react/src/components/Slider` (Carbon v11.99.0). `SliderSkeleton` is exported alongside. Left out: the value tooltip over the thumb when `hideTextInput` is set, custom-value (low / medium / high) labels, RTL layout, fluid variant, the `light` prop, `onRelease` / `onBlur` callbacks and the auto-correct status announcement.

**When to use**
- A single number or a range where the relative position matters more than the exact figure: node counts, a size budget, a sampling rate.

**When not to use**
- Very wide ranges (1 to 1000) or tiny ones (1 to 3): use a NumberInput.
- Non-numeric values or dates: use a Select, RadioButtonGroup or DatePicker.

**You provide:**
- `labelText` (`hideLabel` keeps it for screen readers only) and `id` (auto-generated if omitted).
- `min` (default 0), `max` (default 100), `step` (default 1), `stepMultiplier` (default 4; used by Shift+arrow, PageUp and PageDown).
- `value` (controlled) or `defaultValue`; for a range add `twoHandles` with `valueUpper` / `defaultValueUpper`. `onChange` receives `{ value }` or `{ value, valueUpper }`.
- Labels: `formatLabel(value, label)` formats the min / max labels and `aria-valuetext`; `minLabel` / `maxLabel` are appended by the default formatter. `hideTextInput` removes the number box.
- `ariaLabelInput` / `ariaLabelInputUpper` name the boxes (defaults "Lower bound" / "Upper bound" for a range) and the range handles.
- States: `invalid` + `invalidText` (default "Value is out of range"), `warn` + `warnText`, `disabled`, `readOnly` (thumb hidden, value still shown). `invalid` and `warn` are ignored while disabled or read only; a typed value outside `min`/`max` also shows invalid until it is clamped on blur.
- Also passed through: `name`, `nameUpper` (form names of the boxes), `className`.
- `SliderSkeleton`: `twoHandles`, `hideLabel`, `className`.

**Example**
```jsx
const { Slider } = window.Carbon;
<Slider id="volume-size" labelText="Size (GB)" min={20} max={2000} step={10}
  value={size} onChange={({ value }) => setSize(value)} />
<Slider labelText="Autoscaling range (nodes)" twoHandles min={0} max={50}
  defaultValue={3} defaultValueUpper={24}
  ariaLabelInput="Minimum nodes" ariaLabelInputUpper="Maximum nodes" />
```

**Rules**
- Track is 2px `border-subtle-01`; filled part and thumb are `layer-selected-inverse`. On focus the thumb becomes `interactive` with a gap ring and the fill `border-interactive`; hover and focus scale the round thumb.
- Range handles are the bracketed arrow glyphs; focus draws the framed variant in `interactive`. The lower box sits left of the min label, the upper box right of the max label.
- The track is 200px to 640px wide; range labels are `body-compact-01` in `text-primary`; the number box is 64px wide (wider with the invalid or warning icon).
- Invalid outlines the box in `support-error` with a `text-error` message; warning shows the `support-warning` icon and a `text-primary` message. Disabled uses `border-disabled` and `text-disabled`.
- Copy: a short label naming the value and unit ("Size (GB)"); describe ranges in numbers; avoid large, uneven steps.

**Accessibility**
- Each thumb is `role="slider"` with `aria-valuemin` / `aria-valuemax` / `aria-valuenow` / `aria-valuetext`; a single slider is named by its label, range handles by `ariaLabelInput` / `ariaLabelInputUpper`.
- Keyboard: Tab reaches the thumb(s) and the number box(es); arrows move by `step`, Shift+arrow, PageUp and PageDown by `step x stepMultiplier`, Home / End jump to the ends. Clicking the track moves the nearest handle.
- The thumb and the box stay in sync; the invalid message uses `role="alert"`. Disabled thumbs leave the tab order; a read-only slider hides its thumb (not focusable) and keeps its box focusable and read only.
