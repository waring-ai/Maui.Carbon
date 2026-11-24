namespace Maui.Carbon;

/// <summary>
/// The message row a fluid group draws under all of its parts (the fluid time picker, and the picker-level message of a
/// fluid range date picker) in the invalid and warning states: a 1px <c>border-subtle-01</c> divider inset 16px, the
/// message (<c>text-error</c> when invalid, <c>text-primary</c> for a warning) on <c>field-01</c>, the status icon 8px
/// below the divider at the end, and for a warning the 1px <c>border-strong-01</c> bottom rule that the parts give up.
/// </summary>
internal sealed class CdsFluidGroupMessage : Grid
{
    readonly BoxView _divider = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03 + 1, 48, CdsSpacing.Spacing03) };
    readonly CdsIcon _icon = new() { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.End, Margin = new Thickness(0, CdsSpacing.Spacing03 + 1, CdsSpacing.Spacing05, 0) };
    readonly BoxView _rule = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
    (bool Invalid, bool Warn)? _applied;

    public CdsFluidGroupMessage()
    {
        IsVisible = false;
        Add(_divider);
        Add(_text);
        Add(_icon);
        Add(_rule);
        CdsTokens.SetColor(this, BackgroundColorProperty, CdsColorKeys.Field01);
        CdsTokens.SetColor(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(_rule, BoxView.ColorProperty, CdsColorKeys.BorderStrong01);
    }

    /// <summary>The message shown, or <c>null</c> while the row is hidden.</summary>
    public string? Message => IsVisible && !string.IsNullOrEmpty(_text.Text) ? _text.Text : null;

    /// <summary>Whether the warning's bottom rule is drawn (for tests).</summary>
    internal bool ShowsRule => IsVisible && _rule.IsVisible;

    /// <summary>Shows the row for an invalid (wins) or warning state, or hides it.</summary>
    public void Apply(bool invalid, bool warn, string? text)
    {
        warn &= !invalid;
        IsVisible = invalid || warn;
        _text.Text = text;
        _text.IsVisible = !string.IsNullOrEmpty(text);
        _rule.IsVisible = warn;
        if (_applied == (invalid, warn)) return;
        _applied = (invalid, warn);
        _icon.Icon = invalid ? CdsIconName.WarningFilled : CdsIconName.WarningAltFilled;
        CdsTokens.SetColor(_icon, CdsIcon.ColorProperty, invalid ? CdsColorKeys.SupportError : CdsColorKeys.SupportWarning);
        CdsTokens.SetColor(_text, Label.TextColorProperty, invalid ? CdsColorKeys.TextError : CdsColorKeys.TextPrimary);
    }
}
