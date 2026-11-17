using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A text input for a time ("hh:mm", set in the mono font) with an optional AM/PM select and a time zone select beside it.
/// It does not open a clock. The typed text is checked against the 12- or 24-hour pattern when the field loses focus or
/// the user presses Return (<see cref="Validate"/>); <see cref="CdsFormField.IsInvalid"/> forces the error state too. The
/// message sits under the whole group. 12-hour times need the AM/PM select; a specific moment needs a time zone; 24-hour
/// times need neither.
/// </summary>
public partial class CdsTimePicker : CdsControl
{
    /// <summary>The field scaffold of the time input (label and message are drawn by the picker itself).</summary>
    protected readonly CdsFieldChrome Chrome = new() { HideLabel = true };

    /// <summary>The native entry for the time.</summary>
    protected readonly CdsNativeEntry Entry = new() { MaxLength = 5 };

    /// <summary>The AM/PM select.</summary>
    protected readonly CdsSelect PeriodSelect = new() { HideLabel = true, Label = "AM or PM" };

    /// <summary>The time zone select.</summary>
    protected readonly CdsSelect ZoneSelect = new() { HideLabel = true, Label = "Time zone", IsVisible = false };

    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly HorizontalStackLayout _row = new() { Spacing = 1 };
    readonly CdsText _message = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, IsVisible = false, Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0) };
    bool _autoInvalid;
    bool _syncing;

    /// <summary>Creates a 12-hour time picker.</summary>
    public CdsTimePicker()
    {
        CdsTypography.Apply(Entry, CdsTypeStyle.Code02);
        Chrome.Input = Entry;
        PeriodSelect.Items.Add(new CdsSelectItem { Value = CdsTimePeriod.Am, Text = "AM" });
        PeriodSelect.Items.Add(new CdsSelectItem { Value = CdsTimePeriod.Pm, Text = "PM" });
        PeriodSelect.SelectedItem = CdsTimePeriod.Am;
        PeriodSelect.SelectedItemChanged += (_, _) =>
        {
            if (PeriodSelect.SelectedItem is CdsTimePeriod period && Period != period) Period = period;
        };
        ZoneSelect.ItemsSource = TimeZones;
        ZoneSelect.SelectedItemChanged += (_, _) =>
        {
            if (!_syncing) SelectedTimeZone = ZoneSelect.SelectedItem as string;
        };
        TimeZones.CollectionChanged += (_, _) => UpdateChrome();

        _row.Add(Chrome);
        _row.Add(PeriodSelect);
        _row.Add(ZoneSelect);
        _root.Add(_label);
        _root.Add(_row);
        _root.Add(_message);

        Entry.TextChanged += OnEntryTextChanged;
        Entry.Unfocused += (_, _) => Validate();
        Entry.Completed += (_, _) =>
        {
            Validate();
            Completed?.Invoke(this, EventArgs.Empty);
        };
        TrackFocus(Entry);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Raised when the time text changes.</summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <summary>Raised when the user presses Return in the time field.</summary>
    public event EventHandler? Completed;

    /// <summary>The time zones offered by the zone select (leave empty for none).</summary>
    public ObservableCollection<string> TimeZones { get; } = [];

    /// <summary>True when the text is empty or a well-formed time of the current kind (12- or 24-hour).</summary>
    public bool IsTextValid => string.IsNullOrEmpty(Text) || TimePickerModel.IsValid(Text, !Is24Hour);

    /// <summary>
    /// Checks the typed text: shows the invalid state with <see cref="CdsFormField.InvalidText"/> (default
    /// "Invalid time format.") when it is not a time. Empty text is valid (use a required-field rule of your own).
    /// </summary>
    /// <returns>True when the text is valid.</returns>
    public bool Validate()
    {
        _autoInvalid = !IsTextValid;
        UpdateChrome();
        return !_autoInvalid;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        var invalid = (IsInvalid || _autoInvalid) && !disabled && !IsReadOnly;
        var warn = IsWarning && !invalid && !disabled && !IsReadOnly;
        var state = State;
        if (invalid) state |= CdsInteractionState.Invalid;
        if (warn) state |= CdsInteractionState.Warn;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        Chrome.Apply(state);

        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        SetColor(Entry, Microsoft.Maui.Controls.Entry.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(Entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        Entry.IsEnabled = IsEnabled;

        string? text = null;
        string key = CdsColorKeys.TextPrimary;
        if (invalid)
        {
            text = string.IsNullOrEmpty(InvalidText) ? "Invalid time format." : InvalidText;
            key = CdsColorKeys.TextError;
        }
        else if (warn && !string.IsNullOrEmpty(WarnText))
        {
            text = WarnText;
        }
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrEmpty(text);
        SetColor(_message, Microsoft.Maui.Controls.Label.TextColorProperty, key);
        SemanticProperties.SetHint(Entry, _message.IsVisible ? text : null);

        // 4.875rem wide; 6.175rem when the error or warning icon shows.
        Chrome.WidthRequest = invalid || warn ? 98.8 : 78;
        foreach (var select in new[] { PeriodSelect, ZoneSelect })
        {
            select.IsReadOnly = IsReadOnly;
            select.IsEnabled = IsEnabled;
        }
    }

    /// <summary>Re-applies texts, sizes and accessibility after a property change.</summary>
    protected virtual void UpdateChrome()
    {
        _label.Text = Label;
        _label.IsVisible = !HideLabel && !string.IsNullOrEmpty(Label);
        Chrome.Size = Size;
        Entry.Placeholder = Placeholder;
        Entry.IsReadOnly = IsReadOnly;
        PeriodSelect.Size = Size;
        ZoneSelect.Size = Size;
        PeriodSelect.IsVisible = !Is24Hour;
        ZoneSelect.IsVisible = TimeZones.Count > 0;
        SemanticProperties.SetDescription(Entry, Label);
        RefreshVisuals();
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        Text = e.NewTextValue;
        if (_autoInvalid && IsTextValid)
        {
            _autoInvalid = false;
            UpdateChrome();
        }
        TextChanged?.Invoke(this, e);
    }

    void OnTextPropertyChanged(string? text)
    {
        if (Entry.Text != text) Entry.Text = text;
        SyncTime();
    }

    void OnPeriodChanged(CdsTimePeriod period)
    {
        if (!Equals(PeriodSelect.SelectedItem, period)) PeriodSelect.SelectedItem = period;
        SyncTime();
    }

    void OnTimePropertyChanged(TimeSpan? time)
    {
        if (_syncing || time is not { } value) return;
        var (text, period) = TimePickerModel.Format(value, !Is24Hour);
        _syncing = true;
        try
        {
            Text = text;
            if (!Is24Hour) Period = period;
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnTimeZoneChanged(string? zone)
    {
        _syncing = true;
        try
        {
            if (!Equals(ZoneSelect.SelectedItem, zone)) ZoneSelect.SelectedItem = zone;
        }
        finally
        {
            _syncing = false;
        }
    }

    void SyncTime()
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            Time = TimePickerModel.ToTimeOfDay(Text, !Is24Hour, Period);
        }
        finally
        {
            _syncing = false;
        }
    }
}
