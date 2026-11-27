namespace Maui.Carbon;

public partial class CdsEditInPlace
{
    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(string), typeof(CdsEditInPlace), null, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsEditInPlace)b).OnValuePropertyChanged((string?)o, (string?)n));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsEditInPlace), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsEditInPlace), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsEditInPlace), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsEditInPlace), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsEditInPlace), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsEditInPlace), CdsSize.Sm, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="TypeStyle"/>.</summary>
    public static readonly BindableProperty TypeStyleProperty = BindableProperty.Create(
        nameof(TypeStyle), typeof(CdsTypeStyle), typeof(CdsEditInPlace), CdsTypeStyle.BodyCompact01, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="EditAlwaysVisible"/>.</summary>
    public static readonly BindableProperty EditAlwaysVisibleProperty = BindableProperty.Create(
        nameof(EditAlwaysVisible), typeof(bool), typeof(CdsEditInPlace), false, propertyChanged: static (b, _, _) => ((CdsEditInPlace)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="EditLabel"/>.</summary>
    public static readonly BindableProperty EditLabelProperty = BindableProperty.Create(
        nameof(EditLabel), typeof(string), typeof(CdsEditInPlace), "Edit", propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="SaveLabel"/>.</summary>
    public static readonly BindableProperty SaveLabelProperty = BindableProperty.Create(
        nameof(SaveLabel), typeof(string), typeof(CdsEditInPlace), "Save", propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="CancelLabel"/>.</summary>
    public static readonly BindableProperty CancelLabelProperty = BindableProperty.Create(
        nameof(CancelLabel), typeof(string), typeof(CdsEditInPlace), "Cancel", propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="ReadOnlyLabel"/>.</summary>
    public static readonly BindableProperty ReadOnlyLabelProperty = BindableProperty.Create(
        nameof(ReadOnlyLabel), typeof(string), typeof(CdsEditInPlace), "Edit off", propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="ReadOnlyToggletipText"/>.</summary>
    public static readonly BindableProperty ReadOnlyToggletipTextProperty = BindableProperty.Create(
        nameof(ReadOnlyToggletipText), typeof(string), typeof(CdsEditInPlace), "This field is read-only and cannot be edited", propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="TooltipPlacement"/>.</summary>
    public static readonly BindableProperty TooltipPlacementProperty = BindableProperty.Create(
        nameof(TooltipPlacement), typeof(CdsPlacement), typeof(CdsEditInPlace), CdsPlacement.Top, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="EditTooltipPlacement"/>.</summary>
    public static readonly BindableProperty EditTooltipPlacementProperty = BindableProperty.Create(
        nameof(EditTooltipPlacement), typeof(CdsPlacement?), typeof(CdsEditInPlace), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="SaveTooltipPlacement"/>.</summary>
    public static readonly BindableProperty SaveTooltipPlacementProperty = BindableProperty.Create(
        nameof(SaveTooltipPlacement), typeof(CdsPlacement?), typeof(CdsEditInPlace), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="CancelTooltipPlacement"/>.</summary>
    public static readonly BindableProperty CancelTooltipPlacementProperty = BindableProperty.Create(
        nameof(CancelTooltipPlacement), typeof(CdsPlacement?), typeof(CdsEditInPlace), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="ToggletipPlacement"/>.</summary>
    public static readonly BindableProperty ToggletipPlacementProperty = BindableProperty.Create(
        nameof(ToggletipPlacement), typeof(CdsPlacement), typeof(CdsEditInPlace), CdsPlacement.Top, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="SaveCommand"/>.</summary>
    public static readonly BindableProperty SaveCommandProperty = BindableProperty.Create(
        nameof(SaveCommand), typeof(System.Windows.Input.ICommand), typeof(CdsEditInPlace), null);

    /// <summary>Bindable property for <see cref="CancelCommand"/>.</summary>
    public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(
        nameof(CancelCommand), typeof(System.Windows.Input.ICommand), typeof(CdsEditInPlace), null);

    /// <summary>The value (two-way). Setting it while not editing also makes it the value a cancel returns to.</summary>
    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The accessible name of the input (Carbon's <c>labelText</c>). Required: the label is not drawn.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Shown while the value is empty: an example, never the label.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Error state: a 2px <c>support-error</c> border at rest, the error icon in the toolbar and <see cref="InvalidText"/> under the field. Save is disabled while invalid.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>What is wrong and how to fix it, under the field while <see cref="IsInvalid"/>.</summary>
    public string? InvalidText
    {
        get => (string?)GetValue(InvalidTextProperty);
        set => SetValue(InvalidTextProperty, value);
    }

    /// <summary>Read only: the value can be focused and copied but not changed; an edit-off toggletip explains why.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Height: sm (default) 40, md 48, lg 64, with a 32, 40 or 48 input inside. Other sizes clamp to sm or lg.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The type of the value (Carbon's <c>inheritTypography</c>): use the style of the text it edits, for example <see cref="CdsTypeStyle.Heading03"/> for a title. Pick a <see cref="Size"/> it fits in.</summary>
    public CdsTypeStyle TypeStyle
    {
        get => (CdsTypeStyle)GetValue(TypeStyleProperty);
        set => SetValue(TypeStyleProperty, value);
    }

    /// <summary>Show the edit button at rest too, not only on hover (touch screens have no hover).</summary>
    public bool EditAlwaysVisible
    {
        get => (bool)GetValue(EditAlwaysVisibleProperty);
        set => SetValue(EditAlwaysVisibleProperty, value);
    }

    /// <summary>Tooltip and accessible name of the edit button.</summary>
    public string EditLabel
    {
        get => (string)GetValue(EditLabelProperty);
        set => SetValue(EditLabelProperty, value);
    }

    /// <summary>Tooltip and accessible name of the save button.</summary>
    public string SaveLabel
    {
        get => (string)GetValue(SaveLabelProperty);
        set => SetValue(SaveLabelProperty, value);
    }

    /// <summary>Tooltip and accessible name of the cancel button.</summary>
    public string CancelLabel
    {
        get => (string)GetValue(CancelLabelProperty);
        set => SetValue(CancelLabelProperty, value);
    }

    /// <summary>Accessible name of the edit-off button shown while read only.</summary>
    public string ReadOnlyLabel
    {
        get => (string)GetValue(ReadOnlyLabelProperty);
        set => SetValue(ReadOnlyLabelProperty, value);
    }

    /// <summary>The text of the toggletip the edit-off button opens while read only.</summary>
    public string ReadOnlyToggletipText
    {
        get => (string)GetValue(ReadOnlyToggletipTextProperty);
        set => SetValue(ReadOnlyToggletipTextProperty, value);
    }

    /// <summary>Where the tooltips of the edit, save and cancel buttons sit. Default <see cref="CdsPlacement.Top"/>.</summary>
    public CdsPlacement TooltipPlacement
    {
        get => (CdsPlacement)GetValue(TooltipPlacementProperty);
        set => SetValue(TooltipPlacementProperty, value);
    }

    /// <summary>The edit button's tooltip placement, when it differs from <see cref="TooltipPlacement"/>.</summary>
    public CdsPlacement? EditTooltipPlacement
    {
        get => (CdsPlacement?)GetValue(EditTooltipPlacementProperty);
        set => SetValue(EditTooltipPlacementProperty, value);
    }

    /// <summary>The save button's tooltip placement, when it differs from <see cref="TooltipPlacement"/>.</summary>
    public CdsPlacement? SaveTooltipPlacement
    {
        get => (CdsPlacement?)GetValue(SaveTooltipPlacementProperty);
        set => SetValue(SaveTooltipPlacementProperty, value);
    }

    /// <summary>The cancel button's tooltip placement, when it differs from <see cref="TooltipPlacement"/>.</summary>
    public CdsPlacement? CancelTooltipPlacement
    {
        get => (CdsPlacement?)GetValue(CancelTooltipPlacementProperty);
        set => SetValue(CancelTooltipPlacementProperty, value);
    }

    /// <summary>Where the read-only toggletip opens. Default <see cref="CdsPlacement.Top"/>.</summary>
    public CdsPlacement ToggletipPlacement
    {
        get => (CdsPlacement)GetValue(ToggletipPlacementProperty);
        set => SetValue(ToggletipPlacementProperty, value);
    }

    /// <summary>Runs after <see cref="Saved"/>, with the saved value as parameter.</summary>
    public System.Windows.Input.ICommand? SaveCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    /// <summary>Runs after <see cref="Canceled"/>, with the restored value as parameter.</summary>
    public System.Windows.Input.ICommand? CancelCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    static void OnChromeChanged(BindableObject b, object? o, object? n) => ((CdsEditInPlace)b).UpdateChrome();
}
