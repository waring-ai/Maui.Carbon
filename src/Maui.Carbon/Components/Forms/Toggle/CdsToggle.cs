namespace Maui.Carbon;

/// <summary>
/// Immediately switches a single setting on or off: a 48x24 pill track with an 18px knob (<c>toggle-off</c> when off,
/// <c>support-success</c> when on), or the 32x16 <see cref="CdsToggleSize.Sm"/> size with a check mark. Use it only when
/// the change takes effect at once; if a Save step follows, use a <see cref="CdsCheckbox"/>. The state text beside the
/// switch, not the track color, tells the state.
/// </summary>
public partial class CdsToggle : CdsControl
{
    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0, HorizontalOptions = LayoutOptions.Start };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary };
    readonly Grid _switchRow = new() { ColumnSpacing = CdsSpacing.Spacing03 };
    readonly Grid _trackHost = new() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
    readonly CdsToggleGlyph _glyph = new();
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Offset, 12);
    readonly CdsText _stateText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center };
    readonly CdsHitTarget _hit;

    /// <summary>Creates an off toggle.</summary>
    public CdsToggle()
    {
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _switchRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _switchRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _trackHost.Add(_glyph);
        _trackHost.Add(_ring);
        _switchRow.Add(_trackHost, 0);
        _switchRow.Add(_stateText, 1);
        _stateText.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        AutomationProperties.SetExcludedWithChildren(_switchRow, true);
        _switchRow.InputTransparent = true;
        _label.InputTransparent = true;

        _hit = CreateHitTarget(CdsAccessibilityRole.Switch);
        _root.Add(_label, 0, 0);
        _root.Add(_switchRow, 0, 1);
        _root.Add(_hit, 0, 0);
        Grid.SetRowSpan(_hit, 2);
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Raised when <see cref="IsToggled"/> changes (by the user or in code).</summary>
    public event EventHandler<ToggledEventArgs>? Toggled;

    /// <summary>Raised when the user activates the switch (click, tap, Enter or Space), after the value changed.</summary>
    public event EventHandler? Clicked;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsReadOnly) return;
        IsToggled = !IsToggled;
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        var v = CdsToggleTokens.Resolve(state, IsToggled);
        SetColor(_glyph, CdsToggleGlyph.TrackColorProperty, v.Track);
        SetColor(_glyph, CdsToggleGlyph.TrackBorderColorProperty, v.TrackBorder);
        SetColor(_glyph, CdsToggleGlyph.KnobColorProperty, v.Knob);
        SetColor(_glyph, CdsToggleGlyph.CheckColorProperty, Size == CdsToggleSize.Sm ? v.Check : null);
        SetColor(_stateText, Microsoft.Maui.Controls.Label.TextColorProperty, v.StateText);
        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, v.LabelText);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
        // aria-readonly: focusable, and screen readers say why it does not switch.
        _hit.IsReadOnly = IsReadOnly && !Is(CdsInteractionState.Disabled);
    }

    void UpdateChrome()
    {
        var small = Size == CdsToggleSize.Sm;
        _glyph.Apply(small);
        _trackHost.WidthRequest = _glyph.TrackWidth;
        _trackHost.HeightRequest = _glyph.TrackHeight;
        _ring.Margin = new Thickness(-3);
        _label.Text = LabelText;
        _label.IsVisible = !HideLabel && !string.IsNullOrEmpty(LabelText);
        _label.Margin = new Thickness(0, 0, 0, small ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05);
        UpdateStateText();
        _glyph.Progress = IsToggled ? 1 : 0;
        SemanticProperties.SetDescription(_hit, LabelText);
        _hit.IsChecked = IsToggled;
        RefreshVisuals();
    }

    // The state text is "Off"/"On"; with HideLabel the label text takes its place beside the switch.
    void UpdateStateText()
    {
        _stateText.Text = HideLabel && !string.IsNullOrEmpty(LabelText) ? LabelText : IsToggled ? LabelB : LabelA;
        _stateText.IsVisible = !string.IsNullOrEmpty(_stateText.Text);
    }

    void OnToggledChanged(bool value)
    {
        UpdateStateText();
        _hit.IsChecked = value;
        RefreshVisuals();
        // The knob slides over duration-fast-01; it jumps when reduced motion is on or the control is not on screen.
        _ = CdsMotion.AnimateAsync(this, "CdsToggleKnob", p => _glyph.Progress = p,
            _glyph.Progress, value ? 1 : 0, CdsDuration.Fast01, CdsEasing.ExitProductive);
        Toggled?.Invoke(this, new ToggledEventArgs(value));
    }
}
