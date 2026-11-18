using System.Globalization;

namespace Maui.Carbon;

/// <summary>
/// A date field with a calendar popup: <see cref="CdsDatePickerType.Single"/> picks one date, <see cref="CdsDatePickerType.Range"/>
/// a start and end date (two fields, one calendar) and <see cref="CdsDatePickerType.Simple"/> is a typed date with no
/// calendar. Typing always works: Enter or leaving the field applies the date, and text that is not a date in
/// <see cref="DateFormat"/>, or is outside <see cref="MinDate"/>..<see cref="MaxDate"/> or on a disabled day, shows an error
/// message. Use a time picker for a time of day and a dropdown of presets for relative periods.
/// <para>
/// The calendar is an overlay under the first field. Keyboard: Down moves from the field into the calendar; arrows move by day
/// and week, PageUp and PageDown by month, Home and End to the first and last day of the week; Enter picks; Escape closes and
/// returns to the field. Single closes after a pick; range after the second pick.
/// </para>
/// </summary>
public partial class CdsDatePicker : CdsFormField
{
    /// <summary>Source of today's date (a seam for screenshots and tests).</summary>
    internal static Func<DateOnly> TodayProvider { get; set; } = static () => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>The native entry of the (first) date field, in <c>code-02</c> mono.</summary>
    protected readonly CdsNativeEntry StartEntry = new();

    /// <summary>The native entry of the end date field of a range picker.</summary>
    protected readonly CdsNativeEntry EndEntry = new();

    /// <summary>The scaffold of the end field of a range picker (the first field uses <see cref="CdsFormField"/>'s <c>Chrome</c>).</summary>
    protected readonly CdsFieldChrome EndChrome = new();

    readonly CdsIcon _startIcon = new(CdsIconName.Calendar, 16);
    readonly CdsIcon _endIcon = new(CdsIconName.Calendar, 16);
    readonly Grid _startIconHost = new() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center, BackgroundColor = Colors.Transparent };
    readonly Grid _endIconHost = new() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center, BackgroundColor = Colors.Transparent };
    readonly Grid _fields = new() { ColumnSpacing = 1, HorizontalOptions = LayoutOptions.Start };
    readonly CdsText _message = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, IsVisible = false, Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0) };
    CdsDateEntryError _startError;
    CdsDateEntryError _endError;
    bool _startFocused;
    bool _endFocused;
    bool _startHover;
    bool _endHover;
    bool _syncing;
    bool _syncingDates;
    bool _batching;
    bool _waitingForSize;
    bool _engageOnOpen;
    bool _inCalendar;
    int _activeField;
    CalendarModel? _model;
    CdsCalendarView? _view;
    CdsOverlay? _overlay;

    /// <summary>Creates a single date picker.</summary>
    public CdsDatePicker()
    {
        HorizontalOptions = LayoutOptions.Start;
        foreach (var entry in new[] { StartEntry, EndEntry }) CdsTypography.Apply(entry, CdsTypeStyle.Code02);

        Chrome.Input = StartEntry;
        EndChrome.Input = EndEntry;
        EndChrome.IsVisible = false;
        BuildIcon(Chrome, _startIcon, _startIconHost, 0);
        BuildIcon(EndChrome, _endIcon, _endIconHost, 1);
        AddHover(Chrome, isStart: true);
        AddHover(EndChrome, isStart: false);

        StartEntry.TextChanged += (_, e) => OnEntryTextChanged(0, e);
        EndEntry.TextChanged += (_, e) => OnEntryTextChanged(1, e);
        StartEntry.Completed += (_, _) => OnEntryCompleted(0);
        EndEntry.Completed += (_, _) => OnEntryCompleted(1);
        StartEntry.Focused += (_, _) => SetFieldFocus(0, true);
        EndEntry.Focused += (_, _) => SetFieldFocus(1, true);
        StartEntry.Unfocused += (_, _) => OnEntryUnfocused(0);
        EndEntry.Unfocused += (_, _) => OnEntryUnfocused(1);
        TrackFocus(EndEntry);
        TrackFocus(StartEntry);

        _fields.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _fields.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _fields.Add(Chrome, 0);
        _fields.Add(EndChrome, 1);

        var root = new Grid { RowSpacing = 0, HorizontalOptions = LayoutOptions.Start };
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.Add(_fields, 0, 0);
        root.Add(_message, 0, 1);

        Loaded += (_, _) => Dispatcher.Dispatch(SyncCalendar);
        Unloaded += (_, _) => HideCalendar(keepOpenFlag: true);

        SetChrome(root);
        UpdateChrome();
    }

    /// <summary>Raised when the date or the range changes (a pick, a typed date applied, or code).</summary>
    public event EventHandler<CdsDateChangedEventArgs>? DateChanged;

    /// <inheritdoc/>
    protected override VisualElement InputElement => StartEntry;

    /// <summary>The culture from <see cref="Locale"/> (the current culture when it is unset or unknown).</summary>
    public CultureInfo Culture
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Locale)) return CultureInfo.CurrentCulture;
            try
            {
                return CultureInfo.GetCultureInfo(Locale);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.CurrentCulture;
            }
        }
    }

    /// <summary>Whether the calendar can open now: a single or range picker that is enabled and not read only.</summary>
    protected bool CanOpenCalendar => Type != CdsDatePickerType.Simple && IsEnabled && !IsReadOnly;

    bool IsRange => Type == CdsDatePickerType.Range;

    DateOnly? Min => DatePickerFormat.ToDate(MinDate);

    DateOnly? Max => DatePickerFormat.ToDate(MaxDate);

    Func<DateOnly, bool>? DisabledPredicate => IsDateDisabled is { } f ? d => f(d.ToDateTime(TimeOnly.MinValue)) : null;

    /// <inheritdoc/>
    protected override bool EffectiveInvalid => IsInvalid || _startError != CdsDateEntryError.None || _endError != CdsDateEntryError.None;

    /// <inheritdoc/>
    protected override string? EffectiveInvalidText
    {
        get
        {
            var error = _startError != CdsDateEntryError.None ? _startError : _endError;
            return error != CdsDateEntryError.None ? DatePickerFormat.Message(error, DateFormat, Min, Max) : InvalidText;
        }
    }

    /// <inheritdoc/>
    protected override string? HintText() => _message.IsVisible ? _message.Text : null;

    // ---- construction helpers ----

    void BuildIcon(CdsFieldChrome chrome, CdsIcon icon, Grid host, int field)
    {
        host.Add(icon);
        AutomationProperties.SetExcludedWithChildren(host, true);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => OnIconTapped(field);
        host.GestureRecognizers.Add(tap);
        chrome.Trailing.Add(host);
    }

    void AddHover(CdsFieldChrome chrome, bool isStart)
    {
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetHover(isStart, true);
        pointer.PointerExited += (_, _) => SetHover(isStart, false);
        chrome.Field.GestureRecognizers.Add(pointer);
    }

    void SetHover(bool isStart, bool on)
    {
        if (isStart) _startHover = on;
        else _endHover = on;
        RefreshVisuals();
    }

    void SetFieldFocus(int field, bool on)
    {
        if (field == 0) _startFocused = on;
        else _endFocused = on;
        _activeField = field;
        RefreshVisuals();
    }

    // ---- chrome ----

    void OnTypeChanged()
    {
        if (!CanOpenCalendar) IsOpen = false;
        UpdateChrome();
    }

    void OnFormatChanged()
    {
        SyncEntries();
        UpdateChrome();
        RenderCalendar();
    }

    void OnLimitsChanged()
    {
        if (_model is { } m)
        {
            m.Min = Min;
            m.Max = Max;
            m.IsDisabledDate = DisabledPredicate;
            RenderCalendar();
        }
    }

    /// <inheritdoc/>
    protected override void UpdateChrome()
    {
        base.UpdateChrome();
        // The picker draws one message under both fields; the field chromes only show the outline and status icon.
        foreach (var c in new[] { Chrome, EndChrome })
        {
            c.HelperText = null;
            c.InvalidText = null;
            c.WarnText = null;
            c.IsInline = false;
            c.Size = CdsListBoxTokens.Coerce(Size);
            c.HideLabel = HideLabel;
        }
        EndChrome.Label = EndLabel;
        EndChrome.IsVisible = IsRange;

        var format = DateFormat;
        StartEntry.Placeholder = Placeholder ?? DatePickerFormat.Placeholder(format);
        EndEntry.Placeholder = EndPlaceholder ?? DatePickerFormat.Placeholder(format);
        StartEntry.IsReadOnly = IsReadOnly;
        EndEntry.IsReadOnly = IsReadOnly;
        SemanticProperties.SetDescription(StartEntry, Label);
        SemanticProperties.SetDescription(EndEntry, EndLabel);

        var hasStatus = (EffectiveInvalid || IsWarning) && IsEnabled && !IsReadOnly;
        var width = CdsCalendarTokens.FieldWidth(Type, IsShort, hasStatus);
        Chrome.WidthRequest = width;
        EndChrome.WidthRequest = width;
        var hasCalendar = Type != CdsDatePickerType.Simple;
        _startIconHost.IsVisible = hasCalendar;
        _endIconHost.IsVisible = hasCalendar;
        UpdateMessage();
        RefreshVisuals();
    }

    void UpdateMessage()
    {
        var disabled = !IsEnabled;
        var readOnly = IsReadOnly;
        var invalid = EffectiveInvalid && !disabled && !readOnly;
        var warn = IsWarning && !invalid && !disabled && !readOnly;
        string? text;
        string key;
        if (invalid && !string.IsNullOrEmpty(EffectiveInvalidText)) (text, key) = (EffectiveInvalidText, CdsColorKeys.TextError);
        else if (warn && !string.IsNullOrEmpty(WarnText)) (text, key) = (WarnText, CdsColorKeys.TextPrimary);
        else (text, key) = (HelperText, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextHelper);
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrEmpty(text);
        SetColor(_message, Microsoft.Maui.Controls.Label.TextColorProperty, key);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        // Focus and hover belong to one field of a range, not to the whole control.
        var shared = FieldState & ~(CdsInteractionState.Focused | CdsInteractionState.FocusVisible | CdsInteractionState.Hover);
        Chrome.Apply(shared | (_startFocused ? CdsInteractionState.Focused : 0) | (_startHover ? CdsInteractionState.Hover : 0));
        EndChrome.Apply(shared | (_endFocused ? CdsInteractionState.Focused : 0) | (_endHover ? CdsInteractionState.Hover : 0));
        UpdateMessage();
        var hint = HintText();
        SemanticProperties.SetHint(StartEntry, hint);
        SemanticProperties.SetHint(EndEntry, hint);
        ApplyInput();
    }

    /// <inheritdoc/>
    protected override void ApplyInput()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        var readOnly = IsReadOnly;
        foreach (var entry in new[] { StartEntry, EndEntry })
        {
            entry.IsEnabled = IsEnabled;
            SetColor(entry, Microsoft.Maui.Controls.Entry.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
            SetColor(entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        }
        // The calendar icon gives way to the error and warning icons.
        var status = (EffectiveInvalid || IsWarning) && !disabled && !readOnly;
        var iconKey = disabled || readOnly ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;
        SetColor(_startIcon, CdsIcon.ColorProperty, iconKey);
        SetColor(_endIcon, CdsIcon.ColorProperty, iconKey);
        var show = Type != CdsDatePickerType.Simple && !status;
        _startIcon.IsVisible = show;
        _endIcon.IsVisible = show;
    }

    // ---- dates and text ----

    void SyncEntries()
    {
        var format = DateFormat;
        _syncing = true;
        try
        {
            StartEntry.Text = DatePickerFormat.ToDate(StartDate) is { } s ? DatePickerFormat.Format(s, format) : string.Empty;
            EndEntry.Text = DatePickerFormat.ToDate(EndDate) is { } e ? DatePickerFormat.Format(e, format) : string.Empty;
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnValueChanged(DateTime? value)
    {
        if (_syncingDates) return;
        StartDate = value;
    }

    void OnStartDateChanged(DateTime? value)
    {
        if (!_syncingDates)
        {
            _syncingDates = true;
            try
            {
                Value = value;
            }
            finally
            {
                _syncingDates = false;
            }
        }
        _startError = CdsDateEntryError.None;
        SyncEntries();
        UpdateChrome();
        RenderCalendar();
        RaiseDateChanged();
    }

    void OnEndDateChanged(DateTime? value)
    {
        _endError = CdsDateEntryError.None;
        SyncEntries();
        UpdateChrome();
        RenderCalendar();
        RaiseDateChanged();
    }

    void RaiseDateChanged()
    {
        if (_batching) return;
        DateChanged?.Invoke(this, new CdsDateChangedEventArgs(StartDate, EndDate));
    }

    void OnEntryTextChanged(int field, TextChangedEventArgs e)
    {
        if (_syncing) return;
        _inCalendar = false;
        _overlay?.SetKeys(CdsOverlayKeys.VerticalNavigation);
        // Typing clears a stale error; it is judged again when the date is applied.
        var cleared = false;
        if (field == 0 && _startError != CdsDateEntryError.None) { _startError = CdsDateEntryError.None; cleared = true; }
        if (field == 1 && _endError != CdsDateEntryError.None) { _endError = CdsDateEntryError.None; cleared = true; }
        if (cleared) UpdateChrome();
        // A date typed in full shows its month in the open calendar.
        if (_model is { } m && DatePickerFormat.TryParse(e.NewTextValue, DateFormat, out var date) && !m.IsDisabled(date))
        {
            m.Show(date);
            RenderCalendar();
        }
    }

    void OnEntryUnfocused(int field)
    {
        SetFieldFocus(field, false);
        CommitEntry(field);
    }

    void OnEntryCompleted(int field)
    {
        if (_overlay is not null && _inCalendar && _model is { } m)
        {
            PickDay(m.Cursor);
            return;
        }
        if (CommitEntry(field) && _overlay is not null && !IsRange) IsOpen = false;
    }

    /// <summary>Applies the text of a field as a date. Returns <c>true</c> when it is valid (or empty).</summary>
    bool CommitEntry(int field)
    {
        var entry = field == 0 ? StartEntry : EndEntry;
        var (date, error) = DatePickerFormat.Validate(entry.Text, DateFormat, Min, Max, DisabledPredicate);
        if (error == CdsDateEntryError.None && field == 1 && date is { } end && DatePickerFormat.ToDate(StartDate) is { } start && end < start)
            error = CdsDateEntryError.EndBeforeStart;

        if (field == 0) _startError = error;
        else _endError = error;

        if (error == CdsDateEntryError.None)
        {
            var value = DatePickerFormat.ToDateTime(date);
            var before = field == 0 ? StartDate : EndDate;
            if (field == 0) StartDate = value;
            else EndDate = value;
            if (before == value) SyncEntries();
            UpdateChrome();
            return true;
        }
        UpdateChrome();
        return false;
    }

    // ---- calendar ----

    void OnIconTapped(int field)
    {
        if (!CanOpenCalendar) return;
        _activeField = field;
        (field == 0 ? StartEntry : EndEntry).Focus();
        IsOpen = !IsOpen;
    }

    void SyncCalendar()
    {
        var want = IsOpen && CanOpenCalendar && IsLoaded;
        if (!want)
        {
            HideCalendar(keepOpenFlag: !IsOpen || !IsLoaded);
            return;
        }
        if (_overlay is not null) return;
        var anchor = Chrome.Field;
        if (anchor.Width <= 0)
        {
            if (_waitingForSize) return;
            _waitingForSize = true;
            anchor.SizeChanged += OnAnchorSized;
            return;
        }
        ShowCalendar();
    }

    void OnAnchorSized(object? sender, EventArgs e)
    {
        if (Chrome.Field.Width <= 0) return;
        Chrome.Field.SizeChanged -= OnAnchorSized;
        _waitingForSize = false;
        SyncCalendar();
    }

    void ShowCalendar()
    {
        var model = new CalendarModel(Culture, TodayProvider(), IsRange)
        {
            Min = Min,
            Max = Max,
            IsDisabledDate = DisabledPredicate,
        };
        model.SetSelection(DatePickerFormat.ToDate(StartDate), DatePickerFormat.ToDate(EndDate));
        model.ShowSelectionOrToday();
        var view = new CdsCalendarView();
        view.DayPicked += (_, date) => PickDay(date);
        view.PreviousRequested += (_, _) => Navigate(previous: true);
        view.NextRequested += (_, _) => Navigate(previous: false);
        _model = model;
        _view = view;
        _inCalendar = _engageOnOpen;
        _engageOnOpen = false;
        view.Render(model, _inCalendar);

        var overlay = CdsOverlayService.Open(view, new CdsOverlayOptions
        {
            Anchor = Chrome.Field,
            Placement = CdsPlacement.BottomStart,
            Offset = -2,
            LightDismiss = true,
            ReturnFocusTo = _activeField == 1 ? EndEntry : StartEntry,
            // Typing in the field keeps Left/Right/Home/End; once Down moves into the calendar they move the day.
            Keys = _inCalendar ? CdsOverlayKeys.Navigation : CdsOverlayKeys.VerticalNavigation,
            KeyHandler = OnCalendarKey,
        });
        _overlay = overlay;
        overlay.Closed += (_, _) => OnCalendarClosed(overlay);
    }

    void HideCalendar(bool keepOpenFlag)
    {
        if (_waitingForSize)
        {
            Chrome.Field.SizeChanged -= OnAnchorSized;
            _waitingForSize = false;
        }
        if (_overlay is not { } overlay) return;
        _overlay = null;
        if (keepOpenFlag) _keepOpen = true;
        overlay.Close();
    }

    bool _keepOpen;

    void OnCalendarClosed(CdsOverlay overlay)
    {
        if (ReferenceEquals(_overlay, overlay)) _overlay = null;
        _model = null;
        _view = null;
        _inCalendar = false;
        var keep = _keepOpen;
        _keepOpen = false;
        if (IsOpen && !keep) SetValue(IsOpenProperty, false);
    }

    void RenderCalendar()
    {
        if (_model is not { } m || _view is not { } v) return;
        m.SetSelection(DatePickerFormat.ToDate(StartDate), DatePickerFormat.ToDate(EndDate));
        v.Render(m, _inCalendar);
    }

    void Navigate(bool previous)
    {
        if (_model is not { } m) return;
        if (previous) m.GoPrevious();
        else m.GoNext();
        RenderCalendar();
    }

    void PickDay(DateOnly date)
    {
        if (_model is not { } m) return;
        var result = m.Pick(date);
        if (result == CalendarPick.Ignored) return;
        _batching = true;
        try
        {
            StartDate = DatePickerFormat.ToDateTime(m.Start);
            EndDate = DatePickerFormat.ToDateTime(m.End);
        }
        finally
        {
            _batching = false;
        }
        RaiseDateChanged();
        if (result == CalendarPick.Completed) IsOpen = false;
        else RenderCalendar();
    }

    bool OnCalendarKey(CdsKeyEventArgs e)
    {
        if (_model is not { } m) return false;
        switch (e.Key)
        {
            case CdsKey.Down when !_inCalendar:
                // Down moves from the field into the calendar, which then takes every navigation key.
                _inCalendar = true;
                _overlay?.SetKeys(CdsOverlayKeys.Navigation);
                RenderCalendar();
                return true;
            case CdsKey.PageUp or CdsKey.PageDown:
            case CdsKey.Left or CdsKey.Right or CdsKey.Up or CdsKey.Down or CdsKey.Home or CdsKey.End when _inCalendar:
                _inCalendar = true;
                m.MoveCursor(e.Key);
                RenderCalendar();
                return true;
            default:
                // Left, Right, Home and End edit the typed text until the keyboard is in the calendar.
                return false;
        }
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => CanOpenCalendar;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (_overlay is not null || !CanOpenCalendar || e.Key != CdsKey.Down) return false;
        _engageOnOpen = true;
        IsOpen = true;
        return true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsDatePicker"/>: a label bar over one 288px field, or two 143.5px fields when
/// <see cref="Range"/>.
/// </summary>
public class CdsDatePickerSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="Range"/>.</summary>
    public static readonly BindableProperty RangeProperty = BindableProperty.Create(
        nameof(Range), typeof(bool), typeof(CdsDatePickerSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsDatePickerSkeleton)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsDatePickerSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsDatePickerSkeleton)b).UpdateLayout());

    readonly Grid _row = new() { ColumnSpacing = 1, HorizontalOptions = LayoutOptions.Start };
    readonly Grid _labels = new() { ColumnSpacing = 1, HorizontalOptions = LayoutOptions.Start };

    /// <summary>Creates a single-field skeleton with a label bar.</summary>
    public CdsDatePickerSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        HorizontalOptions = LayoutOptions.Start;
        var stack = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Start };
        stack.Add(_labels);
        stack.Add(_row);
        SetChrome(stack);
        UpdateLayout();
    }

    /// <summary>Two fields, for a range picker.</summary>
    public bool Range
    {
        get => (bool)GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    /// <summary>Hides the label bar.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    void UpdateLayout()
    {
        var count = Range ? 2 : 1;
        var width = CdsCalendarTokens.FieldWidth(Range ? CdsDatePickerType.Range : CdsDatePickerType.Single, false, false);
        _row.Children.Clear();
        _row.ColumnDefinitions.Clear();
        _labels.Children.Clear();
        _labels.ColumnDefinitions.Clear();
        for (var i = 0; i < count; i++)
        {
            _row.ColumnDefinitions.Add(new ColumnDefinition(width));
            _labels.ColumnDefinitions.Add(new ColumnDefinition(width));
            _row.Add(new CdsShimmerView { HeightRequest = CdsSize.Md.Height() }, i);
            _labels.Add(new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) }, i);
        }
        _labels.IsVisible = !HideLabel;
    }
}
