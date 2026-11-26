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
/// focus and error outlines wrap the field and its message; the size and inline settings are ignored. As in Carbon's
/// fluid wrappers, a read-only fluid field keeps its <c>field-01</c> fill, a long label truncates on one line, and the
/// field fills the height of its cell (the space under the field and message is <c>field-01</c>) so fields of different
/// heights in a <see cref="CdsFluidFormRow"/> line up.
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
    // Fluid: fills the rest of a cell taller than the field (a neighbour in a CdsFluidFormRow shows a message), as
    // Carbon's fluid wrappers do with block-size: 100% on a field-01 background.
    // An empty Grid measures 0 high (a BoxView asks for 40), so the fill takes only space the cell has to spare.
    readonly Grid _fluidFill = new() { InputTransparent = true, IsVisible = false };
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
    readonly Grid _overlayGrid = new() { InputTransparent = true };
    readonly CdsIcon _statusIcon = new() { IsVisible = false, Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0) };
    readonly CdsText _message = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, IsVisible = false };
    readonly Dictionary<(BindableObject, BindableProperty), string?> _applied = [];
    readonly CdsMessageAlert _alert = new();
    bool _inline;
    bool _inlineHugs;
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
        // The rule and outline cancel the field's horizontal padding, whatever a control sets it to (the inline select pads
        // its start by 8): a fixed -16 drew the inline select's error outline 8 to the left of the field.
        _field.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == PaddingProperty.PropertyName) SyncOverlayMargin();
        };
        SyncOverlayMargin();
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

    void SyncOverlayMargin()
    {
        var padding = _field.Padding;
        _overlayGrid.Margin = new Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom);
    }

    /// <summary>The overlay of rule and outline (for tests).</summary>
    internal Grid OverlayGrid => _overlayGrid;

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

    /// <summary>
    /// Label (and helper text, in the label column under the label) to the start of the field, for dense settings rows.
    /// Invalid and warning messages stay under the field.
    /// </summary>
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
    /// Inline only: the field is as wide as its content, at most <see cref="InlineFieldMaxWidth"/>, instead of filling the
    /// row (Carbon's inline Dropdown, MultiSelect and Select: <c>inline-size: auto</c>, the field at most 30rem). An error
    /// or warning message under it may use the rest of the row.
    /// </summary>
    internal bool InlineFieldHugsContent
    {
        get => _inlineHugs;
        set
        {
            if (_inlineHugs == value) return;
            _inlineHugs = value;
            Layout();
        }
    }

    /// <summary>Widest a content-sized inline field gets (Carbon's <c>.cds--dropdown--inline</c> field: 30rem).</summary>
    internal const double InlineFieldMaxWidth = 480;

    /// <summary>
    /// Overrides the bottom rule's color key in every state (list boxes use <c>border-subtle-01</c> while open);
    /// <c>null</c> restores the Carbon defaults.
    /// </summary>
    public string? RuleColorKeyOverride { get; set; }

    /// <summary>
    /// The bottom rule's color key while the field is disabled; <c>null</c> (the default) drops the rule, as Carbon does for
    /// most disabled fields. Fluid number inputs and fluid search keep a <c>border-subtle-01</c> rule.
    /// </summary>
    public string? DisabledRuleColorKey { get; set; }

    /// <summary>
    /// Hides the bottom rule in every state, for a field that is one part of a group drawing its own outline and message
    /// (the parts of a fluid time picker or range date picker while the group shows an error or warning).
    /// </summary>
    internal bool IsRuleSuppressed { get; set; }

    /// <summary>The 1px bottom rule (for controls that animate or hide it).</summary>
    public BoxView Rule => _rule;

    /// <summary>Horizontal padding inside the field box. Default 16 (<c>spacing-05</c>).</summary>
    public double FieldPadding
    {
        get => _field.Padding.Left;
        set
        {
            _field.Padding = new Thickness(value, 0);
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

        var fluid = IsFluidLayout;
        Set(_field, VisualElement.BackgroundColorProperty, FieldFillKey(fluid, readOnly, hover, IsFlat));
        Set(_rule, BoxView.ColorProperty,
            RuleColorKeyOverride ?? (IsFlat ? null : disabled ? DisabledRuleColorKey : readOnly ? CdsColorKeys.BorderSubtle01 : CdsColorKeys.BorderStrong01));
        Set(_fluidFill, VisualElement.BackgroundColorProperty, IsFlat ? null : CdsColorKeys.Field01);
        Set(_label, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        Set(_counter, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);

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
        string? alert = null;
        if (invalid && !string.IsNullOrEmpty(InvalidText)) (text, key, alert) = (InvalidText, CdsColorKeys.TextError, InvalidText);
        else if (warn && !string.IsNullOrEmpty(WarnText)) (text, key, alert) = (WarnText, CdsColorKeys.TextPrimary, WarnText);
        else (text, key) = (fluid ? null : HelperText, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextHelper);
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrEmpty(text);
        Set(_message, Microsoft.Maui.Controls.Label.TextColorProperty, key);
        PlaceInlineMessage(isHelper: alert is null);
        // role="alert": an error or warning that appears or changes while the field is on screen is spoken once.
        _alert.Update(this, alert);

        // Fluid: the message row (divider, text, status icon) belongs to the field and shows for invalid and warning
        // states; the outline replaces the bottom rule, and a warning keeps a strong rule under the message.
        _messageBlock.IsVisible = fluid && (invalid || warn);
        _shellRule.IsVisible = fluid && warn;
        _rule.Opacity = IsRuleSuppressed || (fluid && (invalid || warn)) ? 0 : 1;
        Set(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        Set(_shellRule, BoxView.ColorProperty, CdsColorKeys.BorderStrong01);
        Set(_messageBlock, VisualElement.BackgroundColorProperty, CdsColorKeys.Field01);

        _labelRow.IsVisible = (fluid || !HideLabel) && (!string.IsNullOrEmpty(Label) || _counter.IsVisible);
    }

    /// <summary>
    /// The field fill: <c>field-01</c>, <c>field-hover-01</c> on hover, none when flat. Read-only fields are transparent in
    /// the default layout; a fluid field keeps <c>field-01</c> (Carbon's fluid wrappers paint it under the transparent
    /// read-only input), so a read-only part of a fluid row never shows what is behind it.
    /// </summary>
    internal static string? FieldFillKey(bool fluid, bool readOnly, bool hover, bool flat) =>
        readOnly ? (fluid && !flat ? CdsColorKeys.Field01 : null) : hover ? CdsColorKeys.FieldHover01 : flat ? null : CdsColorKeys.Field01;

    // Inline (Carbon's label-helper wrapper): helper text sits in the label column under the label, at most 8rem wide;
    // an invalid or warning message sits under the field.
    void PlaceInlineMessage(bool isHelper)
    {
        if (!_inline || _layoutFluid) return;
        var column = isHelper ? 0 : 1;
        if (GetColumn((BindableObject)_message) != column) SetColumn((BindableObject)_message, column);
        SetColumnSpan((BindableObject)_message, !isHelper && _inlineHugs ? 2 : 1);
        _message.MaximumWidthRequest = isHelper ? InlineHelperMaxWidth : double.PositiveInfinity;
        _message.Margin = isHelper ? new Thickness(0, CdsSpacing.Spacing02, CdsSpacing.Spacing05, 0) : new Thickness(0, CdsSpacing.Spacing02, 0, 0);
    }

    /// <summary>Widest an inline helper text gets (Carbon's <c>label-helper-wrapper</c>: 8rem).</summary>
    internal const double InlineHelperMaxWidth = 128;

    /// <summary>The grid column of the message row (for tests): 0 under the label, 1 under the field when inline.</summary>
    internal int MessageColumn => GetColumn((BindableObject)_message);

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
        _fluidFill.IsVisible = _layoutFluid;
        var hugsField = _inline && _inlineHugs && !_layoutFluid;
        _field.HorizontalOptions = hugsField ? LayoutOptions.Start : LayoutOptions.Fill;
        _field.MaximumWidthRequest = hugsField ? InlineFieldMaxWidth : double.PositiveInfinity;
        if (_layoutFluid)
        {
            LayoutFluid();
            return;
        }

        // A long label wraps above the field.
        _label.MaxLines = -1;
        _label.LineBreakMode = LineBreakMode.WordWrap;

        Trailing.Insert(0, _statusIcon);
        _statusIcon.Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
        _statusIcon.VerticalOptions = LayoutOptions.Fill;
        _statusIcon.HorizontalOptions = LayoutOptions.Fill;
        _inputSlot.Margin = new Thickness(0);
        Trailing.VerticalOptions = LayoutOptions.Center;
        Trailing.Margin = new Thickness(0);
        _labelRow.InputTransparent = false;
        _message.Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0);
        _message.MaximumWidthRequest = double.PositiveInfinity;
        var hugs = _inline && _inlineHugs;
        if (_inline)
        {
            // [label | field]; Apply puts helper text under the label and an error or warning under the field. A field
            // that hugs its content sits in an Auto column and leaves the rest of the row empty.
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ColumnDefinitions.Add(new ColumnDefinition(hugs ? GridLength.Auto : GridLength.Star));
            if (hugs) ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _labelRow.Margin = new Thickness(0, 0, CdsSpacing.Spacing05, 0);
            _labelRow.VerticalOptions = LayoutOptions.Center;
            this.Add(_labelRow, 0, 0);
            this.Add(_field, 1, 0);
            this.Add(_message, 1, 1);
            SetColumnSpan((BindableObject)_message, hugs ? 2 : 1);
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
            SetColumnSpan((BindableObject)_message, 1);
        }
    }

    void LayoutFluid()
    {
        // The label floats over the top of the field (13 from the top); the input sits below it.
        _labelRow.Margin = new Thickness(0, 13, 0, 0);
        _labelRow.VerticalOptions = LayoutOptions.Start;
        _labelRow.InputTransparent = true;
        // The label floats in a fixed band of the 64px field: a long one truncates instead of wrapping over the value
        // (Carbon scrolls it). The input keeps the full label as its accessible name.
        _label.MaxLines = 1;
        _label.LineBreakMode = LineBreakMode.TailTruncation;
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

        // The shell fills the cell it is given (Carbon's block-size: 100%), so fields of different heights in one row line
        // up: below the field and its message, the rest is field-01.
        RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _shell.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _shell.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _shell.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _shell.Add(_fluidFill, 0, 2);
        _shell.Add(_field, 0, 0);
        _shell.Add(_messageBlock, 0, 1);
        _shell.Add(_shellRule, 0, 1);
        _shell.Add(_fluidOutline, 0, 0);
        SetRowSpan((BindableObject)_fluidOutline, 2);
        this.Add(_shell, 0, 0);
    }
}

/// <summary>
/// Speaks a form message once when an error or warning appears or changes while its owner is on screen, as Carbon's
/// <c>role="alert"</c> message does (assertive). A message already showing when the owner loads is not spoken; it is in the
/// input's hint. Clearing the message re-arms it, so the same error is spoken again the next time it appears.
/// </summary>
internal sealed class CdsMessageAlert
{
    string? _current;

    /// <summary>The alert text last seen (for tests).</summary>
    public string? Current => _current;

    /// <summary>Records the error or warning text now shown (<c>null</c> for none or helper text) and speaks a new one.</summary>
    public void Update(VisualElement owner, string? alertText)
    {
        if (string.IsNullOrEmpty(alertText))
        {
            _current = null;
            return;
        }
        if (alertText == _current) return;
        _current = alertText;
        if (owner.IsLoaded) CdsAnnouncer.Announce(alertText, CdsAnnouncePriority.Assertive);
    }
}
