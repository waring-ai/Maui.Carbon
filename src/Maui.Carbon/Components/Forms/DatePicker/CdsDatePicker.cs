using System.Globalization;

namespace Maui.Carbon;

/// <summary>
/// A date field with a calendar popup: <see cref="CdsDatePickerType.Single"/> picks one date, <see cref="CdsDatePickerType.Range"/>
/// a start and end date (two fields, one calendar) and <see cref="CdsDatePickerType.Simple"/> is a typed date with no
/// calendar. Typing always works: Enter or leaving the field applies the date. Text that is not a date in
/// <see cref="DateFormat"/> reverts to the last applied date (Carbon: unparseable text reverts); a date outside
/// <see cref="MinDate"/>..<see cref="MaxDate"/> or on a disabled day stays and shows an error message. Use a time picker
/// for a time of day and a dropdown of presets for relative periods.
/// <para>
/// The calendar is an overlay under the first field. Clicking or tapping a field (focusing it) opens it. Keyboard: Down moves
/// from the field into the calendar; arrows move by day and week, PageUp and PageDown by month, Home and End to the first and
/// last day of the month; Enter or Space picks; Escape closes and returns to the field. Single closes after a pick; range after
/// the second pick. Screen readers hear the day under the keyboard cursor after every move, and the month when the month
/// buttons change it.
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
    Grid _root = null!;
    readonly Grid _fields = new() { ColumnSpacing = 1, HorizontalOptions = LayoutOptions.Start };
    // Fluid range: the 1px split on the start edge of the end field (drawn over it, as Carbon's border-inline-start is),
    // the picker-level outline and message under both fields, and the field-01 fill of a taller cell.
    readonly BoxView _rangeDivider = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
    readonly CdsFluidGroupMessage _fluidMessage = new();
    readonly CdsFocusRing _fluidRing = new(CdsFocusRingStyle.Inset);
    readonly Grid _fluidFill = new() { InputTransparent = true, IsVisible = false };
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
    bool _startStatus;
    bool _endStatus;
    long? _closedAt;
    readonly Queue<string>[] _queuedTexts = [new(), new()];
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
        StartEntry.Focused += (_, _) =>
        {
            SetFieldFocus(0, true);
            OnEntryFocused(0);
        };
        EndEntry.Focused += (_, _) =>
        {
            SetFieldFocus(1, true);
            OnEntryFocused(1);
        };
        StartEntry.Unfocused += (_, _) => OnEntryUnfocused(0);
        EndEntry.Unfocused += (_, _) => OnEntryUnfocused(1);
        TrackFocus(EndEntry);
        TrackFocus(StartEntry);

        _fields.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _fields.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _fields.Add(Chrome, 0);
        _fields.Add(EndChrome, 1);
        _fields.Add(_rangeDivider, 1);

        _root = new Grid { RowSpacing = 0, HorizontalOptions = LayoutOptions.Start };
        var root = _root;
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        // Fluid: the picker fills its cell (Carbon's block-size: 100%); the rest below the fields is field-01.
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        root.Add(_fields, 0, 0);
        root.Add(_fluidMessage, 0, 1);
        root.Add(_message, 0, 2);
        root.Add(_fluidFill, 0, 3);
        root.Add(_fluidRing, 0, 0);
        Grid.SetRowSpan(_fluidRing, 2);
        CdsTokens.SetColor(_fluidFill, BackgroundColorProperty, CdsColorKeys.Field01);
        _fluidRing.SetColorKey(CdsColorKeys.SupportError);

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
    protected override bool EffectiveInvalid => IsInvalid || StartInputInvalid || EndInputInvalid;

    /// <inheritdoc/>
    protected override string? EffectiveInvalidText
    {
        get
        {
            var error = _startError != CdsDateEntryError.None ? _startError : IsRange ? _endError : CdsDateEntryError.None;
            if (error != CdsDateEntryError.None) return DatePickerFormat.Message(error, DateFormat, Min, Max);
            if (IsInvalid) return InvalidText;
            if (IsStartInvalid) return StartInvalidText;
            return IsRange && IsEndInvalid ? EndInvalidText : InvalidText;
        }
    }

    /// <summary>Input-level error on the first field: <see cref="IsStartInvalid"/> or a typed date that is not valid.</summary>
    internal bool StartInputInvalid => IsStartInvalid || _startError != CdsDateEntryError.None;

    /// <summary>Input-level error on the end field of a range.</summary>
    internal bool EndInputInvalid => IsRange && (IsEndInvalid || _endError != CdsDateEntryError.None);

    string? StartInputInvalidText => _startError != CdsDateEntryError.None
        ? DatePickerFormat.Message(_startError, DateFormat, Min, Max)
        : StartInvalidText;

    string? EndInputInvalidText => _endError != CdsDateEntryError.None
        ? DatePickerFormat.Message(_endError, DateFormat, Min, Max)
        : EndInvalidText;

    /// <summary>
    /// Fluid range: picker-level invalid and warning states draw one outline around both fields and one message row under
    /// them; input-level errors stay on their own field.
    /// </summary>
    bool UsesGroupMessage => IsFluidMode && IsRange;

    /// <summary>The picker-level message row of a fluid range (for tests).</summary>
    internal CdsFluidGroupMessage FluidMessage => _fluidMessage;

    /// <summary>The 1px split between the halves of a fluid range (for tests).</summary>
    internal BoxView RangeDivider => _rangeDivider;

    /// <summary>The calendar icons of the two fields (for tests).</summary>
    internal (CdsIcon Start, CdsIcon End) CalendarIcons => (_startIcon, _endIcon);

    /// <inheritdoc/>
    protected override string? HintText() => IsFluidMode ? FluidHint(Chrome) : _message.IsVisible ? _message.Text : null;

    // A field's own message first, else the picker-level one under a fluid range.
    string? FluidHint(CdsFieldChrome chrome) => chrome.CurrentMessage ?? (UsesGroupMessage ? _fluidMessage.Message : null);

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
        var fluid = IsFluidMode;
        EndChrome.IsFluid = Chrome.IsFluid;
        // The picker draws one message under both fields; the field chromes only show the outline and status icon.
        // Fluid: the first field shows the message inside itself (a range outlines both, one message under the first).
        // A fluid range: each field shows its own input-level error, and the picker draws the picker-level message.
        var group = UsesGroupMessage;
        foreach (var c in new[] { Chrome, EndChrome })
        {
            c.HelperText = null;
            c.InvalidText = !fluid ? null : group ? (c == Chrome ? StartInputInvalidText : EndInputInvalidText) : c == Chrome ? EffectiveInvalidText : null;
            c.WarnText = fluid && !group && c == Chrome ? WarnText : null;
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
        var width = fluid ? -1 : CdsCalendarTokens.FieldWidth(Type, IsShort, hasStatus);
        Chrome.WidthRequest = width;
        EndChrome.WidthRequest = width;
        _fields.ColumnDefinitions.Clear();
        var columns = IsRange ? 2 : 1;
        for (var i = 0; i < 2; i++)
            _fields.ColumnDefinitions.Add(new ColumnDefinition(fluid && i < columns ? GridLength.Star : GridLength.Auto));
        // Only a range has a split: a 1px gap between the default fields, a divider view in fluid (never a background
        // showing through a gap, which turned read-only fields into a border-strong-01 slab and left a stray line on the
        // right of single pickers).
        _fields.ColumnSpacing = IsRange && !fluid ? 1 : 0;
        _fields.HorizontalOptions = fluid ? LayoutOptions.Fill : LayoutOptions.Start;
        _root.HorizontalOptions = fluid ? LayoutOptions.Fill : LayoutOptions.Start;
        HorizontalOptions = fluid ? LayoutOptions.Fill : LayoutOptions.Start;
        CdsTokens.SetColor(_fields, BackgroundColorProperty, fluid ? CdsColorKeys.Field01 : null);
        _rangeDivider.IsVisible = fluid && IsRange;
        _fluidFill.IsVisible = fluid;
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
        if (IsFluidMode) text = null; // fluid: the first field shows the message inside itself
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrEmpty(text);
        SetColor(_message, Microsoft.Maui.Controls.Label.TextColorProperty, key);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        // Focus and hover belong to one field of a range, not to the whole control; so do input-level errors.
        var shared = FieldState & ~(CdsInteractionState.Focused | CdsInteractionState.FocusVisible | CdsInteractionState.Hover
            | CdsInteractionState.Invalid | CdsInteractionState.Warn);
        var group = UsesGroupMessage;
        var active = IsEnabled && !IsReadOnly;
        var groupInvalid = group && IsInvalid && active;
        var groupWarn = group && IsWarning && !groupInvalid && active;
        CdsInteractionState startStatus, endStatus;
        if (group)
        {
            // Fluid range: picker-level states go to the group below; each field keeps only its own error.
            startStatus = StartInputInvalid ? CdsInteractionState.Invalid : 0;
            endStatus = EndInputInvalid ? CdsInteractionState.Invalid : 0;
        }
        else
        {
            var warn = IsWarning ? CdsInteractionState.Warn : 0;
            startStatus = (IsInvalid || StartInputInvalid || (!IsRange && EffectiveInvalid) ? CdsInteractionState.Invalid : 0) | warn;
            endStatus = (IsInvalid || EndInputInvalid ? CdsInteractionState.Invalid : 0) | warn;
        }
        _startStatus = startStatus != 0 && active;
        _endStatus = endStatus != 0 && active;

        // While the group shows its message the fields give up their bottom rules (a warning draws one under the message).
        Chrome.IsRuleSuppressed = groupInvalid || groupWarn;
        EndChrome.IsRuleSuppressed = groupInvalid || groupWarn;
        Chrome.Apply(shared | startStatus | (_startFocused ? CdsInteractionState.Focused : 0) | (_startHover ? CdsInteractionState.Hover : 0));
        EndChrome.Apply(shared | endStatus | (_endFocused ? CdsInteractionState.Focused : 0) | (_endHover ? CdsInteractionState.Hover : 0));

        _fluidMessage.Apply(groupInvalid, groupWarn, groupInvalid ? InvalidText : WarnText);
        _fluidRing.IsActive = groupInvalid;
        ApplyRangeDivider(groupInvalid || groupWarn);

        UpdateMessage();
        SemanticProperties.SetHint(StartEntry, HintText());
        SemanticProperties.SetHint(EndEntry, IsFluidMode ? FluidHint(EndChrome) : HintText());
        ApplyInput();
    }

    // The fluid range split: border-strong-01 (border-subtle-01 when read only), inset 8px from the top and bottom while the
    // picker-level message shows, and gone next to a field with its own error outline.
    void ApplyRangeDivider(bool groupMessage)
    {
        if (!_rangeDivider.IsVisible) return;
        var active = IsEnabled && !IsReadOnly;
        var key = active && (StartInputInvalid || EndInputInvalid) ? null
            : IsReadOnly && !groupMessage ? CdsColorKeys.BorderSubtle01
            : CdsColorKeys.BorderStrong01;
        SetColor(_rangeDivider, BoxView.ColorProperty, key);
        _rangeDivider.Margin = groupMessage ? new Thickness(0, CdsSpacing.Spacing03) : new Thickness(0);
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
        // The calendar icon gives way to a field's error or warning icon. In fluid layout the status icon sits in the
        // message row, so the calendar icon stays (the calendar can still be opened by pointer).
        var iconKey = disabled || readOnly ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;
        SetColor(_startIcon, CdsIcon.ColorProperty, iconKey);
        SetColor(_endIcon, CdsIcon.ColorProperty, iconKey);
        var fluid = IsFluidMode;
        var calendar = Type != CdsDatePickerType.Simple;
        _startIcon.IsVisible = calendar && (fluid || !_startStatus);
        _endIcon.IsVisible = calendar && (fluid || !_endStatus);
    }

    // ---- dates and text ----

    void SyncEntries()
    {
        var format = DateFormat;
        SetEntryText(0, DatePickerFormat.ToDate(StartDate) is { } s ? DatePickerFormat.Format(s, format) : string.Empty);
        SetEntryText(1, DatePickerFormat.ToDate(EndDate) is { } e ? DatePickerFormat.Format(e, format) : string.Empty);
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
        if (_syncing || IsQueuedText(field, e.NewTextValue)) return;
        // Space on the highlighted day picks it (the field keeps focus, so the key arrives as a typed space): pick, and
        // rewrite the text without the space, so the keyboard stays in the calendar.
        if (_inCalendar && _model is { } cal && DatePickerFormat.IsTypedSpace(e.OldTextValue, e.NewTextValue))
        {
            PickDay(cal.Cursor);
            SyncEntries();
            return;
        }
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
        if (_inCalendar && _model is { } m)
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

        // Carbon: unparseable text reverts to the last applied date. Out-of-range and disabled dates keep their text and
        // show why they were refused.
        if (error == CdsDateEntryError.Unparseable)
        {
            error = CdsDateEntryError.None;
            if (field == 0) _startError = error;
            else _endError = error;
            RevertEntry(field);
            UpdateChrome();
            return false;
        }

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

    void RevertEntry(int field)
    {
        var date = DatePickerFormat.ToDate(field == 0 ? StartDate : EndDate);
        SetEntryText(field, date is { } d ? DatePickerFormat.Format(d, DateFormat) : string.Empty);
    }

    // Writes a field's text without treating it as typing. A write made while that entry's own TextChanged is running is
    // applied by MAUI only after the handler returns (nested sets of one property are queued), when _syncing is already
    // off: such writes are remembered and their late TextChanged is recognised and ignored.
    void SetEntryText(int field, string? text)
    {
        var entry = field == 0 ? StartEntry : EndEntry;
        text ??= string.Empty;
        if ((entry.Text ?? string.Empty) == text) return;
        _syncing = true;
        try
        {
            entry.Text = text;
            if ((entry.Text ?? string.Empty) != text) _queuedTexts[field].Enqueue(text);
        }
        finally
        {
            _syncing = false;
        }
    }

    bool IsQueuedText(int field, string? text)
    {
        var queue = _queuedTexts[field];
        if (queue.Count == 0 || queue.Peek() != (text ?? string.Empty)) return false;
        queue.Dequeue();
        return true;
    }

    // ---- calendar ----

    void OnIconTapped(int field)
    {
        if (!CanOpenCalendar) return;
        // Focusing the field may already open the calendar (OnEntryFocused); the tap decides the final state.
        var open = !IsOpen;
        _activeField = field;
        (field == 0 ? StartEntry : EndEntry).Focus();
        IsOpen = open;
    }

    /// <summary>
    /// Clicking or tapping a field (it takes focus) opens the calendar, as Carbon's inputs do. Focus that comes back to
    /// the field as the calendar closes (Escape, a pick, a click outside) does not open it again.
    /// </summary>
    internal void OnEntryFocused(int field)
    {
        if (!CanOpenCalendar || IsOpen || _overlay is not null) return;
        if (_closedAt is { } closed && Environment.TickCount64 - closed < ReopenGuardMilliseconds) return;
        _activeField = field;
        IsOpen = true;
    }

    /// <summary>Focus that reaches a field this soon after the calendar closed is the focus coming back from it.</summary>
    const long ReopenGuardMilliseconds = 400;

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
        var (_, view) = BuildCalendar();
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
        if (_inCalendar) CdsAnnouncer.Announce(_model?.CursorAnnouncement());
    }

    /// <summary>Creates the calendar model and view for the current dates (the overlay is opened by the caller).</summary>
    internal (CalendarModel Model, CdsCalendarView View) BuildCalendar()
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
        return (model, view);
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
        _closedAt = Environment.TickCount64;
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
        var moved = previous ? m.GoPrevious() : m.GoNext();
        RenderCalendar();
        // The month buttons are not focused by the keyboard; say which month is now shown.
        if (moved) CdsAnnouncer.Announce(m.MonthLabel);
    }

    void PickDay(DateOnly date)
    {
        if (_model is not { } m) return;
        var result = m.Pick(date);
        if (result == CalendarPick.Ignored) return;
        // Read both ends first: setting StartDate re-renders, which syncs the model from the (still old) EndDate.
        var (start, end) = (m.Start, m.End);
        _batching = true;
        try
        {
            StartDate = DatePickerFormat.ToDateTime(start);
            EndDate = DatePickerFormat.ToDateTime(end);
        }
        finally
        {
            _batching = false;
        }
        RaiseDateChanged();
        if (result == CalendarPick.Completed) IsOpen = false;
        else RenderCalendar();
    }

    /// <summary>The calendar's key handler (internal for tests): moves the cursor, picks, and announces the cursor day.</summary>
    internal bool OnCalendarKey(CdsKeyEventArgs e)
    {
        if (_model is not { } m) return false;
        switch (e.Key)
        {
            case CdsKey.Down when !_inCalendar:
                // Down moves from the field into the calendar, which then takes every navigation key.
                _inCalendar = true;
                _overlay?.SetKeys(CdsOverlayKeys.Navigation);
                RenderCalendar();
                CdsAnnouncer.Announce(m.CursorAnnouncement());
                return true;
            case CdsKey.PageUp or CdsKey.PageDown:
            case CdsKey.Left or CdsKey.Right or CdsKey.Up or CdsKey.Down or CdsKey.Home or CdsKey.End when _inCalendar:
                _inCalendar = true;
                m.MoveCursor(e.Key);
                RenderCalendar();
                // Virtual focus: the day is never focused, so say where the cursor is.
                CdsAnnouncer.Announce(m.CursorAnnouncement());
                return true;
            case CdsKey.Enter or CdsKey.Space when _inCalendar:
                PickDay(m.Cursor);
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
