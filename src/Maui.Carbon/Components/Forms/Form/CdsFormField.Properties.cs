namespace Maui.Carbon;

public abstract partial class CdsFormField
{
    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsFormField), null,
        propertyChanged: static (b, _, n) => ((CdsFormField)b).OnDecoratorChanged((View?)n));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsFormField), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsFormField), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsFormField), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsFormField), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsFormField), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsFormField), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsFormField), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsFormField), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsFormField), CdsSize.Md, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsInline"/>.</summary>
    public static readonly BindableProperty IsInlineProperty = BindableProperty.Create(
        nameof(IsInline), typeof(bool), typeof(CdsFormField), false, propertyChanged: OnChromeChanged);

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

    /// <summary>Read-only: transparent fill and subtle rule; still focusable and readable, not editable.</summary>
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
}
