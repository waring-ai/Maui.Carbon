using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsSaving
{
    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsSavingType), typeof(CdsSaving), CdsSavingType.Auto, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Status"/>.</summary>
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(CdsSavingStatus), typeof(CdsSaving), CdsSavingStatus.Default,
        propertyChanged: static (b, _, _) => ((CdsSaving)b).OnStatusChanged());

    /// <summary>Bindable property for <see cref="DefaultText"/>.</summary>
    public static readonly BindableProperty DefaultTextProperty = BindableProperty.Create(
        nameof(DefaultText), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="InProgressText"/>.</summary>
    public static readonly BindableProperty InProgressTextProperty = BindableProperty.Create(
        nameof(InProgressText), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="SuccessText"/>.</summary>
    public static readonly BindableProperty SuccessTextProperty = BindableProperty.Create(
        nameof(SuccessText), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="FailText"/>.</summary>
    public static readonly BindableProperty FailTextProperty = BindableProperty.Create(
        nameof(FailText), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="DefaultIconDescription"/>.</summary>
    public static readonly BindableProperty DefaultIconDescriptionProperty = BindableProperty.Create(
        nameof(DefaultIconDescription), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="InProgressIconDescription"/>.</summary>
    public static readonly BindableProperty InProgressIconDescriptionProperty = BindableProperty.Create(
        nameof(InProgressIconDescription), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="SuccessIconDescription"/>.</summary>
    public static readonly BindableProperty SuccessIconDescriptionProperty = BindableProperty.Create(
        nameof(SuccessIconDescription), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="FailIconDescription"/>.</summary>
    public static readonly BindableProperty FailIconDescriptionProperty = BindableProperty.Create(
        nameof(FailIconDescription), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="SaveCommand"/>.</summary>
    public static readonly BindableProperty SaveCommandProperty = BindableProperty.Create(
        nameof(SaveCommand), typeof(ICommand), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="SaveCommandParameter"/>.</summary>
    public static readonly BindableProperty SaveCommandParameterProperty = BindableProperty.Create(
        nameof(SaveCommandParameter), typeof(object), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="CancelCommand"/>.</summary>
    public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(
        nameof(CancelCommand), typeof(ICommand), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Bindable property for <see cref="CancelCommandParameter"/>.</summary>
    public static readonly BindableProperty CancelCommandParameterProperty = BindableProperty.Create(
        nameof(CancelCommandParameter), typeof(object), typeof(CdsSaving), null, propertyChanged: OnContentChanged);

    /// <summary>Auto (a status line, default) or manual (cancel and save buttons). See the usage guidelines.</summary>
    public CdsSavingType Type
    {
        get => (CdsSavingType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// The save status: <see cref="CdsSavingStatus.Default"/> (untouched), then <see cref="CdsSavingStatus.InProgress"/>
    /// when a save starts, then <see cref="CdsSavingStatus.Success"/> or <see cref="CdsSavingStatus.Fail"/>. Changes are
    /// announced while the control is shown (a failure assertively).
    /// </summary>
    public CdsSavingStatus Status
    {
        get => (CdsSavingStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>The save button text before anything is saved ("Save", manual). Leave it unset in auto mode: the guidelines show no line until a save starts.</summary>
    public string? DefaultText
    {
        get => (string?)GetValue(DefaultTextProperty);
        set => SetValue(DefaultTextProperty, value);
    }

    /// <summary>The text while a save is in progress ("Saving...").</summary>
    public string? InProgressText
    {
        get => (string?)GetValue(InProgressTextProperty);
        set => SetValue(InProgressTextProperty, value);
    }

    /// <summary>The text after a successful save ("Saved").</summary>
    public string? SuccessText
    {
        get => (string?)GetValue(SuccessTextProperty);
        set => SetValue(SuccessTextProperty, value);
    }

    /// <summary>The text after a failed save ("Failed to save"; "Failed to save. Try again?" on the manual button).</summary>
    public string? FailText
    {
        get => (string?)GetValue(FailTextProperty);
        set => SetValue(FailTextProperty, value);
    }

    /// <summary>Description of the default state's <c>save</c> icon (manual).</summary>
    public string? DefaultIconDescription
    {
        get => (string?)GetValue(DefaultIconDescriptionProperty);
        set => SetValue(DefaultIconDescriptionProperty, value);
    }

    /// <summary>Description of the in-progress icon (manual).</summary>
    public string? InProgressIconDescription
    {
        get => (string?)GetValue(InProgressIconDescriptionProperty);
        set => SetValue(InProgressIconDescriptionProperty, value);
    }

    /// <summary>Description of the success icon (manual).</summary>
    public string? SuccessIconDescription
    {
        get => (string?)GetValue(SuccessIconDescriptionProperty);
        set => SetValue(SuccessIconDescriptionProperty, value);
    }

    /// <summary>Description of the failure icon (manual).</summary>
    public string? FailIconDescription
    {
        get => (string?)GetValue(FailIconDescriptionProperty);
        set => SetValue(FailIconDescriptionProperty, value);
    }

    /// <summary>The cancel button text ("Cancel", manual). The button is enabled only while a save is in progress.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Command run by the save button (manual), after <see cref="RequestSave"/>.</summary>
    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="SaveCommand"/>.</summary>
    public object? SaveCommandParameter
    {
        get => GetValue(SaveCommandParameterProperty);
        set => SetValue(SaveCommandParameterProperty, value);
    }

    /// <summary>Command run by the cancel button (manual), after <see cref="RequestCancel"/>.</summary>
    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="CancelCommand"/>.</summary>
    public object? CancelCommandParameter
    {
        get => GetValue(CancelCommandParameterProperty);
        set => SetValue(CancelCommandParameterProperty, value);
    }
}
