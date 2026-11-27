namespace Maui.Carbon;

public partial class CdsCreateStep
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsCreateStep), null,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).Sync());

    /// <summary>The step heading, also its label in the progress indicator.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsCreateStep), null,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).Sync());

    /// <summary>A short line under the title.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsCreateStep), null,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).Sync());

    /// <summary>One or two sentences that explain the step.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="SecondaryLabel"/>.</summary>
    public static readonly BindableProperty SecondaryLabelProperty = BindableProperty.Create(
        nameof(SecondaryLabel), typeof(string), typeof(CdsCreateStep), null,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).NotifyFlow());

    /// <summary>Extra text under the step's label in the progress indicator ("Optional").</summary>
    public string? SecondaryLabel
    {
        get => (string?)GetValue(SecondaryLabelProperty);
        set => SetValue(SecondaryLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsIncluded"/>.</summary>
    public static readonly BindableProperty IsIncludedProperty = BindableProperty.Create(
        nameof(IsIncluded), typeof(bool), typeof(CdsCreateStep), true,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).NotifyFlow());

    /// <summary>Whether the flow includes the step (Carbon: <c>includeStep</c>). A left-out step is skipped and not listed.</summary>
    public bool IsIncluded
    {
        get => (bool)GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsIntro"/>.</summary>
    public static readonly BindableProperty IsIntroProperty = BindableProperty.Create(
        nameof(IsIntro), typeof(bool), typeof(CdsCreateStep), false,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).NotifyFlow());

    /// <summary>An introduction step: shown first, but not listed in the progress indicator, which stays hidden while it shows.</summary>
    public bool IsIntro
    {
        get => (bool)GetValue(IsIntroProperty);
        set => SetValue(IsIntroProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsCreateStep), false,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).NotifyFlow());

    /// <summary>Marks the step invalid in the progress indicator.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsSubmitDisabled"/>.</summary>
    public static readonly BindableProperty IsSubmitDisabledProperty = BindableProperty.Create(
        nameof(IsSubmitDisabled), typeof(bool), typeof(CdsCreateStep), false,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).NotifyFlow());

    /// <summary>Disables the Next (or submit) button while the step is current, until its fields are valid (Carbon: <c>disableSubmit</c>).</summary>
    public bool IsSubmitDisabled
    {
        get => (bool)GetValue(IsSubmitDisabledProperty);
        set => SetValue(IsSubmitDisabledProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsBackDisabled"/>.</summary>
    public static readonly BindableProperty IsBackDisabledProperty = BindableProperty.Create(
        nameof(IsBackDisabled), typeof(bool), typeof(CdsCreateStep), false,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).NotifyFlow());

    /// <summary>Disables the Back button while the step is current (Carbon: <c>secondaryButtonDisabled</c>).</summary>
    public bool IsBackDisabled
    {
        get => (bool)GetValue(IsBackDisabledProperty);
        set => SetValue(IsBackDisabledProperty, value);
    }

    /// <summary>Bindable property for <see cref="HasFieldset"/>.</summary>
    public static readonly BindableProperty HasFieldsetProperty = BindableProperty.Create(
        nameof(HasFieldset), typeof(bool), typeof(CdsCreateStep), true,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).Sync());

    /// <summary>Wraps the content in a fieldset named by <see cref="FieldsetLegend"/>.</summary>
    public bool HasFieldset
    {
        get => (bool)GetValue(HasFieldsetProperty);
        set => SetValue(HasFieldsetProperty, value);
    }

    /// <summary>Bindable property for <see cref="FieldsetLegend"/>.</summary>
    public static readonly BindableProperty FieldsetLegendProperty = BindableProperty.Create(
        nameof(FieldsetLegend), typeof(string), typeof(CdsCreateStep), null,
        propertyChanged: static (b, _, _) => ((CdsCreateStep)b).Sync());

    /// <summary>The legend of the fieldset around the content (required by Carbon when <see cref="HasFieldset"/> is true).</summary>
    public string? FieldsetLegend
    {
        get => (string?)GetValue(FieldsetLegendProperty);
        set => SetValue(FieldsetLegendProperty, value);
    }

    /// <summary>Bindable property for <see cref="NextAsync"/>.</summary>
    public static readonly BindableProperty NextAsyncProperty = BindableProperty.Create(
        nameof(NextAsync), typeof(Func<Task<bool>>), typeof(CdsCreateStep), null);

    /// <summary>Runs when the person moves on from this step (Carbon: <c>onNext</c>): validate or save here and return false (or throw) to stay. The button is disabled while it runs.</summary>
    public Func<Task<bool>>? NextAsync
    {
        get => (Func<Task<bool>>?)GetValue(NextAsyncProperty);
        set => SetValue(NextAsyncProperty, value);
    }

    /// <summary>Bindable property for <see cref="PreviousAsync"/>.</summary>
    public static readonly BindableProperty PreviousAsyncProperty = BindableProperty.Create(
        nameof(PreviousAsync), typeof(Func<Task<bool>>), typeof(CdsCreateStep), null);

    /// <summary>Runs when the person goes back from this step (Carbon: <c>onPrevious</c>); return false to stay.</summary>
    public Func<Task<bool>>? PreviousAsync
    {
        get => (Func<Task<bool>>?)GetValue(PreviousAsyncProperty);
        set => SetValue(PreviousAsyncProperty, value);
    }
}
