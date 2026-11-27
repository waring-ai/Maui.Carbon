namespace Maui.Carbon;

/// <summary>
/// A 16px box for choosing zero, one or several independent options. Checked and indeterminate fill with
/// <c>icon-primary</c> and draw the mark in <c>icon-inverse</c>. The box and its label are one click target; focus is a
/// 2px <c>focus</c> outline offset 1px from the box. Use a <see cref="CdsToggle"/> when the change applies at once.
/// </summary>
public partial class CdsCheckbox : CdsControl
{
    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly Grid _boxHost = new() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Start };
    readonly CdsCheckGlyph _box = new();
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Offset, CdsRadius.Checkbox);
    // The label and the message row are created when first shown: a table's row checkboxes hide their label and never
    // have a message.
    CdsText? _label;
    CdsFormMessage? _message;
    readonly CdsHitTarget _hit;
    CdsInteractionState _inherited;
    string? _groupMessage;

    /// <summary>Creates an unchecked checkbox.</summary>
    public CdsCheckbox()
    {
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        _boxHost.Add(_box);
        _boxHost.Add(_ring);
        AutomationProperties.SetExcludedWithChildren(_boxHost, true);

        _hit = CreateHitTarget(CdsAccessibilityRole.Checkbox);

        _root.Add(_boxHost, 0, 0);
        _root.Add(_hit, 0, 0);
        Grid.SetColumnSpan(_hit, 2);

        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Creates a checkbox with a label.</summary>
    public CdsCheckbox(string label, bool isChecked = false) : this()
    {
        Label = label;
        IsChecked = isChecked;
    }

    /// <summary>Raised when <see cref="IsChecked"/> changes (by the user or in code).</summary>
    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    /// <summary>Raised when the user activates the checkbox (click, tap, Space), after the value changed.</summary>
    public event EventHandler? Clicked;

    /// <summary>Whether the checkbox is currently in the disabled state (own or inherited from a group).</summary>
    internal bool IsEffectivelyDisabled => Is(CdsInteractionState.Disabled) || _inherited.HasFlag(CdsInteractionState.Disabled);

    /// <summary>
    /// Sets what a <see cref="CdsCheckboxGroup"/> passes down: its state (invalid, read-only, disabled) and its message
    /// (helper, invalid or warning text), which joins the checkbox's own message in its accessibility hint: the group's
    /// message row is not in the accessibility tree.
    /// </summary>
    internal void SetInherited(CdsInteractionState flags, string? groupMessage = null)
    {
        if (_inherited == flags && _groupMessage == groupMessage) return;
        _inherited = flags;
        _groupMessage = groupMessage;
        RefreshVisuals();
    }

    /// <summary>The accessibility hint of the checkbox (for tests).</summary>
    internal string? AccessibilityHint => SemanticProperties.GetHint(_hit);

    /// <summary>The checkbox's hit target (for tests).</summary>
    internal CdsHitTarget HitTarget => _hit;

    /// <summary>The hint: the checkbox's own message, then its group's; <c>null</c> when there is neither.</summary>
    internal static string? CombineHint(string? own, string? group) =>
        string.IsNullOrEmpty(own) ? (string.IsNullOrEmpty(group) ? null : group)
        : string.IsNullOrEmpty(group) || group == own ? own : $"{own}. {group}";

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsEffectivelyDisabled || IsReadOnly || _inherited.HasFlag(CdsInteractionState.ReadOnly)) return;
        if (IsIndeterminate)
        {
            // Native behavior: activating a mixed checkbox checks it; the parent recomputes the mixed state.
            IsIndeterminate = false;
            IsChecked = true;
        }
        else
        {
            IsChecked = !IsChecked;
        }
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = EffectiveState();
        bool? shown = IsIndeterminate ? null : IsChecked;
        var v = CdsCheckboxTokens.Resolve(state, shown);
        SetColor(_box, CdsCheckGlyph.BorderColorProperty, v.Border);
        SetColor(_box, CdsCheckGlyph.FillColorProperty, v.Fill);
        SetColor(_box, CdsCheckGlyph.MarkColorProperty, v.Mark);
        _box.Mark = shown is null ? CdsCheckMark.Dash : shown == true ? CdsCheckMark.Check : CdsCheckMark.None;
        if (_label is not null) SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, v.Label);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);

        var message = CdsFormMessageResolver.Resolve(state, HelperText, InvalidText, WarnText);
        if (_message is not null || !string.IsNullOrEmpty(message.Text)) EnsureMessage().Apply(message);
        _hit.IsEnabled = !state.HasFlag(CdsInteractionState.Disabled);
        // aria-readonly (own or from the group): focusable, and screen readers say why it does not toggle.
        _hit.IsReadOnly = state.HasFlag(CdsInteractionState.ReadOnly) && !state.HasFlag(CdsInteractionState.Disabled);
        SemanticProperties.SetHint(_hit, CombineHint(message.Text, _groupMessage));
    }

    CdsInteractionState EffectiveState()
    {
        var state = State | _inherited;
        if (IsInvalid) state |= CdsInteractionState.Invalid;
        if (IsWarning) state |= CdsInteractionState.Warn;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        return state;
    }

    void UpdateChrome()
    {
        var shown = !HideLabel && !string.IsNullOrEmpty(Label);
        if (shown || _label is not null)
        {
            var label = EnsureLabel();
            label.Text = Label;
            label.IsVisible = shown;
        }
        _boxHost.Margin = new Thickness(0, 2, 0, shown ? 0 : 6);
        SemanticProperties.SetDescription(_hit, Label);
        RefreshVisuals();
    }

    CdsText EnsureLabel()
    {
        if (_label is not null) return _label;
        _label = new CdsText
        {
            LineBreakMode = LineBreakMode.WordWrap,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(CdsSpacing.Spacing03 + 2, 3, 0, 3),
            InputTransparent = true,
        };
        AutomationProperties.SetExcludedWithChildren(_label, true);
        // Under the hit target, which spans both columns.
        Grid.SetColumn(_label, 1);
        _root.Children.Insert(_root.Children.IndexOf(_hit), _label);
        return _label;
    }

    CdsFormMessage EnsureMessage()
    {
        if (_message is not null) return _message;
        _message = new CdsFormMessage { Margin = new Thickness(26, 2, 0, 0) };
        _root.Add(_message, 0, 1);
        Grid.SetColumnSpan(_message, 2);
        return _message;
    }

    void OnCheckedChanged(bool value)
    {
        _hit.IsChecked = IsIndeterminate ? null : value;
        RefreshVisuals();
        CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));
    }

    void OnIndeterminateChanged()
    {
        _hit.IsChecked = IsIndeterminate ? null : IsChecked;
        RefreshVisuals();
    }
}
