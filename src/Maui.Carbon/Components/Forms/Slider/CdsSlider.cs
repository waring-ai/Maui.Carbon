namespace Maui.Carbon;

/// <summary>
/// A draggable thumb on a 2px track for choosing a number (or, with <see cref="IsRange"/>, a range) between
/// <see cref="Min"/> and <see cref="Max"/>, with an editable number box. Use it when the relative position matters more
/// than the exact figure; use a number input for very wide or tiny ranges. Drag a thumb, tap the track (the nearest
/// handle moves), or use the keyboard: arrows move by <see cref="Step"/>, Shift+arrow, PageUp and PageDown by
/// <see cref="Step"/> times <see cref="StepMultiplier"/>, Home and End jump to the ends. Each thumb is an adjustable
/// element whose value is the formatted value, so screen readers can step it too (VoiceOver swipe up and down, TalkBack
/// scroll forward and backward). The track is 200 to 640 wide.
/// </summary>
public partial class CdsSlider : CdsControl
{
    const double HostHeight = 40;
    const double MinTrack = 200;
    const double MaxTrack = 640;

    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly Grid _container = new() { ColumnSpacing = 0, HorizontalOptions = LayoutOptions.Fill, MaximumWidthRequest = 920 };
    readonly CdsText _minText = new() { VerticalOptions = LayoutOptions.Center };
    readonly CdsText _maxText = new() { VerticalOptions = LayoutOptions.Center };
    readonly Grid _track = new() { HeightRequest = HostHeight, VerticalOptions = LayoutOptions.Center, MinimumWidthRequest = MinTrack + 2 * CdsSliderGlyph.PadX, MaximumWidthRequest = MaxTrack + 2 * CdsSliderGlyph.PadX };
    readonly CdsSliderGlyph _glyph = new() { };
    readonly Grid _lowerWrap = new() { HeightRequest = 24, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
    readonly Grid _upperWrap = new() { HeightRequest = 24, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, IsVisible = false };
    readonly CdsHitTarget _lowerHit;
    readonly CdsHitTarget _upperHit;
    readonly CdsFieldChrome _lowerBox = new() { HideLabel = true, VerticalOptions = LayoutOptions.Center };
    readonly CdsFieldChrome _upperBox = new() { HideLabel = true, VerticalOptions = LayoutOptions.Center };
    readonly CdsNativeEntry _lowerEntry = new() { Keyboard = Keyboard.Numeric };
    readonly CdsNativeEntry _upperEntry = new() { Keyboard = Keyboard.Numeric };
    readonly CdsFormMessage _message = new() { Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0) };

    SliderHandle? _focused;
    SliderHandle? _pressed;
    SliderHandle? _hover;
    SliderHandle? _boxFocused;
    SliderHandle _dragHandle;
    bool _dragging;
    double _dragStart;
    bool _syncingText;
    bool _lowerOut;
    bool _upperOut;
    bool _typing;
    string? _announcedInvalid;

    /// <summary>Creates a single slider from 0 to 100.</summary>
    public CdsSlider()
    {
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        foreach (var width in new[] { GridLength.Auto, GridLength.Auto, GridLength.Star, GridLength.Auto, GridLength.Auto })
            _container.ColumnDefinitions.Add(new ColumnDefinition(width));

        _lowerBox.Input = _lowerEntry;
        _upperBox.Input = _upperEntry;
        _lowerBox.Field.HorizontalOptions = LayoutOptions.Fill;

        _lowerHit = CreateHitTarget(CdsAccessibilityRole.Adjustable);
        _upperHit = CreateHitTarget(CdsAccessibilityRole.Adjustable);
        _lowerWrap.Add(_lowerHit);
        _upperWrap.Add(_upperHit);
        _track.Add(_glyph);
        _track.Add(_upperWrap);
        _track.Add(_lowerWrap);
        TrackFocus(_lowerHit);
        Wire(_lowerHit, _lowerWrap, SliderHandle.Lower);
        Wire(_upperHit, _upperWrap, SliderHandle.Upper);
        WireBox(_lowerEntry, SliderHandle.Lower);
        WireBox(_upperEntry, SliderHandle.Upper);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPan;
        _track.GestureRecognizers.Add(pan);
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        _track.GestureRecognizers.Add(tap);
        _track.SizeChanged += (_, _) => UpdateThumbs();

        _container.Add(_lowerBox, 0);
        _container.Add(_minText, 1);
        _container.Add(_track, 2);
        _container.Add(_maxText, 3);
        _container.Add(_upperBox, 4);
        _root.Add(_label, 0, 0);
        _root.Add(_container, 0, 1);
        _root.Add(_message, 0, 2);

        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Raised when <see cref="Value"/> or <see cref="UpperValue"/> changes (by the user or in code).</summary>
    public event EventHandler<CdsSliderChangedEventArgs>? ValueChanged;

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (_focused is not { } handle || IsReadOnly || Is(CdsInteractionState.Disabled)) return false;
        var current = handle == SliderHandle.Lower ? Value : UpperValue;
        if (SliderModel.KeyboardValue(e.Key, e.Shift, current, Min, Max, Step, StepMultiplier) is not { } next) return false;
        SetFromUser(handle, next);
        e.Handled = true;
        CdsAnnouncer.Announce(FormatValue(handle == SliderHandle.Lower ? Value : UpperValue));
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var (_, thumbState) = ActiveThumb();
        var v = CdsSliderTokens.Resolve(state, thumbState, IsRange);
        SetColor(_glyph, CdsSliderGlyph.TrackColorProperty, v.Track);
        SetColor(_glyph, CdsSliderGlyph.FillColorProperty, v.Fill);
        SetColor(_glyph, CdsSliderGlyph.ThumbColorProperty, v.Thumb);
        SetColor(_glyph, CdsSliderGlyph.RingColorProperty, v.Ring);
        SetColor(_glyph, CdsSliderGlyph.GapColorProperty, v.Gap);
        SetColor(_minText, Microsoft.Maui.Controls.Label.TextColorProperty, v.RangeLabel);
        SetColor(_maxText, Microsoft.Maui.Controls.Label.TextColorProperty, v.RangeLabel);
        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);

        var boxState = state;
        var outOfRange = _lowerOut || _upperOut;
        if (IsInvalid || outOfRange) boxState |= CdsInteractionState.Invalid;
        if (IsWarning) boxState |= CdsInteractionState.Warn;
        ApplyBox(_lowerBox, _lowerEntry, boxState, SliderHandle.Lower);
        ApplyBox(_upperBox, _upperEntry, boxState, SliderHandle.Upper);

        var message = CdsFormMessageResolver.Resolve(boxState, null, InvalidText, WarnText);
        _message.Apply(message);
        AnnounceInvalid(message);

        _lowerHit.IsEnabled = !disabled;
        _upperHit.IsEnabled = !disabled;
        SemanticProperties.SetHint(_lowerEntry, message.Text);
        SemanticProperties.SetHint(_upperEntry, message.Text);
        UpdateThumbs();
    }

    // The invalid message is role="alert": speak it once, assertively, when it appears or changes. Once the slider is on
    // screen its CdsFormMessage already does that (CdsMessageAlert). Before then a slider built invalid stays quiet, but a
    // typed out-of-range value is the user's own edit and is announced here at once.
    void AnnounceInvalid(CdsFormMessageVisuals message)
    {
        var text = message.Kind == CdsFormMessageKind.Invalid ? message.Text : null;
        if (text == _announcedInvalid) return;
        _announcedInvalid = text;
        if (text is not null && _typing && !_message.IsLoaded) CdsAnnouncer.Announce(text, CdsAnnouncePriority.Assertive);
    }

    void ApplyBox(CdsFieldChrome box, CdsNativeEntry entry, CdsInteractionState state, SliderHandle handle)
    {
        if (_boxFocused == handle && !state.HasFlag(CdsInteractionState.Disabled)) state |= CdsInteractionState.Focused;
        box.Apply(state);
        var showIcon = state.HasFlag(CdsInteractionState.Invalid) || state.HasFlag(CdsInteractionState.Warn);
        var blocked = state.HasFlag(CdsInteractionState.Disabled) || state.HasFlag(CdsInteractionState.ReadOnly);
        box.WidthRequest = showIcon && !blocked ? 96 : 64;
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        entry.IsEnabled = !disabled;
        entry.IsReadOnly = IsReadOnly;
        SetColor(entry, Entry.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
    }

    void UpdateLayout()
    {
        _label.Text = Label;
        _label.IsVisible = !HideLabel && !string.IsNullOrEmpty(Label);
        _upperWrap.IsVisible = IsRange;
        var boxes = !HideTextInput;
        _lowerBox.IsVisible = boxes;
        _upperBox.IsVisible = boxes && IsRange;
        if (IsRange)
        {
            Grid.SetColumn(_lowerBox, 0);
            _lowerBox.Margin = new Thickness(0, 0, CdsSpacing.Spacing05, 0);
            _upperBox.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
        }
        else
        {
            Grid.SetColumn(_lowerBox, 4);
            _lowerBox.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
        }
        var showThumbs = !IsReadOnly;
        _lowerHit.IsReadOnly = IsReadOnly;
        _upperHit.IsReadOnly = IsReadOnly;
        _lowerWrap.IsVisible = showThumbs;
        _upperWrap.IsVisible = showThumbs && IsRange;
        UpdateTexts();
        UpdateAccessibility();
        RefreshVisuals();
    }

    void OnBoundsChanged()
    {
        // CoerceValue does not touch a property still at its default, so assign the clamped values explicitly.
        var lower = CoerceToRange(Value);
        if (lower != Value) Value = lower;
        var upper = CoerceToRange(UpperValue);
        if (upper != UpperValue) UpperValue = upper;
        UpdateTexts();
        RefreshVisuals();
    }

    double CoerceToRange(double value) => SliderModel.Clamp(value, Min, Max);

    string FormatValue(double value) => ValueFormatter?.Invoke(value) ?? SliderModel.Format(value, Step);

    string FormatLabel(double value, string? suffix) => ValueFormatter?.Invoke(value) ?? SliderModel.Format(value, Step) + suffix;

    void UpdateTexts()
    {
        _minText.Text = FormatLabel(Min, MinLabel);
        _maxText.Text = FormatLabel(Max, MaxLabel);
        SyncBoxText(SliderHandle.Lower);
        SyncBoxText(SliderHandle.Upper);
        // The formatter and the step also shape the thumbs' value text.
        UpdateAccessibility();
    }

    void UpdateAccessibility()
    {
        var lowerName = IsRange ? InputLabel ?? "Lower bound" : InputLabel ?? Label;
        var upperName = InputLabelUpper ?? "Upper bound";
        SemanticProperties.SetDescription(_lowerHit, lowerName);
        SemanticProperties.SetDescription(_upperHit, upperName);
        // aria-valuetext: the element's value, not a hint, so VoiceOver reads it right after the name.
        _lowerHit.AccessibleValue = FormatValue(Value);
        _upperHit.AccessibleValue = FormatValue(UpperValue);
        SemanticProperties.SetDescription(_lowerEntry, lowerName ?? Label);
        SemanticProperties.SetDescription(_upperEntry, upperName);
    }

    void OnValuesChanged()
    {
        SyncBoxText(SliderHandle.Lower);
        SyncBoxText(SliderHandle.Upper);
        UpdateAccessibility();
        UpdateThumbs();
        ValueChanged?.Invoke(this, new CdsSliderChangedEventArgs(Value, UpperValue));
    }

    // Moves a handle to a snapped value, keeping a range ordered.
    void SetFromUser(SliderHandle handle, double value)
    {
        var snapped = SliderModel.Snap(value, Min, Max, Step);
        if (!IsRange)
        {
            Value = snapped;
            return;
        }
        var (lower, upper) = SliderModel.OrderRange(
            handle == SliderHandle.Lower ? snapped : Value,
            handle == SliderHandle.Upper ? snapped : UpperValue,
            handle);
        if (lower != Value) Value = lower;
        if (upper != UpperValue) UpperValue = upper;
    }

    // ---- thumbs ------------------------------------------------------------------------------------------------

    (SliderHandle Handle, CdsSliderThumbState State) ActiveThumb()
    {
        if (_dragging) return (_dragHandle, CdsSliderThumbState.Pressed);
        if (_pressed is { } p) return (p, CdsSliderThumbState.Pressed);
        if (_focused is { } f) return (f, CdsSliderThumbState.Focus);
        if (_hover is { } h) return (h, CdsSliderThumbState.Hover);
        return (SliderHandle.Lower, CdsSliderThumbState.Normal);
    }

    double FractionOf(SliderHandle handle) => SliderModel.ToFraction(handle == SliderHandle.Lower ? Value : UpperValue, Min, Max);

    void UpdateThumbs()
    {
        var (handle, thumbState) = ActiveThumb();
        var lowerFraction = FractionOf(SliderHandle.Lower);
        var upperFraction = FractionOf(SliderHandle.Upper);
        if (IsRange && lowerFraction > upperFraction) (lowerFraction, upperFraction) = (upperFraction, lowerFraction);
        _glyph.Set(lowerFraction, upperFraction, IsRange, !IsReadOnly, handle, thumbState);

        var length = _glyph.TrackLength;
        var lowerX = CdsSliderGlyph.PadX + lowerFraction * length;
        var upperX = CdsSliderGlyph.PadX + upperFraction * length;
        if (IsRange)
        {
            _lowerWrap.WidthRequest = 16;
            _upperWrap.WidthRequest = 16;
            _lowerWrap.TranslationX = lowerX - 16;
            _upperWrap.TranslationX = upperX;
        }
        else
        {
            _lowerWrap.WidthRequest = 24;
            _lowerWrap.TranslationX = lowerX - 12;
        }
    }

    // A screen reader's increment or decrement steps like the Right and Left arrows; it reads the new value itself.
    void AdjustFromAssistiveTech(SliderHandle handle, bool increase)
    {
        if (IsReadOnly || !IsEnabled) return;
        var current = handle == SliderHandle.Lower ? Value : UpperValue;
        if (SliderModel.KeyboardValue(increase ? CdsKey.Right : CdsKey.Left, false, current, Min, Max, Step, StepMultiplier) is { } next)
            SetFromUser(handle, next);
    }

    void Wire(CdsHitTarget hit, Grid wrap, SliderHandle handle)
    {
        hit.Incremented += (_, _) => AdjustFromAssistiveTech(handle, increase: true);
        hit.Decremented += (_, _) => AdjustFromAssistiveTech(handle, increase: false);
        hit.Focused += (_, _) =>
        {
            _focused = handle;
            CdsFocusTracker.SetFocused(this);
            RefreshVisuals();
        };
        hit.Unfocused += (_, _) =>
        {
            if (_focused == handle) _focused = null;
            RefreshVisuals();
        };
        hit.Pressed += (_, _) =>
        {
            _pressed = handle;
            RefreshVisuals();
        };
        hit.Released += (_, _) =>
        {
            if (!_dragging) _pressed = null;
            RefreshVisuals();
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            _hover = handle;
            RefreshVisuals();
        };
        pointer.PointerExited += (_, _) =>
        {
            if (_hover == handle) _hover = null;
            RefreshVisuals();
        };
        wrap.GestureRecognizers.Add(pointer);
    }

    void OnPan(object? sender, PanUpdatedEventArgs e)
    {
        if (!IsEnabled || IsReadOnly) return;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _dragHandle = IsRange ? _pressed ?? _focused ?? SliderHandle.Lower : SliderHandle.Lower;
                _dragStart = FractionOf(_dragHandle);
                _dragging = true;
                RefreshVisuals();
                break;
            case GestureStatus.Running when _dragging:
                var length = _glyph.TrackLength;
                if (length <= 0) return;
                SetFromUser(_dragHandle, SliderModel.FromFraction(_dragStart + e.TotalX / length, Min, Max, Step));
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _dragging = false;
                _pressed = null;
                RefreshVisuals();
                break;
        }
    }

    void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled || IsReadOnly || e.GetPosition(_track) is not { } point) return;
        var length = _glyph.TrackLength;
        if (length <= 0) return;
        // Taps on a thumb are the thumb's own (it takes focus); only the bare track moves the nearest handle.
        foreach (var handle in IsRange ? new[] { SliderHandle.Lower, SliderHandle.Upper } : [SliderHandle.Lower])
        {
            var x = CdsSliderGlyph.PadX + FractionOf(handle) * length;
            if (Math.Abs(point.X - x) <= 10) return;
        }
        var fraction = (point.X - CdsSliderGlyph.PadX) / length;
        var value = SliderModel.FromFraction(fraction, Min, Max, Step);
        var target = IsRange ? SliderModel.NearestHandle(value, Value, UpperValue) : SliderHandle.Lower;
        SetFromUser(target, value);
        (target == SliderHandle.Lower ? _lowerHit : _upperHit).Focus();
    }

    // ---- number boxes ------------------------------------------------------------------------------------------

    void WireBox(CdsNativeEntry entry, SliderHandle handle)
    {
        entry.TextChanged += (_, e) => OnBoxTextChanged(handle, e.NewTextValue);
        entry.Focused += (_, _) =>
        {
            _boxFocused = handle;
            RefreshVisuals();
        };
        entry.Unfocused += (_, _) =>
        {
            if (_boxFocused == handle) _boxFocused = null;
            CommitBox(handle);
        };
        entry.Completed += (_, _) => CommitBox(handle);
    }

    CdsNativeEntry EntryOf(SliderHandle handle) => handle == SliderHandle.Lower ? _lowerEntry : _upperEntry;

    void SyncBoxText(SliderHandle handle)
    {
        var entry = EntryOf(handle);
        var text = SliderModel.Format(handle == SliderHandle.Lower ? Value : UpperValue, Step);
        // A box being typed in keeps its text until it commits.
        if (_syncingText || entry.Text == text || (_boxFocused == handle && (handle == SliderHandle.Lower ? _lowerOut : _upperOut))) return;
        _syncingText = true;
        try
        {
            entry.Text = text;
        }
        finally
        {
            _syncingText = false;
        }
    }

    void OnBoxTextChanged(SliderHandle handle, string? text)
    {
        if (_syncingText) return;
        var parsed = SliderModel.TryParse(text, out var value);
        var out_ = parsed && SliderModel.IsOutOfRange(value, Min, Max);
        if (handle == SliderHandle.Lower) _lowerOut = out_;
        else _upperOut = out_;
        _typing = true;
        try
        {
            if (parsed && !out_)
            {
                // Accept an in-range number as typed; it is snapped when the box commits.
                _syncingText = true;
                try
                {
                    SetFromUser(handle, value);
                }
                finally
                {
                    _syncingText = false;
                }
            }
            RefreshVisuals();
        }
        finally
        {
            _typing = false;
        }
    }

    // On blur or Return: clamp, snap and rewrite the text.
    void CommitBox(SliderHandle handle)
    {
        var entry = EntryOf(handle);
        if (SliderModel.TryParse(entry.Text, out var value)) SetFromUser(handle, value);
        _lowerOut = false;
        _upperOut = false;
        SyncBoxText(handle);
        RefreshVisuals();
    }
}
