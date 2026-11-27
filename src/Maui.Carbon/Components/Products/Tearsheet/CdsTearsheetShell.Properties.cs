namespace Maui.Carbon;

public abstract partial class CdsTearsheetShell
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsTearsheetShell), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).OnIsOpenChanged());

    /// <summary>Shows the tearsheet while true. Two-way: the close button, Escape and (on a passive tearsheet) a tap outside set it back to false, unless a <see cref="RequestClose"/> handler cancels.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsTearsheetShell), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).Sync());

    /// <summary>The heading: what the task produces or acts on ("Create pipeline"). Also names the tearsheet for screen readers.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTearsheetShell), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).Sync());

    /// <summary>Small text above the title: the product area or the object the task belongs to.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsTearsheetShell), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).Sync());

    /// <summary>One or two sentences under the title that set up the task.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="HasCloseIcon"/>.</summary>
    public static readonly BindableProperty HasCloseIconProperty = BindableProperty.Create(
        nameof(HasCloseIcon), typeof(bool?), typeof(CdsTearsheetShell), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).Sync());

    /// <summary>Shows the close button. Default: only on a passive tearsheet (one without actions); a tearsheet with actions closes through them.</summary>
    public bool? HasCloseIcon
    {
        get => (bool?)GetValue(HasCloseIconProperty);
        set => SetValue(HasCloseIconProperty, value);
    }

    /// <summary>Bindable property for <see cref="CloseButtonLabel"/>.</summary>
    public static readonly BindableProperty CloseButtonLabelProperty = BindableProperty.Create(
        nameof(CloseButtonLabel), typeof(string), typeof(CdsTearsheetShell), "Close",
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).Sync());

    /// <summary>The accessible name and tooltip of the close button.</summary>
    public string CloseButtonLabel
    {
        get => (string)GetValue(CloseButtonLabelProperty);
        set => SetValue(CloseButtonLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="InitialFocusElement"/>.</summary>
    public static readonly BindableProperty InitialFocusElementProperty = BindableProperty.Create(
        nameof(InitialFocusElement), typeof(VisualElement), typeof(CdsTearsheetShell), null);

    /// <summary>The element focused when the tearsheet opens (Carbon: <c>selectorPrimaryFocus</c>). Default: its first control.</summary>
    public VisualElement? InitialFocusElement
    {
        get => (VisualElement?)GetValue(InitialFocusElementProperty);
        set => SetValue(InitialFocusElementProperty, value);
    }

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsTearsheetShell), null);

    /// <summary>The element focus returns to when the tearsheet closes (Carbon: <c>launcherButtonRef</c>). Default: the control focused before it opened.</summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsTearsheetShell), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheetShell)b).Sync());

    /// <summary>An AI label (<see cref="CdsAILabel"/>, small) in the header when the content is generated by AI.</summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(System.Windows.Input.ICommand), typeof(CdsTearsheetShell), null);

    /// <summary>Runs when the tearsheet closes from its chrome, before <see cref="IsOpen"/> becomes false.</summary>
    public System.Windows.Input.ICommand? RequestCloseCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }
}
