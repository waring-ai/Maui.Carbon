using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsCreateModal
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsCreateModal), null,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsCreateModal), null,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsCreateModal), null,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsCreateModal), null,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsCreateModal), null,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsCreateModal), null,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryButtonDisabled"/>.</summary>
    public static readonly BindableProperty PrimaryButtonDisabledProperty = BindableProperty.Create(
        nameof(PrimaryButtonDisabled), typeof(bool), typeof(CdsCreateModal), false,
        propertyChanged: static (b, _, _) => ((CdsCreateModal)b).Sync());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsCreateModal), null);

    /// <summary>The heading (<c>title</c>), usually the product or service name or the thing created: "Create partition". It also names the dialog.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional <c>body-compact-01</c> line in <c>text-secondary</c> under the title (<c>subtitle</c>).</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>The paragraph above the form (<c>description</c>): what is created and what happens next.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The form (<c>children</c>): a few fields, 16 apart (a <see cref="VerticalStackLayout"/> with <c>Spacing="16"</c>). Keep it short; use a tearsheet for long forms.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Label of the primary button (<c>primaryButtonText</c>): "Create".</summary>
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

    /// <summary>Disables the primary button until the required fields are valid (<c>disableSubmit</c>).</summary>
    public bool PrimaryButtonDisabled
    {
        get => (bool)GetValue(PrimaryButtonDisabledProperty);
        set => SetValue(PrimaryButtonDisabledProperty, value);
    }

    /// <summary>Runs when the primary button is activated (<c>onRequestSubmit</c>), after <see cref="CdsCreateModal.RequestSubmit"/>.</summary>
    public ICommand? RequestSubmitCommand
    {
        get => (ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }
}
