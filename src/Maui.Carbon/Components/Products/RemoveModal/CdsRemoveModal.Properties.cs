using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsRemoveModal
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="BodyText"/>.</summary>
    public static readonly BindableProperty BodyTextProperty = BindableProperty.Create(
        nameof(BodyText), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsRemoveModal), "Close",
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="ResourceName"/>.</summary>
    public static readonly BindableProperty ResourceNameProperty = BindableProperty.Create(
        nameof(ResourceName), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="RequiresTextConfirmation"/>.</summary>
    public static readonly BindableProperty RequiresTextConfirmationProperty = BindableProperty.Create(
        nameof(RequiresTextConfirmation), typeof(bool), typeof(CdsRemoveModal), false,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="ConfirmationText"/>.</summary>
    public static readonly BindableProperty ConfirmationTextProperty = BindableProperty.Create(
        nameof(ConfirmationText), typeof(string), typeof(CdsRemoveModal), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputLabel"/>.</summary>
    public static readonly BindableProperty InputLabelProperty = BindableProperty.Create(
        nameof(InputLabel), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputPlaceholder"/>.</summary>
    public static readonly BindableProperty InputPlaceholderProperty = BindableProperty.Create(
        nameof(InputPlaceholder), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputInvalidText"/>.</summary>
    public static readonly BindableProperty InputInvalidTextProperty = BindableProperty.Create(
        nameof(InputInvalidText), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryButtonDisabled"/>.</summary>
    public static readonly BindableProperty PrimaryButtonDisabledProperty = BindableProperty.Create(
        nameof(PrimaryButtonDisabled), typeof(bool), typeof(CdsRemoveModal), false,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryDangerDescription"/>.</summary>
    public static readonly BindableProperty PrimaryDangerDescriptionProperty = BindableProperty.Create(
        nameof(PrimaryDangerDescription), typeof(string), typeof(CdsRemoveModal), null,
        propertyChanged: static (b, _, _) => ((CdsRemoveModal)b).Sync());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsRemoveModal), null);

    /// <summary>The heading (<c>title</c>): "Confirm delete" or "Confirm removal". It also names the dialog.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Small <c>label-01</c> text above the title (<c>label</c>), usually the action and the resource: "Delete bx1001".</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The message (<c>body</c> as text): what is removed and whether it can be undone. It wraps at 80% of the body width.</summary>
    public string? BodyText
    {
        get => (string?)GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    /// <summary>Your own body content in place of <see cref="BodyText"/> (<c>body</c> as a node), for example a paragraph with a link.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Accessible name of the close button (<c>iconDescription</c>). Default <c>Close</c>.</summary>
    public string IconDescription
    {
        get => (string)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>The name of the resource being removed (<c>resourceName</c>). With <see cref="RequiresTextConfirmation"/> the person types it exactly to enable the danger button.</summary>
    public string? ResourceName
    {
        get => (string?)GetValue(ResourceNameProperty);
        set => SetValue(ResourceNameProperty, value);
    }

    /// <summary>High-impact removal (<c>textConfirmation</c>): shows a text input and keeps the danger button disabled until it holds <see cref="ResourceName"/> exactly. Focus starts in the input.</summary>
    public bool RequiresTextConfirmation
    {
        get => (bool)GetValue(RequiresTextConfirmationProperty);
        set => SetValue(RequiresTextConfirmationProperty, value);
    }

    /// <summary>What the person typed in the confirmation input (two-way). Cleared each time the modal opens, not when a submit fails.</summary>
    public string? ConfirmationText
    {
        get => (string?)GetValue(ConfirmationTextProperty);
        set => SetValue(ConfirmationTextProperty, value);
    }

    /// <summary>Label of the confirmation input (<c>inputLabelText</c>): "Type bx1001 to confirm".</summary>
    public string? InputLabel
    {
        get => (string?)GetValue(InputLabelProperty);
        set => SetValue(InputLabelProperty, value);
    }

    /// <summary>Placeholder of the confirmation input (<c>inputPlaceholderText</c>).</summary>
    public string? InputPlaceholder
    {
        get => (string?)GetValue(InputPlaceholderProperty);
        set => SetValue(InputPlaceholderProperty, value);
    }

    /// <summary>Shown under the confirmation input (<c>inputInvalidText</c>) once the person left it holding text that does not match <see cref="ResourceName"/>.</summary>
    public string? InputInvalidText
    {
        get => (string?)GetValue(InputInvalidTextProperty);
        set => SetValue(InputInvalidTextProperty, value);
    }

    /// <summary>Label of the danger button (<c>primaryButtonText</c>): the verb, "Delete" or "Remove".</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Label of the secondary button (<c>secondaryButtonText</c>), "Cancel"; it asks to close.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Disables the danger button whatever was typed (<c>primaryButtonDisabled</c>; it overrides the text confirmation).</summary>
    public bool PrimaryButtonDisabled
    {
        get => (bool)GetValue(PrimaryButtonDisabledProperty);
        set => SetValue(PrimaryButtonDisabledProperty, value);
    }

    /// <summary>What a screen reader adds to the danger button (<c>primaryDangerDescription</c>), for example "danger".</summary>
    public string? PrimaryDangerDescription
    {
        get => (string?)GetValue(PrimaryDangerDescriptionProperty);
        set => SetValue(PrimaryDangerDescriptionProperty, value);
    }

    /// <summary>Runs when the danger button is activated (<c>onRequestSubmit</c>), after <see cref="CdsRemoveModal.RequestSubmit"/>.</summary>
    public ICommand? RequestSubmitCommand
    {
        get => (ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }
}
