namespace Maui.Carbon;

/// <summary>
/// Base class of interactive Carbon controls. A control builds its own visual tree with <see cref="SetChrome"/>,
/// tracks its <see cref="State"/> (hover, pressed, focus, disabled, selection, validation), and re-applies token
/// colors in <see cref="ApplyVisuals"/> whenever the state changes. Interaction comes from a <see cref="CdsHitTarget"/>
/// (see <see cref="CreateHitTarget"/>) or a native input (see <see cref="TrackFocus"/>).
/// </summary>
public abstract class CdsControl : ContentView
{
    static readonly BindableProperty ContextThemeProperty = BindableProperty.Create(
        "ContextTheme", typeof(CdsThemeName), typeof(CdsControl), CdsThemeName.White,
        propertyChanged: static (b, _, _) => ((CdsControl)b).OnContextChanged());

    static readonly BindableProperty ContextLevelProperty = BindableProperty.Create(
        "ContextLevel", typeof(int), typeof(CdsControl), 1,
        propertyChanged: static (b, _, _) => ((CdsControl)b).OnContextChanged());

    readonly Dictionary<(BindableObject Target, BindableProperty Property), string?> _tokens = [];
    System.Windows.Input.ICommand? _command;
    object? _commandParameter;
    CdsInteractionState _state;
    bool _settingChrome;
    bool _visualsPending;
    VisualElement? _focusTarget;

    /// <summary>Initializes the control: context tracking and pointer hover.</summary>
    protected CdsControl()
    {
        SetDynamicResource(ContextThemeProperty, CdsContextKeys.ThemeId);
        SetDynamicResource(ContextLevelProperty, CdsContextKeys.LayerLevel);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetState(CdsInteractionState.Hover, true);
        pointer.PointerExited += (_, _) =>
        {
            SetState(CdsInteractionState.Hover, false);
            SetState(CdsInteractionState.Pressed, false);
        };
        GestureRecognizers.Add(pointer);
    }

    /// <summary>Raised after <see cref="State"/> changes.</summary>
    public event EventHandler? StateChanged;

    /// <summary>The current interaction and validation state.</summary>
    public CdsInteractionState State => _state;

    /// <summary>The theme in effect where the control sits (nearest <see cref="CdsTheme"/> zone, else the app theme).</summary>
    public CdsThemeName ContextTheme => (CdsThemeName)GetValue(ContextThemeProperty);

    /// <summary>The layer level in effect where the control sits (1, 2 or 3).</summary>
    public int ContextLayerLevel => (int)GetValue(ContextLevelProperty);

    /// <summary>The element that receives focus for this control (its hit target or native input).</summary>
    protected VisualElement? FocusTarget => _focusTarget;

    /// <summary>The element screen readers focus for this control (where names and hints belong).</summary>
    internal VisualElement? AccessibilityTarget => _focusTarget;

    /// <summary>True when the state includes every flag in <paramref name="flags"/>.</summary>
    public bool Is(CdsInteractionState flags) => (_state & flags) == flags;

    /// <summary>Moves focus to the control's hit target or input.</summary>
    public new bool Focus() => _focusTarget?.Focus() ?? base.Focus();

    /// <summary>Removes focus from the control.</summary>
    public new void Unfocus()
    {
        if (_focusTarget is not null) _focusTarget.Unfocus();
        else base.Unfocus();
    }

    /// <summary>Sets or clears state flags, then re-applies visuals when anything changed.</summary>
    protected void SetState(CdsInteractionState flags, bool on)
    {
        var next = on ? _state | flags : _state & ~flags;
        if (next == _state) return;
        var previous = _state;
        _state = next;
        OnStateChanged(previous);
    }

    /// <summary>Called after the state changed. The default re-applies visuals and raises <see cref="StateChanged"/>.</summary>
    protected virtual void OnStateChanged(CdsInteractionState previous)
    {
        RefreshVisuals();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Called when the theme or layer context changes (a token-only control rarely needs this).</summary>
    protected virtual void OnContextChanged()
    {
    }

    /// <summary>Re-applies token colors and other state-driven visuals. Called on every state change.</summary>
    protected virtual void ApplyVisuals()
    {
    }

    /// <summary>Calls <see cref="ApplyVisuals"/> (call it from property-changed callbacks).</summary>
    protected void RefreshVisuals()
    {
        if (_settingChrome)
        {
            _visualsPending = true;
            return;
        }
        ApplyVisuals();
    }

    /// <summary>Sets the control's visual tree. Controls never let consumers assign <c>Content</c> directly.</summary>
    protected void SetChrome(View chrome)
    {
        _settingChrome = true;
        try
        {
            Content = chrome;
        }
        finally
        {
            _settingChrome = false;
        }
        if (_visualsPending)
        {
            _visualsPending = false;
            ApplyVisuals();
        }
    }

    /// <summary>Binds a Color property of <paramref name="target"/> to a color key; skips work when unchanged. <c>null</c> = transparent.</summary>
    protected void SetColor(Element target, BindableProperty colorProperty, string? key)
    {
        if (_tokens.TryGetValue((target, colorProperty), out var current) && current == key) return;
        _tokens[(target, colorProperty)] = key;
        CdsTokens.SetColor(target, colorProperty, key);
    }

    /// <summary>Binds a Brush property of <paramref name="target"/> to a color key; skips work when unchanged. <c>null</c> = transparent.</summary>
    protected void SetBrush(Element target, BindableProperty brushProperty, string? key)
    {
        if (_tokens.TryGetValue((target, brushProperty), out var current) && current == key) return;
        _tokens[(target, brushProperty)] = key;
        CdsTokens.SetBrush(target, brushProperty, key);
    }

    /// <summary>
    /// Creates a hit target wired to this control: press, focus and activation update <see cref="State"/> and call
    /// <see cref="OnActivated"/>. Place it last in the chrome (on top of the visuals) and mark the visuals
    /// <c>InputTransparent</c>.
    /// </summary>
    protected CdsHitTarget CreateHitTarget(CdsAccessibilityRole role = CdsAccessibilityRole.Button)
    {
        var target = new CdsHitTarget { Role = role };
        target.Pressed += (_, _) => SetState(CdsInteractionState.Pressed, true);
        target.Released += (_, _) => SetState(CdsInteractionState.Pressed, false);
        target.Clicked += (_, _) =>
        {
            SetState(CdsInteractionState.Pressed, false);
            if (IsEnabled) OnActivated();
        };
        TrackFocus(target, () => target.IsFocusFromKeyboard);
        return target;
    }

    /// <summary>
    /// Tracks focus of <paramref name="focusable"/> (a hit target or native input) as this control's focus.
    /// <paramref name="isKeyboardFocus"/> decides whether the focus ring shows; default: always.
    /// </summary>
    protected void TrackFocus(VisualElement focusable, Func<bool>? isKeyboardFocus = null)
    {
        _focusTarget = focusable;
        focusable.Focused += (_, _) =>
        {
            SetState(CdsInteractionState.FocusVisible, isKeyboardFocus?.Invoke() ?? true);
            SetState(CdsInteractionState.Focused, true);
            CdsFocusTracker.SetFocused(this);
        };
        focusable.Unfocused += (_, _) =>
        {
            SetState(CdsInteractionState.Focused | CdsInteractionState.FocusVisible, false);
            CdsFocusTracker.ClearFocused(this);
        };
    }

    /// <summary>
    /// Connects the control's activation command: <c>IsEnabled</c> follows <c>CanExecute</c> (like MAUI's Button) and
    /// <see cref="ExecuteCommand"/> runs it. Call it from the Command and CommandParameter property-changed callbacks.
    /// </summary>
    protected void SetCommand(System.Windows.Input.ICommand? command, object? parameter)
    {
        if (!ReferenceEquals(_command, command))
        {
            if (_command is not null) _command.CanExecuteChanged -= OnCanExecuteChanged;
            _command = command;
            if (_command is not null) _command.CanExecuteChanged += OnCanExecuteChanged;
        }
        _commandParameter = parameter;
        RefreshIsEnabledProperty();
    }

    /// <summary>Executes the command set with <see cref="SetCommand"/> when it can execute.</summary>
    protected void ExecuteCommand()
    {
        if (_command?.CanExecute(_commandParameter) == true) _command.Execute(_commandParameter);
    }

    /// <inheritdoc/>
    protected override bool IsEnabledCore => base.IsEnabledCore && (_command?.CanExecute(_commandParameter) ?? true);

    void OnCanExecuteChanged(object? sender, EventArgs e) => RefreshIsEnabledProperty();

    /// <summary>Called when the hit target is activated (click, tap, Enter or Space) while enabled.</summary>
    protected virtual void OnActivated()
    {
    }

    /// <summary>
    /// Handles a key routed from the page host while this control (or a descendant) has focus. Return <c>true</c>
    /// to consume it. Composite controls (tabs, radio groups, menus) handle arrow keys here.
    /// </summary>
    protected internal virtual bool OnKeyDown(CdsKeyEventArgs e) => false;

    /// <summary>Whether this control wants arrow/Home/End keys routed to it while focused. Default <c>false</c>.</summary>
    protected internal virtual bool WantsNavigationKeys => false;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
        {
            SetState(CdsInteractionState.Disabled, !IsEnabled);
        }
        else if (propertyName == ContentProperty.PropertyName && !_settingChrome)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} draws its own content. Use its content properties (Body, Items, Text...) instead of Content.");
        }
    }
}

/// <summary>A <see cref="CdsControl"/> that hosts consumer content in <see cref="Body"/>.</summary>
[ContentProperty(nameof(Body))]
public abstract class CdsContainer : CdsControl
{
    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsContainer), null,
        propertyChanged: static (b, o, n) => ((CdsContainer)b).OnBodyChanged((View?)o, (View?)n));

    /// <summary>The consumer content.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Called when <see cref="Body"/> changes; place the new body in the chrome.</summary>
    protected abstract void OnBodyChanged(View? oldBody, View? newBody);
}
