namespace Maui.Carbon;

/// <summary>
/// A single-line field that masks its value, with a button at the end to show or hide the characters. It uses the
/// <see cref="CdsTextInput"/> surface; the toggle is as wide as the field is tall (32 / 40 / 48), shows a
/// "Show password" / "Hide password" tooltip below the button and names itself the same way for screen readers. Put
/// format rules in <see cref="CdsTextInput.HelperText"/> and never echo the password in helper or error text.
/// </summary>
public partial class CdsPasswordInput : CdsTextInput
{
    /// <summary>Bindable property for <see cref="IsPasswordVisible"/>.</summary>
    public static readonly BindableProperty IsPasswordVisibleProperty = BindableProperty.Create(
        nameof(IsPasswordVisible), typeof(bool), typeof(CdsPasswordInput), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsPasswordInput)b).OnVisibilityChanged());

    /// <summary>Bindable property for <see cref="ShowPasswordLabel"/>.</summary>
    public static readonly BindableProperty ShowPasswordLabelProperty = BindableProperty.Create(
        nameof(ShowPasswordLabel), typeof(string), typeof(CdsPasswordInput), "Show password",
        propertyChanged: static (b, _, _) => ((CdsPasswordInput)b).UpdateToggle());

    /// <summary>Bindable property for <see cref="HidePasswordLabel"/>.</summary>
    public static readonly BindableProperty HidePasswordLabelProperty = BindableProperty.Create(
        nameof(HidePasswordLabel), typeof(string), typeof(CdsPasswordInput), "Hide password",
        propertyChanged: static (b, _, _) => ((CdsPasswordInput)b).UpdateToggle());

    // Field initializers run before the base constructor, which already calls UpdateChrome.
    readonly CdsFieldIconButton _toggle = new(CdsIconName.View);
    readonly CdsTooltip _tooltip = new()
    {
        Placement = CdsPlacement.BottomEnd,
        VerticalOptions = LayoutOptions.Fill,
        HorizontalOptions = LayoutOptions.End,
        Margin = new Thickness(0, 0, -CdsSpacing.Spacing05, 0),
    };

    /// <summary>Creates a medium password input.</summary>
    public CdsPasswordInput()
    {
        _toggle.Clicked += (_, _) =>
        {
            IsPasswordVisible = !IsPasswordVisible;
            PasswordVisibilityToggled?.Invoke(this, EventArgs.Empty);
        };
        _tooltip.Body = _toggle;
        Chrome.Trailing.Add(_tooltip);
        UpdateToggle();
    }

    /// <summary>Raised each time the show/hide button is used.</summary>
    public event EventHandler? PasswordVisibilityToggled;

    /// <summary>True while the characters are shown (two-way). Default false: masked.</summary>
    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    /// <summary>Name and tooltip of the toggle while the password is masked. Default "Show password".</summary>
    public string? ShowPasswordLabel
    {
        get => (string?)GetValue(ShowPasswordLabelProperty);
        set => SetValue(ShowPasswordLabelProperty, value);
    }

    /// <summary>Name and tooltip of the toggle while the password is shown. Default "Hide password".</summary>
    public string? HidePasswordLabel
    {
        get => (string?)GetValue(HidePasswordLabelProperty);
        set => SetValue(HidePasswordLabelProperty, value);
    }

    /// <inheritdoc/>
    protected override void UpdateChrome()
    {
        base.UpdateChrome();
        UpdateToggle();
    }

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (((previous ^ State) & CdsInteractionState.Disabled) != 0) UpdateToggle();
    }

    void OnVisibilityChanged() => UpdateToggle();

    void UpdateToggle()
    {
        var shown = IsPasswordVisible;
        Entry.IsPassword = !shown;
        var label = shown ? HidePasswordLabel : ShowPasswordLabel;
        _toggle.Icon = shown ? CdsIconName.ViewOff : CdsIconName.View;
        _toggle.Description = label;
        _tooltip.Text = label;
        var height = Size.Height();
        _toggle.SetSize(height, height);
        _toggle.IsEnabled = IsEnabled;
    }
}
