namespace Maui.Carbon;

public partial class CdsDatePicker
{
    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsDatePickerType), typeof(CdsDatePicker), CdsDatePickerType.Single,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).OnTypeChanged());

    /// <summary>Bindable property for <see cref="EndLabel"/>.</summary>
    public static readonly BindableProperty EndLabelProperty = BindableProperty.Create(
        nameof(EndLabel), typeof(string), typeof(CdsDatePicker), "End date",
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsDatePicker), null,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="EndPlaceholder"/>.</summary>
    public static readonly BindableProperty EndPlaceholderProperty = BindableProperty.Create(
        nameof(EndPlaceholder), typeof(string), typeof(CdsDatePicker), null,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(DateTime?), typeof(CdsDatePicker), null, BindingMode.TwoWay,
        coerceValue: static (_, v) => ((DateTime?)v)?.Date,
        propertyChanged: static (b, _, n) => ((CdsDatePicker)b).OnValueChanged((DateTime?)n));

    /// <summary>Bindable property for <see cref="StartDate"/>.</summary>
    public static readonly BindableProperty StartDateProperty = BindableProperty.Create(
        nameof(StartDate), typeof(DateTime?), typeof(CdsDatePicker), null, BindingMode.TwoWay,
        coerceValue: static (_, v) => ((DateTime?)v)?.Date,
        propertyChanged: static (b, _, n) => ((CdsDatePicker)b).OnStartDateChanged((DateTime?)n));

    /// <summary>Bindable property for <see cref="EndDate"/>.</summary>
    public static readonly BindableProperty EndDateProperty = BindableProperty.Create(
        nameof(EndDate), typeof(DateTime?), typeof(CdsDatePicker), null, BindingMode.TwoWay,
        coerceValue: static (_, v) => ((DateTime?)v)?.Date,
        propertyChanged: static (b, _, n) => ((CdsDatePicker)b).OnEndDateChanged((DateTime?)n));

    /// <summary>Bindable property for <see cref="DateFormat"/>.</summary>
    public static readonly BindableProperty DateFormatProperty = BindableProperty.Create(
        nameof(DateFormat), typeof(string), typeof(CdsDatePicker), DatePickerFormat.DefaultFormat,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).OnFormatChanged());

    /// <summary>Bindable property for <see cref="Locale"/>.</summary>
    public static readonly BindableProperty LocaleProperty = BindableProperty.Create(
        nameof(Locale), typeof(string), typeof(CdsDatePicker), null,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).OnFormatChanged());

    /// <summary>Bindable property for <see cref="MinDate"/>.</summary>
    public static readonly BindableProperty MinDateProperty = BindableProperty.Create(
        nameof(MinDate), typeof(DateTime?), typeof(CdsDatePicker), null,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).OnLimitsChanged());

    /// <summary>Bindable property for <see cref="MaxDate"/>.</summary>
    public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(
        nameof(MaxDate), typeof(DateTime?), typeof(CdsDatePicker), null,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).OnLimitsChanged());

    /// <summary>Bindable property for <see cref="IsDateDisabled"/>.</summary>
    public static readonly BindableProperty IsDateDisabledProperty = BindableProperty.Create(
        nameof(IsDateDisabled), typeof(Func<DateTime, bool>), typeof(CdsDatePicker), null,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).OnLimitsChanged());

    /// <summary>Bindable property for <see cref="IsShort"/>.</summary>
    public static readonly BindableProperty IsShortProperty = BindableProperty.Create(
        nameof(IsShort), typeof(bool), typeof(CdsDatePicker), false,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsDatePicker), false, BindingMode.TwoWay,
        coerceValue: static (b, v) => (bool)v && ((CdsDatePicker)b).CanOpenCalendar,
        propertyChanged: static (b, _, _) => ((CdsDatePicker)b).SyncCalendar());

    /// <summary><see cref="CdsDatePickerType.Single"/> (default), <see cref="CdsDatePickerType.Range"/> or <see cref="CdsDatePickerType.Simple"/>.</summary>
    public CdsDatePickerType Type
    {
        get => (CdsDatePickerType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>Label of the second field of a range picker. Default "End date". Label each field for what it is ("Start date", "Snapshot date").</summary>
    public string? EndLabel
    {
        get => (string?)GetValue(EndLabelProperty);
        set => SetValue(EndLabelProperty, value);
    }

    /// <summary>Placeholder of the (first) field; default is derived from <see cref="DateFormat"/> ("mm/dd/yyyy"). It never replaces the label.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Placeholder of the end field of a range picker; default derived from <see cref="DateFormat"/>.</summary>
    public string? EndPlaceholder
    {
        get => (string?)GetValue(EndPlaceholderProperty);
        set => SetValue(EndPlaceholderProperty, value);
    }

    /// <summary>The date of a single or simple picker (two-way); the same value as <see cref="StartDate"/>.</summary>
    public DateTime? Value
    {
        get => (DateTime?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The date, or the start of a range (two-way). The time of day is dropped.</summary>
    public DateTime? StartDate
    {
        get => (DateTime?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    /// <summary>The end of a range (two-way); <c>null</c> while only the start is chosen.</summary>
    public DateTime? EndDate
    {
        get => (DateTime?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    /// <summary>
    /// The date text format with Carbon's flatpickr-style tokens: <c>m</c> month (two digits), <c>n</c> month, <c>d</c> day (two
    /// digits), <c>j</c> day, <c>Y</c> four-digit year, <c>y</c> two-digit year; other characters are literal. Default <c>m/d/Y</c>.
    /// </summary>
    public string DateFormat
    {
        get => (string)GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    /// <summary>The culture name ("de-DE") that gives the calendar its first day of the week, month names and weekday headings. Default: the current culture.</summary>
    public string? Locale
    {
        get => (string?)GetValue(LocaleProperty);
        set => SetValue(LocaleProperty, value);
    }

    /// <summary>Days before this date are disabled and typed dates before it are rejected.</summary>
    public DateTime? MinDate
    {
        get => (DateTime?)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>Days after this date are disabled and typed dates after it are rejected.</summary>
    public DateTime? MaxDate
    {
        get => (DateTime?)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    /// <summary>Marks single days as unavailable (weekends, holidays): they cannot be picked and typed dates on them are rejected.</summary>
    public Func<DateTime, bool>? IsDateDisabled
    {
        get => (Func<DateTime, bool>?)GetValue(IsDateDisabledProperty);
        set => SetValue(IsDateDisabledProperty, value);
    }

    /// <summary>A narrower field for a <see cref="CdsDatePickerType.Simple"/> picker (91px instead of 120px).</summary>
    public bool IsShort
    {
        get => (bool)GetValue(IsShortProperty);
        set => SetValue(IsShortProperty, value);
    }

    /// <summary>Whether the calendar is open (two-way). Setting it before the control loads opens the calendar once it does. Ignored by simple pickers.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
}
