# DatePicker

A date field with a calendar popup: `single` picks one date, `range` picks a start and end date (two inputs, one calendar), and `simple` is a typed date field with no calendar.

Hand-written from `packages/styles/scss/components/date-picker` including its flatpickr calendar styles (Carbon v11.99.0); the calendar is implemented in React rather than with flatpickr. `DatePickerInput` (the bare field) and `DatePickerSkeleton` are exported alongside. Left out: month / year dropdown selectors, week numbers, time input (use TimePicker), multiple-date selection, locales (English names, Sunday start), `appendTo`, composing `DatePicker` from `DatePickerInput` children (it renders its own inputs), fluid (see FluidDatePicker) and AI-label decorators, `light`.

**When to use**
- `single`: one date where its relation to other days matters, such as scheduling a snapshot or a go-live.
- `range`: a start and end date, such as a usage export or a report period.
- `simple`: dates people know by heart or approximate past dates (a contract start, a birth date).

**When not to use**
- Dates far in the past or future: the calendar is slow to reach them; use `simple`.
- A time of day: use a TimePicker (pair the two when both matter).
- Relative periods ("last 7 days"): use a Dropdown or Select of presets.

**You provide:**
- `datePickerType` `single` (default) | `range` | `simple`; `labelText` (and `endLabelText`, default "End date", for range); `id`.
- `value` + `onChange(dates)` (controlled) or `defaultValue`. Values are `Date`s, strings in `dateFormat`, or ISO `yyyy-mm-dd` strings (an array of two for range); `onChange` receives an array of `Date`.
- `dateFormat` with `m`/`n`, `d`/`j`, `Y`/`y` tokens (default `m/d/Y`); `placeholder` (default derived from the format, such as `mm/dd/yyyy`) and `endPlaceholder`; `pattern`.
- `minDate` / `maxDate` (days outside are disabled and typed dates outside are rejected).
- `size` `sm` | `md` | `lg` (default md, 32 / 40 / 48px); `short` (narrower simple field); `hideLabel`.
- States: `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`. For range, both inputs are outlined and one message spans them. `invalid` and `warn` are ignored while disabled or read only.
- `open` / `defaultOpen` / `onOpenChange` (previews use `defaultOpen`); `className`.
- `DatePickerInput` (for your own calendar or layout): `labelText`, `id`, `placeholder`, `value` / `defaultValue`, `datePickerType`, `size`, `helperText`, `invalid` + `invalidText`, `warn` + `warnText`, `disabled`, `readOnly`, `pattern`, `hideLabel`, event handlers, `inputRef`, `containerClassName`, `className`, `children` (rendered inside the field wrapper). Wrap it in an element with `cds--date-picker cds--date-picker--single` for the standard 288px width.
- `DatePickerSkeleton`: `range`, `hideLabel`, `className`.

**Example**
```jsx
const { DatePicker } = window.Carbon;
<DatePicker id="usage-range" datePickerType="range" labelText="From" endLabelText="To"
  value={range} onChange={setRange} maxDate={new Date()}
  invalid={days > 92} invalidText="Choose a range of 92 days or less" />
```

**Rules**
- The input uses `field-01` (`field-02` inside a Layer) with a 1px `border-strong-01` underline and `code-02` mono text; width is 288px for single, 120px for simple (152px with an icon), 143.5px per field for range. The calendar icon (`icon-primary`) sits 16px from the right and is replaced by the error or warning icon.
- The calendar is 288 x 336px on `layer-01` with a shadow, directly under the first field; it does not grow with the field size. Previous / next month buttons, month name and a year input, a weekday row, then six weeks of 40px day cells.
- Day states: hover `layer-hover-01`; today in `link-primary`, semibold, with a 4px dot; selected `button-primary` with `text-on-color`; neighbouring-month days `text-helper`; disabled `text-disabled`; days inside a range `highlight`.
- Invalid: 2px `support-error` outline and `text-error` message; warning: `support-warning` icon and `text-primary` message. Disabled uses `text-disabled`; read only drops the fill and does not open the calendar.
- Copy: label each field for what it is ("Start date", "Snapshot date"); show the format in the placeholder and, for `simple`, in the label or helper text too; default the calendar near the likely date.

**Accessibility**
- Each input has a `<label>`; text entry always works, so the calendar is never the only way in. `aria-describedby` points at the helper text or the message; invalid sets `aria-invalid`.
- The calendar is `role="dialog"` ("Choose date"); each day is a `<button>` with a full-date `aria-label` and `aria-pressed` when selected; month navigation buttons are named.
- Keyboard: type a date and press Enter or leave the field to apply it (unparseable text reverts); ArrowDown moves into the calendar; arrows move by day and week, PageUp / PageDown by month, Home / End to the first and last day; Enter or Space picks; Escape closes and returns to the input. Single closes after a pick; range after the second pick.
