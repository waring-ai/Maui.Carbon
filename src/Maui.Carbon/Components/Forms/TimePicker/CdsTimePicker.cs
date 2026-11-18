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
    readonly Grid _fluidRow = new() { ColumnSpacing = 1 };
    readonly Grid _fluidGroup = new();
    readonly Grid _fluidMessageBlock = new() { IsVisible = false };
    readonly BoxView _fluidDivider = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
    readonly CdsText _fluidMessage = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03 + 1, 48, CdsSpacing.Spacing03) };
    readonly CdsIcon _fluidIcon = new() { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.End, Margin = new Thickness(0, CdsSpacing.Spacing03 + 1, CdsSpacing.Spacing05, 0), IsVisible = false };
    readonly CdsFocusRing _fluidRing = new(CdsFocusRingStyle.Inset);
    bool _autoInvalid;
    bool _syncing;
    bool _fluidBuilt;

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
        Chrome.FluidLayoutChanged += (_, _) => UpdateChrome();
        _fluidGroup.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _fluidGroup.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _fluidMessageBlock.Add(_fluidDivider);
        _fluidMessageBlock.Add(_fluidMessage);
        _fluidMessageBlock.Add(_fluidIcon);
        _fluidGroup.Add(_fluidRow, 0, 0);
        _fluidGroup.Add(_fluidMessageBlock, 0, 1);
        _fluidGroup.Add(_fluidRing, 0, 0);
        Grid.SetRowSpan(_fluidRing, 2);
        CdsTokens.SetColor(_fluidRow, BackgroundColorProperty, CdsColorKeys.BorderStrong01);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>
    /// Whether the picker renders its fluid layout (inside a <see cref="CdsFluidForm"/>, or a fluid variant): the time field
    /// and its selects become 64px parts of one row with 1px dividers, and one message sits under all of them.
    /// </summary>
    protected bool IsFluidMode => Chrome.IsFluidLayout;

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
        var fluid = IsFluidMode;
        // Fluid: the group draws the outline and message, so the time part shows only its own focus.
        Chrome.Apply(fluid ? state & ~(CdsInteractionState.Invalid | CdsInteractionState.Warn) : state);

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

        if (fluid)
        {
            Chrome.WidthRequest = -1;
            _fluidMessageBlock.IsVisible = invalid || warn;
            _fluidMessage.Text = text;
            _fluidMessage.IsVisible = !string.IsNullOrEmpty(text);
            SetColor(_fluidMessage, Microsoft.Maui.Controls.Label.TextColorProperty, key);
            SetColor(_fluidDivider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
            SetColor(_fluidMessageBlock, BackgroundColorProperty, CdsColorKeys.Field01);
            _fluidIcon.IsVisible = invalid || warn;
            _fluidIcon.Icon = invalid ? CdsIconName.WarningFilled : CdsIconName.WarningAltFilled;
            SetColor(_fluidIcon, CdsIcon.ColorProperty, invalid ? CdsColorKeys.SupportError : CdsColorKeys.SupportWarning);
            _fluidRing.IsActive = invalid || (Is(CdsInteractionState.Focused) && !disabled);
            _fluidRing.SetColorKey(Is(CdsInteractionState.Focused) ? CdsColorKeys.Focus : CdsColorKeys.SupportError);
        }
        else
        {
            // 4.875rem wide; 6.175rem when the error or warning icon shows.
            Chrome.WidthRequest = invalid || warn ? 98.8 : 78;
        }
        foreach (var select in new[] { PeriodSelect, ZoneSelect })
        {
            select.IsReadOnly = IsReadOnly;
            select.IsEnabled = IsEnabled;
        }
    }

    /// <summary>Re-applies texts, sizes and accessibility after a property change.</summary>
    protected virtual void UpdateChrome()
    {
        var fluid = IsFluidMode;
        _label.Text = Label;
        _label.IsVisible = !fluid && !HideLabel && !string.IsNullOrEmpty(Label);
        Chrome.Label = fluid ? Label : null;
        PeriodSelect.SetFluid(fluid);
        ZoneSelect.SetFluid(fluid);
        Chrome.Size = Size;
        Entry.Placeholder = Placeholder;
        Entry.IsReadOnly = IsReadOnly;
        PeriodSelect.Size = Size;
        ZoneSelect.Size = Size;
        PeriodSelect.IsVisible = !Is24Hour;
        ZoneSelect.IsVisible = TimeZones.Count > 0;
        SemanticProperties.SetDescription(Entry, Label);
        LayoutParts(fluid);
        RefreshVisuals();
    }

    // Fluid: [time | period | zone] share one 64px row on a border-strong background (1px dividers); otherwise the
    // default stacked row with the label above.
    void LayoutParts(bool fluid)
    {
        if (fluid)
        {
            _fluidRow.Children.Clear();
            _fluidRow.ColumnDefinitions.Clear();
            _row.Remove(Chrome);
            _row.Remove(PeriodSelect);
            _row.Remove(ZoneSelect);
            var parts = new List<View> { Chrome };
            if (PeriodSelect.IsVisible) parts.Add(PeriodSelect);
            if (ZoneSelect.IsVisible) parts.Add(ZoneSelect);
            for (var i = 0; i < parts.Count; i++)
            {
                // 25% / 25% / rest, as the web's flex-basis does; a lone part fills the row.
                _fluidRow.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(i == 2 ? 2 : 1, GridUnitType.Star)));
                _fluidRow.Add(parts[i], i);
            }
            if (!_fluidBuilt)
            {
                _fluidBuilt = true;
                _root.Clear();
                _root.Add(_fluidGroup);
            }
            HorizontalOptions = LayoutOptions.Fill;
        }
        else if (_fluidBuilt)
        {
            _fluidBuilt = false;
            _fluidRow.Children.Clear();
            _root.Clear();
            _row.Add(Chrome);
            _row.Add(PeriodSelect);
            _row.Add(ZoneSelect);
            _root.Add(_label);
            _root.Add(_row);
            _root.Add(_message);
            HorizontalOptions = LayoutOptions.Start;
        }
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
