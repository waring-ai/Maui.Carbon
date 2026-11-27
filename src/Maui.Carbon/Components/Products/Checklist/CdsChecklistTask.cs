using System.Collections.Specialized;
using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>The status of a checklist task; each has its own glyph, so the status never rests on color alone.</summary>
public enum CdsChecklistTaskKind
{
    /// <summary>Not started: <c>CircleDash</c> in <c>icon-primary</c>.</summary>
    Unchecked,
    /// <summary>In progress: <c>Incomplete</c> in <c>interactive</c>.</summary>
    Indeterminate,
    /// <summary>Complete: <c>CheckmarkOutline</c> in <c>interactive</c>.</summary>
    Checked,
    /// <summary>Not available: <c>CircleDash</c> in <c>icon-disabled</c> with <c>text-disabled</c> text.</summary>
    Disabled,
    /// <summary>Failed: <c>Warning</c> in <c>support-error</c>.</summary>
    Error,
}

/// <summary>
/// One task of a <see cref="CdsChecklist"/>: a status glyph and a label. Handling <see cref="Clicked"/> or setting
/// <see cref="Command"/> renders the label as a link-styled button (Carbon's <c>onClick</c>); otherwise it is text.
/// Labels wrap to two lines, then end with an ellipsis (the full label is the tooltip).
/// </summary>
public class CdsChecklistTask : BindableObject
{
    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsChecklistTaskKind), typeof(CdsChecklistTask), CdsChecklistTaskKind.Unchecked);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsChecklistTask), null);

    /// <summary>Bindable property for <see cref="StatusDescription"/>.</summary>
    public static readonly BindableProperty StatusDescriptionProperty = BindableProperty.Create(
        nameof(StatusDescription), typeof(string), typeof(CdsChecklistTask), null);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsChecklistTask), null);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsChecklistTask), null);

    EventHandler? _clicked;

    /// <summary>Creates an unchecked task.</summary>
    public CdsChecklistTask()
    {
    }

    /// <summary>Creates a task with a label and status.</summary>
    public CdsChecklistTask(string label, CdsChecklistTaskKind kind = CdsChecklistTaskKind.Unchecked)
    {
        Label = label;
        Kind = kind;
    }

    /// <summary>Raised when the task's link is activated, before <see cref="Command"/> runs. Handling it makes the label a link.</summary>
    public event EventHandler? Clicked
    {
        add
        {
            _clicked += value;
            OnPropertyChanged(nameof(IsClickable));
        }
        remove
        {
            _clicked -= value;
            OnPropertyChanged(nameof(IsClickable));
        }
    }

    /// <summary>The status glyph. Default <see cref="CdsChecklistTaskKind.Unchecked"/>.</summary>
    public CdsChecklistTaskKind Kind
    {
        get => (CdsChecklistTaskKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The task name, sentence case ("Connect a data source"). Required.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// What screen readers say for the status after the label; defaults to "Not started", "In progress", "Complete",
    /// "Not available" or "Error" (<see cref="CdsChecklistTokens.StatusText"/>). Translate it here.
    /// </summary>
    public string? StatusDescription
    {
        get => (string?)GetValue(StatusDescriptionProperty);
        set => SetValue(StatusDescriptionProperty, value);
    }

    /// <summary>Executed when the task's link is activated; setting it makes the label a link.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Whether the label is a link: <see cref="Clicked"/> is handled or <see cref="Command"/> is set.</summary>
    public bool IsClickable => _clicked is not null || Command is not null;

    /// <summary>Raises <see cref="Clicked"/> and runs the command.</summary>
    internal void Activate()
    {
        _clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
    }
}

/// <summary>A titled group of tasks in a <see cref="CdsChecklist"/> (Carbon for IBM Products' task sub-list).</summary>
[ContentProperty(nameof(Tasks))]
public class CdsChecklistTaskList : BindableObject
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsChecklistTaskList), null);

    readonly CdsItemCollection<CdsChecklistTask> _tasks = [];

    /// <summary>Creates an empty list.</summary>
    public CdsChecklistTaskList() => _tasks.CollectionChanged += OnTasksChanged;

    /// <summary>Creates a list with a title.</summary>
    public CdsChecklistTaskList(string? title) : this() => Title = title;

    /// <summary>An optional section label above the tasks (<c>body-compact-01</c>, two lines at most).</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The tasks, in order.</summary>
    public IList<CdsChecklistTask> Tasks => _tasks;

    /// <summary>Raised when a task is added, removed or changed.</summary>
    internal event EventHandler? Changed;

    void OnTasksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsChecklistTask task in e.OldItems) task.PropertyChanged -= OnTaskChanged;
        if (e.NewItems is not null)
            foreach (CdsChecklistTask task in e.NewItems) task.PropertyChanged += OnTaskChanged;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    void OnTaskChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => Changed?.Invoke(this, EventArgs.Empty);
}
