// Types for window.Carbon (documentation; not type-checked).
// Unlisted props (aria-*, data-*, id, name, onBlur, ...) are passed through to the native element.
export type ButtonKind = 'primary' | 'secondary' | 'tertiary' | 'ghost' | 'danger' | 'danger--primary' | 'danger--tertiary' | 'danger--ghost';
export type ControlSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';

export interface ButtonProps {
  /** Visual emphasis. Default 'primary'. 'danger--primary' is an alias of 'danger'. */
  kind?: ButtonKind;
  /** Height: xs 24, sm 32, md 40, lg 48 (default), xl 64, 2xl 80. */
  size?: ControlSize;
  /** Expressive style: 16px (body-compact-02) label and 20px icon; use with size 'lg' for the "large expressive" button. */
  isExpressive?: boolean;
  /** Ghost icon-only buttons only: selected (background-selected fill, aria-pressed=true). Passing false sets aria-pressed=false. */
  isSelected?: boolean;
  disabled?: boolean;
  /** Icon file stem from the Icons group, e.g. 'add'. 16px (20px when isExpressive). */
  renderIcon?: string;
  /** Square icon-only button; requires iconDescription. */
  hasIconOnly?: boolean;
  /** Accessible name (and title tooltip) for icon-only buttons. */
  iconDescription?: string;
  /** Renders an anchor (<a href>) styled as a button; ignored while disabled (renders a disabled <button>). */
  href?: string;
  /** Native button type. Default 'button'. */
  type?: 'button' | 'submit' | 'reset';
  onClick?: (e: unknown) => void;
  className?: string;
  children?: unknown;
}

export type TagType = 'red' | 'magenta' | 'purple' | 'blue' | 'cyan' | 'teal' | 'green' | 'gray' | 'cool-gray' | 'warm-gray' | 'high-contrast' | 'outline';
export interface TagProps {
  /** Default 'gray'. */
  type?: TagType;
  /** sm 18px, md 24px (default), lg 32px. */
  size?: 'sm' | 'md' | 'lg';
  /** Decorative 16px icon before the label (icon file stem); not drawn at size 'sm'. */
  renderIcon?: string;
  /** Adds a close button (prefer DismissibleTag). */
  filter?: boolean;
  onClose?: (e: unknown) => void;
  /** Accessible name of the close button when `filter`. Default 'Clear filter'. */
  title?: string;
  /** Makes the tag an operational button (prefer OperationalTag). */
  onClick?: (e: unknown) => void;
  disabled?: boolean;
  className?: string;
  children?: unknown;
}

export interface TextInputProps {
  /** Visible label (always provide one; use hideLabel to hide it visually). */
  labelText?: unknown;
  /** Visually hide the label; it stays the accessible name. */
  hideLabel?: boolean;
  /** Shown below the field (beside the label when inline); replaced by the invalid / warn message. */
  helperText?: unknown;
  /** Error state: 2px support-error outline, warning--filled icon, invalidText below. Ignored when disabled or readOnly. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Warning state: support-warning triangle icon and warnText in text-primary. Ignored when invalid, disabled or readOnly. */
  warn?: boolean;
  warnText?: unknown;
  placeholder?: string;
  /** xs 24px, sm 32px, md 40px (default), lg 48px. */
  size?: 'xs' | 'sm' | 'md' | 'lg';
  /** Label (and helper text) to the start of the field instead of above. */
  inline?: boolean;
  /** Shows "count/maxCount" at the end of the label row and caps input at maxCount characters. Not shown when inline. */
  enableCounter?: boolean;
  maxCount?: number;
  /** Native input type. Default 'text'. */
  type?: string;
  id?: string;
  value?: string;
  defaultValue?: string;
  disabled?: boolean;
  /** Read-only: transparent fill, border-subtle-01 rule; focusable and announced, not editable. */
  readOnly?: boolean;
  onChange?: (e: unknown) => void;
  className?: string;
}

export interface CheckboxProps {
  labelText: unknown;
  /** Visually hide the label; it stays the accessible name. */
  hideLabel?: boolean;
  id?: string;
  /** Controlled checked state (pair with onChange); omit and use defaultChecked for uncontrolled. */
  checked?: boolean;
  defaultChecked?: boolean;
  /** Mixed state for a "select all" parent (aria-checked="mixed"). */
  indeterminate?: boolean;
  disabled?: boolean;
  /** Focusable and announced but cannot be toggled; outlined box with an icon-primary mark. */
  readOnly?: boolean;
  /** Below the label; replaced by the invalid / warn message. */
  helperText?: unknown;
  invalid?: boolean;
  invalidText?: unknown;
  warn?: boolean;
  warnText?: unknown;
  /** Called with the native event and { checked, id }. Not called while readOnly. */
  onChange?: (e: unknown, data: { checked: boolean; id: string }) => void;
  title?: string;
  className?: string;
}

export interface ToggleProps {
  /** The setting's name, above the switch. */
  labelText?: unknown;
  /** State text when off (default 'Off'). */
  labelA?: string;
  /** State text when on (default 'On'). */
  labelB?: string;
  /** Visually hides labelText above the switch and shows it beside the switch instead of the state text. */
  hideLabel?: boolean;
  /** Controlled state (pair with onToggle); omit and use defaultToggled for uncontrolled. */
  toggled?: boolean;
  defaultToggled?: boolean;
  onToggle?: (next: boolean) => void;
  onClick?: (e: unknown) => void;
  /** md: 48x24 (default); sm: 32x16 with a check mark when on. */
  size?: 'md' | 'sm';
  disabled?: boolean;
  /** Focusable and announced (aria-readonly) but does not switch; outlined track. */
  readOnly?: boolean;
  id?: string;
  className?: string;
}

export interface InlineNotificationProps {
  /** Default 'info'. */
  kind?: 'error' | 'success' | 'warning' | 'warning-alt' | 'info' | 'info-square';
  title?: unknown;
  /** Text or any node (e.g. text with a Link). */
  subtitle?: unknown;
  /** Extra content after the subtitle. */
  children?: unknown;
  /** Low-contrast variant uses the notification-background-* tints. */
  lowContrast?: boolean;
  hideCloseButton?: boolean;
  /** Called when the close button (or Escape inside the notification) closes it; return false to keep it open. */
  onClose?: (e: unknown) => boolean | void;
  /** Called after every close-button click. */
  onCloseButtonClick?: (e: unknown) => void;
  /** Close button accessible name. Default 'Close notification'. */
  ariaLabel?: string;
  /** Accessible name of the status icon. Default '<kind> icon'. */
  statusIconDescription?: string;
  /** Action node, e.g. a ghost Button (prefer ActionableNotification). */
  actions?: unknown;
  /** Default 'status'; use 'alert' only for urgent, time-sensitive errors. */
  role?: 'status' | 'alert' | 'log';
  className?: string;
}

export interface TileProps {
  /** Makes it a clickable link tile with a trailing arrow. Prefer ClickableTile. */
  href?: string;
  /** Makes it a clickable button tile. Prefer ClickableTile. */
  onClick?: (e: unknown) => void;
  disabled?: boolean;
  className?: string;
  children?: unknown;
}

export interface ClickableTileProps {
  /** Renders an <a href>; without it the tile is a <button> calling onClick. */
  href?: string;
  target?: string;
  rel?: string;
  onClick?: (e: unknown) => void;
  /** Icon file stem drawn bottom-right at 20px in icon-interactive. Default 'arrow--right'; 'launch' for external; null for none. */
  renderIcon?: string | null;
  /** Non-interactive (aria-disabled), text-disabled. */
  disabled?: boolean;
  className?: string;
  children?: unknown;
}

export interface LinkProps {
  href?: string;
  target?: string;
  /** Default 'noopener noreferrer' when target is '_blank'. */
  rel?: string;
  /** sm 12/16, md 14/18 (default), lg 16/22. */
  size?: 'sm' | 'md' | 'lg';
  /** Underlined, display:inline, for running text. Never shows renderIcon. */
  inline?: boolean;
  /** Apply link-visited once the destination has been visited (off by default). */
  visited?: boolean;
  /** Renders <a role="link" aria-disabled="true"> without href. */
  disabled?: boolean;
  /** Icon file stem after the text, e.g. 'launch' (external) or 'arrow--right'. */
  renderIcon?: string;
  onClick?: (e: unknown) => void;
  className?: string;
  children?: unknown;
}

export interface TabItem {
  /** Tab text; for icon-only tabs it becomes the aria-label and title. */
  label: string;
  content?: unknown;
  disabled?: boolean;
  /** Icon file stem: pinned after the label, or the glyph itself when Tabs iconOnly. */
  renderIcon?: string;
  /** Second line under the label; makes contained tabs tall (64px). */
  secondaryLabel?: string;
}
export interface TabsProps {
  items: TabItem[];
  /** Contained (48px blocks) instead of line tabs. */
  contained?: boolean;
  /** Contained only: tabs share the width equally (auto width below 42rem). */
  fullWidth?: boolean;
  /** Selected tab. Controlled when onChange is also passed; otherwise the initial tab. */
  selectedIndex?: number;
  defaultSelectedIndex?: number;
  onChange?: (data: { selectedIndex: number }) => void;
  /** 'automatic' (default): arrows select. 'manual': arrows move focus, Enter/Space select. */
  activation?: 'automatic' | 'manual';
  /** Icon-only tabs (every item needs renderIcon); iconSize 'lg' makes them 48px. */
  iconOnly?: boolean;
  iconSize?: 'default' | 'lg';
  /** Close icon on each tab; Delete on a focused tab also requests closing. You remove the item. */
  dismissable?: boolean;
  onTabCloseRequest?: (index: number) => void;
  ariaLabel?: string;
  className?: string;
}

export interface IconProps {
  /** File stem from the Icons group, e.g. 'settings', 'error--filled'. */
  name: string;
  /** Pixel size; 16 (default), 20, 24, 32. The closest source glyph is scaled. */
  size?: number;
  /** Accessible name; omit for decorative icons (they get aria-hidden). */
  title?: string;
  className?: string;
  style?: unknown;
}

// ---- A1
export interface TextAreaProps {
  /** Label above the field. Hide visually with `hideLabel`. */
  labelText?: unknown;
  hideLabel?: boolean;
  id?: string;
  helperText?: unknown;
  placeholder?: string;
  /** Visible rows (default 4). */
  rows?: number;
  /** Fixed column width; the field then stops stretching and its resize handle is disabled. */
  cols?: number;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  disabled?: boolean;
  readOnly?: boolean;
  /** Ignored while `disabled` or `readOnly`. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Ignored while `invalid`, `disabled` or `readOnly`. */
  warn?: boolean;
  warnText?: unknown;
  /** Show an `n/maxCount` counter above the field (requires `maxCount`). Input past the limit is not accepted (native `maxLength` in character mode; ignored keystrokes in word mode). */
  enableCounter?: boolean;
  maxCount?: number;
  counterMode?: 'character' | 'word';
  name?: string;
  required?: boolean;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  onClick?: (event: unknown) => void;
  className?: string;
}

export interface NumberInputProps {
  id?: string;
  /** Label above the field. */
  label?: unknown;
  /** Alias of `label`. */
  labelText?: unknown;
  /** Accessible name of the + stepper (default "Increment number"). */
  incrementNumLabel?: string;
  /** Accessible name of the - stepper (default "Decrement number"). */
  decrementNumLabel?: string;
  hideLabel?: boolean;
  helperText?: unknown;
  /** Controlled value. */
  value?: number | string;
  /** Initial value when uncontrolled (default 0, or '' with `allowEmpty`). */
  defaultValue?: number | string;
  min?: number;
  max?: number;
  /** Increment of the steppers and the native input (default 1). */
  step?: number;
  size?: 'sm' | 'md' | 'lg';
  /** Called on typing and on stepper clicks with `{ value, direction }`. */
  onChange?: (event: unknown, state: { value: number | string; direction: 'up' | 'down' }) => void;
  onClick?: (event: unknown, state?: { value: number | string; direction: 'up' | 'down' }) => void;
  name?: string;
  placeholder?: string;
  onBlur?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  onKeyUp?: (event: unknown) => void;
  /** Treat an empty field as valid. */
  allowEmpty?: boolean;
  hideSteppers?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  /** Forced invalid; a value outside min/max or an empty field is also invalid. Suppressed while `disabled` or `readOnly`. */
  invalid?: boolean;
  invalidText?: unknown;
  warn?: boolean;
  warnText?: unknown;
  className?: string;
}

export interface PasswordInputProps {
  id?: string;
  labelText?: unknown;
  hideLabel?: boolean;
  helperText?: unknown;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  /** Initial input type; `'text'` starts revealed. */
  type?: 'password' | 'text';
  /** Tooltip / accessible name of the toggle while the password is visible (default "Hide password"). */
  hidePasswordLabel?: string;
  /** Tooltip / accessible name of the toggle while hidden (default "Show password"). */
  showPasswordLabel?: string;
  onTogglePasswordVisibility?: (event: unknown) => void;
  name?: string;
  autoComplete?: string;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  readOnly?: boolean;
  invalid?: boolean;
  invalidText?: unknown;
  warn?: boolean;
  warnText?: unknown;
  className?: string;
}

export interface SearchProps {
  /** Accessible label, visually hidden. */
  labelText: unknown;
  id?: string;
  placeholder?: string;
  size?: 'sm' | 'md' | 'lg';
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  /** Called after the clear button or Escape empties the field. */
  onClear?: () => void;
  closeButtonLabelText?: string;
  disabled?: boolean;
  /** Collapse to a magnifier button until clicked or focused (same as `ExpandableSearch`). */
  expandable?: boolean;
  /** Controlled expanded state for `expandable`. */
  isExpanded?: boolean;
  defaultExpanded?: boolean;
  onExpand?: (event: unknown) => void;
  /** Icon name for the magnifier (default `search`). */
  renderIcon?: string;
  name?: string;
  /** Input type (default `search`). */
  type?: string;
  /** Default `off`. */
  autoComplete?: string;
  onKeyDown?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  className?: string;
}
/** Search that starts collapsed to a magnifier button. Same props as `Search`. */
export type ExpandableSearchProps = SearchProps;

export interface SelectProps {
  /** Required; links the label to the native `<select>`. */
  id: string;
  labelText?: unknown;
  hideLabel?: boolean;
  /** Do not render a label at all (supply your own accessible name). */
  noLabel?: boolean;
  helperText?: unknown;
  /** `SelectItem` / `SelectItemGroup` children (or raw `<option>`). */
  children?: unknown;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  size?: 'sm' | 'md' | 'lg';
  name?: string;
  required?: boolean;
  autoComplete?: string;
  title?: string;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onMouseDown?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  /** Label and select on one line, no field background. */
  inline?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  invalid?: boolean;
  invalidText?: unknown;
  warn?: boolean;
  warnText?: unknown;
  className?: string;
}
export interface SelectItemProps { value: string; text: string; disabled?: boolean; hidden?: boolean; }
export interface SelectItemGroupProps { label: string; disabled?: boolean; children?: unknown; }

export interface RadioButtonProps {
  /** Label shown beside the radio. */
  labelText: unknown;
  value: string | number;
  id?: string;
  name?: string;
  /** Controlled checked state (set by `RadioButtonGroup`). */
  checked?: boolean;
  defaultChecked?: boolean;
  /** Called with `(value, name, event)`. */
  onChange?: (value: string | number, name: string | undefined, event: unknown) => void;
  labelPosition?: 'left' | 'right';
  hideLabel?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  required?: boolean;
  className?: string;
}

export interface RadioButtonGroupProps {
  /** Shared `name` of the radios (required). */
  name: string;
  /** `RadioButton` children. */
  children?: unknown;
  /** Legend rendered as the group label. */
  legendText?: unknown;
  helperText?: unknown;
  /** Controlled selected value. */
  valueSelected?: string | number;
  defaultSelected?: string | number;
  /** Called with `(selectedValue, name, event)`. */
  onChange?: (selection: string | number, name: string, event: unknown) => void;
  orientation?: 'horizontal' | 'vertical';
  /** Which side of each radio the label sits on. */
  labelPosition?: 'left' | 'right';
  disabled?: boolean;
  readOnly?: boolean;
  required?: boolean;
  invalid?: boolean;
  invalidText?: unknown;
  warn?: boolean;
  warnText?: unknown;
  className?: string;
}

export interface TimePickerProps {
  /** Required; links the label to the input. */
  id: string;
  labelText?: unknown;
  hideLabel?: boolean;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  placeholder?: string;
  /** Input pattern, default 12-hour `(1[012]|[1-9]):[0-5][0-9](\s)?`. */
  pattern?: string;
  maxLength?: number;
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  readOnly?: boolean;
  invalid?: boolean;
  invalidText?: unknown;
  warning?: boolean;
  warningText?: unknown;
  name?: string;
  onBlur?: (event: unknown) => void;
  onClick?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  /** Input type (default `text`). */
  type?: string;
  /** Class on the `<input>`. */
  inputClassName?: string;
  /** Class on the `cds--time-picker` row. */
  pickerClassName?: string;
  /** `TimePickerSelect` children (AM/PM, time zone); they inherit `disabled` and `readOnly`. */
  children?: unknown;
  className?: string;
}
/** A Select sized to sit beside the time input. Takes `SelectProps` (label is visually hidden). */
export type TimePickerSelectProps = SelectProps;

export interface SliderProps {
  id?: string;
  labelText: unknown;
  hideLabel?: boolean;
  min?: number;
  max?: number;
  step?: number;
  /** Multiplier applied to `step` with Shift+arrow, PageUp and PageDown (default 4). */
  stepMultiplier?: number;
  /** Controlled value (lower handle when `twoHandles`). */
  value?: number;
  defaultValue?: number;
  valueUpper?: number;
  defaultValueUpper?: number;
  /** Render a range slider with two handles and two inputs. */
  twoHandles?: boolean;
  /** Called with `{ value }` or, for two handles, `{ value, valueUpper }`. */
  onChange?: (state: { value: number; valueUpper?: number }) => void;
  minLabel?: string;
  maxLabel?: string;
  /** Format the min/max labels and `aria-valuetext`. */
  formatLabel?: (value: number, label: string | undefined) => string;
  hideTextInput?: boolean;
  ariaLabelInput?: string;
  ariaLabelInputUpper?: string;
  /** Form name of the (lower) number input. */
  name?: string;
  /** Form name of the upper number input (two handles). */
  nameUpper?: string;
  disabled?: boolean;
  readOnly?: boolean;
  /** Ignored while `disabled` or `readOnly`; typing a value outside min/max into the input also shows it. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Ignored while `invalid`, `disabled` or `readOnly`. */
  warn?: boolean;
  warnText?: unknown;
  className?: string;
}

/** Loading placeholders shown while the real field's data loads (Carbon's `*Skeleton` components). */
export interface FieldSkeletonProps {
  /** Omit the label bar. */
  hideLabel?: boolean;
  className?: string;
}
export type TextAreaSkeletonProps = FieldSkeletonProps;
export type NumberInputSkeletonProps = FieldSkeletonProps;
export type SelectSkeletonProps = FieldSkeletonProps;
export interface SearchSkeletonProps {
  /** Field height (default md). */
  size?: 'sm' | 'md' | 'lg';
  /** Legacy shorthand for `size="sm"`. */
  small?: boolean;
  className?: string;
}
export interface RadioButtonSkeletonProps { className?: string; }
export interface SliderSkeletonProps extends FieldSkeletonProps {
  /** Placeholder for a range slider. */
  twoHandles?: boolean;
}

// ---- A2
export type ListBoxSize = 'sm' | 'md' | 'lg';

interface ListBoxCommonProps<ItemType> {
  /** Items to choose from: strings, or objects with `label`/`text`/`name`. An object with `disabled: true` cannot be chosen. */
  items: ItemType[];
  /** Maps an item to its text. Defaults to the item itself, or its `label`, `text` or `name`. */
  itemToString?: (item: ItemType | null) => string;
  /** Visible label above the field. */
  titleText?: React.ReactNode;
  /** Visually hide `titleText` but keep it for screen readers. */
  hideLabel?: boolean;
  /** Helper text shown under the field when not invalid or warning. */
  helperText?: React.ReactNode;
  /** Error outline, icon and `invalidText`. Ignored while `disabled` or `readOnly` (as Carbon's normalized input props). */
  invalid?: boolean;
  invalidText?: React.ReactNode;
  /** Warning icon and `warnText`. Ignored while `invalid`, `disabled` or `readOnly`. */
  warn?: boolean;
  warnText?: React.ReactNode;
  disabled?: boolean;
  readOnly?: boolean;
  /** Field height: sm 32px, md 40px, lg 48px. Default md. */
  size?: ListBoxSize;
  /** `inline` removes the fill and puts the label beside the field. */
  type?: 'default' | 'inline';
  /** Open the list upwards. */
  direction?: 'bottom' | 'top';
  /** Controlled open state. */
  open?: boolean;
  /** Initial open state when uncontrolled (used by previews). */
  defaultOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  /** Render rich content for each object item. */
  itemToElement?: React.ComponentType<any>;
  id?: string;
  className?: string;
  'aria-label'?: string;
}

/** Select-like control that opens a list and holds one selected item. */
export interface DropdownProps<ItemType = any> extends ListBoxCommonProps<ItemType> {
  /** Placeholder text shown while nothing is selected. */
  label: string;
  selectedItem?: ItemType | null;
  initialSelectedItem?: ItemType | null;
  onChange?: (data: { selectedItem: ItemType | null }) => void;
}

/** Text field with a filtered list of options and a clear button. */
export interface ComboBoxProps<ItemType = any> extends ListBoxCommonProps<ItemType> {
  placeholder?: string;
  selectedItem?: ItemType | null;
  initialSelectedItem?: ItemType | null;
  /** Initial text in the input (used by previews to show a filtered list). */
  initialInputValue?: string;
  onChange?: (data: { selectedItem: ItemType | null; inputValue?: string }) => void;
  onInputChange?: (inputValue: string) => void;
  /** Replace the default case-insensitive "contains" filter. */
  shouldFilterItem?: (args: { item: ItemType; itemToString: (item: ItemType | null) => string; inputValue: string }) => boolean;
  /** Keep typed text that matches no item. */
  allowCustomValue?: boolean;
}

/** Field that opens a checkbox list and holds several selected items, summarised by a count tag. */
export interface MultiSelectProps<ItemType = any> extends ListBoxCommonProps<ItemType> {
  /** Field text (also the accessible name of the toggle). */
  label: string;
  selectedItems?: ItemType[];
  initialSelectedItems?: ItemType[];
  onChange?: (data: { selectedItems: ItemType[] }) => void;
  /** Make the field a text input that filters the list. */
  filterable?: boolean;
  /** Where selected items sit in the open list: `top-after-reopen` (default) moves the items that were selected when the menu opened to the top; `top` moves them as they are chosen; `fixed` keeps the order of `items`. */
  selectionFeedback?: 'top' | 'fixed' | 'top-after-reopen';
  placeholder?: string;
  shouldFilterItem?: (args: { item: ItemType; itemToString: (item: ItemType | null) => string; inputValue: string }) => boolean;
}

export type FileUploaderStatus = 'uploading' | 'edit' | 'complete';

export interface FileUploaderFile {
  uuid?: string;
  name: string;
  /** `uploading` shows a spinner, `edit` a remove button, `complete` a check. */
  status?: FileUploaderStatus;
  invalid?: boolean;
  errorSubject?: string;
  errorBody?: string;
}

/** Lets users choose files and shows each file with its upload status. */
export interface FileUploaderProps {
  /** `button` shows a Button; `drop` shows a drag-and-drop container. Default `button`. */
  variant?: 'button' | 'drop';
  labelTitle?: string;
  labelDescription?: string;
  /** Button text (button variant). */
  buttonLabel?: string;
  /** Any Button kind. Default `primary`. */
  buttonKind?: 'primary' | 'secondary' | 'tertiary' | 'ghost' | 'danger' | 'danger--tertiary' | 'danger--ghost';
  /** Drop container text (drop variant). */
  dropLabel?: string;
  /** Accepted types, such as `['.pdf', 'image/*']`. */
  accept?: string[];
  multiple?: boolean;
  disabled?: boolean;
  name?: string;
  /** Button size and file row size. */
  size?: 'sm' | 'md' | 'lg';
  /** Controlled list of files. */
  files?: FileUploaderFile[];
  defaultFiles?: FileUploaderFile[];
  /** Status given to files added through the control. Default `edit`. */
  filenameStatus?: FileUploaderStatus;
  iconDescription?: string;
  onChange?: (event: { files: FileUploaderFile[]; addedFiles: File[] }) => void;
  onDelete?: (event: React.SyntheticEvent, data: { uuid: string; file: FileUploaderFile }) => void;
  className?: string;
}

/** The button used by FileUploader. */
export interface FileUploaderButtonProps {
  /** Button text. After a pick it changes to the file name (or "n files") unless `disableLabelChanges`. */
  labelText: string;
  /** Keep `labelText` after files are chosen (FileUploader sets this). */
  disableLabelChanges?: boolean;
  /** Raw change event of the hidden file input. */
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  /** Id of the hidden file input. */
  id?: string;
  buttonKind?: FileUploaderProps['buttonKind'];
  size?: 'sm' | 'md' | 'lg';
  accept?: string[];
  multiple?: boolean;
  disabled?: boolean;
  name?: string;
  onAddFiles?: (event: React.SyntheticEvent, data: { addedFiles: File[] }) => void;
  className?: string;
}

/** The drag-and-drop container used by FileUploader. */
export interface FileUploaderDropContainerProps {
  /** Default "Drag and drop files here or click to upload". */
  labelText?: string;
  /** Id of the hidden file input. */
  id?: string;
  accept?: string[];
  multiple?: boolean;
  disabled?: boolean;
  name?: string;
  onAddFiles?: (event: React.SyntheticEvent, data: { addedFiles: File[] }) => void;
  className?: string;
}

/** One file row. */
export interface FileUploaderItemProps extends FileUploaderFile {
  size?: 'sm' | 'md';
  iconDescription?: string;
  onDelete?: (event: React.SyntheticEvent, data: { uuid?: string }) => void;
  className?: string;
}

/** The bare date input with its label, calendar icon and messages. */
export interface DatePickerInputProps {
  id?: string;
  labelText?: React.ReactNode;
  hideLabel?: boolean;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  datePickerType?: 'single' | 'range' | 'simple';
  helperText?: React.ReactNode;
  invalid?: boolean;
  invalidText?: React.ReactNode;
  warn?: boolean;
  warnText?: React.ReactNode;
  disabled?: boolean;
  readOnly?: boolean;
  size?: 'sm' | 'md' | 'lg';
  pattern?: string;
  onChange?: React.ChangeEventHandler<HTMLInputElement>;
  onClick?: React.MouseEventHandler<HTMLInputElement>;
  onBlur?: React.FocusEventHandler<HTMLInputElement>;
  onKeyDown?: React.KeyboardEventHandler<HTMLInputElement>;
  onFocus?: React.FocusEventHandler<HTMLInputElement>;
  /** Ref to the `<input>`. */
  inputRef?: React.Ref<HTMLInputElement>;
  /** Class on the outer `cds--date-picker-container`. */
  containerClassName?: string;
  className?: string;
  /** Internal (set by a range DatePicker): id of a shared message rendered by the parent; the input then shows the invalid / warn styling but no message of its own. */
  msgId?: string;
  /** Rendered inside the input wrapper (used to place the calendar). */
  children?: React.ReactNode;
}

type DateValue = Date | string;

/** Date field with a calendar popup: single date, range, or text-only (simple). */
export interface DatePickerProps {
  /** Default `single`. */
  datePickerType?: 'single' | 'range' | 'simple';
  labelText?: React.ReactNode;
  /** Label of the second input (range). Default "End date". */
  endLabelText?: React.ReactNode;
  placeholder?: string;
  endPlaceholder?: string;
  /** Controlled value; an array of two for range. */
  value?: DateValue | DateValue[];
  defaultValue?: DateValue | DateValue[];
  /** Receives one `Date` (single) or one or two (range). */
  onChange?: (dates: Date[]) => void;
  /** Format tokens `m n d j Y y`; default `m/d/Y`. */
  dateFormat?: string;
  /** Days before this date are disabled. */
  minDate?: DateValue;
  /** Days after this date are disabled. */
  maxDate?: DateValue;
  size?: 'sm' | 'md' | 'lg';
  helperText?: React.ReactNode;
  /** Outlines the field(s); for `range` both inputs are outlined and one message spans them. Ignored while disabled or read-only. */
  invalid?: boolean;
  invalidText?: React.ReactNode;
  warn?: boolean;
  warnText?: React.ReactNode;
  disabled?: boolean;
  readOnly?: boolean;
  /** `pattern` attribute of the input(s). */
  pattern?: string;
  hideLabel?: boolean;
  /** Narrower simple input. */
  short?: boolean;
  /** Controlled calendar open state. */
  open?: boolean;
  /** Initial calendar open state (used by previews). */
  defaultOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  id?: string;
  className?: string;
}

/** Loading placeholder for Dropdown, ComboBox and MultiSelect. */
export interface DropdownSkeletonProps { size?: ListBoxSize; hideLabel?: boolean; className?: string; }
/** Loading placeholder for DatePicker. */
export interface DatePickerSkeletonProps { /** Two fields. */ range?: boolean; hideLabel?: boolean; className?: string; }
/** Loading placeholder for FileUploader: heading, description and button bars. */
export interface FileUploaderSkeletonProps { className?: string; }

// ---- B1
export interface DataTableHeader {
  /** Key of the row field this column shows. */
  key: string;
  /** Column header text. */
  header: React.ReactNode;
  /** Text alignment of the header and its cells. */
  align?: 'left' | 'center' | 'right';
  /** Overrides the table-level `isSortable` for this column. */
  isSortable?: boolean;
}
export interface DataTableRow {
  /** Unique row id (used for selection and expansion). */
  id: string;
  /** Disables the row's selection control. */
  disabled?: boolean;
  /** Cell values, keyed by header `key`. Values can be strings, numbers or nodes. */
  [key: string]: any;
}
export interface DataTableBatchAction {
  label: string;
  /** Icon file stem, e.g. "download". */
  icon?: string;
  /** Called with the selected rows. */
  onClick?: (selectedRows: DataTableRow[]) => void;
}
/** One item of the per-row overflow menu (`rowActions`). */
export interface DataTableRowAction {
  label: string;
  /** Called with the row the menu belongs to. */
  onClick?: (row: DataTableRow) => void;
  /** `danger` for destructive actions (red on hover / focus). */
  kind?: 'default' | 'danger';
  disabled?: boolean;
  /** Icon file stem shown in the menu item. */
  icon?: string;
}
export type DataTableSortDirection = 'NONE' | 'ASC' | 'DESC';

/** Declarative data table: pass `headers` and `rows`. Search, sort, selection and expansion are built in. */
export interface DataTableProps {
  headers: DataTableHeader[];
  rows: DataTableRow[];
  /** Row height: xs 24px, sm 32px, md 40px, lg 48px (default), xl 64px. */
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  /** Alternate row backgrounds. */
  useZebraStyles?: boolean;
  /** Makes every column sortable (cycle none, ascending, descending). */
  isSortable?: boolean;
  /** Row selection: `true`/`'multi'` for checkboxes with select all, `'radio'` for single select. */
  selectable?: boolean | 'multi' | 'radio' | 'single';
  /** Adds an expand chevron column; use with `renderExpanded`. */
  expandable?: boolean;
  /** Content of the expanded child row. */
  renderExpanded?: (row: DataTableRow) => React.ReactNode;
  /** Custom cell renderer. */
  renderCell?: (value: any, row: DataTableRow, header: DataTableHeader) => React.ReactNode;
  /** Title and description shown above the toolbar. */
  title?: React.ReactNode;
  description?: React.ReactNode;
  /** Shows a search field in the toolbar; filters on the text of string and number cells. */
  search?: boolean;
  searchPlaceholder?: string;
  /** Keep the search field open instead of expanding on focus. */
  persistentSearch?: boolean;
  searchValue?: string;
  defaultSearchValue?: string;
  onSearchChange?: (value: string) => void;
  /** Actions shown in the batch bar while rows are selected (multi selection only). */
  batchActions?: DataTableBatchAction[];
  /** Extra toolbar content at the end of the toolbar (buttons). */
  toolbarContent?: React.ReactNode;
  /** Adds a last column with an OverflowMenu per row: a list, or a function returning the list for a row. */
  rowActions?: DataTableRowAction[] | ((row: DataTableRow) => DataTableRowAction[]);
  /** Visually hidden header text of the overflow-menu column. Default `Actions`. */
  rowActionsHeader?: string;
  /** Hide row overflow menus until the row is hovered or the menu focused (hover-capable devices). Default true. */
  overflowMenuOnHover?: boolean;
  /** Pin the header row while the body scrolls; the table area is capped at 300px high. */
  stickyHeader?: boolean;
  /** Shown below the table when no rows match; a node or `{ title, description, action }`. */
  emptyState?: React.ReactNode | { title?: React.ReactNode; description?: React.ReactNode; action?: React.ReactNode };
  /** Controlled / initial selection (row ids). */
  selectedIds?: string[];
  defaultSelectedIds?: string[];
  onSelectionChange?: (ids: string[]) => void;
  /** Controlled / initial expanded rows (row ids). */
  expandedIds?: string[];
  defaultExpandedIds?: string[];
  onExpandChange?: (ids: string[]) => void;
  /** Controlled / initial sort. */
  sortKey?: string | null;
  sortDirection?: DataTableSortDirection;
  defaultSortKey?: string;
  defaultSortDirection?: DataTableSortDirection;
  onSortChange?: (key: string, direction: DataTableSortDirection) => void;
  /** Accessible name when there is no `title` (the title labels the table otherwise). */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  id?: string;
  className?: string;
  style?: React.CSSProperties;
}

/** Primitive wrapper: scroll container plus the `<table>` element. */
export interface TableProps extends React.TableHTMLAttributes<HTMLTableElement> {
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  useZebraStyles?: boolean;
  isSortable?: boolean;
  /** Pin the header row; the scroll area is capped at 300px high. */
  stickyHeader?: boolean;
  /** `false` keeps `.cds--table-column-menu` overflow menus visible instead of showing them on row hover. */
  overflowMenuOnHover?: boolean;
  /** Class on the scroll container around the `<table>`. */
  contentClassName?: string;
}
export interface TableHeadProps extends React.HTMLAttributes<HTMLTableSectionElement> {}
export interface TableBodyProps extends React.HTMLAttributes<HTMLTableSectionElement> {}
export interface TableRowProps extends React.HTMLAttributes<HTMLTableRowElement> {
  /** Applies the selected row background. */
  isSelected?: boolean;
}
export interface TableCellProps extends React.TdHTMLAttributes<HTMLTableCellElement> {}
export interface TableHeaderProps {
  children?: React.ReactNode;
  /** Renders the header as a sort button. */
  isSortable?: boolean;
  /** True when this column is the one currently sorted. */
  isSortHeader?: boolean;
  sortDirection?: DataTableSortDirection;
  onClick?: React.MouseEventHandler<HTMLButtonElement>;
  align?: 'left' | 'center' | 'right';
  colSpan?: number;
  scope?: string;
  id?: string;
  className?: string;
}

/** Items per page select, item range, page select and previous / next buttons. */
export interface PaginationProps {
  /** Page sizes: numbers or `{ text, value }`. */
  pageSizes: number[] | { text: string; value: number }[];
  pageSize?: number;
  /** 1-based current page. */
  page?: number;
  totalItems?: number;
  /** Page count is unknown: shows "page N" and keeps next enabled. */
  pagesUnknown?: boolean;
  isLastPage?: boolean;
  disabled?: boolean;
  pageInputDisabled?: boolean;
  pageSizeInputDisabled?: boolean;
  size?: 'sm' | 'md' | 'lg';
  backwardText?: string;
  forwardText?: string;
  itemsPerPageText?: string;
  itemText?: (min: number, max: number) => string;
  itemRangeText?: (min: number, max: number, total: number) => string;
  pageRangeText?: (current: number, total: number) => string;
  pageText?: (page: number) => string;
  onChange?: (data: { page: number; pageSize: number }) => void;
  id?: string | number;
  className?: string;
}

/** Numbered page buttons with overflow selects and previous / next buttons. */
export interface PaginationNavProps {
  totalItems: number;
  /** 0-based current page. */
  page?: number;
  /** Number of items shown before the rest collapse into overflow selects. */
  itemsShown?: number;
  size?: 'sm' | 'md' | 'lg';
  /** Wrap around at the first / last page. */
  loop?: boolean;
  disableOverflow?: boolean;
  /** Called with the 0-based page index. */
  onChange?: (page: number) => void;
  'aria-label'?: string;
  className?: string;
}

/** Table-like list built from div rows and cells. Pass `columns` and `rows`, or compose the parts. */
export interface StructuredListProps {
  /** Column header labels. */
  columns?: React.ReactNode[];
  /** Data rows: `id` and a `cells` array. `disabled` blocks selecting the row. */
  rows?: { id?: string; cells: React.ReactNode[]; disabled?: boolean }[];
  /** Adds a radio per row; one row can be selected. */
  selection?: boolean;
  selectedInitialRow?: string;
  selectedRow?: string | null;
  onSelectionChange?: (rowId: string) => void;
  isCondensed?: boolean;
  /** Removes the start padding (ignored with `selection`). */
  isFlush?: boolean;
  noWrap?: boolean;
  children?: React.ReactNode;
  'aria-label'?: string;
  className?: string;
}
export interface StructuredListHeadProps { children?: React.ReactNode; className?: string; }
export interface StructuredListBodyProps { children?: React.ReactNode; className?: string; }
export interface StructuredListRowProps {
  /** Header row inside StructuredListHead. */
  head?: boolean;
  id?: string;
  /** In a selectable list: the row cannot be selected and is dimmed. */
  disabled?: boolean;
  /** Accessible name of the row's radio. */
  label?: string;
  onClick?: React.MouseEventHandler;
  children?: React.ReactNode;
  className?: string;
}
export interface StructuredListCellProps { head?: boolean; noWrap?: boolean; children?: React.ReactNode; className?: string; }

/** List with an optional header and action. Children are `ContainedListItem`s; a `Search` child is pinned under the header. */
export interface ContainedListProps {
  label?: React.ReactNode;
  /** Action in the header, usually an icon-only ghost Button or an `ExpandableSearch`. */
  action?: React.ReactNode;
  /** `on-page` (default) sits on the page background; `disclosed` sits in a layer. */
  kind?: 'on-page' | 'disclosed';
  /** Item height: sm 32, md 40, lg 48 (default), xl 64. */
  size?: 'sm' | 'md' | 'lg' | 'xl';
  /** Inset the dividers by the item padding. */
  isInset?: boolean;
  children?: React.ReactNode;
  /** Accessible name of the list when there is no `label`. */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  className?: string;
}
export interface ContainedListItemProps {
  /** Makes the item a button. */
  onClick?: () => void;
  disabled?: boolean;
  /** Icon file stem shown before the content, e.g. "star". */
  renderIcon?: string;
  /** Action shown at the end of the item, usually an icon-only ghost Button. */
  action?: React.ReactNode;
  children?: React.ReactNode;
  className?: string;
}

// ---- B2
import type * as React from 'react';

export interface AccordionProps {
  /** Where the chevron sits relative to the title. Default `end`. */
  align?: 'start' | 'end';
  /** Heading height: sm 32px, md 40px, lg 48px. Default `md`. */
  size?: 'sm' | 'md' | 'lg';
  /** No outer padding, so titles and dividers align with neighbouring content; ignored with `align="start"`. */
  isFlush?: boolean;
  /** Disable every item (an item's own `disabled` takes precedence). */
  disabled?: boolean;
  /** Render an `<ol>` instead of a `<ul>`. */
  ordered?: boolean;
  className?: string;
  children?: React.ReactNode;
}
export interface AccordionItemProps {
  /** Heading content. */
  title?: React.ReactNode;
  /** Expanded state; changing it re-syncs the item. */
  open?: boolean;
  disabled?: boolean;
  /** Called with the new open state when the heading is clicked. */
  onHeadingClick?: (data: { isOpen: boolean; event: React.MouseEvent }) => void;
  /** Called on every heading click, after `onHeadingClick`. */
  onClick?: (event: React.MouseEvent) => void;
  /** Accessible name of the heading button. */
  'aria-label'?: string;
  className?: string;
  children?: React.ReactNode;
}

export interface TreeViewProps {
  /** Accessible name / visible label of the tree. */
  label?: string;
  hideLabel?: boolean;
  /** Accessible name when there is no `label`. */
  'aria-label'?: string;
  /** Row height: xs 24px, sm 32px. Default `sm`. */
  size?: 'xs' | 'sm';
  /** Id of the active node. Seeds the state and re-syncs when it changes; clicking a node still moves it. */
  active?: string | number;
  /** Ids of selected nodes. Seeds the state and re-syncs when the list changes; clicking still updates it. */
  selected?: Array<string | number>;
  /** Ctrl/Cmd+click adds or removes nodes from the selection. */
  multiselect?: boolean;
  onSelect?: (event: React.SyntheticEvent, node: { id: string; label: React.ReactNode; value?: any; selected: Array<string | number> }) => void;
  onActivate?: (node: { id: string; label: React.ReactNode; value?: any }) => void;
  className?: string;
  children?: React.ReactNode;
}
export interface TreeNodeProps {
  /** Node text. */
  label: React.ReactNode;
  /** Unique id, used by `active` / `selected`. Generated when omitted. */
  id?: string;
  value?: any;
  /** Icon file stem (e.g. `"folder"`) shown before the label. */
  renderIcon?: string;
  disabled?: boolean;
  /** Controlled expansion; `defaultIsExpanded` sets the initial state. */
  isExpanded?: boolean;
  defaultIsExpanded?: boolean;
  onToggle?: (event: React.SyntheticEvent, data: { id: string; isExpanded: boolean; label: React.ReactNode; value?: any }) => void;
  onSelect?: (event: React.SyntheticEvent, node: { id: string; label: React.ReactNode; value?: any }) => void;
  className?: string;
  /** Child `TreeNode`s; presence makes this a parent node. */
  children?: React.ReactNode;
}

export interface CodeSnippetProps {
  /** Form of the snippet. Default `single`. */
  type?: 'inline' | 'single' | 'multi';
  /** The code. */
  children?: React.ReactNode;
  disabled?: boolean;
  hideCopyButton?: boolean;
  wrapText?: boolean;
  light?: boolean;
  /** Text copied instead of the rendered code. */
  copyText?: string;
  /** Tooltip shown after copying. Default `Copied!`. */
  feedback?: string;
  /** Milliseconds the feedback stays. Default 2000. */
  feedbackTimeout?: number;
  /** Tooltip and accessible name of the copy button. */
  copyButtonDescription?: string;
  'aria-label'?: string;
  align?: 'bottom' | 'bottom-start' | 'bottom-end' | 'top' | 'top-start' | 'top-end' | 'left' | 'right';
  minCollapsedNumberOfRows?: number;
  maxCollapsedNumberOfRows?: number;
  minExpandedNumberOfRows?: number;
  maxExpandedNumberOfRows?: number;
  showMoreText?: string;
  showLessText?: string;
  onClick?: (event: React.MouseEvent) => void;
  className?: string;
}

export interface ProgressBarProps {
  /** What is progressing (required). */
  label: string;
  /** Current value; omit for the indeterminate state. */
  value?: number;
  /** Maximum value. Default 100. */
  max?: number;
  helperText?: string;
  hideLabel?: boolean;
  /** Track height: big 8px, small 4px. Default `big`. */
  size?: 'big' | 'small';
  status?: 'active' | 'finished' | 'error';
  type?: 'default' | 'inline' | 'indented';
  className?: string;
}

export interface ProgressIndicatorProps {
  /** Zero-based index of the current step. */
  currentIndex?: number;
  vertical?: boolean;
  /** Share the width equally between steps (horizontal only). */
  spaceEqually?: boolean;
  /** Makes steps clickable (except the current and disabled ones); receives the step index. */
  onChange?: (index: number) => void;
  className?: string;
  children?: React.ReactNode;
}
export interface ProgressStepProps {
  label: string;
  /** Small text under the label, e.g. "Optional". */
  secondaryLabel?: string;
  /** Accessible title of the step icon. */
  description?: string;
  complete?: boolean;
  current?: boolean;
  invalid?: boolean;
  disabled?: boolean;
  onClick?: (event: React.SyntheticEvent) => void;
  className?: string;
}

export interface SkeletonTextProps {
  /** CSS width, default `100%`. */
  width?: string;
  /** Taller (24px) heading bar. */
  heading?: boolean;
  /** Render `lineCount` lines of varying width. */
  paragraph?: boolean;
  /** Lines in paragraph mode. Default 3. */
  lineCount?: number;
  className?: string;
}
export interface SkeletonPlaceholderProps {
  /** Default size is 100px by 100px; override with `style` or a class. */
  style?: React.CSSProperties;
  className?: string;
}

export interface LoadingProps {
  /** Spinning when true (default); false winds the spinner down. */
  active?: boolean;
  /** 16px spinner instead of 88px. */
  small?: boolean;
  /** Cover the viewport with a dimmed fixed overlay. Default true. */
  withOverlay?: boolean;
  /** Accessible title. Default `loading`. */
  description?: string;
  id?: string;
  className?: string;
}
export interface InlineLoadingProps {
  /** `active` spins; `inactive` winds the spinner down and leaves it stopped; `finished` / `error` show a status icon. Default `active`. */
  status?: 'inactive' | 'active' | 'finished' | 'error';
  /** Status text next to the icon. */
  description?: React.ReactNode;
  /** Accessible title of the icon. Defaults to `loading`, `not loading`, `finished` or `error`. */
  iconDescription?: string;
  /** Live-region politeness of the container. Default `assertive`. */
  'aria-live'?: 'off' | 'polite' | 'assertive';
  /** Called `successDelay` ms after status becomes `finished`. */
  onSuccess?: () => void;
  /** Default 1500. */
  successDelay?: number;
  className?: string;
}

// ---- C1
type IconRef = string | (() => any) | any;

/** The 48px application header (`<header class="cds--header">`), fixed to the top of its containing block. */
export interface HeaderProps {
  /** Accessible name of the banner landmark. */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  className?: string;
  children?: any;
}

/** Product name link in the header. */
export interface HeaderNameProps {
  /** Company or family prefix shown in regular weight before the name. Pass `''` to hide. Default `'IBM'`. */
  prefix?: string;
  href?: string;
  /** Replace the underlying `<a>` with another element or component. */
  element?: any;
  className?: string;
  children: any;
}

/** Top-level navigation list (`nav > ul`); hidden below the lg breakpoint (1056px). Children are `HeaderMenuItem`. */
export interface HeaderNavigationProps {
  'aria-label'?: string;
  'aria-labelledby'?: string;
  className?: string;
  children?: any;
}

/** A link in `HeaderNavigation`. `isActive` (or `aria-current="page"`) draws the 3px interactive underline. */
export interface HeaderMenuItemProps {
  href?: string;
  isActive?: boolean;
  /** @deprecated use `isActive` */
  isCurrentPage?: boolean;
  'aria-current'?: boolean | 'page' | 'true' | 'step' | 'location' | 'date' | 'time';
  element?: any;
  /** Class applied to the wrapping `<li>`. */
  className?: string;
  role?: string;
  tabIndex?: number;
  children: any;
}

/** Right-aligned container for `HeaderGlobalAction`. */
export interface HeaderGlobalBarProps { className?: string; children?: any; }

/** 48x48 icon button in the header. Children are an `Icon` (20px). */
export interface HeaderGlobalActionProps {
  /** Accessible name; also used as the native tooltip (`title`). */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  /** Show the pressed/open state (layer fill, subtle side borders). */
  isActive?: boolean;
  onClick?: (event: any) => void;
  className?: string;
  children: any;
}

/** Hamburger button that toggles the side nav. Hidden at lg and above unless `isCollapsible`. */
export interface HeaderMenuButtonProps {
  'aria-label'?: string;
  'aria-labelledby'?: string;
  /** Shows the close icon instead of the menu icon. */
  isActive?: boolean;
  /** Keep the button visible at the lg breakpoint and above. */
  isCollapsible?: boolean;
  /** Custom menu icon element (default: `Icon name="menu"`). */
  renderMenuIcon?: any;
  /** Custom close icon element (default: `Icon name="close"`). */
  renderCloseIcon?: any;
  onClick?: (event: any) => void;
  className?: string;
}

/** Visually hidden link that appears on keyboard focus. */
export interface SkipToContentProps {
  /** Default `'#main-content'`. */
  href?: string;
  /** Default `'Skip to main content'`. */
  children?: string;
  tabIndex?: number;
  className?: string;
}

/** Vertical navigation panel, 256px wide when expanded. Provides expansion state to its links and menus. */
export interface SideNavProps {
  /** Accessible name of the navigation landmark. */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  /** Controlled expansion. Setting it makes the component controlled. */
  expanded?: boolean;
  /** Initial expansion when uncontrolled. */
  defaultExpanded?: boolean;
  /** 48px collapsed rail (icons only) that expands on hover or focus. */
  isRail?: boolean;
  /** Standalone persistent nav; collapses off-canvas when not `expanded`. */
  isFixedNav?: boolean;
  /** Sits under a `Header` (adds the 48px offset). Default `true`. */
  isChildOfHeader?: boolean;
  /** `false` hides the nav (width 0). Default `true`. */
  isPersistent?: boolean;
  /** Called when focus, hover, Escape or a click would expand or collapse the nav. */
  onToggle?: (eventOrFlag: any, expanded: boolean) => void;
  /** Called when the dimming overlay (below lg) is clicked. */
  onOverlayClick?: (event: any) => void;
  onSideNavBlur?: () => void;
  addFocusListeners?: boolean;
  addMouseListeners?: boolean;
  /** Where Escape navigates to (id of the main content). */
  href?: string;
  className?: string;
  children?: any;
}

/** The `<ul>` list inside `SideNav`. */
export interface SideNavItemsProps { className?: string; children: any; }

/** A top-level link in the side nav. */
export interface SideNavLinkProps {
  href?: string;
  /** Selected: `background-selected` fill, 3px `border-interactive` bar, semibold primary text. */
  isActive?: boolean;
  /** Icon file stem (`"dashboard"`) or a component rendering a 16px icon. */
  renderIcon?: IconRef;
  /** 48px row instead of 32px. */
  large?: boolean;
  element?: any;
  tabIndex?: number;
  isSideNavExpanded?: boolean;
  className?: string;
  children: any;
}

/** Expandable group with a chevron button; children are `SideNavMenuItem`. */
export interface SideNavMenuProps {
  title: string;
  renderIcon?: IconRef;
  /** Start expanded. */
  defaultExpanded?: boolean;
  /** Mark the group as the current section. It is also marked automatically when collapsed with an active child. */
  isActive?: boolean;
  large?: boolean;
  tabIndex?: number;
  isSideNavExpanded?: boolean;
  className?: string;
  children?: any;
}

/** A link nested inside `SideNavMenu`. */
export interface SideNavMenuItemProps {
  href?: string;
  isActive?: boolean;
  as?: any;
  element?: any;
  className?: string;
  children?: any;
}

/** Horizontal rule between side nav groups. */
export interface SideNavDividerProps { className?: string; }

/** Navigation trail (`nav > ol`). Children are `BreadcrumbItem`. */
export interface BreadcrumbProps {
  /** Default `'Breadcrumb'`. */
  'aria-label'?: string;
  /** Omit the trailing `/` after the last item. */
  noTrailingSlash?: boolean;
  /** `sm` uses label-01 type and tighter spacing. Default `md`. */
  size?: 'sm' | 'md';
  /** Class applied to the `<nav>`. */
  className?: string;
  children?: any;
}

/** One crumb. A string child renders a link (with `href`) or plain text (without). An `OverflowMenu` child becomes the overflow crumb: a link-colored horizontal ellipsis (sm, `iconDescription` default 'Show more breadcrumbs') whose `MenuItem`s navigate to the hidden levels. Any other element (e.g. a router link) is cloned with the `cds--link` class. */
export interface BreadcrumbItemProps {
  href?: string;
  /** Marks the current page: primary text color, no hover underline, `aria-current`. */
  isCurrentPage?: boolean;
  'aria-current'?: boolean | 'page' | 'true' | 'step' | 'location' | 'date' | 'time';
  className?: string;
  children?: any;
}

export interface SwitchChangeEvent { index?: number; name?: string | number; text?: string; key?: string | number; }

/** Segmented control that switches between 2 or more views; exactly one `Switch` is selected. Children are `Switch`, or all `IconSwitch` for the icon-only variant. */
export interface ContentSwitcherProps {
  /** `sm` 32px, `md` 40px, `lg` 48px. Default `md`. */
  size?: 'sm' | 'md' | 'lg';
  /** Index of the selected switch. Re-applied whenever the prop changes. Default `0`. */
  selectedIndex?: number;
  /** Called with `{ index, name, text }` when the selection changes. */
  onChange?: (event: SwitchChangeEvent) => void;
  /** `automatic` selects on arrow-key focus (default); `manual` needs Enter or Space. */
  selectionMode?: 'automatic' | 'manual';
  /** Quieter style: a `layer-01` track with the selected switch on `layer-02` and `text-primary`, instead of the inverse fill. */
  lowContrast?: boolean;
  className?: string;
  children?: any;
}

/** One option in a `ContentSwitcher`. `ContentSwitcher` supplies `index`, `selected` and `size`. */
export interface SwitchProps {
  /** Label. Falls back to children. */
  text?: string;
  /** Passed back in `onChange`. */
  name?: string | number;
  disabled?: boolean;
  className?: string;
  children?: any;
}

/** Icon-only option in a `ContentSwitcher` (all children must be `IconSwitch`): a square switch the height of the control. */
export interface IconSwitchProps {
  /** Accessible name and native tooltip (`title`). Required. */
  text: string;
  /** Icon file stem (16px), or pass an icon element as children. */
  renderIcon?: string | (() => any);
  /** Passed back in `onChange`. */
  name?: string | number;
  disabled?: boolean;
  className?: string;
  children?: any;
}

// ---- C2
import type * as React from 'react';

export type MenuSize = 'xs' | 'sm' | 'md' | 'lg';
export type MenuAlignment = 'top' | 'top-start' | 'top-end' | 'bottom' | 'bottom-start' | 'bottom-end';

/** Props shared by the components that toggle an attached Menu (MenuButton, OverflowMenu, ComboButton). */
export interface AttachedMenuProps {
  /** Controlled open state. */
  open?: boolean;
  /** Initial open state when uncontrolled. */
  defaultOpen?: boolean;
  /** Called when the menu opens. */
  onOpen?: () => void;
  /** Called when the menu closes (Escape, item chosen, click outside, focus leaves). */
  onClose?: () => void;
  className?: string;
  style?: React.CSSProperties;
}

export interface MenuProps {
  /** Accessible name of the menu (aria-label). */
  label: string;
  /** Whether the menu is shown. A closed root menu renders nothing. */
  open?: boolean;
  /** Item height: xs 24px, sm 32px (default), md 40px, lg 48px. Submenus inherit the root size. */
  size?: MenuSize;
  /** Called when the menu should close: Escape, an item is chosen, focus leaves, or ArrowLeft in a submenu. */
  onClose?: () => void;
  /** Called when the menu opens. */
  onOpen?: () => void;
  /** Horizontal offset in px from the left of the nearest positioned ancestor (or an [x1, x2] range; x1 is used). */
  x?: number | [number, number];
  /** Vertical offset in px from the top of the nearest positioned ancestor (or a [y1, y2] range; y1 is used). */
  y?: number | [number, number];
  /** Place the menu against its wrapper (which must be position: relative) instead of using x and y. */
  menuAlignment?: MenuAlignment;
  /** Use position: fixed (viewport coordinates) instead of absolute. */
  fixed?: boolean;
  /** Draw a 1px border-subtle outline. */
  border?: boolean;
  /** Surface token: 'layer' (default) or 'background'. */
  backgroundToken?: 'layer' | 'background';
  /** Ref of the element that wraps trigger and menu; focus moving inside it does not close the menu. */
  containerRef?: React.RefObject<HTMLElement>;
  /** Ref of the element that receives focus when the menu closes. */
  returnFocusRef?: React.RefObject<HTMLElement>;
  className?: string;
  style?: React.CSSProperties;
  id?: string;
  children?: React.ReactNode;
}

export interface MenuItemProps {
  /** Text of the item. Also used as its title. */
  label: string;
  /** Renders a submenu opened by hover, click or ArrowRight when MenuItems are passed as children. */
  children?: React.ReactNode;
  /** Icon file stem (e.g. 'edit'), an element or a component. Enables the icon column for the whole menu. */
  renderIcon?: string | React.ComponentType | React.ReactElement;
  /** Keyboard shortcut text shown at the end of the row. Ignored on items with a submenu. */
  shortcut?: string;
  /** 'danger' turns the hover/focus background red. */
  kind?: 'default' | 'danger';
  disabled?: boolean;
  onClick?: (event: React.SyntheticEvent) => void;
  /** Show the submenu open on first render (preview/documentation use; not in Carbon). */
  defaultSubmenuOpen?: boolean;
  className?: string;
}

export interface MenuItemSelectableProps {
  label: string;
  /** Controlled checked state. */
  selected?: boolean;
  defaultSelected?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  shortcut?: string;
  renderIcon?: string | React.ComponentType;
  className?: string;
}

export interface MenuItemGroupProps {
  /** Accessible name of the group (aria-label); not rendered visibly. */
  label: string;
  className?: string;
  children?: React.ReactNode;
}

export interface MenuItemRadioGroupProps<Item = any> {
  /** Accessible name of the group (aria-label). */
  label: string;
  items: Item[];
  /** Maps an item to its label. Defaults to the string itself or item.label. */
  itemToString?: (item: Item) => string;
  selectedItem?: Item;
  defaultSelectedItem?: Item;
  onChange?: (item: Item) => void;
  className?: string;
}

export interface MenuItemDividerProps {
  className?: string;
}

export interface MenuButtonProps extends AttachedMenuProps {
  /** Text of the trigger button (also the menu's accessible name). */
  label: string;
  /** Button kind. Default 'primary'. */
  kind?: 'primary' | 'secondary' | 'tertiary' | 'ghost';
  /** Button and menu size. Default 'lg'. */
  size?: MenuSize;
  disabled?: boolean;
  /** Where the menu sits relative to the button. 'bottom' and 'top' make the menu at least as wide as the button. Default 'bottom'. */
  menuAlignment?: MenuAlignment;
  menuBorder?: boolean;
  menuBackgroundToken?: 'layer' | 'background';
  /** MenuItem, MenuItemGroup, MenuItemDivider ... */
  children?: React.ReactNode;
}

export interface OverflowMenuProps extends AttachedMenuProps {
  /** Trigger and menu size: xs 24, sm 32, md 40 (default), lg 48. Match the row height. */
  size?: MenuSize;
  /** Align the menu to the trigger's end edge instead of its start edge. */
  flipped?: boolean;
  /** Open the menu above the trigger. Default 'bottom'. */
  direction?: 'top' | 'bottom';
  /** Accessible name and title of the trigger. Default 'Options'. */
  iconDescription?: string;
  /** Alias of `iconDescription` (the name used by Carbon's menu-based OverflowMenu). */
  label?: string;
  /** Icon file stem: 'overflow-menu--vertical' (default) or 'overflow-menu--horizontal'; or an element or component. */
  renderIcon?: string | React.ComponentType | React.ReactElement;
  disabled?: boolean;
  children?: React.ReactNode;
}

export interface ComboButtonProps extends AttachedMenuProps {
  /** Label of the primary action button. */
  label: string;
  /** Called when the primary action is clicked. */
  onClick?: (event: React.MouseEvent<HTMLButtonElement>) => void;
  /** Button and menu size: sm, md or lg (default). */
  size?: 'sm' | 'md' | 'lg' | 'xs';
  disabled?: boolean;
  /** Where the menu sits relative to the buttons. Default 'bottom'. */
  menuAlignment?: MenuAlignment;
  children?: React.ReactNode;
}

// ---- D1
import type * as React from 'react';

export interface ModalSecondaryButton {
  /** Label of the extra secondary button. */
  buttonText?: React.ReactNode;
  /** Click handler of the extra secondary button. */
  onClick?: React.MouseEventHandler<HTMLButtonElement>;
}

/** Props shared by every way of rendering a modal. */
interface ModalFrameProps {
  /**
   * Preview aid. Render the modal `position: absolute` inside a bordered,
   * fixed-height, `position: relative` frame instead of `position: fixed` over the viewport.
   * Also skips initial focus and the body scroll lock. Do not use in a product. `static` is an alias.
   */
  inline?: boolean;
  /** Alias of `inline`. */
  static?: boolean;
  /** Height of the preview frame when `inline`. Defaults to `24rem`. */
  inlineHeight?: string;
  /** Extra class on the scrim element (`cds--modal`). */
  className?: string;
  /** Called on every keydown inside the modal. */
  onKeyDown?: React.KeyboardEventHandler<HTMLElement>;
  /**
   * Selector of the element to focus on open. Defaults to `[data-modal-primary-focus]`. Without a match, focus goes to the
   * secondary button of a danger modal, else the first enabled field in the body, else the last enabled footer button, else the close button.
   */
  selectorPrimaryFocus?: string;
  /** Element that receives focus again when the modal closes. Without it, the previously focused element is restored. */
  launcherButtonRef?: React.RefObject<HTMLElement | null>;
  /**
   * Do not close on a click on the scrim. Default: `true` for transactional modals (a `Modal` with buttons, a `ComposedModal`
   * with a `ModalFooter` child) and `false` for passive ones; pass `false` explicitly to let a transactional modal close on a scrim click.
   */
  preventCloseOnClickOutside?: boolean;
}

/** Self-contained modal dialog: heading, body, optional footer buttons. */
export interface ModalProps extends ModalFrameProps {
  /** Whether the modal is shown. */
  open?: boolean;
  /** Heading text. */
  modalHeading?: React.ReactNode;
  /** Small label above the heading. */
  modalLabel?: React.ReactNode;
  /** Label of the primary button. Ignored when `passiveModal`. */
  primaryButtonText?: React.ReactNode;
  /** Label of the secondary button. Ignored when `passiveModal`. Clicking runs `onSecondarySubmit`, else `onRequestClose`. */
  secondaryButtonText?: React.ReactNode;
  /** Up to two secondary buttons (three-button footer); replaces `secondaryButtonText`. */
  secondaryButtons?: ModalSecondaryButton[];
  /** Disable the primary button. */
  primaryButtonDisabled?: boolean;
  /** Loading state of the primary action. Anything but `inactive` (the default) disables the footer buttons and replaces the primary label with an `InlineLoading`. */
  loadingStatus?: 'inactive' | 'active' | 'finished' | 'error';
  /** Text of the `InlineLoading` in the primary button, such as `Creating cluster...`. */
  loadingDescription?: string;
  /** Accessible description of the loading icon. */
  loadingIconDescription?: string;
  /** Called 1.5s after `loadingStatus` becomes `finished` (close the modal here). */
  onLoadingSuccess?: () => void;
  /** Destructive confirmation: danger primary button, and the secondary button takes initial focus. */
  danger?: boolean;
  /** No footer buttons; the close button comes first in the tab order and a scrim click closes the modal. */
  passiveModal?: boolean;
  /** Width and height step. `md` is the default. */
  size?: 'xs' | 'sm' | 'md' | 'lg';
  /** Make the body scroll with the overflow fade even when it fits. */
  hasScrollingContent?: boolean;
  /** Remove the body padding. */
  isFullWidth?: boolean;
  /** Use `role="alertdialog"` (non-passive modals). */
  alert?: boolean;
  /** Submit on Enter from a non-button element. */
  shouldSubmitOnEnter?: boolean;
  /** Accessible name; defaults to the label or heading when they are strings. */
  'aria-label'?: string;
  /** Accessible label of the close button. Defaults to `Close`. */
  closeButtonLabel?: string;
  /** Called by the close button, Escape, the secondary button and (when allowed) a scrim click. */
  onRequestClose?: React.ReactEventHandler<HTMLElement>;
  /** Called by the primary button. */
  onRequestSubmit?: React.ReactEventHandler<HTMLElement>;
  /** Called by the secondary button instead of `onRequestClose`. */
  onSecondarySubmit?: React.ReactEventHandler<HTMLElement>;
  /** Body content. */
  children?: React.ReactNode;
}

/** Composable modal: put `ModalHeader`, `ModalBody` and `ModalFooter` inside. */
export interface ComposedModalProps extends ModalFrameProps {
  /** Whether the modal is shown. Closing from inside keeps its own state in sync. */
  open?: boolean;
  /** Width and height step. `md` is the default. */
  size?: 'xs' | 'sm' | 'md' | 'lg';
  /** Danger modal: footer primary button becomes danger. */
  danger?: boolean;
  /** Called when the modal asks to close (close button, Escape, secondary button). Return `false` to keep it open. */
  onClose?: (event: React.SyntheticEvent | Event) => boolean | void;
  /** Extra class on the container element. */
  containerClassName?: string;
  /** Accessible name; defaults to the header title. */
  'aria-label'?: string;
  children?: React.ReactNode;
}

export interface ModalHeaderProps {
  /** Heading text. */
  title?: React.ReactNode;
  /** Small label above the heading. */
  label?: React.ReactNode;
  /** Accessible label of the close button. Defaults to `Close`. */
  iconDescription?: string;
  /** Extra handler for the close button; the modal is then closed. */
  buttonOnClick?: React.MouseEventHandler<HTMLButtonElement>;
  /** Close the modal; filled in from the parent `ComposedModal`. */
  closeModal?: (event: React.SyntheticEvent) => void;
  className?: string;
  titleClassName?: string;
  labelClassName?: string;
  children?: React.ReactNode;
}

export interface ModalBodyProps {
  /** The body holds form fields: no extra right padding. */
  hasForm?: boolean;
  /** Always apply scrolling treatment (overflow fade, focusable region). Applied automatically when the content overflows. */
  hasScrollingContent?: boolean;
  /** Accessible name of the scrolling region. */
  'aria-label'?: string;
  className?: string;
  children?: React.ReactNode;
}

export interface ModalFooterProps {
  /** Label of the primary button. */
  primaryButtonText?: React.ReactNode;
  /** Label of the secondary button; it closes the modal. */
  secondaryButtonText?: React.ReactNode;
  /** Up to two secondary buttons (three-button footer). */
  secondaryButtons?: ModalSecondaryButton[];
  primaryButtonDisabled?: boolean;
  /** Danger primary button. Defaults to the parent modal's `danger`. */
  danger?: boolean;
  /** Called by the primary button. */
  onRequestSubmit?: React.ReactEventHandler<HTMLElement>;
  /** Called by the secondary button before the modal is closed. */
  onRequestClose?: React.ReactEventHandler<HTMLElement>;
  /** Close the modal; filled in from the parent `ComposedModal`. */
  closeModal?: (event: React.SyntheticEvent) => void;
  className?: string;
  /** Custom footer content, used instead of the generated buttons. */
  children?: React.ReactNode;
}

type NotificationKind = 'error' | 'success' | 'warning' | 'warning-alt' | 'info' | 'info-square';

/** Floating notification with title, subtitle and caption (288px wide). */
export interface ToastNotificationProps {
  /** Status. Defaults to `error`. */
  kind?: NotificationKind;
  /** Bold title. */
  title?: React.ReactNode;
  /** Message under the title. */
  subtitle?: React.ReactNode;
  /** Small text under the subtitle, such as a timestamp. */
  caption?: React.ReactNode;
  /** Light tinted variant instead of the inverse (dark) one. */
  lowContrast?: boolean;
  /** Close automatically after this many milliseconds. `0` (default) never closes. */
  timeout?: number;
  /** Do not render the close button. */
  hideCloseButton?: boolean;
  /** Accessible label of the close button. Defaults to `close notification`. */
  'aria-label'?: string;
  /** Accessible description of the status icon. Defaults to `<kind> icon`. */
  statusIconDescription?: string;
  /** ARIA role. Defaults to `status`. */
  role?: string;
  /** Called when the notification closes (button or timeout). Return `false` to keep it. */
  onClose?: (event?: unknown) => boolean | void;
  /** Called by the close button before `onClose`. */
  onCloseButtonClick?: React.MouseEventHandler<HTMLButtonElement>;
  className?: string;
  style?: React.CSSProperties;
  /** Extra content under the caption (no interactive elements except links). */
  children?: React.ReactNode;
}

/** Notification with one action button. Toast style by default; `inline` gives the wide, flat layout. */
export interface ActionableNotificationProps {
  /** Status. Defaults to `error`. */
  kind?: NotificationKind;
  title?: string;
  subtitle?: React.ReactNode;
  caption?: string;
  /** Label of the action button (tertiary in toast style, ghost when `inline`). */
  actionButtonLabel?: string;
  /** Called by the action button. */
  onActionButtonClick?: React.MouseEventHandler<HTMLButtonElement>;
  /** Inline layout: full width, content and action on one row from 672px up. Otherwise toast layout (288px, stacked). */
  inline?: boolean;
  lowContrast?: boolean;
  hideCloseButton?: boolean;
  /** Accessible label of the close button. Defaults to `close notification`. */
  'aria-label'?: string;
  statusIconDescription?: string;
  /** ARIA role. Defaults to `alertdialog`, which moves focus to the action button on mount and keeps Tab inside. Any other value turns that off. */
  role?: string;
  /** Move focus to the action button on mount (only for `alertdialog`). Defaults to `true`. */
  hasFocus?: boolean;
  /** Close on Escape while focus is inside. Defaults to `true`. */
  closeOnEscape?: boolean;
  onClose?: (event?: unknown) => boolean | void;
  onCloseButtonClick?: React.MouseEventHandler<HTMLButtonElement>;
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
}

// ---- D2
export type PopoverAlignment =
  | 'top' | 'top-start' | 'top-end' | 'top-left' | 'top-right'
  | 'bottom' | 'bottom-start' | 'bottom-end' | 'bottom-left' | 'bottom-right'
  | 'left' | 'left-start' | 'left-end' | 'left-top' | 'left-bottom'
  | 'right' | 'right-start' | 'right-end' | 'right-top' | 'right-bottom';

export interface PopoverProps {
  /** Which side of the trigger the content sits on, and how it aligns along that edge. Default 'bottom'. */
  align?: PopoverAlignment;
  /** Element type of the container. Default 'span'. */
  as?: string;
  /** Show the pointer caret. Default true. */
  caret?: boolean;
  /** Draw a 1px subtle border around the content (and caret). */
  border?: boolean;
  /** Drop shadow on the content. Default true. */
  dropShadow?: boolean;
  /** Inverse colors (background-inverse / text-inverse). Default false. */
  highContrast?: boolean;
  /** Whether the content is visible. Controlled only. */
  open?: boolean;
  /** Called on a mousedown outside the popover, or when focus leaves it, while open. */
  onRequestClose?: () => void;
  className?: string;
  /** The trigger element followed by a PopoverContent. */
  children?: any;
}
export interface PopoverContentProps {
  className?: string;
  /** Content of the popover. Extra props (id, role, aria-*) go on the outer `.cds--popover` span. */
  children?: any;
}

export interface TooltipProps {
  /** Tooltip text that labels the trigger (sets aria-labelledby). Plain text only. */
  label?: any;
  /** Tooltip text that describes the trigger (sets aria-describedby); used when `label` is not given. */
  description?: any;
  /** Default 'top'. */
  align?: PopoverAlignment;
  /** Start open (uncontrolled). */
  defaultOpen?: boolean;
  /** Force the tooltip open or closed, bypassing hover/focus; leave undefined for the normal behavior. */
  open?: boolean;
  /** Milliseconds before showing on pointer enter. Default 100. */
  enterDelayMs?: number;
  /** Milliseconds before hiding on pointer leave. Default 300. */
  leaveDelayMs?: number;
  /** Close when the trigger is clicked. Default false. */
  closeOnActivation?: boolean;
  /** Default true (dark surface). */
  highContrast?: boolean;
  /** Default false. */
  dropShadow?: boolean;
  /** Show the caret. Default true. */
  caret?: boolean;
  /** id of the tooltip content (generated when omitted); the trigger points at it. */
  id?: string;
  className?: string;
  /** Exactly one focusable trigger element. */
  children: any;
}

export interface ToggletipProps {
  /** Default 'bottom'. */
  align?: PopoverAlignment;
  /** Start open. */
  defaultOpen?: boolean;
  as?: string;
  className?: string;
  /** A ToggletipButton and a ToggletipContent. */
  children?: any;
}
export interface ToggletipButtonProps {
  /** Accessible name. Default 'Show information'. */
  label?: string;
  className?: string;
  /** Button content; defaults to the 16px information icon. */
  children?: any;
}
export interface ToggletipContentProps {
  className?: string;
  /** Interactive content, usually <p> text and a ToggletipActions row. */
  children?: any;
}
export interface ToggletipActionsProps {
  className?: string;
  children?: any;
}

export interface IconButtonProps {
  /** Tooltip text; also the accessible name. Required. */
  label: any;
  /** Icon name from the Icons group, rendered at 16px. Alternatively pass an icon element as `children`. */
  renderIcon?: string;
  children?: any;
  /** Default 'primary'. */
  kind?: 'primary' | 'secondary' | 'tertiary' | 'ghost';
  /** Square size: xs 24, sm 32, md 40, lg 48. Default 'lg'. */
  size?: 'xs' | 'sm' | 'md' | 'lg';
  /** Tooltip alignment. Default 'top'. */
  align?: PopoverAlignment;
  disabled?: boolean;
  /** Selected (toggled on) look, `background-selected`. When defined it also sets `aria-pressed`. */
  isSelected?: boolean;
  /** Renders a BadgeIndicator inside the button: a count, or a dot when 0. Intended for ghost, lg. */
  badgeCount?: number;
  defaultOpen?: boolean;
  /** Force the tooltip open or closed. */
  open?: boolean;
  /** Default true. */
  closeOnActivation?: boolean;
  /** Default 100. */
  enterDelayMs?: number;
  /** Default 100. */
  leaveDelayMs?: number;
  highContrast?: boolean;
  dropShadow?: boolean;
  /** Extra classes for the tooltip wrapper. */
  wrapperClasses?: string;
  className?: string;
  onClick?: (e: any) => void;
  /** Other attributes (`id`, `aria-*`, `data-*`, `aria-pressed`, handlers) go on the `<button>`. */
  [attr: string]: any;
}

export interface CopyButtonProps {
  /** Accessible name and tooltip before copying. Default 'Copy to clipboard'. */
  iconDescription?: string;
  /** Tooltip text after a click. Default 'Copied!'. */
  feedback?: string;
  /** Milliseconds the feedback stays visible. Default 2000. */
  feedbackTimeout?: number;
  /** Tooltip alignment. Default 'bottom'. */
  align?: PopoverAlignment;
  /** Square size: sm 32, md 40 (default), lg 48. */
  size?: 'sm' | 'md' | 'lg';
  /** Start in the feedback state (shows the feedback label until the next click cycle). */
  defaultCopied?: boolean;
  /** Start with the tooltip (the `iconDescription` label) open. */
  defaultOpen?: boolean;
  /** Force the tooltip open. */
  open?: boolean;
  disabled?: boolean;
  /** Called on click; copying to the clipboard is up to the caller. The feedback text is also announced through a polite live region. */
  onClick?: (e: any) => void;
  className?: string;
}

export type IconIndicatorKind = 'failed' | 'caution-major' | 'caution-minor' | 'undefined' | 'succeeded' | 'normal' | 'in-progress' | 'incomplete' | 'not-started' | 'pending' | 'unknown' | 'informative';
export interface IconIndicatorProps {
  kind: IconIndicatorKind;
  /** Text next to the glyph. */
  label: string;
  /** Glyph size in px. Default 16. */
  size?: 16 | 20;
  className?: string;
}

export type ShapeIndicatorKind = 'failed' | 'critical' | 'high' | 'medium' | 'low' | 'cautious' | 'undefined' | 'stable' | 'informative' | 'incomplete' | 'draft';
export interface ShapeIndicatorProps {
  kind: ShapeIndicatorKind;
  /** Text next to the shape. */
  label: string;
  /** Label type size in px (the shape is always 16). Default 12. */
  textSize?: 12 | 14;
  className?: string;
}

export interface BadgeIndicatorProps {
  /** Omit for a plain dot. Values above 999 show '999+'. */
  count?: number;
  id?: string;
  className?: string;
}

// ---- E
export type LayerLevelValue = 1 | 2 | 3;
/** Names for the three layers: one = 1, two = 2, three = 3. */
export const LayerLevel: { one: 1; two: 2; three: 3 };

/** Level (1-3) of the nearest enclosing Layer; 1 outside any Layer or directly inside a Theme zone. */
export function useLayer(): { level: LayerLevelValue };

export interface LayerProps {
  /** Layer this wrapper establishes (1, 2 or 3). Default: one step above the enclosing Layer (a Layer on the page is level 2, one inside it is 3, then it stays at 3). */
  level?: LayerLevelValue;
  /** Also paint the layer-background token for this level as the wrapper background. */
  withBackground?: boolean;
  /** Element or component to render. Default 'div'. */
  as?: unknown;
  className?: string;
  children?: unknown;
}

export interface StackProps {
  /** 'vertical' (default) lays children in a column, 'horizontal' in a row. */
  orientation?: 'vertical' | 'horizontal';
  /** Spacing scale step 1-13 (0.125, 0.25, 0.5, 0.75, 1, 1.5, 2, 2.5, 3, 4, 5, 6, 10 rem) or any CSS length string. */
  gap?: number | string;
  /** Element or component to render. Default 'div'. */
  as?: unknown;
  className?: string;
  style?: Record<string, unknown>;
  children?: unknown;
}

export type ColumnSpanValue = boolean | number | '25%' | '50%' | '75%' | '100%' | { span?: number | '25%' | '50%' | '75%' | '100%'; offset?: number; start?: number; end?: number };

export interface GridProps {
  /** Pulls the grid to the start or end edge instead of centring it. */
  align?: 'start' | 'center' | 'end';
  /** 1px gutters between columns. */
  condensed?: boolean;
  /** Removes the gutter before each column (columns touch the left edge). */
  narrow?: boolean;
  /** Lifts the 99rem maximum width. */
  fullWidth?: boolean;
  as?: unknown;
  className?: string;
  children?: unknown;
}
/** FlexGrid takes condensed, narrow, fullWidth, as, className; it pairs with Row and Column (flex mode). */
export interface FlexGridProps {
  condensed?: boolean;
  narrow?: boolean;
  fullWidth?: boolean;
  as?: unknown;
  className?: string;
  children?: unknown;
}
export interface RowProps {
  condensed?: boolean;
  narrow?: boolean;
  as?: unknown;
  className?: string;
  children?: unknown;
}
export interface ColumnProps {
  /** Span at the sm breakpoint (4 columns, from 20rem). A number spans that many columns, true means auto, a string like '50%' a fraction, an object adds offset/start/end. */
  sm?: ColumnSpanValue;
  /** md: 8 columns, from 42rem. */
  md?: ColumnSpanValue;
  /** lg: 16 columns, from 66rem. */
  lg?: ColumnSpanValue;
  /** xlg: 16 columns, from 82rem. */
  xlg?: ColumnSpanValue;
  /** max: 16 columns, from 99rem. */
  max?: ColumnSpanValue;
  /** Span at every breakpoint (CSS Grid only). */
  span?: number | '25%' | '50%' | '75%' | '100%' | { span?: number; start?: number; end?: number };
  as?: unknown;
  className?: string;
  children?: unknown;
}

export interface ButtonSetProps {
  /** Stack the buttons vertically. */
  stacked?: boolean;
  /** Fluid layout: right-aligned, buttons ordered by kind, stacks itself when the container is narrow. */
  fluid?: boolean;
  className?: string;
  children?: unknown;
}

export interface AspectRatioProps {
  /** Default '1x1'. */
  ratio?: '16x9' | '9x16' | '2x1' | '1x2' | '4x3' | '3x4' | '3x2' | '2x3' | '1x1';
  as?: unknown;
  className?: string;
  children?: unknown;
}

export interface SelectableTileProps {
  /** Selected state (syncs when the prop changes). */
  selected?: boolean;
  disabled?: boolean;
  id?: string;
  title?: string;
  tabIndex?: number;
  /** Deprecated Carbon prop: use a Layer instead. Uses layer-02. */
  light?: boolean;
  /** Node shown at the top-right, left of the checkbox (usually an AILabel). Adds `cds--tile--decorator`; with an AILabel the tile gets the AI gradient and 4.5rem end padding. Clicks and keys inside the decorator do not toggle the tile. */
  decorator?: unknown;
  onChange?: (event: unknown, selected: boolean, id: string) => void;
  onClick?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  className?: string;
  children?: unknown;
}

export interface ExpandableTileProps {
  /** Expanded state (syncs when the prop changes). */
  expanded?: boolean;
  /** Accessible text while collapsed. Default 'Interact to expand Tile'. */
  tileCollapsedIconText?: string;
  /** Accessible text while expanded. Default 'Interact to collapse Tile'. */
  tileExpandedIconText?: string;
  /** Visible text beside the chevron while collapsed (non-interactive variant), label-01 in text-secondary. */
  tileCollapsedLabel?: string;
  /** Visible text beside the chevron while expanded (non-interactive variant). */
  tileExpandedLabel?: string;
  /** Deprecated Carbon prop: use a Layer instead. */
  light?: boolean;
  /** Node pinned 1rem from the top-right (usually an AILabel). Adds `cds--tile--decorator` (AI gradient with an AILabel) and makes the tile the interactive variant. */
  decorator?: unknown;
  tabIndex?: number;
  onClick?: (event: unknown) => void;
  onKeyUp?: (event: unknown) => void;
  className?: string;
  /** Exactly two children: TileAboveTheFoldContent and TileBelowTheFoldContent. */
  children?: unknown;
}
export interface TileAboveTheFoldContentProps { children?: unknown; }
export interface TileBelowTheFoldContentProps { children?: unknown; }

export interface RadioTileProps {
  /** Value reported to onChange; required. */
  value: string | number;
  /** Set by TileGroup; set it yourself only when using RadioTile alone. */
  checked?: boolean;
  name?: string;
  disabled?: boolean;
  required?: boolean;
  id?: string;
  tabIndex?: number;
  light?: boolean;
  /** Node at the top-right, left of the check mark (usually an AILabel). Adds `cds--tile--decorator`. */
  decorator?: unknown;
  onChange?: (value: string | number, name: string | undefined, event: unknown) => void;
  className?: string;
  children?: unknown;
}

export interface TileGroupProps {
  /** Shared radio name; required. */
  name: string;
  /** Visible legend (renders a label above the tiles). */
  legend?: string;
  /** Initially selected value (uncontrolled). */
  defaultSelected?: string | number;
  /** Selected value (controlled). */
  valueSelected?: string | number;
  /** Disables every RadioTile inside. */
  disabled?: boolean;
  required?: boolean;
  onChange?: (selection: string | number, name: string, event: unknown) => void;
  className?: string;
  /** RadioTile elements, optionally wrapped in layout elements. */
  children?: unknown;
}

export interface ListItemProps { className?: string; children?: unknown; }
export interface OrderedListProps {
  /** Mark a list inside a ListItem: indented 2rem with lower-latin markers. */
  nested?: boolean;
  /** Use the browser's decimal markers instead of the counter. */
  native?: boolean;
  /** 1rem / 1.5 line height instead of 0.875rem / 1.42857. */
  isExpressive?: boolean;
  className?: string;
  children?: unknown;
}
export interface UnorderedListProps {
  nested?: boolean;
  isExpressive?: boolean;
  className?: string;
  children?: unknown;
}

// ---- G1
export type AILabelAlignment =
  | 'top' | 'top-start' | 'top-end' | 'bottom' | 'bottom-start' | 'bottom-end'
  | 'left' | 'left-start' | 'left-end' | 'right' | 'right-start' | 'right-end';

export interface AILabelProps {
  /** Text shown in the button, translated as needed. Default 'AI'. */
  aiText?: string;
  /** Extra text shown after the AI text when `kind` is 'inline' (renders the bordered "inline with content" style). */
  textLabel?: string;
  /** @deprecated Use `textLabel`. */
  aiTextLabel?: string;
  /** Accessible name suffix: the button is labelled `${aiText} ${aria-label}` (or `${aiText} ${textLabel}`). Default 'Show information'. */
  'aria-label'?: string;
  /** @deprecated Use `aria-label`. */
  slugLabel?: string;
  /** 'default' is the square bordered "AI" button; 'inline' is a dot + text with no box. Default 'default'. */
  kind?: 'default' | 'inline';
  /** Button size: mini 16px, 2xs 20px, xs 24px, sm 32px, md 40px, lg 48px, xl 64px. Inline uses sm, md or lg. Default 'xs'. */
  size?: 'mini' | '2xs' | 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  /** Callout position relative to the button. Default 'bottom'. No auto-alignment. */
  align?: AILabelAlignment;
  /** Controlled open state of the callout. */
  open?: boolean;
  /** Initial open state when uncontrolled. */
  defaultOpen?: boolean;
  /** Called with the next open state when the button is toggled, Escape is pressed or the user clicks outside. */
  onOpenChange?: (open: boolean) => void;
  /** Replace the AI button with a ghost "revert" icon button (undo icon). */
  revertActive?: boolean;
  /** Tooltip / accessible name of the revert button. Default 'Revert to AI input'. */
  revertLabel?: string;
  /** Called when the revert button is clicked. */
  onRevertClick?: (event: any) => void;
  className?: string;
  /** An AILabelContent element (the callout). */
  children?: any;
}

export interface AILabelContentProps {
  className?: string;
  /** Callout body (explainability text). An AILabelActions child is docked to the bottom edge. */
  children?: any;
}

export interface AILabelActionsProps {
  className?: string;
  /** Toolbar buttons: ghost IconButtons plus at most one primary Button (placed last, rounded bottom-right corner). */
  children?: any;
}

export interface AISkeletonTextProps {
  /** Render several lines (widths vary unless `width` is set). */
  paragraph?: boolean;
  /** Number of lines when `paragraph`. Default 3. */
  lineCount?: number;
  /** Line width (CSS length or %). Default '100%'. */
  width?: string;
  /** 24px-tall heading line instead of 16px. */
  heading?: boolean;
  className?: string;
}

export interface AISkeletonPlaceholderProps {
  /** Size it with a class or inline style; default 100 x 100px. */
  className?: string;
  style?: any;
}

export interface AISkeletonIconProps {
  /** 16 x 16px by default; resize with style. */
  className?: string;
  style?: any;
}

export interface ChatButtonProps {
  /** Button label. */
  children?: any;
  /** primary | secondary | tertiary | ghost | danger. Default 'primary'. Forced to 'ghost' when `isQuickAction`. */
  kind?: 'primary' | 'secondary' | 'tertiary' | 'ghost' | 'danger';
  /** sm 32px, md 40px, lg 48px. Default 'lg'. Forced to 'sm' when `isQuickAction`. Other values log an error and fall back to 'lg'. */
  size?: 'sm' | 'md' | 'lg';
  /** Outlined pill used for suggested prompts in a chat. */
  isQuickAction?: boolean;
  /** Selected state of a quick action. */
  isSelected?: boolean;
  disabled?: boolean;
  /** Icon file stem shown after the label (e.g. 'add'). */
  renderIcon?: string;
  onClick?: (event: any) => void;
  type?: 'button' | 'submit' | 'reset';
  className?: string;
  /** Other button attributes (id, aria-*, title, ...) are passed to the <button>. */
  [attr: string]: any;
}

// ---- G2
export interface FluidFormProps {
  /** Fluid fields (FluidTextInput, FluidSelect, ...) laid out edge to edge; wrap side-by-side fields in a flex row. */
  children?: unknown;
  className?: string;
  /** Any other <form> attribute (onSubmit, aria-label, ...) is passed through. */
  [attr: string]: unknown;
}

export interface FluidTextInputProps {
  id?: string;
  /** Label shown inside the field, top-left. Required. */
  labelText: unknown;
  placeholder?: string;
  value?: string | number;
  defaultValue?: string | number;
  onChange?: (event: unknown) => void;
  onClick?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  autoComplete?: string;
  pattern?: string;
  required?: boolean;
  /** Input type for the non-password variant. Default 'text'. */
  type?: string;
  name?: string;
  disabled?: boolean;
  readOnly?: boolean;
  /** Shows invalidText in the field's bottom row with a divider and a 2px support-error outline. Ignored when disabled/readOnly. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Shows warnText in the field's bottom row with a divider. Ignored when invalid, disabled or readOnly. */
  warn?: boolean;
  warnText?: unknown;
  /** Show the n/max counter at the top-right and set maxLength. */
  enableCounter?: boolean;
  maxCount?: number;
  /** Renders the password variant with a show/hide toggle. */
  isPassword?: boolean;
  /** Password toggle labels. Defaults 'Show password' / 'Hide password'. */
  showPasswordLabel?: string;
  hidePasswordLabel?: string;
  onTogglePasswordVisibility?: (event: unknown) => void;
  className?: string;
}

export interface FluidTextAreaProps {
  id?: string;
  labelText: unknown;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onClick?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  required?: boolean;
  /** Visible rows. Default 4. */
  rows?: number;
  enableCounter?: boolean;
  maxCount?: number;
  /** 'character' (default, sets maxLength) or 'word'. */
  counterMode?: 'character' | 'word';
  disabled?: boolean;
  readOnly?: boolean;
  /** Error message row under a divider, 2px support-error border. Ignored when disabled/readOnly. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Warning message row. Ignored when invalid, disabled or readOnly. */
  warn?: boolean;
  warnText?: unknown;
  name?: string;
  className?: string;
}

export interface FluidNumberInputProps {
  id?: string;
  /** Label shown inside the field. `labelText` is accepted as an alias. */
  label?: unknown;
  labelText?: unknown;
  placeholder?: string;
  value?: number | '';
  defaultValue?: number | '';
  /** Called with (event, { value, direction: 'up' | 'down' }). */
  onChange?: (event: unknown, state: { value: number | ''; direction: 'up' | 'down' }) => void;
  onFocus?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  /** Called after a stepper click with (event, { value, direction }). */
  onClick?: (event: unknown, state: { value: number | ''; direction: 'up' | 'down' }) => void;
  min?: number;
  max?: number;
  /** Stepper increment. Default 1. */
  step?: number;
  /** Allow an empty value without flagging it invalid. */
  allowEmpty?: boolean;
  hideSteppers?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  /** Invalid is also set automatically when the value is outside min/max. */
  invalid?: boolean;
  /** Default 'Number is not valid'. */
  invalidText?: unknown;
  warn?: boolean;
  warnText?: unknown;
  incrementNumLabel?: string;
  decrementNumLabel?: string;
  name?: string;
  className?: string;
}

export interface FluidSearchProps {
  id?: string;
  /** Visible label inside the field (unlike Search, where it is visually hidden). */
  labelText: unknown;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  /** Called after the clear button (or Escape) empties the field. */
  onClear?: () => void;
  onKeyDown?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  /** aria-label of the clear button. Default 'Clear search input'. */
  closeButtonLabelText?: string;
  disabled?: boolean;
  /** Input role. Default 'searchbox'. */
  role?: string;
  /** Input type. Default 'text'. */
  type?: string;
  autoComplete?: string;
  name?: string;
  className?: string;
}

export interface FluidSelectProps {
  id?: string;
  labelText: unknown;
  /** Omit the label element. */
  noLabel?: boolean;
  /** SelectItem / SelectItemGroup children (from the Select component) or plain <option>s. */
  children?: unknown;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  required?: boolean;
  autoComplete?: string;
  disabled?: boolean;
  /** Read-only: the value is shown but cannot be changed (mouse and arrow/space keys are blocked). */
  readOnly?: boolean;
  /** Error message row under a divider, 2px support-error outline. Ignored when disabled/readOnly. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Warning message row. Ignored when invalid, disabled or readOnly. */
  warn?: boolean;
  warnText?: unknown;
  name?: string;
  className?: string;
}

export interface FluidTimePickerSelectProps extends FluidSelectProps {}

export interface FluidTimePickerProps {
  /** id of the time text input. */
  id?: string;
  /** Label of the time text input (the first segment). */
  labelText: unknown;
  /** Default none; Carbon stories use 'hh:mm'. */
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  onChange?: (event: unknown) => void;
  onBlur?: (event: unknown) => void;
  onFocus?: (event: unknown) => void;
  onClick?: (event: unknown) => void;
  onKeyDown?: (event: unknown) => void;
  name?: string;
  pattern?: string;
  required?: boolean;
  autoComplete?: string;
  /** One or two FluidTimePickerSelect children (e.g. AM/PM and time zone). With two the input and first select take 25% each. */
  children?: unknown;
  /** Disables the input and every select. */
  disabled?: boolean;
  /** Makes the input and every select read-only. */
  readOnly?: boolean;
  /** Error for the whole picker: 2px support-error outline, divider and one message row. Ignored when disabled/readOnly. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Warning for the whole picker. Ignored when invalid, disabled or readOnly. */
  warn?: boolean;
  warnText?: unknown;
  className?: string;
}

/** Loading placeholder for a fluid field: a 64px skeleton box (128px for the text area) with a label bar and a value bar.
 *  FluidTextInputSkeleton, FluidTextAreaSkeleton, FluidNumberInputSkeleton, FluidSearchSkeleton and FluidSelectSkeleton take only this. */
export interface FluidSkeletonProps {
  className?: string;
}
export interface FluidTimePickerSkeletonProps {
  /** Two parts split 50/50 (time + one select) instead of three (25% / 25% / 50%). */
  isOnlyTwo?: boolean;
  className?: string;
}

// ---- G3
interface FluidListBoxCommonProps<ItemType> {
  /** Items to choose from: strings, or objects with `label`/`text`/`name`. An object with `disabled: true` cannot be chosen. */
  items: ItemType[];
  /** Maps an item to its text. Defaults to the item itself, or its `label`, `text` or `name`. */
  itemToString?: (item: ItemType | null) => string;
  /** Label drawn inside the top of the 64px field. Required for fluid inputs. */
  titleText: React.ReactNode;
  /** Error state: red outline around field + message, error icon beside the message. Ignored when disabled or readOnly. */
  invalid?: boolean;
  invalidText?: React.ReactNode;
  /** Warning state: warning icon beside the message, message in `text-primary`. Ignored when invalid, disabled or readOnly. */
  warn?: boolean;
  warnText?: React.ReactNode;
  disabled?: boolean;
  readOnly?: boolean;
  /** 40px menu rows instead of the 64px fluid rows. */
  isCondensed?: boolean;
  /** Open the list upwards. */
  direction?: 'bottom' | 'top';
  /** Controlled open state. */
  open?: boolean;
  /** Initial open state when uncontrolled (used by previews). */
  defaultOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  /** Render rich content for each object item. */
  itemToElement?: React.ComponentType<any>;
  id?: string;
  /** Extra class on the `.cds--list-box__wrapper--fluid` wrapper. */
  className?: string;
}

/** Dropdown with its label inside a 64px field, for fluid forms. */
export interface FluidDropdownProps<ItemType = any> extends FluidListBoxCommonProps<ItemType> {
  /** Placeholder text shown in the field while nothing is selected. */
  label: string;
  selectedItem?: ItemType | null;
  initialSelectedItem?: ItemType | null;
  onChange?: (data: { selectedItem: ItemType | null }) => void;
}

/** Filterable combo box with its label inside a 64px field, for fluid forms. */
export interface FluidComboBoxProps<ItemType = any> extends FluidListBoxCommonProps<ItemType> {
  placeholder?: string;
  selectedItem?: ItemType | null;
  initialSelectedItem?: ItemType | null;
  /** Initial text in the input. */
  initialInputValue?: string;
  onChange?: (data: { selectedItem: ItemType | null; inputValue?: string }) => void;
  onInputChange?: (inputValue: string) => void;
  shouldFilterItem?: (args: { item: ItemType; itemToString: (item: ItemType | null) => string; inputValue: string }) => boolean;
  allowCustomValue?: boolean;
}

/** Multi-select (checkbox list, count tag) with its label inside a 64px field, for fluid forms. */
export interface FluidMultiSelectProps<ItemType = any> extends FluidListBoxCommonProps<ItemType> {
  /** Field text (non-filterable) or input placeholder (filterable). */
  label: string;
  /** Render the filterable variant: a text input that filters the list. */
  isFilterable?: boolean;
  placeholder?: string;
  selectedItems?: ItemType[];
  initialSelectedItems?: ItemType[];
  onChange?: (data: { selectedItems: ItemType[] }) => void;
}

/** One input of a FluidDatePicker; label inside the 64px field. Use as a child of FluidDatePicker (one for single, two for range). */
export interface FluidDatePickerInputProps {
  id?: string;
  /** Label drawn inside the top of the field. */
  labelText: React.ReactNode;
  placeholder?: string;
  pattern?: string;
  /** Error on this input only (e.g. one end of a range): outline around this input and its message. Ignored when disabled/readOnly. */
  invalid?: boolean;
  invalidText?: React.ReactNode;
  /** Warning on this input only. */
  warn?: boolean;
  warnText?: React.ReactNode;
  disabled?: boolean;
  readOnly?: boolean;
  /** Inline style on the `<input>` (Carbon stories size the input this way). */
  style?: React.CSSProperties;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  onClick?: (event: React.MouseEvent<HTMLInputElement>) => void;
  onFocus?: (event: React.FocusEvent<HTMLInputElement>) => void;
  onBlur?: (event: React.FocusEvent<HTMLInputElement>) => void;
  onKeyDown?: (event: React.KeyboardEvent<HTMLInputElement>) => void;
  className?: string;
}

/** Date picker for fluid forms: 64px fields with inside labels and an inline calendar. */
export interface FluidDatePickerProps {
  /** `simple` (text only), `single` (one date + calendar) or `range` (two inputs + calendar). Default `single`. */
  datePickerType?: 'simple' | 'single' | 'range';
  /** FluidDatePickerInput elements (one, or two for range). If omitted, inputs are built from `labelText`/`endLabelText`. */
  children?: React.ReactNode;
  /** flatpickr-style format tokens m, n, d, j, Y, y. Default `m/d/Y`. */
  dateFormat?: string;
  /** Controlled dates (Date, string in `dateFormat`, ISO string or timestamp; an array for range). */
  value?: Date | string | number | Array<Date | string | number>;
  defaultValue?: Date | string | number | Array<Date | string | number>;
  /** Called with the selected dates. */
  onChange?: (dates: Date[]) => void;
  minDate?: Date | string;
  maxDate?: Date | string;
  /** Controlled calendar open state. */
  open?: boolean;
  /** Initial calendar open state (used by previews). */
  defaultOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  /** Error for the whole picker: outline around all inputs, divider, one message below. Ignored when disabled/readOnly. */
  invalid?: boolean;
  invalidText?: React.ReactNode;
  /** Warning for the whole picker. */
  warn?: boolean;
  warnText?: React.ReactNode;
  readOnly?: boolean;
  disabled?: boolean;
  /** Fallback labels/placeholders when no children are given. */
  labelText?: React.ReactNode;
  endLabelText?: React.ReactNode;
  placeholder?: string;
  endPlaceholder?: string;
  id?: string;
  className?: string;
  /** Inline style on the outer `.cds--date-picker--fluid` element (it is inline-flex; set a width). */
  style?: React.CSSProperties;
}

/** Loading placeholder for FluidDropdown, FluidComboBox and FluidMultiSelect (FluidDropdownSkeleton, FluidComboBoxSkeleton,
 *  FluidMultiSelectSkeleton): a 64px skeleton field with a label bar and a value bar. */
export interface FluidListBoxSkeletonProps {
  className?: string;
}
/** Loading placeholder for FluidDatePicker: one 64px skeleton field, two side by side for `range`. */
export interface FluidDatePickerSkeletonProps {
  /** Default `single`. */
  datePickerType?: 'simple' | 'single' | 'range';
  className?: string;
}

// ---- G4
export interface FormProps {
  /** Form fields, usually FormGroup / FormItem / inputs stacked with Stack. */
  children?: unknown;
  /** Extra class on the `<form class="cds--form">`. */
  className?: string;
  /** Any other native `<form>` attribute (onSubmit, noValidate, aria-label...) is passed through. */
  [attr: string]: unknown;
}

export interface FormGroupProps {
  /** Text of the `<legend class="cds--label">`; becomes the fieldset's accessible name. Required. */
  legendText: unknown;
  /** Form controls inside the `<fieldset class="cds--fieldset">`. */
  children?: unknown;
  /** Id for the legend (also used as the fieldset's aria-labelledby). Generated when omitted. */
  legendId?: string;
  /** Disables the native fieldset: every nested control is disabled and labels/helper text turn text-disabled. */
  disabled?: boolean;
  /** Marks the fieldset `data-invalid`; the message (if shown) uses text-error. */
  invalid?: boolean;
  /** Show the `.cds--form__requirements` message under the controls. */
  message?: boolean;
  /** Text of that message (wired to the fieldset with aria-describedby). */
  messageText?: string;
  className?: string;
  [attr: string]: unknown;
}

export interface FormLabelProps {
  /** Label content. */
  children?: unknown;
  /** Id of the control this label is for (rendered as `htmlFor`). */
  id?: string;
  className?: string;
  [attr: string]: unknown;
}

export interface FormItemProps {
  /** Content of the `.cds--form-item` column (label, control, helper/error text). */
  children?: unknown;
  className?: string;
  [attr: string]: unknown;
}

export interface CheckboxGroupProps {
  /** Legend text (`legend.cds--label`). Required. */
  legendText: unknown;
  /** Checkbox elements (Carbon.Checkbox). */
  children?: unknown;
  /** 'vertical' (default) stacks the boxes 8px apart; 'horizontal' places them in a wrapping row 16px apart. */
  orientation?: 'vertical' | 'horizontal';
  /** Helper text under the group (hidden while invalid or warn). */
  helperText?: unknown;
  /** Red box borders and the invalid message with a warning--filled icon. */
  invalid?: boolean;
  invalidText?: unknown;
  /** Warning message with a warning-alt icon (ignored when invalid or readOnly). */
  warn?: boolean;
  warnText?: unknown;
  /** Boxes keep focus but cannot be toggled; drawn with icon-disabled borders and a text-primary checkmark. */
  readOnly?: boolean;
  /** Native fieldset disable: every checkbox, the legend and helper text become disabled. */
  disabled?: boolean;
  /** Id for the legend / fieldset aria-labelledby. */
  legendId?: string;
  /** Node shown after the legend text (e.g. an AI label). */
  decorator?: unknown;
  className?: string;
}

export type HeadingLevel = 1 | 2 | 3 | 4 | 5 | 6;

export interface SectionProps {
  /** Element to render. Default 'section'. */
  as?: unknown;
  /** Force the heading level for this section's Headings instead of parent + 1. */
  level?: HeadingLevel;
  children?: unknown;
  className?: string;
  [attr: string]: unknown;
}

export interface HeadingProps {
  /** Heading text. Renders h1 outside any Section, h2 inside one Section, and so on up to h6. Unstyled: add a cds--type-* class. */
  children?: unknown;
  className?: string;
  [attr: string]: unknown;
}

export interface TextProps {
  /** Element to render. Default 'span'. */
  as?: unknown;
  /** Text direction. Default 'auto'; a nested Text only writes `dir` when it differs from its parent's. */
  dir?: 'ltr' | 'rtl' | 'auto';
  children?: unknown;
  className?: string;
  [attr: string]: unknown;
}

export interface TextDirectionProps {
  /** Direction for every Text inside. Default 'auto'. */
  dir?: 'ltr' | 'rtl' | 'auto';
  /** Called with each nested Text's string children; return 'ltr' | 'rtl' | 'auto' to override its direction. */
  getTextDirection?: (text: string | string[] | undefined) => 'ltr' | 'rtl' | 'auto';
  children?: unknown;
}

export interface GlobalThemeProps {
  /** Theme reported by useTheme() for the whole app. Does not paint anything: set data-theme on <html> yourself. */
  theme?: 'white' | 'g10' | 'g90' | 'g100';
  /** A single element (it receives the forwarded ref). */
  children?: unknown;
}

export interface ThemeProps {
  /** Zone theme: sets data-theme and class cds--<theme> on the wrapper, re-binding every color token and painting background / text-primary. */
  theme?: 'white' | 'g10' | 'g90' | 'g100';
  /** Element to render. Default 'div'. */
  as?: unknown;
  children?: unknown;
  className?: string;
  [attr: string]: unknown;
}

/** Returns the closest Theme / GlobalTheme value: `{ theme, isDark }` (isDark is true for g90 / g100). Default `{ theme: 'white', isDark: false }`. */
export function useTheme(): { theme?: 'white' | 'g10' | 'g90' | 'g100'; isDark?: boolean };

// ---- G5
export type G5TagSize = 'sm' | 'md' | 'lg';
export type G5TagColor = 'red' | 'magenta' | 'purple' | 'blue' | 'cyan' | 'teal' | 'green' | 'gray' | 'cool-gray' | 'warm-gray';
export type G5Align =
  | 'top' | 'top-start' | 'top-end' | 'bottom' | 'bottom-start' | 'bottom-end'
  | 'left' | 'left-start' | 'left-end' | 'right' | 'right-start' | 'right-end';

export interface DismissibleTagProps {
  /** Label text. Truncated with an ellipsis past 13rem. */
  text?: string;
  /** Color. Default 'gray'. Also 'high-contrast' and 'outline'. */
  type?: G5TagColor | 'high-contrast' | 'outline';
  /** Height 18/24/32px. Default 'md'. */
  size?: G5TagSize;
  /** Disables the close button and greys the tag out. */
  disabled?: boolean;
  /** Called when the close button is clicked. The consumer removes the tag. */
  onClose?: (event: any) => void;
  /** Tooltip of the close button. Default 'Dismiss'. The button's accessible name is this followed by `text` ("Dismiss us-east"). */
  title?: string;
  /** Close label used when the text is truncated. Default 'Dismiss tag'. */
  dismissTooltipLabel?: string;
  /** Alignment of the close button tooltip. Default 'bottom'. */
  dismissTooltipAlignment?: G5Align;
  /** Native title on the label (defaults to `text`). */
  tagTitle?: string;
  /** Icon file stem shown before the label (not at size sm). */
  renderIcon?: string;
  /** Element rendered after the label (e.g. a small AI label). */
  decorator?: any;
  id?: string;
  className?: string;
}

export interface OperationalTagProps {
  /** Label text. When truncated, a tooltip shows the full text. */
  text?: string;
  /** Color. Default 'gray'. */
  type?: G5TagColor;
  /** Height 18/24/32px. Default 'md'. */
  size?: G5TagSize;
  disabled?: boolean;
  /** Called when the tag is clicked (e.g. to open a popover listing more items). */
  onClick?: (event: any) => void;
  /** Icon file stem shown before the label (not at size sm). */
  renderIcon?: string;
  id?: string;
  className?: string;
}

export interface SelectableTagProps {
  /** Label text. */
  text?: string;
  /** Height 18/24/32px. Default 'md'. */
  size?: G5TagSize;
  /** Controlled selected state. */
  selected?: boolean;
  /** Initial selected state when uncontrolled. Default false. */
  defaultSelected?: boolean;
  /** Called with the new selected value on click. */
  onChange?: (selected: boolean) => void;
  onClick?: (event: any) => void;
  disabled?: boolean;
  /** Icon file stem shown before the label (not at size sm). */
  renderIcon?: string;
  id?: string;
  className?: string;
}

export interface CalloutProps {
  /** Status. Carbon recommends 'info' and 'warning'; 'success' and 'error' (and -alt/-square) still render. Default 'info'. */
  kind?: 'info' | 'warning' | 'success' | 'error' | 'info-square' | 'warning-alt';
  /** Bold title, shown inline before the subtitle. */
  title?: string;
  /** id for the title element; give interactive children aria-describedby with this id. */
  titleId?: string;
  /** Body text. */
  subtitle?: any;
  /** Extra content after the subtitle, e.g. a Link. */
  children?: any;
  /** Label of the ghost action button at the end. */
  actionButtonLabel?: string;
  onActionButtonClick?: (event: any) => void;
  /** Low-contrast (tinted) style. Default true here; false gives the inverse dark style. */
  lowContrast?: boolean;
  /** Accessible name of the status icon. Default '<kind> icon'. */
  statusIconDescription?: string;
  className?: string;
}

export interface DataTableSkeletonProps {
  /** Number of body rows. Default 5. */
  rowCount?: number;
  /** Number of columns. Default 5. */
  columnCount?: number;
  /** Real header labels instead of header skeleton bars. */
  headers?: { header: any; key?: string }[];
  /** Title/description skeleton above the table. Default true. */
  showHeader?: boolean;
  /** Toolbar row with a button skeleton. Default true. */
  showToolbar?: boolean;
  /** Alternate row backgrounds. */
  zebra?: boolean;
  /** Adds Carbon's legacy compact class (no v11 styles). */
  compact?: boolean;
  className?: string;
}

export interface SkeletonIconProps {
  /** Extra class; size the placeholder with CSS for icons other than 16px. */
  className?: string;
  style?: any;
}

export interface DefinitionTooltipProps {
  /** The term; rendered as a button with a dotted underline. */
  children: any;
  /** The definition shown in the tooltip. */
  definition: any;
  /** Deprecated alias of `definition`; wins when both are given. */
  tooltipText?: any;
  /** Called on keydown on the term (after Escape handling). */
  onKeyDown?: (event: any) => void;
  /** Tooltip position. Default 'bottom'. */
  align?: G5Align;
  /** Also open on mouse hover (always opens on focus and click). */
  openOnHover?: boolean;
  /** Open on first render. */
  defaultOpen?: boolean;
  /** id of the tooltip content. */
  id?: string;
  /** Class on the popover container. */
  className?: string;
  /** Class on the trigger button. */
  triggerClassName?: string;
}

// ---- G6
/** Icon for PageHeader parts: an icon file stem ("bee"), a React element, or a component. */
export type PageHeaderIcon = string | unknown;

export interface PageHeaderProps {
  /** BreadcrumbBar, Content, TabBar (or a Grid holding them plus a HeroImage). */
  children?: unknown;
  className?: string;
}

export interface PageHeaderBreadcrumbBarProps {
  /** Breadcrumb to render (e.g. Carbon.Breadcrumb). */
  children?: unknown;
  /** Draw the 1px border-subtle-01 bottom border. Default true. */
  border?: boolean;
  /** 16px icon shown before the breadcrumb. */
  renderIcon?: PageHeaderIcon;
  /** Actions shown to the left of pageActions (e.g. a md Button). */
  contentActions?: unknown;
  /** Remove the 12px gap between contentActions and pageActions. */
  contentActionsFlush?: boolean;
  /** Right-aligned actions, usually md ghost icon-only buttons. */
  pageActions?: unknown;
  /** Remove the grid's end margin/gutter so pageActions sit flush with the page edge. */
  pageActionsFlush?: boolean;
  className?: string;
}

export interface PageHeaderContentProps {
  /** Page title (heading-04 style, clamps at 2 lines, or 1 line with contextualActions; a clipped title gets a native tooltip). */
  title: string;
  /** 32px icon shown before the title. */
  renderIcon?: PageHeaderIcon;
  /** Elements beside the title, e.g. a Tag. */
  contextualActions?: unknown;
  /** Right-aligned actions; usually a PageHeaderContentPageActions. */
  pageActions?: unknown;
  /** Usually a PageHeaderContentText. */
  children?: unknown;
  className?: string;
}

export interface PageHeaderAction {
  id: string;
  /** Element to render (e.g. an IconButton or Button). */
  body: unknown;
  /** Click handler merged into body. */
  onClick?: () => void;
  /** Accepted for Carbon compatibility; unused because actions never collapse into a menu here. */
  menuItem?: { label: string };
}

export interface PageHeaderContentPageActionsProps {
  /** Array of action configs, or any node. */
  actions?: PageHeaderAction[] | unknown;
  /** Accepted for Carbon compatibility; the overflow MenuButton is not implemented. */
  menuButtonLabel?: string;
  className?: string;
}

export interface PageHeaderContentTextProps {
  /** Subtitle above the body text (heading-03 style). */
  subtitle?: string;
  /** Body text (body-01, max 40rem wide). */
  children?: unknown;
  className?: string;
}

export interface PageHeaderHeroImageProps {
  /** An img or picture; the box is 2x1 at >= 66rem viewport and 3x2 below. */
  children?: unknown;
  className?: string;
}

export interface PageHeaderTab {
  label: string;
  disabled?: boolean;
  id?: string;
  /** id of the panel this tab controls (aria-controls). */
  panelId?: string;
}

export interface PageHeaderTagItem {
  type: string;
  text: string;
  size?: 'sm' | 'md' | 'lg';
  id?: string;
}

export interface PageHeaderTabBarProps {
  /** Tabs to render as a Carbon tab strip (line tabs). Use with selectedIndex/onChange to switch the page content below the header. */
  tabs?: PageHeaderTab[];
  /** Controlled selected tab index. */
  selectedIndex?: number;
  /** Initial selected tab index when uncontrolled. Default 0. */
  defaultSelectedIndex?: number;
  /** Called with { selectedIndex } on click and arrow/Home/End keys. */
  onChange?: (e: { selectedIndex: number }) => void;
  /** aria-label of the tablist. Default 'Page tabs'. */
  ariaLabel?: string;
  /** Tags aligned to the end of the tab row. */
  tags?: PageHeaderTagItem[];
  /** Alternative to `tabs`: any tab strip (e.g. Carbon.Tabs; its panels are hidden inside the bar). */
  children?: unknown;
  className?: string;
}

export interface DialogProps {
  /** Show the dialog. Default false. */
  open?: boolean;
  /** true: showModal() (top layer, backdrop, inert page). false: show() (non-modal). */
  modal?: boolean;
  /** Called on backdrop click (modal), on Escape (modal and non-modal), and should be wired to close buttons. */
  onRequestClose?: (e: unknown) => void;
  /** Escape: the native cancel event (modal) or the keydown event (non-modal). Call e.preventDefault() to stop onRequestClose. */
  onCancel?: (e: unknown) => void;
  /** Native close event. */
  onClose?: (e: unknown) => void;
  onClick?: (e: unknown) => void;
  /** Element to focus when the dialog goes from open to closed (not on a first render that starts closed). */
  focusAfterCloseRef?: { current: unknown };
  /** Use 'alertdialog' for interruptive confirmations. */
  role?: '' | 'alertdialog';
  ariaLabel?: string;
  /** Defaults to the DialogTitle id. */
  ariaLabelledBy?: string;
  ariaDescribedBy?: string;
  /** Bundle addition: lay a non-modal dialog out in the document flow instead of fixed and centered. */
  inline?: boolean;
  /** Bundle addition on the Modal size scale. Max width of the viewport at md / lg / xlg breakpoints: xs 48/32/24%, sm 60/42/36%, md 84/60/48% (default, 48rem wide), lg 96/84/72%. Inline: 24, 36, 48, 72rem. */
  size?: 'xs' | 'sm' | 'md' | 'lg';
  className?: string;
  children?: unknown;
}
export interface DialogHeaderProps { children?: unknown; className?: string; }
export interface DialogControlsProps { children?: unknown; className?: string; }
export interface DialogCloseButtonProps {
  onClick?: (e: unknown) => void;
  /** Accessible name and tooltip. Default 'Close'. */
  label?: string;
}
export interface DialogTitleProps { children?: unknown; id?: string; className?: string; }
export interface DialogSubtitleProps { children?: unknown; id?: string; className?: string; }
export interface DialogBodyProps {
  children?: unknown;
  /** Force the scrolling treatment (fade mask, focusable region labelled by the DialogTitle); otherwise detected from overflow. */
  hasScrollingContent?: boolean;
  className?: string;
}
export interface DialogFooterProps {
  /** Custom buttons; replaces the generated ones. */
  children?: unknown;
  /** Default 'Save'. */
  primaryButtonText?: unknown;
  primaryButtonDisabled?: boolean;
  /** Default 'Cancel'; pass '' to hide. */
  secondaryButtonText?: unknown;
  /** Up to two secondary buttons (three-button footer). */
  secondaryButtons?: { buttonText: unknown; onClick: (e: unknown) => void }[];
  onRequestSubmit?: (e: unknown) => void;
  /** Secondary click; falls back to onRequestClose. */
  onSecondarySubmit?: (e: unknown) => void;
  onRequestClose?: (e: unknown) => void;
  /** Primary becomes kind danger and the secondary button takes focus on open. */
  danger?: boolean;
  loadingStatus?: 'inactive' | 'active' | 'finished' | 'error';
  loadingDescription?: string;
  loadingIconDescription?: string;
  onLoadingSuccess?: () => void;
  className?: string;
}

// ---- G7
type IconRef = string | (() => any) | any;

/** A header navigation entry with a chevron that opens a sub-menu of `HeaderMenuItem` links below it (`li.cds--header__submenu`). Place inside `HeaderNavigation` or `HeaderSideNavItems`. */
export interface HeaderMenuProps {
  /** Visible label of the trigger. */
  menuLinkName: string;
  /** Accessible name of the trigger and the sub-menu list. */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  /** Marks the menu as the current section (3px `border-interactive` underline). Also applied automatically while closed when a child `HeaderMenuItem` has `isActive`. */
  isActive?: boolean;
  /** @deprecated use `isActive` */
  isCurrentPage?: boolean;
  /** Controlled open state. */
  expanded?: boolean;
  /** Initial open state when uncontrolled. Default false. */
  defaultExpanded?: boolean;
  /** Called with the new open state. */
  onToggle?: (expanded: boolean) => void;
  /** Replaces the chevron with your own component. */
  renderMenuContent?: () => any;
  /** Ref (function or object) to the trigger anchor. */
  focusRef?: any;
  onBlur?: (event: any) => void;
  onClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
  tabIndex?: number;
  /** Class on the `<li>`. */
  className?: string;
  /** `HeaderMenuItem` elements. */
  children?: any;
}

/** A 256px panel pinned to the right edge under the 48px header (`div.cds--header-panel`); usually holds a `Switcher`. Escape, blur and (for a Switcher) outside clicks close it. */
export interface HeaderPanelProps {
  /** Controlled open state. */
  expanded?: boolean;
  /** Initial open state when uncontrolled. */
  defaultExpanded?: boolean;
  /** Close on blur and Escape. Default true. */
  addFocusListeners?: boolean;
  /** Called when the panel asks to close (blur, Escape, outside click); toggle your `expanded` state here. */
  onHeaderPanelFocus?: () => void;
  /** Where focus returns on Escape, usually `#<id of the HeaderGlobalAction>`. */
  href?: string;
  /** Names the panel; with a label and no `role`, the panel gets `role="region"`. */
  'aria-label'?: string;
  'aria-labelledby'?: string;
  role?: string;
  className?: string;
  children?: any;
}

/** A vertical list of product or app links (`ul.cds--switcher`) with ArrowUp/ArrowDown navigation between items. */
export interface SwitcherProps {
  'aria-label'?: string;
  'aria-labelledby'?: string;
  /** Pass the panel's open state: items are tabbable only when true. */
  expanded?: boolean;
  className?: string;
  /** `SwitcherItem` and `SwitcherDivider` elements. */
  children: any;
}

/** One link in a `Switcher`. */
export interface SwitcherItemProps {
  href?: string;
  target?: string;
  rel?: string;
  /** Selected app: `layer-selected` background and `aria-current="page"`. */
  isSelected?: boolean;
  'aria-label'?: string;
  'aria-labelledby'?: string;
  /** Overrides the default (0 when the Switcher is expanded, else -1). */
  tabIndex?: number;
  onClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
  /** Replace the `<a>` with another element or component. */
  element?: any;
  /** Class on the `<li>`. */
  className?: string;
  children: any;
}

/** A 1px `border-subtle` rule between groups of `SwitcherItem`s. */
export interface SwitcherDividerProps {
  className?: string;
}

/** Repeats the header links inside the `SideNav` below the lg breakpoint (1056px), where `HeaderNavigation` is hidden. Place first in `SideNavItems`; children are `HeaderMenuItem` / `HeaderMenu`. */
export interface HeaderSideNavItemsProps {
  /** Draws a 1px divider 16px below the group. */
  hasDivider?: boolean;
  className?: string;
  children?: any;
}

/** A 48px header row at the top of a `SideNav` (icon plus label). */
export interface SideNavHeaderProps {
  /** Icon file stem or component. */
  renderIcon?: IconRef;
  className?: string;
  children?: any;
}

/** A 48px footer at the bottom of a `SideNav` with an expand/collapse toggle (chevron when collapsed, close when expanded). */
export interface SideNavFooterProps {
  expanded: boolean;
  onToggle: (event: any) => void;
  /** Accessible name and tooltip of the toggle. Default 'Toggle opening or closing the side navigation'. */
  assistiveText?: string;
  className?: string;
}
