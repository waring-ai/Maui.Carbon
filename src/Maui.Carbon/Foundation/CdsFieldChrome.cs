namespace Maui.Carbon;

/// <summary>
/// The shared Carbon form-field scaffold: label row (label + counter), the field box (<c>field-01</c> fill,
/// 1px <c>border-strong-01</c> bottom rule, 2px focus / error outline, trailing status icon and action slot), and the
/// message row (helper, invalid or warning text). Inputs put their native control in <see cref="Input"/>, set the
/// text properties, and call <see cref="Apply"/> from <see cref="CdsControl.ApplyVisuals"/>.
/// <para>
/// <b>Fluid layout</b> (<see cref="IsFluid"/>, or the <c>Cds.IsFluid</c> context of a <see cref="CdsFluidForm"/>): the
/// label sits inside the 64px field at the top start, the input below it, helper text is not shown, and an invalid or
/// warning message sits inside the field under a 1px <c>border-subtle-01</c> divider with the status icon at its end. The
/// focus and error outlines wrap the field and its message; the size and inline settings are ignored.
/// </para>
/// </summary>
public sealed class CdsFieldChrome : Grid
{
    /// <summary>Height of a fluid field: 4rem.</summary>
    public const double FluidFieldHeight = 64;

    static readonly BindableProperty ContextFluidProperty = BindableProperty.Create(
        "ContextFluid", typeof(bool), typeof(CdsFieldChrome), false,
        propertyChanged: static (b, _, _) => ((CdsFieldChrome)b).OnFluidChanged());

    readonly Grid _shell = new();
    readonly Grid _messageBlock = new();
    readonly BoxView _divider = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
    readonly BoxView _shellRule = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
    readonly CdsFocusRing _fluidOutline = new(CdsFocusRingStyle.Inset);
    bool _fluid;
    bool _layoutFluid;
    double _fieldHeight;
    CdsInteractionState _lastState;
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary };
    readonly CdsText _counter = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, HorizontalOptions = LayoutOptions.End };
    readonly Grid _labelRow = new() { ColumnSpacing = CdsSpacing.Spacing03 };
    readonly Grid _field = new();
    readonly ContentView _inputSlot = new() { VerticalOptions = LayoutOptions.Fill };
    readonly BoxView _rule = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
    readonly CdsFocusRing _outline = new(CdsFocusRingStyle.Inset);
    readonly Grid _overlayGrid = new() { Margin = new Thickness(-CdsSpacing.Spacing05, 0), InputTransparent = true };
    readonly CdsIcon _statusIcon = new() { IsVisible = false, Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0) };
    readonly CdsText _message = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, IsVisible = false };
    readonly Dictionary<(BindableObject, BindableProperty), string?> _applied = [];
    bool _inline;
    CdsSize _size = CdsSize.Md;

    /// <summary>Creates the chrome.</summary>
    public CdsFieldChrome()
    {
        RowSpacing = 0;
        ColumnSpacing = 0;

        _labelRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _labelRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _labelRow.Add(_label, 0);
        _labelRow.Add(_counter, 1);
        _labelRow.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03);
        _counter.IsVisible = false;
        AutomationProperties.SetIsInAccessibleTree(_counter, false);

        Trailing = new HorizontalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        Trailing.Add(_statusIcon);

        _field.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _field.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _field.Padding = new Thickness(CdsSpacing.Spacing05, 0);
        _field.Add(_inputSlot, 0);
        _field.Add(Trailing, 1);
        // Rule and outline span the full field width, including the padding.
        _overlayGrid.Add(_rule);
        _overlayGrid.Add(_outline);
        _field.Add(_overlayGrid, 0);
        SetColumnSpan((BindableObject)_overlayGrid, 2);

        _message.Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0);

        _messageBlock.IsVisible = false;
        SetDynamicResource(ContextFluidProperty, CdsContextKeys.IsFluid);
        // A dynamic resource keeps its last value when the control leaves a fluid form, so re-check on attach.
        Loaded += (_, _) =>
        {
            if (_layoutFluid != IsFluidLayout) OnFluidChanged();
        };

        FieldHeight = CdsSize.Md.Height();
        Layout();
        Apply(CdsInteractionState.None);
    }

    /// <summary>The native input (or any view) shown inside the field box.</summary>
    public View? Input
    {
        get => _inputSlot.Content;
        set => _inputSlot.Content = value;
    }

    /// <summary>The field box itself (for controls that add overlays, such as a list box anchor).</summary>
    public Grid Field => _field;

    /// <summary>Slot at the end of the field, after the status icon (password toggle, clear button, steppers...).</summary>
    public HorizontalStackLayout Trailing { get; }

    /// <summary>The visible label text (<c>label-01</c>, <c>text-secondary</c>).</summary>
    public string? Label
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    /// <summary>Hides the label visually; the owner keeps it as the input's accessible name.</summary>
    public bool HideLabel { get; set; }

    /// <summary>Helper text under the field.</summary>
    public string? HelperText { get; set; }

    /// <summary>Error text shown when the owner is invalid.</summary>
    public string? InvalidText { get; set; }

    /// <summary>Warning text shown when the owner has a warning.</summary>
    public string? WarnText { get; set; }

    /// <summary>Counter text at the end of the label row ("12/100"); <c>null</c> hides it.</summary>
    public string? Counter
    {
        get => _counter.Text;
        set
        {
            _counter.Text = value;
            _counter.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>Field height: xs 24, sm 32, md 40 (default), lg 48. Multi-line inputs set <see cref="FieldHeight"/> instead.</summary>
    public CdsSize Size
    {
        get => _size;
        set
        {
            _size = value;
            FieldHeight = value.Height();
        }
    }

    /// <summary>
    /// Explicit field height (-1 lets the input size it, for text areas). In fluid layout the field is at least
    /// <see cref="FluidFieldHeight"/> and a fixed height becomes exactly that.
    /// </summary>
    public double FieldHeight
    {
        get => _fieldHeight;
        set
        {
            _fieldHeight = value;
            ApplyFieldHeight();
        }
    }

    /// <summary>Forces the fluid layout. It also turns on by itself inside a <see cref="CdsFluidForm"/>.</summary>
    public bool IsFluid
    {
        get => _fluid;
        set
        {
            if (_fluid == value) return;
            var before = IsFluidLayout;
            _fluid = value;
            if (before != IsFluidLayout) OnFluidChanged();
        }
    }

    /// <summary>Whether the fluid layout is in effect (<see cref="IsFluid"/> or the fluid form context).</summary>
    public bool IsFluidLayout => _fluid || ((bool)GetValue(ContextFluidProperty) && CdsContext.Find(this, CdsContextKeys.IsFluid) is true);

    /// <summary>Raised after the fluid layout turned on or off, so the owner can re-apply its sizes and decorations.</summary>
    public event EventHandler? FluidLayoutChanged;

    /// <summary>Label (and helper) to the start of the field, for dense settings rows.</summary>
    public bool IsInline
    {
        get => _inline;
        set
        {
            if (_inline == value) return;
            _inline = value;
            Layout();
        }
    }

    /// <summary>
    /// Overrides the bottom rule's color key in every state (list boxes use <c>border-subtle-01</c> while open);
    /// <c>null</c> restores the Carbon defaults.
    /// </summary>
    public string? RuleColorKeyOverride { get; set; }

    /// <summary>The 1px bottom rule (for controls that animate or hide it).</summary>
    public BoxView Rule => _rule;

    /// <summary>Horizontal padding inside the field box. Default 16 (<c>spacing-05</c>).</summary>
    public double FieldPadding
    {
        get => _field.Padding.Left;
        set
        {
            _field.Padding = new Thickness(value, 0);
            _overlayGrid.Margin = new Thickness(-value, 0);
        }
    }

    /// <summary>
    /// Flat field: no fill and no bottom rule (inline Select and Dropdown, toolbar fields). Focus and error outlines
    /// still show.
    /// </summary>
    public bool IsFlat { get; set; }

    /// <summary>The message currently shown (helper, error or warning), for the input's accessibility hint.</summary>
    public string? CurrentMessage => _message.IsVisible ? _message.Text : null;

    /// <summary>Re-applies colors and texts for a state. Call it from the owner's <c>ApplyVisuals</c>.</summary>
    public void Apply(CdsInteractionState state)
    {
        _lastState = state;
        if (_layoutFluid != IsFluidLayout)
        {
            // The field left (or entered) a fluid form since the last layout.
            OnFluidChanged();
            return;
        }
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var readOnly = state.HasFlag(CdsInteractionState.ReadOnly);
        var invalid = state.HasFlag(CdsInteractionState.Invalid) && !disabled && !readOnly;
        var warn = state.HasFlag(CdsInteractionState.Warn) && !invalid && !disabled && !readOnly;
        var focused = state.HasFlag(CdsInteractionState.Focused) && !disabled;
        var hover = state.HasFlag(CdsInteractionState.Hover) && !disabled && !readOnly;

        Set(_field, VisualElement.BackgroundColorProperty,
            readOnly ? null : hover ? CdsColorKeys.FieldHover01 : IsFlat ? null : CdsColorKeys.Field01);
        Set(_rule, BoxView.ColorProperty,
            RuleColorKeyOverride ?? (disabled || IsFlat ? null : readOnly ? CdsColorKeys.BorderSubtle01 : CdsColorKeys.BorderStrong01));
        Set(_label, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        Set(_counter, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);

        var fluid = IsFluidLayout;
        _outline.IsActive = !fluid && (focused || invalid);
        _outline.SetColorKey(focused ? CdsColorKeys.Focus : CdsColorKeys.SupportError);
        _fluidOutline.IsActive = fluid && (focused || invalid);
        _fluidOutline.SetColorKey(focused ? CdsColorKeys.Focus : CdsColorKeys.SupportError);

        if (invalid)
        {
            _statusIcon.Icon = CdsIconName.WarningFilled;
            _statusIcon.IsVisible = true;
            Set(_statusIcon, CdsIcon.ColorProperty, CdsColorKeys.SupportError);
        }
        else if (warn)
        {
            _statusIcon.Icon = CdsIconName.WarningAltFilled;
            _statusIcon.IsVisible = true;
            Set(_statusIcon, CdsIcon.ColorProperty, CdsColorKeys.SupportWarning);
        }
        else
        {
            _statusIcon.IsVisible = false;
        }

        string? text;
        string key;
        if (invalid && !string.IsNullOrEmpty(InvalidText)) (text, key) = (InvalidText, CdsColorKeys.TextError);
        else if (warn && !string.IsNullOrEmpty(WarnText)) (text, key) = (WarnText, CdsColorKeys.TextPrimary);
        else (text, key) = (fluid ? null : HelperText, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextHelper);
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrEmpty(text);
        Set(_message, Microsoft.Maui.Controls.Label.TextColorProperty, key);

        // Fluid: the message row (divider, text, status icon) belongs to the field and shows for invalid and warning
        // states; the outline replaces the bottom rule, and a warning keeps a strong rule under the message.
        _messageBlock.IsVisible = fluid && (invalid || warn);
        _shellRule.IsVisible = fluid && warn;
        _rule.Opacity = fluid && (invalid || warn) ? 0 : 1;
        Set(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        Set(_shellRule, BoxView.ColorProperty, CdsColorKeys.BorderStrong01);
        Set(_messageBlock, VisualElement.BackgroundColorProperty, readOnly ? null : CdsColorKeys.Field01);

        _labelRow.IsVisible = (fluid || !HideLabel) && (!string.IsNullOrEmpty(Label) || _counter.IsVisible);
    }

    void OnFluidChanged()
    {
        Layout();
        ApplyFieldHeight();
        Apply(_lastState);
        FluidLayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    void ApplyFieldHeight()
    {
        if (IsFluidLayout)
        {
            _field.MinimumHeightRequest = FluidFieldHeight;
            _field.HeightRequest = _fieldHeight < 0 ? -1 : FluidFieldHeight;
        }
        else
        {
            _field.MinimumHeightRequest = -1;
            _field.HeightRequest = _fieldHeight;
        }
    }

    void Set(Element target, BindableProperty property, string? key)
    {
        if (_applied.TryGetValue((target, property), out var current) && current == key) return;
        _applied[(target, property)] = key;
        CdsTokens.SetColor(target, property, key);
    }

    void Layout()
    {
        Children.Clear();
        RowDefinitions.Clear();
        ColumnDefinitions.Clear();
        _shell.Children.Clear();
        _shell.RowDefinitions.Clear();
        _messageBlock.Children.Clear();
        _field.Children.Remove(_labelRow);
        Trailing.Remove(_statusIcon);

        _layoutFluid = IsFluidLayout;
        if (_layoutFluid)
        {
            LayoutFluid();
            return;
        }

        Trailing.Insert(0, _statusIcon);
        _statusIcon.Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
        _statusIcon.VerticalOptions = LayoutOptions.Fill;
        _statusIcon.HorizontalOptions = LayoutOptions.Fill;
        _inputSlot.Margin = new Thickness(0);
        Trailing.VerticalOptions = LayoutOptions.Center;
        Trailing.Margin = new Thickness(0);
        _labelRow.InputTransparent = false;
        _message.Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0);
        if (_inline)
        {
            // [label | field] with the message under the field.
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _labelRow.Margin = new Thickness(0, 0, CdsSpacing.Spacing05, 0);
            _labelRow.VerticalOptions = LayoutOptions.Center;
            this.Add(_labelRow, 0, 0);
            this.Add(_field, 1, 0);
            this.Add(_message, 1, 1);
        }
        else
        {
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _labelRow.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03);
            _labelRow.VerticalOptions = LayoutOptions.Start;
            this.Add(_labelRow, 0, 0);
            this.Add(_field, 0, 1);
            this.Add(_message, 0, 2);
        }
    }

    void LayoutFluid()
    {
        // The label floats over the top of the field (13 from the top); the input sits below it.
        _labelRow.Margin = new Thickness(0, 13, 0, 0);
        _labelRow.VerticalOptions = LayoutOptions.Start;
        _labelRow.InputTransparent = true;
        _field.Add(_labelRow, 0);
        SetColumnSpan((BindableObject)_labelRow, 2);
        _inputSlot.Margin = _fieldHeight < 0 ? new Thickness(0, 32, 0, 13) : new Thickness(0, 29, 0, 10);
        Trailing.VerticalOptions = LayoutOptions.End;
        Trailing.Margin = new Thickness(0, 0, 0, 13);

        // Message row: divider, text, and the status icon at its end, inside the field's outline.
        _statusIcon.Margin = new Thickness(0, CdsSpacing.Spacing03 + 1, CdsSpacing.Spacing05, 0);
        _statusIcon.VerticalOptions = LayoutOptions.Start;
        _statusIcon.HorizontalOptions = LayoutOptions.End;
        _message.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03 + 1, 48, CdsSpacing.Spacing03);
        _messageBlock.MinimumHeightRequest = 33;
        _messageBlock.Add(_divider);
        _messageBlock.Add(_message);
        _messageBlock.Add(_statusIcon);

        RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _shell.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _shell.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _shell.Add(_field, 0, 0);
        _shell.Add(_messageBlock, 0, 1);
        _shell.Add(_shellRule, 0, 1);
        _shell.Add(_fluidOutline, 0, 0);
        SetRowSpan((BindableObject)_fluidOutline, 2);
        this.Add(_shell, 0, 0);
    }
}
