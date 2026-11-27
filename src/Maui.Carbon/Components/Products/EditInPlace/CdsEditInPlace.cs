using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>Event data of <see cref="CdsEditInPlace"/>'s save, cancel and blur events.</summary>
public sealed class CdsEditInPlaceEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsEditInPlaceEventArgs(string value, string initialValue)
    {
        Value = value;
        InitialValue = initialValue;
    }

    /// <summary>The value in the field when the event was raised (for a cancel: the value that was discarded).</summary>
    public string Value { get; }

    /// <summary>The value the edit started from (Carbon's <c>initialValue</c>, passed to <c>onCancel</c> and <c>onBlur</c>).</summary>
    public string InitialValue { get; }
}

/// <summary>
/// A value that reads as text and turns into an input in place (Carbon for IBM Products' <c>EditInPlace</c>): titles,
/// names and short settings people rename without opening a form. At rest it is transparent with an edit button that
/// shows on hover (or always, with <see cref="EditAlwaysVisible"/>); hover and edit mode fill it with <c>field-01</c>.
/// Editing shows cancel and save icon buttons, enabled once the trimmed value changed (save also needs the value to be
/// valid). Enter saves and stays in edit mode; Escape restores the value the edit started from; moving focus away saves a
/// valid change and cancels anything else, unless <see cref="Blurred"/> has handlers. Use a form with a
/// <see cref="CdsTextInput"/> when several values change together or need helper text.
/// </summary>
public partial class CdsEditInPlace : CdsControl
{
    readonly Grid _root = new() { RowSpacing = 0 };
    readonly Grid _box = new() { ColumnSpacing = 0 };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly Border _outline = new()
    {
        StrokeThickness = 2,
        StrokeShape = new Rectangle(),
        BackgroundColor = Colors.Transparent,
        Padding = 0,
        InputTransparent = true,
    };
    readonly CdsNativeEntry _entry = new() { Margin = new Thickness(CdsSpacing.Spacing05, 0) };
    readonly ContentView _clip = new() { IsClippedToBounds = true, HorizontalOptions = LayoutOptions.End };
    readonly HorizontalStackLayout _tools = new() { Spacing = 0, HorizontalOptions = LayoutOptions.Start };
    readonly CdsActionFace _edit;
    readonly CdsIconTooltip _editTip;
    readonly CdsMessageAlert _alert = new();
    CdsActionFace? _cancel;
    CdsIconTooltip? _cancelTip;
    CdsActionFace? _save;
    CdsIconTooltip? _saveTip;
    Grid? _readOnlyCell;
    CdsToggletip? _readOnlyTip;
    CdsIcon? _warning;
    CdsText? _message;
    string _initial = "";
    bool _editing;
    bool _syncing;
    bool _typing;
    bool _suppressBlur;
    bool _blurPending;

    /// <summary>Creates a small edit-in-place field.</summary>
    public CdsEditInPlace()
    {
        _entry.TextChanged += OnEntryTextChanged;
        _entry.Completed += (_, _) => OnEnter();
        _entry.Focused += (_, _) => BeginEditing();
        _entry.Unfocused += (_, _) => ScheduleBlurCheck();

        (_edit, _editTip) = CreateButton(CdsIconName.Edit, () => _entry.Focus());
        _tools.Add(_edit);
        _clip.Content = _tools;

        AutomationProperties.SetExcludedWithChildren(_fill, true);
        AutomationProperties.SetExcludedWithChildren(_outline, true);
        _box.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _box.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _box.Add(_fill);
        Grid.SetColumnSpan(_fill, 2);
        _box.Add(_entry, 0);
        _box.Add(_clip, 1);
        _box.Add(_outline);
        Grid.SetColumnSpan(_outline, 2);

        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.Add(_box, 0, 0);

        TrackFocus(_entry);
        SetChrome(_root, fillsWidth: true);
        UpdateChrome();
    }

    /// <summary>Raised when the value changes (typing, a cancel restoring it, or <see cref="Value"/> set in code).</summary>
    public event EventHandler<TextChangedEventArgs>? ValueChanged;

    /// <summary>
    /// Raised when a change is saved: the save button, Enter, or focus leaving with a valid change. The value becomes the one
    /// the next cancel returns to. <see cref="SaveCommand"/> runs after it with the value as parameter.
    /// </summary>
    public event EventHandler<CdsEditInPlaceEventArgs>? Saved;

    /// <summary>
    /// Raised when a change is cancelled: the cancel button, Escape, or focus leaving without a valid change. The field has
    /// already restored <see cref="CdsEditInPlaceEventArgs.InitialValue"/>. <see cref="CancelCommand"/> runs after it.
    /// </summary>
    public event EventHandler<CdsEditInPlaceEventArgs>? Canceled;

    /// <summary>
    /// Raised when focus leaves the field while editing (Carbon's custom <c>onBlur</c>). When it has handlers they replace
    /// the default (save a valid change, else cancel): the field leaves edit mode and keeps its value, and the handler
    /// decides, for example by setting <see cref="Value"/> back to <see cref="CdsEditInPlaceEventArgs.InitialValue"/>.
    /// </summary>
    public event EventHandler<CdsEditInPlaceEventArgs>? Blurred;

    /// <summary>True while the field is in edit mode (its input or one of its buttons has focus).</summary>
    public bool IsEditing => _editing;

    /// <summary>The value the current edit started from (the last saved value).</summary>
    internal string InitialValue => _initial;

    /// <summary>Whether the save button is enabled.</summary>
    internal bool CanSave => EditInPlaceModel.CanSave(Value, _initial, IsInvalid);

    /// <summary>Whether the cancel button is enabled.</summary>
    internal bool CanCancel => EditInPlaceModel.CanCancel(Value, _initial);

    /// <summary>The native input (for tests).</summary>
    internal CdsNativeEntry Input => _entry;

    /// <summary>The toolbar buttons now in place: edit, cancel and save (for tests).</summary>
    internal (CdsActionFace Edit, CdsActionFace? Cancel, CdsActionFace? Save) Buttons => (_edit, _cancel, _save);

    /// <summary>The read-only toggletip, once read only (for tests).</summary>
    internal CdsToggletip? ReadOnlyToggletip => _readOnlyTip;

    /// <summary>The error message under the field, once shown (for tests).</summary>
    internal CdsText? Message => _message;

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => _editing && !IsReadOnly;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || !_editing || IsReadOnly) return false;
        // Carbon: Escape restores the value the edit started from and keeps the input focused.
        Cancel(exit: false);
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_fill, BoxView.ColorProperty, CdsEditInPlaceTokens.Background(State, _editing));
        SetBrush(_outline, Border.StrokeProperty, CdsEditInPlaceTokens.Outline(_entry.IsFocused && !disabled, _editing, IsInvalid));
        SetColor(_entry, Entry.TextColorProperty, CdsEditInPlaceTokens.Text(disabled));
        SetColor(_entry, Entry.PlaceholderColorProperty, CdsEditInPlaceTokens.Placeholder(disabled));
        if (_warning is not null) SetColor(_warning, CdsIcon.ColorProperty, CdsEditInPlaceTokens.Invalid);
        if (_message is not null) SetColor(_message, Microsoft.Maui.Controls.Label.TextColorProperty, CdsEditInPlaceTokens.Invalid);
        // The edit button shows on hover only (Carbon's visibility: hidden keeps its room), or always.
        _edit.IsVisible = !_editing && !IsReadOnly && !disabled && (EditAlwaysVisible || Is(CdsInteractionState.Hover));
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _edit is not null)
        {
            _entry.IsEnabled = IsEnabled;
            UpdateButtons();
        }
    }

    (CdsActionFace Face, CdsIconTooltip Tip) CreateButton(CdsIconName icon, Action activated)
    {
        var face = new CdsActionFace
        {
            Mode = CdsActionFaceMode.IconOnly,
            Icon = icon,
            Resolver = static s => CdsIconButtonTokens.Resolve(CdsIconButtonKind.Ghost, s),
            RingStyle = CdsIconButtonTokens.FocusRing(CdsIconButtonKind.Ghost),
            VerticalOptions = LayoutOptions.Center,
        };
        var tip = new CdsIconTooltip(face, face);
        face.StateChanged += (_, _) => tip.StateChanged(face);
        face.Activated += (_, _) => activated();
        face.Hit.Unfocused += (_, _) => ScheduleBlurCheck();
        return (face, tip);
    }

    void EnsureEditButtons()
    {
        if (_cancel is not null) return;
        (_cancel, _cancelTip) = CreateButton(CdsIconName.Close, () => Cancel(exit: false));
        (_save, _saveTip) = CreateButton(CdsIconName.Checkmark, () => Save(exit: false));
        _tools.Add(_cancel);
        _tools.Add(_save);
    }

    void EnsureReadOnlyCell()
    {
        if (_readOnlyCell is not null) return;
        _readOnlyTip = new CdsToggletip { Icon = CdsIconName.EditOff, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _readOnlyCell = new Grid();
        _readOnlyCell.Add(_readOnlyTip);
        _tools.Add(_readOnlyCell);
    }

    /// <summary>Re-applies texts, sizes, modes and accessibility after a property change.</summary>
    void UpdateChrome()
    {
        var side = EditInPlaceModel.Side(Size);
        _box.HeightRequest = side;
        _entry.HeightRequest = EditInPlaceModel.InputHeight(Size);
        _entry.Placeholder = Placeholder;
        _entry.IsReadOnly = IsReadOnly;
        CdsTypography.Apply(_entry, TypeStyle);
        SemanticProperties.SetDescription(_entry, Label);
        SemanticProperties.SetHint(_entry, IsInvalid ? InvalidText : null);

        if (IsInvalid)
        {
            _warning ??= new CdsIcon(CdsIconName.WarningFilled, 16)
            {
                Margin = new Thickness(CdsSpacing.Spacing03, 0),
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };
            if (_warning.Parent is null) _tools.Insert(0, _warning);
        }
        if (_warning is not null) _warning.IsVisible = IsInvalid;

        if (IsInvalid && !string.IsNullOrEmpty(InvalidText))
        {
            if (_message is null)
            {
                _message = new CdsText { TypeStyle = CdsTypeStyle.Label01, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0) };
                _root.Add(_message, 0, 1);
            }
            _message.Text = InvalidText;
            _message.IsVisible = true;
        }
        else if (_message is not null)
        {
            _message.IsVisible = false;
        }
        _alert.Update(this, IsInvalid ? InvalidText : null);

        if (IsReadOnly)
        {
            EnsureReadOnlyCell();
            _readOnlyCell!.WidthRequest = side;
            _readOnlyCell.HeightRequest = side;
            _readOnlyTip!.Label = ReadOnlyLabel;
            _readOnlyTip.Text = ReadOnlyToggletipText;
            _readOnlyTip.Placement = ToggletipPlacement;
        }
        if (_readOnlyCell is not null) _readOnlyCell.IsVisible = IsReadOnly;

        _edit.ButtonHeight = side;
        _edit.AccessibleName = EditLabel;
        _editTip.Text = EditLabel;
        _editTip.Placement = EditTooltipPlacement ?? TooltipPlacement;
        if (_cancel is not null)
        {
            _cancel.ButtonHeight = side;
            _cancel.AccessibleName = CancelLabel;
            _cancelTip!.Text = CancelLabel;
            _cancelTip.Placement = CancelTooltipPlacement ?? TooltipPlacement;
            _save!.ButtonHeight = side;
            _save.AccessibleName = SaveLabel;
            _saveTip!.Text = SaveLabel;
            _saveTip.Placement = SaveTooltipPlacement ?? TooltipPlacement;
        }
        _clip.HeightRequest = side;
        UpdateButtons();
        UpdateToolbarWidth(animate: false);
        RefreshVisuals();
    }

    void UpdateButtons()
    {
        var editing = _editing && !IsReadOnly;
        if (editing) EnsureEditButtons();
        if (_cancel is not null)
        {
            _cancel.IsVisible = editing;
            _save!.IsVisible = editing;
            _cancel.IsEnabled = IsEnabled && CanCancel;
            _save.IsEnabled = IsEnabled && CanSave;
            if (_cancel.ButtonHeight != EditInPlaceModel.Side(Size))
            {
                _cancel.ButtonHeight = EditInPlaceModel.Side(Size);
                _save.ButtonHeight = EditInPlaceModel.Side(Size);
            }
            _cancel.AccessibleName = CancelLabel;
            _save.AccessibleName = SaveLabel;
            _cancelTip!.Text = CancelLabel;
            _saveTip!.Text = SaveLabel;
        }
        _edit.IsEnabled = IsEnabled;
        RefreshVisuals();
    }

    void UpdateToolbarWidth(bool animate)
    {
        var target = EditInPlaceModel.ToolbarWidth(Size, _editing, IsInvalid, IsReadOnly);
        var from = _clip.WidthRequest > 0 ? _clip.WidthRequest : target;
        if (!animate || from >= target)
        {
            _clip.AbortAnimation(nameof(CdsEditInPlace));
            _clip.WidthRequest = target;
            return;
        }
        // Carbon's slide-in: the toolbar grows from one square to two (moderate-01, entrance productive).
        _ = CdsMotion.AnimateAsync(_clip, nameof(CdsEditInPlace), w => _clip.WidthRequest = w, from, target,
            CdsDuration.Moderate01, CdsEasing.EntranceProductive);
    }

    /// <summary>Enters edit mode (the input got focus).</summary>
    internal void BeginEditing()
    {
        if (_editing) return;
        _editing = true;
        UpdateButtons();
        UpdateToolbarWidth(animate: !IsReadOnly);
        RefreshVisuals();
    }

    void EndEditing()
    {
        if (!_editing) return;
        _editing = false;
        UpdateButtons();
        UpdateToolbarWidth(animate: false);
        RefreshVisuals();
    }

    void OnEnter()
    {
        if (!_editing || IsReadOnly) return;
        _suppressBlur = true;
        if (EditInPlaceModel.Enter(CanSave) == EditInPlaceOutcome.Save)
        {
            Save(exit: false);
            Defer(() => _suppressBlur = false);
        }
        else
        {
            _entry.Unfocus();
            EndEditing();
            Defer(() => _suppressBlur = false);
        }
    }

    /// <summary>Saves the value: it becomes the initial value; leaves edit mode when <paramref name="exit"/>, else keeps the input focused.</summary>
    internal void Save(bool exit)
    {
        var value = Value ?? "";
        var previous = _initial;
        _initial = value;
        UpdateButtons();
        Saved?.Invoke(this, new CdsEditInPlaceEventArgs(value, previous));
        if (SaveCommand?.CanExecute(value) == true) SaveCommand.Execute(value);
        if (exit) EndEditing();
        else Defer(() => { if (IsLoaded) _entry.Focus(); });
    }

    /// <summary>Restores the initial value; leaves edit mode when <paramref name="exit"/>, else keeps the input focused with the caret at the end.</summary>
    internal void Cancel(bool exit)
    {
        var discarded = Value ?? "";
        var initial = _initial;
        Value = initial;
        UpdateButtons();
        Canceled?.Invoke(this, new CdsEditInPlaceEventArgs(discarded, initial));
        if (CancelCommand?.CanExecute(initial) == true) CancelCommand.Execute(initial);
        if (exit) EndEditing();
        else Defer(() =>
        {
            if (!IsLoaded) return;
            _entry.Focus();
            _entry.CursorPosition = _entry.Text?.Length ?? 0;
        });
    }

    void ScheduleBlurCheck()
    {
        if (_blurPending) return;
        _blurPending = true;
        // Focus moving to one of the toolbar buttons arrives after the input lost it: decide once it settled.
        Defer(() =>
        {
            _blurPending = false;
            CheckBlur();
        });
    }

    static bool HasFocus(CdsActionFace? face) => face is not null && (face.Hit.IsFocused || face.Is(CdsInteractionState.Pressed));

    void CheckBlur()
    {
        if (!_editing || _suppressBlur) return;
        if (_entry.IsFocused || HasFocus(_edit) || HasFocus(_cancel) || HasFocus(_save)) return;
        HandleBlur();
    }

    /// <summary>Focus left the component while editing: the app's handler, else save a valid change, else cancel.</summary>
    internal void HandleBlur()
    {
        if (!_editing) return;
        if (IsReadOnly)
        {
            EndEditing();
            return;
        }
        switch (EditInPlaceModel.Blur(Blurred is not null, CanSave))
        {
            case EditInPlaceOutcome.Custom:
                Blurred?.Invoke(this, new CdsEditInPlaceEventArgs(Value ?? "", _initial));
                EndEditing();
                break;
            case EditInPlaceOutcome.Save:
                Save(exit: true);
                break;
            default:
                Cancel(exit: true);
                break;
        }
    }

    // After the current focus change settled. Off screen (no handler) there is nothing to wait for: run now.
    void Defer(Action action)
    {
        if (Handler is not null && Dispatcher is { } dispatcher) dispatcher.Dispatch(action);
        else action();
    }

    void OnValuePropertyChanged(string? oldValue, string? newValue)
    {
        if (_entry.Text != newValue)
        {
            _syncing = true;
            try
            {
                _entry.Text = newValue;
            }
            finally
            {
                _syncing = false;
            }
        }
        // A value set while not editing is the new starting point (Carbon keeps it until a save).
        if (!_editing && !_typing) _initial = newValue ?? "";
        UpdateButtons();
        ValueChanged?.Invoke(this, new TextChangedEventArgs(oldValue, newValue));
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_syncing) return;
        _typing = true;
        try
        {
            Value = e.NewTextValue;
        }
        finally
        {
            _typing = false;
        }
    }
}
