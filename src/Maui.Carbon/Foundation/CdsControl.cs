namespace Maui.Carbon;

/// <summary>
/// Base class of interactive Carbon controls. A control builds its own visual tree with <see cref="SetChrome"/>,
/// tracks its <see cref="State"/> (hover, pressed, focus, disabled, selection, validation), and re-applies token
/// colors in <see cref="ApplyVisuals"/> whenever the state changes. Interaction comes from a <see cref="CdsHitTarget"/>
/// (see <see cref="CreateHitTarget"/>) or a native input (see <see cref="TrackFocus"/>).
/// <para>
/// A <c>SemanticProperties.Description</c> or <c>Hint</c> set on the control itself never reaches its own platform view
/// (where, on iOS and Mac Catalyst, it would turn the control into one accessibility element and hide its hit target):
/// it is forwarded to the control's one hit target or input, overriding the name the control gives it (a hint is added
/// after the control's own); a control with several, or with interactive content of its own, is named as a group with
/// <see cref="CdsSemantics"/> instead. A control with nothing interactive inside keeps the name itself.
/// </para>
/// </summary>
public abstract class CdsControl : ContentView, IView
{
    static readonly BindableProperty ContextThemeProperty = BindableProperty.Create(
        "ContextTheme", typeof(CdsThemeName), typeof(CdsControl), CdsThemeName.White,
        propertyChanged: static (b, _, _) => ((CdsControl)b).OnContextChanged());

    static readonly BindableProperty ContextLevelProperty = BindableProperty.Create(
        "ContextLevel", typeof(int), typeof(CdsControl), 1,
        propertyChanged: static (b, _, _) => ((CdsControl)b).OnContextChanged());

    // The color keys applied through SetColor/SetBrush live on each target, not in the control: a cache in the control
    // keyed by its views would keep every view it ever colored alive, including rows and cells it has since discarded.
    static readonly BindableProperty AppliedTokensProperty = BindableProperty.CreateAttached(
        "CdsAppliedTokens", typeof(Dictionary<BindableProperty, AppliedToken>), typeof(CdsControl), null);

    static int s_nextColorOwner;
    // An id rather than a reference, so a long-lived view a short-lived control colored does not keep the control alive.
    readonly int _colorOwner = Interlocked.Increment(ref s_nextColorOwner);
    int _tokenGeneration;
    System.Windows.Input.ICommand? _command;
    object? _commandParameter;
    CdsWeakSubscription<CdsControl>? _commandSubscription;
    bool _tracksLoading;
    CdsInteractionState _state;
    bool _settingChrome;
    bool _visualsPending;
    bool _visualsApplied;
    VisualElement? _focusTarget;
    bool _tracksHover;
    // Everything passed to TrackFocus (weakly: a rebuilt chrome drops its old hit targets).
    readonly List<WeakReference<VisualElement>> _focusables = [];
    SemanticForward? _semanticForward;
    bool _forwardedGroup;
    bool _forwardsOnLoad;
    Semantics? _platformSemantics;

    /// <summary>Initializes the control: context tracking.</summary>
    protected CdsControl()
    {
        SetDynamicResource(ContextThemeProperty, CdsContextKeys.ThemeId);
        SetDynamicResource(ContextLevelProperty, CdsContextKeys.LayerLevel);
    }

    /// <summary>
    /// Tracks pointer hover (<see cref="CdsInteractionState.Hover"/>). <see cref="CreateHitTarget"/> and <see cref="TrackFocus"/>
    /// turn it on; call it in the constructor of a control that reacts to hover without either. Only interactive controls
    /// carry the recognizer: each one costs platform gesture setup whenever its page is shown or hidden.
    /// </summary>
    protected void TrackHover()
    {
        if (_tracksHover) return;
        _tracksHover = true;
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

    /// <summary>Raised when the control's input or hit target gains focus.</summary>
    public event EventHandler? FocusGained;

    /// <summary>Raised when the control's input or hit target loses focus (validate fields here: Carbon validates on blur).</summary>
    public event EventHandler? FocusLost;

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

    /// <inheritdoc/>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var size = base.MeasureOverride(widthConstraint, heightConstraint);
        // ContentView measures its content even when that content is hidden, so a control that hides its chrome (the header
        // navigation below lg) would keep its old size in an Auto row or column. Hidden chrome takes no space.
        return Content is { IsVisible: false } ? Size.Zero : size;
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
        _visualsApplied = true;
        ApplyVisuals();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();
        // Construction is complete by the time a parent is set (never true inside a base constructor), so this is the
        // safe place to guarantee one ApplyVisuals for controls whose state never changes after construction.
        if (Parent is not null && !_visualsApplied) ApplyVisualsOnce();
        // Taken out of its parent (an items list cleared): stop listening to the command until it is placed again.
        if (Parent is null) UnsubscribeCommand();
        else if (_command is not null && _commandSubscription is null) SubscribeCommand();
    }

    void ApplyVisualsOnce()
    {
        _visualsApplied = true;
        ApplyVisuals();
    }

    /// <summary>Sets the control's visual tree. Controls never let consumers assign <c>Content</c> directly.</summary>
    protected void SetChrome(View chrome)
    {
        if (Content is View previous) previous.PropertyChanged -= OnChromePropertyChanged;
        chrome.PropertyChanged += OnChromePropertyChanged;
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
            _visualsApplied = true;
            ApplyVisuals();
        }
    }

    // Hiding or showing the chrome changes this control's size (see MeasureOverride), but MAUI does not re-measure the
    // control for it: an Auto column kept the header navigation's old width, and after a window resize the header's
    // global actions were laid out from a measure taken while the navigation was hidden, past the window edge.
    void OnChromePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != IsVisibleProperty.PropertyName) return;
        ((IView)this).InvalidateMeasure();
        (Parent as IView)?.InvalidateMeasure();
    }

    /// <summary>Binds a Color property of <paramref name="target"/> to a color key; skips work when unchanged. <c>null</c> = transparent.</summary>
    protected void SetColor(Element target, BindableProperty colorProperty, string? key)
    {
        if (Remember(target, colorProperty, key)) CdsTokens.SetColor(target, colorProperty, key);
    }

    /// <summary>Forgets the color keys this control applied (the next <see cref="SetColor"/> or <see cref="SetBrush"/> applies again).</summary>
    protected void ForgetColors() => _tokenGeneration++;

    /// <summary>Forgets the color keys applied to <paramref name="targets"/>.</summary>
    protected void ForgetColors(IReadOnlySet<BindableObject> targets)
    {
        foreach (var target in targets) target.ClearValue(AppliedTokensProperty);
    }

    /// <summary>Binds a Brush property of <paramref name="target"/> to a color key; skips work when unchanged. <c>null</c> = transparent.</summary>
    protected void SetBrush(Element target, BindableProperty brushProperty, string? key)
    {
        if (Remember(target, brushProperty, key)) CdsTokens.SetBrush(target, brushProperty, key);
    }

    /// <summary>Records <paramref name="key"/> as applied to the target's property; false when this control already applied it.</summary>
    bool Remember(BindableObject target, BindableProperty property, string? key)
    {
        if (target.GetValue(AppliedTokensProperty) is not Dictionary<BindableProperty, AppliedToken> applied)
        {
            applied = [];
            target.SetValue(AppliedTokensProperty, applied);
        }
        var token = new AppliedToken(_colorOwner, _tokenGeneration, key);
        if (applied.TryGetValue(property, out var current) && current == token) return false;
        applied[property] = token;
        return true;
    }

    readonly record struct AppliedToken(int Owner, int Generation, string? Key);

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
        TrackHover();
        _focusTarget = focusable;
        _focusables.RemoveAll(w => !w.TryGetTarget(out _));
        _focusables.Add(new WeakReference<VisualElement>(focusable));
        if (_semanticForward is not null || _forwardedGroup) UpdateSemanticForwarding();
        focusable.Focused += (_, _) =>
        {
            SetState(CdsInteractionState.FocusVisible, isKeyboardFocus?.Invoke() ?? true);
            SetState(CdsInteractionState.Focused, true);
            CdsFocusTracker.SetFocused(this);
            FocusGained?.Invoke(this, EventArgs.Empty);
        };
        focusable.Unfocused += (_, _) =>
        {
            SetState(CdsInteractionState.Focused | CdsInteractionState.FocusVisible, false);
            CdsFocusTracker.ClearFocused(this);
            FocusLost?.Invoke(this, EventArgs.Empty);
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
            _command = command;
            if (command is not null && !_tracksLoading)
            {
                // While off screen the control needs no CanExecuteChanged (IsEnabled is re-read when it comes back).
                _tracksLoading = true;
                Loaded += (_, _) =>
                {
                    SubscribeCommand();
                    RefreshIsEnabledProperty();
                };
                Unloaded += (_, _) => UnsubscribeCommand();
            }
            SubscribeCommand();
        }
        _commandParameter = parameter;
        RefreshIsEnabledProperty();
    }

    // Weak, like MAUI's Button: a command usually lives in a view model that outlives the control, and a strong
    // CanExecuteChanged handler would keep every control ever bound to it alive.
    void SubscribeCommand()
    {
        UnsubscribeCommand();
        if (_command is not { } command) return;
        _commandSubscription = new CdsWeakSubscription<CdsControl>(
            this, static c => c.RefreshIsEnabledProperty(),
            h => command.CanExecuteChanged += h, h => command.CanExecuteChanged -= h);
    }

    void UnsubscribeCommand()
    {
        _commandSubscription?.Dispose();
        _commandSubscription = null;
    }

    /// <summary>Executes the command set with <see cref="SetCommand"/> when it can execute.</summary>
    protected void ExecuteCommand()
    {
        if (_command?.CanExecute(_commandParameter) == true) _command.Execute(_commandParameter);
    }

    /// <inheritdoc/>
    protected override bool IsEnabledCore => base.IsEnabledCore && (_command?.CanExecute(_commandParameter) ?? true);

    /// <summary>Called when the hit target is activated (click, tap, Enter or Space) while enabled.</summary>
    protected virtual void OnActivated()
    {
    }

    /// <summary>
    /// Handles a key routed from the page host while this control (or a descendant) has focus. Return <c>true</c>
    /// to consume it. Composite controls (tabs, radio groups, menus) handle arrow keys here.
    /// </summary>
    protected internal virtual bool OnKeyDown(CdsKeyEventArgs e) => false;

    /// <summary>Whether this control wants arrow/Home/End/PageUp/PageDown keys routed to it while focused. Default <c>false</c>.</summary>
    protected internal virtual bool WantsNavigationKeys => false;

    /// <summary>
    /// Whether this control wants Escape routed to it while it (or a descendant) has focus and no overlay is open
    /// (clearing a search, dismissing a notification). Default <c>false</c>.
    /// </summary>
    protected internal virtual bool WantsEscapeKey => false;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
        {
            SetState(CdsInteractionState.Disabled, !IsEnabled);
        }
        else if (propertyName == SemanticProperties.DescriptionProperty.PropertyName || propertyName == SemanticProperties.HintProperty.PropertyName)
        {
            UpdateSemanticForwarding();
        }
        else if (propertyName == ContentProperty.PropertyName && !_settingChrome)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} draws its own content. Use its content properties (Body, Items, Text...) instead of Content.");
        }
    }

    // ----- Description and Hint set on the control (see the class summary) -----

    enum SemanticMode
    {
        /// <summary>No Description or Hint on the control.</summary>
        None,
        /// <summary>Nothing interactive inside: the control itself carries the name, as any MAUI view would.</summary>
        Leaf,
        /// <summary>One hit target or input: it carries the name.</summary>
        Target,
        /// <summary>Several, or interactive content: the control is a named group (<see cref="CdsSemantics"/>).</summary>
        Group,
    }

    /// <summary>The one hit target or input that carries a Description or Hint set on the control, if it has one.</summary>
    internal VisualElement? SemanticForwardTarget => _semanticForward?.Target;

    // MAUI maps Semantics from this to the platform view. Outside the leaf case the control's own Description and Hint are
    // left out, so the content view never becomes an accessibility element that hides its hit target.
    Semantics? IView.Semantics
    {
        get
        {
            var description = SemanticProperties.GetDescription(this);
            var hint = SemanticProperties.GetHint(this);
            var heading = SemanticProperties.GetHeadingLevel(this);
            var mode = ResolveSemanticMode(out _);
            if (mode is not (SemanticMode.None or SemanticMode.Leaf))
            {
                description = null;
                hint = null;
            }
            if (description is null && hint is null && heading == SemanticHeadingLevel.None && _platformSemantics is null) return null;
            _platformSemantics ??= new Semantics();
            _platformSemantics.Description = description;
            _platformSemantics.Hint = hint;
            _platformSemantics.HeadingLevel = heading;
            return _platformSemantics;
        }
    }

    SemanticMode ResolveSemanticMode(out VisualElement? target)
    {
        target = null;
        if (string.IsNullOrWhiteSpace(SemanticProperties.GetDescription(this)) && string.IsNullOrWhiteSpace(SemanticProperties.GetHint(this)))
            return SemanticMode.None;
        VisualElement? single = null;
        var count = 0;
        foreach (var weak in _focusables)
        {
            if (!weak.TryGetTarget(out var focusable) || !IsInside(focusable)) continue;
            single = focusable;
            count++;
        }
        if (count == 1)
        {
            target = single;
            return SemanticMode.Target;
        }
        return count > 1 || HasInteractiveContent(Content) ? SemanticMode.Group : SemanticMode.Leaf;
    }

    bool IsInside(Element element)
    {
        for (var e = element.Parent; e is not null; e = e.Parent)
            if (ReferenceEquals(e, this)) return true;
        return false;
    }

    static bool HasInteractiveContent(Element? element)
    {
        if (element is CdsHitTarget or InputView or Button or ImageButton or Picker or DatePicker or TimePicker or Slider or Stepper or Switch or CheckBox or RadioButton)
            return true;
        if (element is not IVisualTreeElement tree) return false;
        foreach (var child in tree.GetVisualChildren())
            if (HasInteractiveContent(child as Element)) return true;
        return false;
    }

    void UpdateSemanticForwarding()
    {
        var mode = ResolveSemanticMode(out var target);
        var description = SemanticProperties.GetDescription(this);
        var hint = SemanticProperties.GetHint(this);

        if (mode == SemanticMode.Target && target is not null)
        {
            if (!ReferenceEquals(_semanticForward?.Target, target))
            {
                _semanticForward?.Detach();
                _semanticForward = new SemanticForward(target);
            }
            _semanticForward.Update(description, hint);
        }
        else if (_semanticForward is not null)
        {
            _semanticForward.Detach();
            _semanticForward = null;
        }

        if (mode == SemanticMode.Group)
        {
            // A control that names itself as a group (side nav, header) keeps its own name.
            if (_forwardedGroup || !CdsSemantics.HasGroup(this))
            {
                _forwardedGroup = true;
                CdsSemantics.SetGroupHint(this, string.IsNullOrWhiteSpace(hint) ? null : hint);
                CdsSemantics.SetGroupName(this, string.IsNullOrWhiteSpace(description) ? null : description);
            }
        }
        else if (_forwardedGroup)
        {
            _forwardedGroup = false;
            CdsSemantics.SetGroupHint(this, null);
            CdsSemantics.SetGroupName(this, null);
        }

        if (mode != SemanticMode.None && !_forwardsOnLoad)
        {
            // Set before the chrome was complete (or rebuilt since): decide again once the control is on screen.
            _forwardsOnLoad = true;
            Loaded += (_, _) => UpdateSemanticForwarding();
        }
        Handler?.UpdateValue(nameof(IView.Semantics));
    }

    /// <summary>
    /// Carries the control's Description and Hint on its one hit target or input. The control keeps setting the target's
    /// own name (from its text, label…): that is remembered and comes back when the control's Description goes away.
    /// </summary>
    sealed class SemanticForward
    {
        string? _ownDescription;
        string? _ownHint;
        string? _description;
        string? _hint;
        string? _writtenDescription;
        string? _writtenHint;
        bool _writing;

        public SemanticForward(VisualElement target)
        {
            Target = target;
            _ownDescription = SemanticProperties.GetDescription(target);
            _ownHint = SemanticProperties.GetHint(target);
            target.PropertyChanged += OnTargetPropertyChanged;
        }

        public VisualElement Target { get; }

        public void Update(string? description, string? hint)
        {
            _description = string.IsNullOrWhiteSpace(description) ? null : description;
            _hint = string.IsNullOrWhiteSpace(hint) ? null : hint;
            Write(_description ?? _ownDescription, CombineHints(_ownHint, _hint));
        }

        public void Detach()
        {
            Target.PropertyChanged -= OnTargetPropertyChanged;
            Write(_ownDescription, _ownHint);
        }

        // A value written from inside the target's own PropertyChanged is applied by MAUI after that change completes (it
        // delays re-entrant sets of a property), so the guard alone does not see it: the value written last is ours too.
        void OnTargetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_writing) return;
            if (e.PropertyName == SemanticProperties.DescriptionProperty.PropertyName)
            {
                var value = SemanticProperties.GetDescription(Target);
                if (value == _writtenDescription) return;
                _ownDescription = value;
            }
            else if (e.PropertyName == SemanticProperties.HintProperty.PropertyName)
            {
                var value = SemanticProperties.GetHint(Target);
                if (value == _writtenHint) return;
                _ownHint = value;
            }
            else
            {
                return;
            }
            Write(_description ?? _ownDescription, CombineHints(_ownHint, _hint));
        }

        void Write(string? description, string? hint)
        {
            _writing = true;
            try
            {
                _writtenDescription = description;
                _writtenHint = hint;
                if (SemanticProperties.GetDescription(Target) != description) SemanticProperties.SetDescription(Target, description);
                if (SemanticProperties.GetHint(Target) != hint) SemanticProperties.SetHint(Target, hint);
            }
            finally
            {
                _writing = false;
            }
        }

        // The control's own hint (helper or error text) first, then the consumer's.
        static string? CombineHints(string? own, string? consumer) =>
            string.IsNullOrWhiteSpace(consumer) ? own
            : string.IsNullOrWhiteSpace(own) || own == consumer ? consumer
            : $"{own}. {consumer}";
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
