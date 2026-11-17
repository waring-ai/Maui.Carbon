namespace Maui.Carbon;

public partial class CdsCheckboxGroup
{
    /// <summary>Bindable property for <see cref="Legend"/>.</summary>
    public static readonly BindableProperty LegendProperty = BindableProperty.Create(
        nameof(Legend), typeof(string), typeof(CdsCheckboxGroup), null,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Orientation"/>.</summary>
    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation), typeof(CdsChoiceOrientation), typeof(CdsCheckboxGroup), CdsChoiceOrientation.Vertical,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RebuildItems());

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsCheckboxGroup), null,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsCheckboxGroup), false,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsCheckboxGroup), null,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsCheckboxGroup), false,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsCheckboxGroup), null,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsCheckboxGroup), false,
        propertyChanged: static (b, _, _) => ((CdsCheckboxGroup)b).RefreshVisuals());

    /// <summary>The group's name, shown above the boxes in <c>label-01</c>. Write it as the question ("Notify me by").</summary>
    public string? Legend
    {
        get => (string?)GetValue(LegendProperty);
        set => SetValue(LegendProperty, value);
    }

    /// <summary>Vertical (default, 8px apart) or horizontal (wrapping row, 16px apart).</summary>
    public CdsChoiceOrientation Orientation
    {
        get => (CdsChoiceOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Helper text under the group; hidden while an invalid or warning message shows.</summary>
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error state: every box border turns <c>support-error</c> and <see cref="InvalidText"/> shows.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>What is wrong and how to fix it ("Select at least one channel").</summary>
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

    /// <summary>Read-only: boxes get <c>icon-disabled</c> borders, checked boxes are transparent with a mark, and nothing toggles.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
}
