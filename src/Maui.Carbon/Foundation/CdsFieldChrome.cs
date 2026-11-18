namespace Maui.Carbon;

/// <summary>
/// The shared Carbon form-field scaffold: label row (label + counter), the field box (<c>field-01</c> fill,
/// 1px <c>border-strong-01</c> bottom rule, 2px focus / error outline, trailing status icon and action slot), and the
/// message row (helper, invalid or warning text). Inputs put their native control in <see cref="Input"/>, set the
/// text properties, and call <see cref="Apply"/> from <see cref="CdsControl.ApplyVisuals"/>.
/// </summary>
public sealed class CdsFieldChrome : Grid
{
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

    /// <summary>Explicit field height (-1 lets the input size it, for text areas).</summary>
    public double FieldHeight
    {
        get => _field.HeightRequest;
        set => _field.HeightRequest = value;
    }

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

        _outline.IsActive = focused || invalid;
        _outline.SetColorKey(focused ? CdsColorKeys.Focus : CdsColorKeys.SupportError);

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
        else (text, key) = (HelperText, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextHelper);
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrEmpty(text);
        Set(_message, Microsoft.Maui.Controls.Label.TextColorProperty, key);

        _labelRow.IsVisible = !HideLabel && (!string.IsNullOrEmpty(Label) || _counter.IsVisible);
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
}
