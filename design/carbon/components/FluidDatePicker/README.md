# FluidDatePicker

Date picker for fluid forms, composed like Carbon's: a `FluidDatePicker` holding one `FluidDatePickerInput` (single or simple) or two (range), each a 64px field with its label inside and a calendar that opens under the first input.

Hand-written from `packages/styles/scss/components/fluid-date-picker` and `date-picker` (Carbon v11.99.0); markup from `FluidDatePicker.tsx`, `FluidDatePickerInput.tsx`, `DatePicker.tsx` and `DatePickerInput.tsx` under fluid context. The calendar is a built-in replacement for flatpickr that reuses the DatePicker calendar styles. `FluidDatePickerSkeleton` is the loading placeholder. Left out: AI-label/decorator slot, toggletip labels, locales, `enable`/`disable` date lists, `appendTo`, `closeOnSelect`, `allowInput`, `short`.

**When to use**
- `single`: scheduling one date (maintenance, rollout) where seeing the calendar helps.
- `range`: a start and end date (report periods, maintenance windows).
- `simple`: dates users know without a calendar (key expiry they were given, a birth date).
- Inside a FluidForm, next to other fluid fields; pair with FluidTimePicker for a date and time.

**When not to use**
- Regular stacked forms: use DatePicker. A time of day: FluidTimePicker. Relative periods ("Last 7 days"): FluidDropdown.

**You provide:**
- `datePickerType`: `'single'` (default), `'range'` or `'simple'`.
- Children: `FluidDatePickerInput` elements (one, or two for range) with `labelText`, `placeholder` and `id`. Without children, inputs are built from `labelText` / `endLabelText` / `placeholder` / `endPlaceholder`.
- Dates: controlled `value` + `onChange(dates)` or uncontrolled `defaultValue` (Date, string in `dateFormat`, ISO string, timestamp, or an array for range).
- `dateFormat` (default `m/d/Y`; tokens m n d j Y y), `minDate`, `maxDate`, `open` / `defaultOpen` / `onOpenChange`, `id`, `style` (the picker is `inline-flex`; give it a width), `className`.
- Picker-level states: `invalid` + `invalidText`, `warn` + `warnText` (ignored when disabled or read-only), `readOnly`, `disabled`.
- **FluidDatePickerInput**: `labelText`, `placeholder`, `id`, `pattern`, `disabled`, `readOnly`, input-level `invalid` + `invalidText` / `warn` + `warnText` (one end of a range; ignored when that input is disabled or read-only), `style` (on the `<input>`), `onChange` / `onClick` / `onFocus` / `onBlur` / `onKeyDown`, `className`.
- **FluidDatePickerSkeleton**: `datePickerType` (`range` draws two fields), `className`.

**Example**
```jsx
const { FluidDatePicker, FluidDatePickerInput } = window.Carbon;
<FluidDatePicker datePickerType="range" value={period} onChange={setPeriod} maxDate={today} style={{ width: '100%' }}
  invalid={days > 92} invalidText="Choose a period of 92 days or less">
  <FluidDatePickerInput id="from" labelText="From" placeholder="mm/dd/yyyy" />
  <FluidDatePickerInput id="to" labelText="To" placeholder="mm/dd/yyyy" />
</FluidDatePicker>
```

**Rules**
- Inputs are transparent on the picker's `field-01` background, at least 64px tall and 144px wide, padding 32px 16px 13px, `code-02` mono text; label is `label-01` in `text-secondary` 13px from the top. Calendar icon (16px, `icon-primary`) is centred 43px from the top, 16px from the right; `simple` has no icon.
- Bottom rule: 1px `border-strong-01` (single/simple on the input, range on each half); range halves are split by a 1px `border-strong-01` line, and read-only switches both to `border-subtle-01`.
- Picker-level invalid/warning: a 2px `support-error` outline (invalid) around the whole picker, the bottom rules go transparent, the range split becomes an inset line (8px from top and bottom), then a 1px `border-subtle-01` divider inset 16px and one message row (`padding: 8px 40px 8px 16px`) with the icon centred 80px from the top. Message is `text-error` (invalid) or `text-primary` (warning).
- Input-level invalid outlines only that input and its own message; prefer it when only one end of a range is wrong, so users know which field to fix.
- Calendar: `layer-01`, 288px wide, 40px day cells; selected day `button-primary`, range days `highlight`, today `link-primary` with a dot; rendered inline, not portalled.
- Content: every input needs a label (range: "Start date" / "End date" or "From" / "To"). The format must not live only in the placeholder, because it disappears on typing: put it in the label ("Expiry date (mm/dd/yyyy)") or the error text. Localize the format where you can.
- Skeleton: one 64px `skeleton-background` field, two side by side for `range`.

**Accessibility**
- Each input is a text `<input>` with its `<label>`; invalid sets `aria-invalid` and `aria-describedby` points at the input-level or picker-level message.
- Clicking an input opens the calendar (`role="dialog"`, "Choose date"); ArrowDown in an input opens it and moves focus to the active day. Day buttons have full-date `aria-label`s and `aria-pressed` when selected.
- In the calendar: arrows move by day/week, PageUp/PageDown by month, Home/End to the month edges, Enter/Space picks, Escape closes and returns focus to the input; picking a single date or the end of a range also closes it, as does a click outside.
- Typed dates commit on Enter or blur; text that does not parse (or is outside `minDate`/`maxDate`) reverts. Read only stays focusable but never opens the calendar; disabled leaves the tab order.
