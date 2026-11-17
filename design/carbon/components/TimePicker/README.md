# TimePicker

A text input for a time (`hh:mm`) with optional selects beside it for AM/PM and a time zone.

Hand-written from `packages/styles/scss/components/time-picker` and `packages/react/src/components/TimePicker` (Carbon v11.99.0). `TimePickerSelect` is exported alongside. Left out: fluid variant (see FluidTimePicker), the `light` prop and a skeleton. It does not open a clock popup and does not parse or validate the value: pass `invalid` yourself.

**When to use**
- A specific time of day: a maintenance window, a backup or report time, a meeting.
- Pair it with a DatePicker when the date matters too.

**When not to use**
- Durations or intervals (use a NumberInput with a unit) and relative times ("in 2 hours").
- Picking from a few fixed slots: use a Select or RadioButtonGroup.

**You provide:**
- `id` (required) and `labelText` (`hideLabel` keeps it for screen readers only).
- `value` + `onChange(event)` or `defaultValue`.
- `TimePickerSelect` children (each a `Select` with a visually hidden label, so give it `id` and a meaningful `labelText` such as "AM or PM" or "Time zone") containing `SelectItem`s. The picker passes `disabled` and `readOnly` down to them.
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px).
- `placeholder` (default `hh:mm`), `pattern` (default 12-hour `(1[012]|[1-9]):[0-5][0-9](\s)?`; pass a 24-hour pattern when you drop the AM/PM select), `maxLength` (default 5), `type` (default `text`).
- States: `invalid` + `invalidText` (default "Invalid time format."), `warning` + `warningText` (note the names), `disabled`, `readOnly`. `invalid` and `warning` are ignored while disabled or read only.
- Also passed through: `name`, `onBlur`, `onClick`, `onFocus`, `inputClassName`, `pickerClassName`, `className`.

**Example**
```jsx
const { TimePicker, TimePickerSelect, SelectItem } = window.Carbon;
<TimePicker id="mw-start" labelText="Start time" value={time}
  onChange={(e) => setTime(e.target.value)}
  invalid={!valid} invalidText="Use the format hh:mm, for example 2:30">
  <TimePickerSelect id="mw-ampm" labelText="AM or PM">
    <SelectItem value="AM" text="AM" /><SelectItem value="PM" text="PM" />
  </TimePickerSelect>
  <TimePickerSelect id="mw-tz" labelText="Time zone">
    <SelectItem value="UTC" text="UTC" /><SelectItem value="CET" text="CET" />
  </TimePickerSelect>
</TimePicker>
```

**Rules**
- The time field is 4.875rem wide (6.175rem when an error or warning icon is shown), set in the mono font (`code-02`) on `field-01` with a 1px `border-strong-01` underline.
- The input and each select share one height and sit 1px apart; selects reserve 3rem for the chevron.
- Invalid draws a 2px `support-error` inset outline on the input and an icon inside it; warning shows the `support-warning` icon. The message appears below the whole group (`text-error` or `text-primary`).
- Read only drops the fills and uses `border-subtle-01`; disabled uses `text-disabled`.
- Copy: 12-hour times need an AM/PM select with uppercase "AM" and "PM" (no periods); a specific moment needs a time zone select. 24-hour times need neither.

**Accessibility**
- The visible label is linked to the input; each select has its own visually hidden label, so all three controls are named and reachable with Tab.
- The error or warning message is linked with `aria-describedby`; invalid sets `aria-invalid`. The default `pattern` lets the browser flag malformed input on form submit.
