namespace Maui.Carbon;

public partial class CdsCheckbox
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsCheckbox), null,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsCheckbox), false,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool), typeof(CdsCheckbox), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsCheckbox)b).OnCheckedChanged((bool)n));

    /// <summary>Bindable property for <see cref="IsIndeterminate"/>.</summary>
    public static readonly BindableProperty IsIndeterminateProperty = BindableProperty.Create(
        nameof(IsIndeterminate), typeof(bool), typeof(CdsCheckbox), false,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).OnIndeterminateChanged());

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsCheckbox), null,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsCheckbox), false,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsCheckbox), null,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsCheckbox), false,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsCheckbox), null,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsCheckbox), false,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(System.Windows.Input.ICommand), typeof(CdsCheckbox), null,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).SetCommand(((CdsCheckbox)b).Command, ((CdsCheckbox)b).CommandParameter));

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsCheckbox), null,
        propertyChanged: static (b, _, _) => ((CdsCheckbox)b).SetCommand(((CdsCheckbox)b).Command, ((CdsCheckbox)b).CommandParameter));

    /// <summary>
    /// The visible label and the accessible name. Always provide one; keep it short (under about three words) and let
    /// long labels wrap rather than truncate.
    /// </summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Hides the label visually (for example in a table row); it stays the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>Whether the box is checked (two-way).</summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// The mixed state of a "select all" parent. You compute it from the children; it wins over <see cref="IsChecked"/>
    /// in the drawing and is announced as mixed. Activating the box clears it and checks the box.
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Text under the checkbox (indented past the box); replaced by the invalid or warning message.</summary>
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error state: the box border turns <c>support-error</c> and <see cref="InvalidText"/> shows with an error icon.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>What is wrong and how to fix it.</summary>
    public string? InvalidText
    {
        get => (string?)GetValue(InvalidTextProperty);
        set => SetValue(InvalidTextProperty, value);
    }

    /// <summary>Warning state: <see cref="WarnText"/> shows with a warning triangle. Ignored when invalid.</summary>
    public bool IsWarning
    {
        get => (bool)GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    /// <summary>The warning message.</summary>
    public string? WarnText
    {
        get => (string?)GetValue(WarnTextProperty);
        set => SetValue(WarnTextProperty, value);
    }

    /// <summary>Read-only: focusable and announced, but cannot be toggled; outlined box with an <c>icon-primary</c> mark.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Command run after the user toggles the checkbox.</summary>
    public System.Windows.Input.ICommand? Command
    {
        get => (System.Windows.Input.ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}
