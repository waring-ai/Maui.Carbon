namespace Maui.Carbon;

public partial class CdsTimePicker
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTimePicker), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTimePicker)b).OnTextPropertyChanged((string?)n));

    /// <summary>Bindable property for <see cref="Time"/>.</summary>
    public static readonly BindableProperty TimeProperty = BindableProperty.Create(
        nameof(Time), typeof(TimeSpan?), typeof(CdsTimePicker), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTimePicker)b).OnTimePropertyChanged((TimeSpan?)n));

    /// <summary>Bindable property for <see cref="Period"/>.</summary>
    public static readonly BindableProperty PeriodProperty = BindableProperty.Create(
        nameof(Period), typeof(CdsTimePeriod), typeof(CdsTimePicker), CdsTimePeriod.Am, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTimePicker)b).OnPeriodChanged((CdsTimePeriod)n));

    /// <summary>Bindable property for <see cref="SelectedTimeZone"/>.</summary>
    public static readonly BindableProperty SelectedTimeZoneProperty = BindableProperty.Create(
        nameof(SelectedTimeZone), typeof(string), typeof(CdsTimePicker), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTimePicker)b).OnTimeZoneChanged((string?)n));

    /// <summary>Bindable property for <see cref="Is24Hour"/>.</summary>
    public static readonly BindableProperty Is24HourProperty = BindableProperty.Create(
        nameof(Is24Hour), typeof(bool), typeof(CdsTimePicker), false, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTimePicker), null, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsTimePicker), false, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsTimePicker), "hh:mm", propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsTimePicker), false, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsTimePicker), null, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsTimePicker), false, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsTimePicker), null, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsTimePicker), false, propertyChanged: OnPickerChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsTimePicker), CdsSize.Md, propertyChanged: OnPickerChanged);

    /// <summary>The typed time text, "h:mm" (12-hour) or "HH:mm" (24-hour) (two-way).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The time of day as a value (two-way); <c>null</c> while the text is empty or malformed. Setting it rewrites the text and period.</summary>
    public TimeSpan? Time
    {
        get => (TimeSpan?)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    /// <summary>AM or PM (two-way); ignored when <see cref="Is24Hour"/>.</summary>
    public CdsTimePeriod Period
    {
        get => (CdsTimePeriod)GetValue(PeriodProperty);
        set => SetValue(PeriodProperty, value);
    }

    /// <summary>The chosen entry of <see cref="TimeZones"/> (two-way).</summary>
    public string? SelectedTimeZone
    {
        get => (string?)GetValue(SelectedTimeZoneProperty);
        set => SetValue(SelectedTimeZoneProperty, value);
    }

    /// <summary>24-hour mode: the AM/PM select is hidden and the text is checked against "HH:mm".</summary>
    public bool Is24Hour
    {
        get => (bool)GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    /// <summary>The visible label; it is also the time field's accessible name.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Hides the label visually; it stays the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>The format hint. Default "hh:mm".</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Error state, in addition to the automatic check by <see cref="Validate"/>.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>The error message under the group. Default "Invalid time format.".</summary>
    public string? InvalidText
    {
        get => (string?)GetValue(InvalidTextProperty);
        set => SetValue(InvalidTextProperty, value);
    }

    /// <summary>Warning state: warning icon and <see cref="WarnText"/>. Ignored when invalid.</summary>
    public bool IsWarning
    {
        get => (bool)GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    /// <summary>The warning message under the group.</summary>
    public string? WarnText
    {
        get => (string?)GetValue(WarnTextProperty);
        set => SetValue(WarnTextProperty, value);
    }

    /// <summary>Read-only: fills drop to transparent with a subtle rule; still focusable, not editable.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Height of the input and the selects: sm 32, md 40 (default), lg 48. Other sizes clamp to that range (Carbon defines no xs or larger time picker).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    static void OnPickerChanged(BindableObject b, object? o, object? n) => ((CdsTimePicker)b).UpdateChrome();
}
