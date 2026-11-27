namespace Maui.Carbon;

public partial class CdsTextInput
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTextInput), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTextInput)b).OnTextPropertyChanged((string?)n));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTextInput), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsTextInput), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsTextInput), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsTextInput), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsTextInput), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsTextInput), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsTextInput), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsTextInput), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsTextInput), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsTextInput), CdsSize.Md, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsInline"/>.</summary>
    public static readonly BindableProperty IsInlineProperty = BindableProperty.Create(
        nameof(IsInline), typeof(bool), typeof(CdsTextInput), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="EnableCounter"/>.</summary>
    public static readonly BindableProperty EnableCounterProperty = BindableProperty.Create(
        nameof(EnableCounter), typeof(bool), typeof(CdsTextInput), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="MaxCount"/>.</summary>
    public static readonly BindableProperty MaxCountProperty = BindableProperty.Create(
        nameof(MaxCount), typeof(int), typeof(CdsTextInput), 0, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Keyboard"/>.</summary>
    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
        nameof(Keyboard), typeof(Keyboard), typeof(CdsTextInput), Keyboard.Default,
        propertyChanged: static (b, _, n) => ((CdsTextInput)b).Entry.Keyboard = (Keyboard)n);

    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsTextInput), null,
        propertyChanged: static (b, _, n) => ((CdsTextInput)b).OnDecoratorChanged((View?)n));

    /// <summary>The value (two-way).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The visible label (always provide one). It is also the input's accessible name.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Hides the label visually; it stays the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>A format hint ("e.g. prod-eu-01"); never the label or required information.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Format or consequence, under the field; replaced by the invalid or warning message.</summary>
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error state: 2px <c>support-error</c> outline, error icon and <see cref="InvalidText"/>.</summary>
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

    /// <summary>Warning state: warning icon and <see cref="WarnText"/>. Ignored when invalid.</summary>
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

    /// <summary>Read-only: transparent fill and subtle rule; still focusable, selectable and copyable.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Field height: xs 24, sm 32, md 40 (default), lg 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Label to the start of the field, for dense settings rows.</summary>
    public bool IsInline
    {
        get => (bool)GetValue(IsInlineProperty);
        set => SetValue(IsInlineProperty, value);
    }

    /// <summary>Shows "n/max" at the end of the label row and stops input at <see cref="MaxCount"/>.</summary>
    public bool EnableCounter
    {
        get => (bool)GetValue(EnableCounterProperty);
        set => SetValue(EnableCounterProperty, value);
    }

    /// <summary>Maximum length when <see cref="EnableCounter"/> is on.</summary>
    public int MaxCount
    {
        get => (int)GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>The soft keyboard (email, URL, numeric...).</summary>
    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    /// <summary>
    /// A view at the end of the field (Carbon's <c>decorator</c>), usually a <see cref="CdsAILabel"/> marking an AI-generated value. The
    /// label is shown at its mini size and the field takes Carbon's AI presence (the AI aura and an <c>ai-border-strong</c> rule)
    /// until the label's <see cref="CdsAILabel.IsRevertActive"/> is set, which you do when the person edits the value.
    /// </summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    void OnDecoratorChanged(View? decorator)
    {
        Chrome.Decorator = decorator;
        RefreshVisuals();
    }

    static void OnChromeChanged(BindableObject b, object? o, object? n) => ((CdsTextInput)b).UpdateChrome();
}
