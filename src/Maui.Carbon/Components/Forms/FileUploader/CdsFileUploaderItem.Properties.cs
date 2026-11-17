using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsFileUploaderItem
{
    /// <summary>Bindable property for <see cref="Name"/>.</summary>
    public static readonly BindableProperty NameProperty = BindableProperty.Create(
        nameof(Name), typeof(string), typeof(CdsFileUploaderItem), null, propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="Status"/>.</summary>
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(CdsFileStatus), typeof(CdsFileUploaderItem), CdsFileStatus.Edit, propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsFileUploaderItem), false, propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="ErrorSubject"/>.</summary>
    public static readonly BindableProperty ErrorSubjectProperty = BindableProperty.Create(
        nameof(ErrorSubject), typeof(string), typeof(CdsFileUploaderItem), null, propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="ErrorBody"/>.</summary>
    public static readonly BindableProperty ErrorBodyProperty = BindableProperty.Create(
        nameof(ErrorBody), typeof(string), typeof(CdsFileUploaderItem), null, propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsFileUploaderItem), CdsSize.Lg, propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsFileUploaderItem), "Remove file", propertyChanged: OnRowChanged);

    /// <summary>Bindable property for <see cref="DeleteCommand"/>.</summary>
    public static readonly BindableProperty DeleteCommandProperty = BindableProperty.Create(
        nameof(DeleteCommand), typeof(ICommand), typeof(CdsFileUploaderItem), null);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsFileUploaderItem), null);

    /// <summary>The file name (truncated with an ellipsis when long).</summary>
    public string? Name
    {
        get => (string?)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    /// <summary>The status icon: spinner, remove button (default) or check.</summary>
    public CdsFileStatus Status
    {
        get => (CdsFileStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Draws the 2px error outline and message. Always set <see cref="ErrorSubject"/> with it.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>The error title: what went wrong.</summary>
    public string? ErrorSubject
    {
        get => (string?)GetValue(ErrorSubjectProperty);
        set => SetValue(ErrorSubjectProperty, value);
    }

    /// <summary>Supporting text: how to fix it.</summary>
    public string? ErrorBody
    {
        get => (string?)GetValue(ErrorBodyProperty);
        set => SetValue(ErrorBodyProperty, value);
    }

    /// <summary>Row height: sm 32, md 40, lg 48 (default).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Accessible name of the remove button, followed by " - " and the file name. Default "Remove file".</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>Command run when the remove button is activated.</summary>
    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    /// <summary>Parameter of <see cref="DeleteCommand"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    static void OnRowChanged(BindableObject b, object? o, object? n) => ((CdsFileUploaderItem)b).UpdateRow();
}
