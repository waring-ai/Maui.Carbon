namespace Maui.Carbon;

public partial class CdsToggle
{
    /// <summary>Bindable property for <see cref="IsToggled"/>.</summary>
    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled), typeof(bool), typeof(CdsToggle), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsToggle)b).OnToggledChanged((bool)n));

    /// <summary>Bindable property for <see cref="LabelText"/>.</summary>
    public static readonly BindableProperty LabelTextProperty = BindableProperty.Create(
        nameof(LabelText), typeof(string), typeof(CdsToggle), null,
        propertyChanged: static (b, _, _) => ((CdsToggle)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="LabelA"/>.</summary>
    public static readonly BindableProperty LabelAProperty = BindableProperty.Create(
        nameof(LabelA), typeof(string), typeof(CdsToggle), "Off",
        propertyChanged: static (b, _, _) => ((CdsToggle)b).UpdateStateText());

    /// <summary>Bindable property for <see cref="LabelB"/>.</summary>
    public static readonly BindableProperty LabelBProperty = BindableProperty.Create(
        nameof(LabelB), typeof(string), typeof(CdsToggle), "On",
        propertyChanged: static (b, _, _) => ((CdsToggle)b).UpdateStateText());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsToggle), false,
        propertyChanged: static (b, _, _) => ((CdsToggle)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsToggleSize), typeof(CdsToggle), CdsToggleSize.Md,
        propertyChanged: static (b, _, _) => ((CdsToggle)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsToggle), false,
        propertyChanged: static (b, _, _) => ((CdsToggle)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(System.Windows.Input.ICommand), typeof(CdsToggle), null,
        propertyChanged: static (b, _, _) => ((CdsToggle)b).SetCommand(((CdsToggle)b).Command, ((CdsToggle)b).CommandParameter));

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsToggle), null,
        propertyChanged: static (b, _, _) => ((CdsToggle)b).SetCommand(((CdsToggle)b).Command, ((CdsToggle)b).CommandParameter));

    /// <summary>Whether the setting is on (two-way).</summary>
    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    /// <summary>
    /// The setting's name, above the switch and the accessible name. Keep it constant and change only the state text;
    /// never rewrite it to describe the state.
    /// </summary>
    public string? LabelText
    {
        get => (string?)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>State text beside the switch while off. Default "Off"; an adjective of three words or fewer ("Disabled").</summary>
    public string? LabelA
    {
        get => (string?)GetValue(LabelAProperty);
        set => SetValue(LabelAProperty, value);
    }

    /// <summary>State text beside the switch while on. Default "On"; an adjective of three words or fewer ("Enabled").</summary>
    public string? LabelB
    {
        get => (string?)GetValue(LabelBProperty);
        set => SetValue(LabelBProperty, value);
    }

    /// <summary>Hides <see cref="LabelText"/> above the switch (it stays the accessible name) and shows it beside the switch instead of the state text.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>Medium 48x24 (default) or small 32x16.</summary>
    public CdsToggleSize Size
    {
        get => (CdsToggleSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Read-only: focusable and announced but does not switch; outlined track with a <c>text-primary</c> knob.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Command run after the user switches the toggle.</summary>
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
