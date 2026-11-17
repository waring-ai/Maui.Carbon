# FluidTimePicker

A fluid time field: a FluidTextInput for the time plus one or two `FluidTimePickerSelect` parts (AM/PM, time zone) joined in one 64px row with 1px dividers between the parts.

Hand-written from `packages/styles/scss/components/fluid-time-picker` and `packages/react/src/components/FluidTimePicker` / `FluidTimePickerSelect` / `FluidTimePickerSkeleton` (Carbon v11.99.0). `FluidTimePickerSelect` is the sub-part (it is a FluidSelect); `FluidTimePickerSkeleton` is the loading placeholder. Left out: toggletip labels, time parsing or validation (the field is free text; validate in `onChange` and set `invalid`).

**When to use**
- Scheduling a specific time (backup windows, rollouts, maintenance) inside a FluidForm, usually next to a FluidDatePicker.

**When not to use**
- Stacked or complex forms: use TimePicker. A time of day picked from a few fixed slots: FluidSelect. Durations: FluidNumberInput with the unit in the label.

**You provide:**
- For the time input: `labelText`, `placeholder` (Carbon uses "hh:mm"), `id`, `name`, `pattern`, `required`, `autoComplete`, and a value: `value` + `onChange(event)` or `defaultValue`. `onFocus`, `onBlur`, `onClick`, `onKeyDown` go to the input too.
- `FluidTimePickerSelect` children (one or two), each with its own `labelText`, `SelectItem` options and the FluidSelect props (`value`/`defaultValue`, `onChange`, `id`, `disabled`, `readOnly`). With two selects the input and the first select take 25% each and the last select takes 50%; with one select the parts split 50/50.
- `invalid` + `invalidText`, `warn` + `warnText` for the whole picker (ignored when disabled or read-only), `disabled` (disables every part), `readOnly` (applies to every part), `className`.
- `FluidTimePickerSkeleton`: `isOnlyTwo` (two parts 50/50 instead of three), `className`.

**Example**
```jsx
const { FluidTimePicker, FluidTimePickerSelect, SelectItem } = window.Carbon;
<FluidTimePicker id="backup-time" labelText="Start time" placeholder="hh:mm" value={time}
  onChange={e => setTime(e.target.value)} invalid={!valid} invalidText="Enter a time from 1:00 to 12:59, for example 2:30">
  <FluidTimePickerSelect id="backup-ampm" labelText="AM/PM" defaultValue="am">
    <SelectItem value="am" text="AM" /><SelectItem value="pm" text="PM" />
  </FluidTimePickerSelect>
  <FluidTimePickerSelect id="backup-tz" labelText="Time zone" defaultValue="ct">
    <SelectItem value="ct" text="Central Time (CT)" /><SelectItem value="utc" text="Coordinated Universal Time (UTC)" />
  </FluidTimePickerSelect>
</FluidTimePicker>
```

**Rules**
- One `field-01` row; each part keeps its 1px `border-strong-01` bottom rule and the selects draw a full-height 1px `border-strong-01` divider on their left edge (hidden on hover, `border-subtle-01` when read-only, `border-disabled` when disabled).
- Invalid and warning apply to the whole picker, never to a single part: a 2px `support-error` outline around the row (invalid only), dividers shortened to leave 8px at top and bottom, a `border-subtle-01` divider and the message row below (`text-error` or `text-primary`) with the icon 72px from the top / 16px from the right.
- Keep the parts in the order time, AM/PM, time zone, and label each part. Use 12-hour time with an "AM/PM" select (uppercase, no periods) or 24-hour time without it; include a time zone whenever the time is specific.
- Error text gives the expected format with an example ("Enter a time in hh:mm format, for example 09:30").
- Skeleton: three (or two, `isOnlyTwo`) 64px `skeleton-background` parts split like the real picker.

**Accessibility**
- Each part is its own labelled control (an `<input>` and native `<select>`s); Tab moves through them in order.
- The picker-level message is plain text below the row and is not linked to a part with `aria-describedby` (as in Carbon); repeat the key point in the time input's label or validate on blur so the user notices it.
- Disabled removes every part from the tab order; read only keeps them focusable.
