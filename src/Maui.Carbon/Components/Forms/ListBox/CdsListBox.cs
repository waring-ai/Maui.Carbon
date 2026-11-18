using System.Collections;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The shared list-box layer behind <see cref="CdsDropdown"/>, <see cref="CdsComboBox"/> and <see cref="CdsMultiSelect"/>
/// (Carbon's <c>cds--list-box</c>): a field (<see cref="CdsFieldChrome"/>) whose trigger is either a hit target with the
/// combo-box role or a native text entry, a chevron that turns over while the list is open, and the open list as an overlay
/// that matches the field's width.
/// <para>
/// The list uses <b>virtual focus</b>: keyboard focus stays on the trigger while a highlighted index moves through the rows
/// (disabled rows are skipped). Rows match the field height, show about five and a half rows and then scroll, and are
/// clickable without taking focus. Subclass it to build a new list-box control: call <see cref="BuildField"/> from the
/// constructor with the trigger, override <see cref="CdsFormField.ApplyInput"/> for state colors, and override the
/// protected hooks.
/// </para>
/// </summary>
public abstract partial class CdsListBox : CdsFormField
{
    /// <summary>The chevron at the end of the field; it rotates 180 degrees while the list is open.</summary>
    protected readonly CdsIcon Chevron = new(CdsIconName.ChevronDown, 16);

    /// <summary>The 24px box around <see cref="Chevron"/>; tapping it toggles the list.</summary>
    protected readonly Grid ChevronHost = new()
    {
        WidthRequest = CdsListBoxTokens.ButtonSize,
        HeightRequest = CdsListBoxTokens.ButtonSize,
        VerticalOptions = LayoutOptions.Center,
        BackgroundColor = Colors.Transparent,
        Margin = new Thickness(CdsSpacing.Spacing03, 0, -CdsSpacing.Spacing03, 0),
    };

    readonly List<ListBoxEntry> _all = [];
    readonly List<object?> _items = [];
    readonly ListBoxModel _model = new();
    CdsListBoxList? _list;
    CdsOverlay? _overlay;
    VisualElement? _trigger;
    INotifyCollectionChanged? _observed;
    bool _keyboardHighlight;
    bool _openByKeyboard;
    bool _waitingForSize;
    bool _hidingForEmpty;

    /// <summary>Creates the list-box scaffold. Derived constructors call <see cref="BuildField"/>.</summary>
    protected CdsListBox()
    {
        ChevronHost.Add(Chevron);
        AutomationProperties.SetExcludedWithChildren(ChevronHost, true);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => OnChevronTapped();
        ChevronHost.GestureRecognizers.Add(tap);
        Chrome.Trailing.Add(ChevronHost);

        Loaded += (_, _) => Dispatcher.Dispatch(SyncOverlay);
        Unloaded += (_, _) => HideOverlay(keepOpenFlag: true);
    }

    /// <summary>Raised after the list opened.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised after the list closed (a row was chosen, Escape, light dismiss, or <see cref="IsOpen"/> set to false).</summary>
    public event EventHandler? Closed;

    /// <summary>The rows currently shown, in order (the filtered, ordered rows), for subclasses and tests.</summary>
    internal IReadOnlyList<ListBoxEntry> VisibleRows => _model.Rows;

    /// <summary>Every row built from <see cref="ItemsSource"/>, before filtering.</summary>
    internal IReadOnlyList<ListBoxEntry> AllRows => _all;

    /// <summary>The virtual-focus model.</summary>
    internal ListBoxModel Model => _model;

    /// <summary>The open overlay, or <c>null</c> while the list is closed.</summary>
    protected CdsOverlay? ListOverlay => _overlay;

    /// <summary>True while the list is shown.</summary>
    protected bool IsListShown => _overlay is not null;

    /// <summary>The element that holds keyboard focus (the hit target, or the text entry).</summary>
    protected VisualElement? Trigger => _trigger;

    /// <inheritdoc/>
    protected override VisualElement InputElement => _trigger!;

    /// <summary>Whether the list can open right now (enabled and not read only).</summary>
    protected bool CanOpen => IsEnabled && !IsReadOnly;

    /// <summary>Whether a native text field is the trigger (combo box, filterable multi-select).</summary>
    protected virtual bool HasTextField => false;

    /// <summary>Whether choosing a row keeps the list open (multi-select).</summary>
    protected virtual bool IsMultiple => false;

    /// <summary>Whether the field is drawn as the inline type (no fill, label to the start).</summary>
    protected bool UsesInlineType => AllowsInline && (Type == CdsListBoxType.Inline || IsInline);

    /// <summary>Whether this control has an inline variant (the combo box does not).</summary>
    protected virtual bool AllowsInline => true;

    /// <summary>
    /// Puts the trigger into the field: <paramref name="input"/> goes into the field box and <paramref name="trigger"/> (a
    /// hit target created with <c>CreateHitTarget(CdsAccessibilityRole.ComboBox)</c>, or a <see cref="CdsNativeEntry"/>)
    /// becomes the focus target. Call it once from the derived constructor, then <see cref="CdsFormField"/>'s UpdateChrome.
    /// </summary>
    protected void BuildField(View input, VisualElement trigger)
    {
        _trigger = trigger;
        Chrome.Input = input;
        Func<bool>? keyboard = trigger is CdsHitTarget hit ? () => hit.IsFocusFromKeyboard : null;
        TrackFocus(trigger, keyboard);
        trigger.Unfocused += (_, _) =>
        {
            // Tab (or any focus move) closes the list without changing the selection.
            if (_overlay is not null) IsOpen = false;
        };
        SetChrome(Chrome);
    }

    /// <summary>Hit target of the trigger when it is not a text entry.</summary>
    CdsHitTarget? TriggerHit => _trigger as CdsHitTarget;

    // ---- items ----

    void OnItemsSourceChanged(IEnumerable? source)
    {
        if (_observed is not null) _observed.CollectionChanged -= OnSourceCollectionChanged;
        _observed = source as INotifyCollectionChanged;
        if (_observed is not null) _observed.CollectionChanged += OnSourceCollectionChanged;
        RebuildItems();
    }

    void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildItems();

    /// <summary>The text of an item: <see cref="ItemToString"/>, else <c>ToString()</c>.</summary>
    protected string TextOf(object? item) => ItemToString?.Invoke(item) ?? item?.ToString() ?? string.Empty;

    void RebuildItems()
    {
        _items.Clear();
        _all.Clear();
        if (ItemsSource is { } source)
        {
            foreach (var item in source)
            {
                _items.Add(item);
                _all.Add(new ListBoxEntry(item, TextOf(item), IsItemDisabled?.Invoke(item) ?? false));
            }
        }
        OnItemsChanged();
        RefreshRows(keepHighlight: false);
    }

    /// <summary>Called after the items were rebuilt (subclasses reconcile their selection).</summary>
    protected virtual void OnItemsChanged()
    {
    }

    /// <summary>The visible rows for the current state (filtering and ordering applied by the subclass).</summary>
    internal virtual IReadOnlyList<ListBoxEntry> BuildRows() => _all;

    /// <summary>Whether a row is the chosen one (single select) or checked (multi-select).</summary>
    internal abstract bool IsRowChosen(ListBoxEntry row);

    /// <summary>Chooses (or toggles) the row at <paramref name="index"/> of <see cref="VisibleRows"/>.</summary>
    internal abstract void ChooseRow(int index);

    /// <summary>The item the highlight lands on when the list opens (the selected item).</summary>
    internal virtual object? HighlightOnOpen => null;

    /// <summary>Re-reads the rows (filter or selection changed) and updates the open list.</summary>
    protected void RefreshRows(bool keepHighlight)
    {
        _model.SetRows(BuildRows(), keepHighlight);
        if (_list is not null)
        {
            if (_model.Rows.Count == 0)
            {
                HideOverlay(keepOpenFlag: true);
                return;
            }
            _list.Build(_model.Rows, i => IsRowChosen(_model.Rows[i]), CreateRowContent);
            RefreshList(scroll: true);
        }
        else if (IsOpen)
        {
            SyncOverlay();
        }
    }

    View? CreateRowContent(int index, ListBoxEntry entry)
    {
        if (ItemTemplate is not { } template) return null;
        var chosen = template is DataTemplateSelector selector ? selector.SelectTemplate(entry.Value!, this) : template;
        return chosen.CreateContent() is View view ? Bind(view, entry.Value) : null;
    }

    static View Bind(View view, object? item)
    {
        view.BindingContext = item;
        return view;
    }

    void RefreshList(bool scroll)
    {
        if (_list is null) return;
        _list.Refresh(_model.Highlighted, _keyboardHighlight, i => i < _model.Rows.Count && IsRowChosen(_model.Rows[i]));
        if (scroll) _list.ScrollTo(_model.Highlighted);
        if (TriggerHit is { } hit && _model.HighlightedRow is { } row) SemanticProperties.SetHint(hit, row.Text);
    }

    /// <summary>Highlights the first enabled row with the focus ring (a combo box does this as the filter changes); no-op while closed.</summary>
    protected void HighlightFirstRow()
    {
        _model.MoveFirst();
        _keyboardHighlight = true;
        RefreshList(scroll: true);
    }

    /// <summary>Redraws the open list's rows (a selection changed); no-op while closed.</summary>
    protected void RefreshOpenList() => RefreshList(scroll: false);

    // ---- opening and closing ----

    void OnIsOpenChanged()
    {
        if (IsOpen && !_openByKeyboard) _keyboardHighlight = false;
        SyncOverlay();
        UpdateOpenVisuals();
    }

    /// <summary>Opens the list from the keyboard: the highlight lands on the selected row (or the first) and shows its focus ring.</summary>
    protected void OpenFromKeyboard()
    {
        _openByKeyboard = true;
        try
        {
            IsOpen = true;
        }
        finally
        {
            _openByKeyboard = false;
        }
        _keyboardHighlight = true;
        if (_overlay is not null)
        {
            _model.HighlightValueOrFirst(HighlightOnOpen);
            RefreshList(scroll: true);
        }
    }

    /// <summary>Toggles the list (a click on the field or chevron).</summary>
    protected void ToggleOpen()
    {
        if (!CanOpen) return;
        IsOpen = !IsOpen;
    }

    void SyncOverlay()
    {
        var want = IsOpen && CanOpen && IsLoaded;
        if (!want)
        {
            HideOverlay(keepOpenFlag: !IsOpen || !IsLoaded);
            return;
        }
        if (_overlay is not null) return;

        _model.SetRows(BuildRows());
        if (_model.Rows.Count == 0) return;

        var anchor = Chrome.Field;
        if (anchor.Width <= 0)
        {
            if (_waitingForSize) return;
            _waitingForSize = true;
            anchor.SizeChanged += OnAnchorSized;
            return;
        }
        ShowOverlay();
    }

    void OnAnchorSized(object? sender, EventArgs e)
    {
        if (Chrome.Field.Width <= 0) return;
        Chrome.Field.SizeChanged -= OnAnchorSized;
        _waitingForSize = false;
        SyncOverlay();
    }

    void ShowOverlay()
    {
        _hidingForEmpty = false;
        OnListOpening();
        var rows = _model.Rows;
        var list = new CdsListBoxList(CdsListBoxTokens.Coerce(Size), UsesInlineType, IsMultiple, Label ?? Placeholder);
        list.Build(rows, i => IsRowChosen(rows[i]), CreateRowContent);
        list.RowChosen += (_, i) => OnRowChosen(i);
        list.RowHovered += (_, i) =>
        {
            if (i >= 0 && _model.SetHighlight(i)) _keyboardHighlight = false;
            RefreshList(scroll: false);
        };
        _list = list;

        if (_keyboardHighlight) _model.HighlightValueOrFirst(HighlightOnOpen);
        else _model.HighlightValue(HighlightOnOpen);
        list.Refresh(_model.Highlighted, _keyboardHighlight, i => IsRowChosen(rows[i]));

        var overlay = CdsOverlayService.Open(list.Surface, new CdsOverlayOptions
        {
            Anchor = Chrome.Field,
            Placement = Direction == CdsListBoxDirection.Top ? CdsPlacement.TopStart : CdsPlacement.BottomStart,
            MatchAnchorWidth = true,
            LightDismiss = true,
            ReturnFocusTo = _trigger,
            Keys = HasTextField ? CdsOverlayKeys.VerticalNavigation : CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation,
            KeyHandler = OnListKey,
        });
        _overlay = overlay;
        overlay.Closed += (_, _) => OnOverlayClosed(overlay);
        overlay.Positioned += (_, _) => _list?.ScrollTo(_model.Highlighted);
        if (TriggerHit is { } hit) hit.IsExpanded = true;
        UpdateOpenVisuals();
        list.ScrollTo(_model.Highlighted);
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Called just before the list is built and shown (a multi-select records the rows to list first).</summary>
    protected virtual void OnListOpening()
    {
    }

    /// <summary>Called after the list closed, whatever the cause.</summary>
    protected virtual void OnListClosed()
    {
    }

    void HideOverlay(bool keepOpenFlag)
    {
        if (_waitingForSize)
        {
            Chrome.Field.SizeChanged -= OnAnchorSized;
            _waitingForSize = false;
        }
        if (_overlay is not { } overlay) return;
        _hidingForEmpty = keepOpenFlag;
        _overlay = null;
        _list = null;
        overlay.Close();
    }

    void OnOverlayClosed(CdsOverlay overlay)
    {
        if (ReferenceEquals(_overlay, overlay))
        {
            _overlay = null;
            _list = null;
        }
        _model.ClearHighlight();
        _model.ResetTypeAhead();
        _keyboardHighlight = false;
        if (IsOpen && !_hidingForEmpty) SetValue(IsOpenProperty, false);
        _hidingForEmpty = false;
        if (TriggerHit is { } hit) hit.IsExpanded = false;
        UpdateOpenVisuals();
        OnListClosed();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    void UpdateOpenVisuals()
    {
        SetState(CdsInteractionState.Open, IsOpen);
        var target = IsOpen ? 180d : 0d;
        if (Math.Abs(Chevron.Rotation - target) > 0.5)
        {
            _ = CdsMotion.AnimateAsync(Chevron, "cds-chevron", v => Chevron.Rotation = v, Chevron.Rotation, target,
                CdsDuration.Fast01, CdsEasing.StandardProductive);
        }
        RefreshVisuals();
    }

    // ---- choosing and keys ----

    void OnRowChosen(int index)
    {
        if (!CanOpen || index < 0 || index >= _model.Rows.Count || _model.Rows[index].IsDisabled) return;
        ChooseRow(index);
        if (!IsMultiple) IsOpen = false;
        else RefreshList(scroll: false);
    }

    bool OnListKey(CdsKeyEventArgs e)
    {
        if (_overlay is null) return false;
        if (OnOpenKey(e)) return true;
        var result = _model.HandleOpenKey(e.Key, HasTextField);
        switch (result.Action)
        {
            case ListBoxAction.Choose:
                OnRowChosen(_model.Highlighted);
                return true;
            case ListBoxAction.Close:
                IsOpen = false;
                return result.Handled;
        }
        if (result.Handled)
        {
            _keyboardHighlight = true;
            RefreshList(scroll: true);
        }
        return result.Handled;
    }

    /// <summary>Lets a subclass handle a key while the list is open before the shared handling. Return <c>true</c> to consume it.</summary>
    protected virtual bool OnOpenKey(CdsKeyEventArgs e) => false;

    /// <summary>Lets a subclass handle a key on the closed field (Delete, Escape). Return <c>true</c> to consume it.</summary>
    protected virtual bool OnClosedKey(CdsKeyEventArgs e) => false;

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => CanOpen;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (_overlay is not null) return false;
        if (!CanOpen) return false;
        if (ListBoxModel.HandleClosedKey(e.Key).Action == ListBoxAction.Open)
        {
            OpenFromKeyboard();
            return true;
        }
        return OnClosedKey(e);
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (!CanOpen) return;
        if (TriggerHit is { IsFocusFromKeyboard: true }) OpenOrToggleFromKeyboard();
        else ToggleOpen();
    }

    void OpenOrToggleFromKeyboard()
    {
        if (IsOpen) IsOpen = false;
        else OpenFromKeyboard();
    }

    void OnChevronTapped() => ToggleFromPointer();

    /// <summary>Focuses the trigger and toggles the list: what a tap on the chevron, or on a decoration in the field, does.</summary>
    protected void ToggleFromPointer()
    {
        if (!CanOpen) return;
        _trigger?.Focus();
        ToggleOpen();
    }

    /// <summary>
    /// Type-ahead: jumps the highlight to the next row that starts with <paramref name="character"/> (letters typed
    /// within half a second build one search string). Opens the list when it is closed. The platform key router does not
    /// deliver characters yet, so hosts and tests call this directly.
    /// </summary>
    public bool TypeAhead(char character)
    {
        if (!CanOpen || HasTextField) return false;
        var now = Environment.TickCount64;
        if (_overlay is null)
        {
            _model.SetRows(BuildRows());
            if (!_model.TypeAhead(character, now)) return false;
            return OnTypeAheadClosed();
        }
        if (!_model.TypeAhead(character, now)) return false;
        _keyboardHighlight = true;
        RefreshList(scroll: true);
        return true;
    }

    /// <summary>Called when type-ahead matched while the list is closed; the default opens the list on the match.</summary>
    protected virtual bool OnTypeAheadClosed()
    {
        var target = _model.Highlighted;
        OpenFromKeyboard();
        if (_overlay is not null && _model.SetHighlight(target))
        {
            RefreshList(scroll: true);
        }
        return true;
    }

    // ---- visuals ----

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (Is(CdsInteractionState.Disabled) && IsOpen) IsOpen = false;
    }

    /// <inheritdoc/>
    protected override void UpdateChrome()
    {
        base.UpdateChrome();
        var inline = UsesInlineType;
        Chrome.IsInline = inline;
        Chrome.IsFlat = inline;
        Chrome.Size = CdsListBoxTokens.Coerce(Size);
        if (Chrome.Input is { } input) input.Margin = new Thickness(inline ? -CdsSpacing.Spacing03 : 0, 0, 0, 0);
        if (IsOpen && !CanOpen) IsOpen = false;
        OnChromeUpdated();
        RefreshVisuals();
    }

    /// <summary>Called at the end of <c>UpdateChrome</c>: subclasses push texts, placeholders and accessibility to their trigger.</summary>
    protected virtual void OnChromeUpdated()
    {
    }

    /// <inheritdoc/>
    protected override void ApplyInput()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        var readOnly = IsReadOnly;
        var open = IsOpen;
        var flat = Chrome.IsFlat;
        var hover = Is(CdsInteractionState.Hover) && !disabled && !readOnly;

        SetColor(Chevron, CdsIcon.ColorProperty, CdsListBoxTokens.FieldIcon(disabled, readOnly));

        // The field fill and underline depend on the open and inline states, which the shared chrome does not know about.
        var fill = flat
            ? CdsListBoxTokens.InlineFill(hover, disabled, readOnly, open)
            : readOnly ? null : hover && !open ? CdsColorKeys.FieldHover01 : CdsColorKeys.Field01;
        CdsTokens.SetColor(Chrome.Field, BackgroundColorProperty, fill);
        if (FindRule() is { } rule) CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsListBoxTokens.FieldRule(disabled, readOnly, open, flat));

        if (TriggerHit is { } hit)
        {
            hit.IsEnabled = !disabled;
        }
        ApplyTrigger(disabled, readOnly);
    }

    /// <summary>Sets the colors of the trigger content (label, tag, entry text) for the current state.</summary>
    protected virtual void ApplyTrigger(bool disabled, bool readOnly)
    {
    }

    BoxView? FindRule()
    {
        foreach (var child in Chrome.Field.Children)
        {
            if (child is Grid overlay and not CdsFocusRing)
            {
                foreach (var inner in overlay.Children)
                {
                    if (inner is BoxView box) return box;
                }
            }
        }
        return null;
    }

    /// <summary>Creates the hit target of a non-text trigger (role combo box, <c>IsTabStop</c> stays on).</summary>
    protected CdsHitTarget CreateTriggerHit() => CreateHitTarget(CdsAccessibilityRole.ComboBox);

    /// <summary>The accessible value ("Frankfurt") appended to the trigger's name.</summary>
    protected virtual string? AccessibleValue => null;

    /// <summary>Sets the accessible name of the trigger: the label, then the current value.</summary>
    protected void UpdateAccessibleName()
    {
        if (_trigger is null) return;
        var name = Label ?? Placeholder;
        var value = AccessibleValue;
        SemanticProperties.SetDescription(_trigger, string.IsNullOrEmpty(value) ? name : $"{name}, {value}");
    }
}
