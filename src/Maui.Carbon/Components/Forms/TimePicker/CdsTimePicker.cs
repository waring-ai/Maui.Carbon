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

    readonly Grid _root = new() { RowSpacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly HorizontalStackLayout _row = new() { Spacing = 1 };
    readonly CdsText _message = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, IsVisible = false, Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0) };
    readonly Grid _fluidRow = new() { ColumnSpacing = 0 };
    readonly Grid _fluidGroup = new() { RowSpacing = 0 };
    readonly CdsFluidGroupMessage _fluidMessage = new();
    // Fills a cell taller than the picker (a neighbour in a CdsFluidFormRow shows a message) with field-01.
    readonly Grid _fluidFill = new() { InputTransparent = true };
    // The 1px divider on the start edge of each select part (drawn over the part, as Carbon's ::before is).
    readonly BoxView _periodDivider = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
    readonly BoxView _zoneDivider = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
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
        BuildDefaultRoot();

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
        _fluidGroup.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _fluidGroup.Add(_fluidRow, 0, 0);
        _fluidGroup.Add(_fluidMessage, 0, 1);
        _fluidGroup.Add(_fluidFill, 0, 2);
        _fluidGroup.Add(_fluidRing, 0, 0);
        Grid.SetRowSpan(_fluidRing, 2);
        // One field-01 row (Carbon's .cds--time-picker--fluid); the parts draw their own fills and the dividers are views.
        CdsTokens.SetColor(_fluidRow, BackgroundColorProperty, CdsColorKeys.Field01);
        CdsTokens.SetColor(_fluidFill, BackgroundColorProperty, CdsColorKeys.Field01);
        // A hovered select hides the dividers on its edges.
        PeriodSelect.StateChanged += (_, _) => { if (IsFluidMode) RefreshVisuals(); };
        ZoneSelect.StateChanged += (_, _) => { if (IsFluidMode) RefreshVisuals(); };
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
        // Fluid: the group draws the outline and message, so the time part shows only its own focus, and every part gives
        // up its bottom rule while the message shows (a warning draws one under the message instead).
        var groupMessage = fluid && (invalid || warn);
        Chrome.IsRuleSuppressed = groupMessage;
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
            _fluidMessage.Apply(invalid, warn, text);
            _fluidRing.IsActive = invalid || (Is(CdsInteractionState.Focused) && !disabled);
            _fluidRing.SetColorKey(Is(CdsInteractionState.Focused) ? CdsColorKeys.Focus : CdsColorKeys.SupportError);
            ApplyFluidDividers(disabled, invalid || warn);
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
            select.SetRuleSuppressed(groupMessage);
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

    // Fluid: [time | period | zone] share one 64px field-01 row, with a 1px divider view on the start edge of each select;
    // otherwise the default stacked row with the label above.
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
            // Dividers go over their part, so they are added after all of them.
            for (var i = 1; i < parts.Count; i++) _fluidRow.Add(parts[i] == PeriodSelect ? _periodDivider : _zoneDivider, i);
            if (!_fluidBuilt)
            {
                _fluidBuilt = true;
                _root.Clear();
                _root.RowDefinitions.Clear();
                // The group fills its cell (Carbon's fluid wrappers are block-size: 100%).
                _root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                _root.Add(_fluidGroup, 0, 0);
            }
            HorizontalOptions = LayoutOptions.Fill;
        }
        else if (_fluidBuilt)
        {
            _fluidBuilt = false;
            _fluidRow.Children.Clear();
            _row.Add(Chrome);
            _row.Add(PeriodSelect);
            _row.Add(ZoneSelect);
            BuildDefaultRoot();
            HorizontalOptions = LayoutOptions.Start;
        }
    }

    void BuildDefaultRoot()
    {
        _root.Clear();
        _root.RowDefinitions.Clear();
        for (var i = 0; i < 3; i++) _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.Add(_label, 0, 0);
        _root.Add(_row, 0, 1);
        _root.Add(_message, 0, 2);
    }

    // The select dividers: border-strong-01, border-subtle-01 when read only, border-disabled when disabled; hidden on the
    // edges of a hovered select; 8px short of the top and bottom while the group shows a message (full height less the
    // bottom rule otherwise).
    void ApplyFluidDividers(bool disabled, bool message)
    {
        var periodShown = PeriodSelect.IsVisible;
        var zoneShown = ZoneSelect.IsVisible;
        var periodHover = periodShown && IsPartHovered(PeriodSelect);
        var zoneHover = zoneShown && IsPartHovered(ZoneSelect);
        var key = DividerColorKey(disabled, IsReadOnly);
        var hoverHides = !disabled && !IsReadOnly;
        SetColor(_periodDivider, BoxView.ColorProperty, hoverHides && periodHover ? null : key);
        // The zone divider is the period select's end edge when both selects show.
        SetColor(_zoneDivider, BoxView.ColorProperty, hoverHides && (zoneHover || (periodShown && periodHover)) ? null : key);
        var margin = message ? new Thickness(0, CdsSpacing.Spacing03) : new Thickness(0, 0, 0, 1);
        _periodDivider.Margin = margin;
        _zoneDivider.Margin = margin;
    }

    static bool IsPartHovered(CdsControl part) => part.State.HasFlag(CdsInteractionState.Hover);

    /// <summary>Color of the fluid dividers between the parts for a state.</summary>
    internal static string DividerColorKey(bool disabled, bool readOnly) =>
        disabled ? CdsColorKeys.BorderDisabled : readOnly ? CdsColorKeys.BorderSubtle01 : CdsColorKeys.BorderStrong01;

    /// <summary>The fluid dividers on the start edge of the AM/PM and time zone parts (for tests).</summary>
    internal (BoxView Period, BoxView Zone) FluidDividers => (_periodDivider, _zoneDivider);

    /// <summary>The fluid group's message row (for tests).</summary>
    internal CdsFluidGroupMessage FluidMessage => _fluidMessage;

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
