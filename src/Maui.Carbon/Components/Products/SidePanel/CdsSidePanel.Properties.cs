namespace Maui.Carbon;

public partial class CdsSidePanel
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsSidePanel), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).OnIsOpenChanged());

    /// <summary>Shows the panel while true. Two-way: the close button, Escape and a tap on the overlay set it back to false (unless a <see cref="RequestClose"/> handler cancels).</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsSidePanel), null,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>The heading, a short noun or verb phrase ("Edit cluster"). Also names the panel for screen readers. Required when <see cref="Label"/> is set.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSidePanel), null,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>Small text above the title (the object or step the panel is about). Shown only with a <see cref="Title"/>.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsSidePanel), null,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>One or two sentences under the title.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSidePanelSize), typeof(CdsSidePanel), CdsSidePanelSize.Md,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Reshape());

    /// <summary>The width: xs 256, sm 320, md 480 (default), lg 640, xl 1040, 2xl 1280; never more than 75% of a viewport from 672 wide.</summary>
    public CdsSidePanelSize Size
    {
        get => (CdsSidePanelSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Bindable property for <see cref="Placement"/>.</summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(CdsSidePanelPlacement), typeof(CdsSidePanel), CdsSidePanelPlacement.End,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Reshow());

    /// <summary>The edge the panel is attached to. Default the end edge.</summary>
    public CdsSidePanelPlacement Placement
    {
        get => (CdsSidePanelPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Bindable property for <see cref="IncludeOverlay"/>.</summary>
    public static readonly BindableProperty IncludeOverlayProperty = BindableProperty.Create(
        nameof(IncludeOverlay), typeof(bool), typeof(CdsSidePanel), false,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Reshow());

    /// <summary>Dims and blocks the page behind the panel with the overlay scrim; a tap on it asks to close. Without it the page stays usable and the panel casts a small shadow.</summary>
    public bool IncludeOverlay
    {
        get => (bool)GetValue(IncludeOverlayProperty);
        set => SetValue(IncludeOverlayProperty, value);
    }

    /// <summary>Bindable property for <see cref="PreventCloseOnClickOutside"/>.</summary>
    public static readonly BindableProperty PreventCloseOnClickOutsideProperty = BindableProperty.Create(
        nameof(PreventCloseOnClickOutside), typeof(bool), typeof(CdsSidePanel), false);

    /// <summary>A tap on the overlay does not close the panel (with <see cref="IncludeOverlay"/>).</summary>
    public bool PreventCloseOnClickOutside
    {
        get => (bool)GetValue(PreventCloseOnClickOutsideProperty);
        set => SetValue(PreventCloseOnClickOutsideProperty, value);
    }

    /// <summary>Bindable property for <see cref="SlideIn"/>.</summary>
    public static readonly BindableProperty SlideInProperty = BindableProperty.Create(
        nameof(SlideIn), typeof(bool), typeof(CdsSidePanel), false,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Reshow());

    /// <summary>The panel pushes <see cref="PageContent"/> aside instead of covering it. A slide-in panel takes no focus when it opens and does not close on Escape; use it for panels people keep open while they work.</summary>
    public bool SlideIn
    {
        get => (bool)GetValue(SlideInProperty);
        set => SetValue(SlideInProperty, value);
    }

    /// <summary>Bindable property for <see cref="PageContent"/>.</summary>
    public static readonly BindableProperty PageContentProperty = BindableProperty.Create(
        nameof(PageContent), typeof(View), typeof(CdsSidePanel), null);

    /// <summary>The page content a <see cref="SlideIn"/> panel narrows (Carbon: <c>selectorPageContent</c>). Without it a slide-in panel covers the page.</summary>
    public View? PageContent
    {
        get => (View?)GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    /// <summary>Bindable property for <see cref="HideCloseButton"/>.</summary>
    public static readonly BindableProperty HideCloseButtonProperty = BindableProperty.Create(
        nameof(HideCloseButton), typeof(bool), typeof(CdsSidePanel), false,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>Hides the close button. Keep another way to close: an action, Escape or the overlay.</summary>
    public bool HideCloseButton
    {
        get => (bool)GetValue(HideCloseButtonProperty);
        set => SetValue(HideCloseButtonProperty, value);
    }

    /// <summary>Bindable property for <see cref="CloseButtonLabel"/>.</summary>
    public static readonly BindableProperty CloseButtonLabelProperty = BindableProperty.Create(
        nameof(CloseButtonLabel), typeof(string), typeof(CdsSidePanel), "Close",
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>The accessible name and tooltip of the close button.</summary>
    public string CloseButtonLabel
    {
        get => (string)GetValue(CloseButtonLabelProperty);
        set => SetValue(CloseButtonLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="BackButtonLabel"/>.</summary>
    public static readonly BindableProperty BackButtonLabelProperty = BindableProperty.Create(
        nameof(BackButtonLabel), typeof(string), typeof(CdsSidePanel), "Back",
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>The accessible name and tooltip of the back button shown on later steps.</summary>
    public string BackButtonLabel
    {
        get => (string)GetValue(BackButtonLabelProperty);
        set => SetValue(BackButtonLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="CurrentStep"/>.</summary>
    public static readonly BindableProperty CurrentStepProperty = BindableProperty.Create(
        nameof(CurrentStep), typeof(int), typeof(CdsSidePanel), 0,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).OnStepChanged());

    /// <summary>The step of a multi-step panel. From step 1 a back button raises <see cref="NavigationBack"/>; changing the step scrolls the body to the top.</summary>
    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    /// <summary>Bindable property for <see cref="CondensedActions"/>.</summary>
    public static readonly BindableProperty CondensedActionsProperty = BindableProperty.Create(
        nameof(CondensedActions), typeof(bool), typeof(CdsSidePanel), false,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Reshape());

    /// <summary>Uses 48-high action buttons instead of 64.</summary>
    public bool CondensedActions
    {
        get => (bool)GetValue(CondensedActionsProperty);
        set => SetValue(CondensedActionsProperty, value);
    }

    /// <summary>Bindable property for <see cref="AnimateTitle"/>.</summary>
    public static readonly BindableProperty AnimateTitleProperty = BindableProperty.Create(
        nameof(AnimateTitle), typeof(bool), typeof(CdsSidePanel), true,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Sync());

    /// <summary>Collapses the label, subtitle and title into a compact title as the body scrolls. Off, the header is fixed with a divider under it.</summary>
    public bool AnimateTitle
    {
        get => (bool)GetValue(AnimateTitleProperty);
        set => SetValue(AnimateTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="InitialFocusElement"/>.</summary>
    public static readonly BindableProperty InitialFocusElementProperty = BindableProperty.Create(
        nameof(InitialFocusElement), typeof(VisualElement), typeof(CdsSidePanel), null);

    /// <summary>The element focused when the panel opens (Carbon: <c>selectorPrimaryFocus</c>). Default: the first control of the panel, usually its back or close button.</summary>
    public VisualElement? InitialFocusElement
    {
        get => (VisualElement?)GetValue(InitialFocusElementProperty);
        set => SetValue(InitialFocusElementProperty, value);
    }

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsSidePanel), null);

    /// <summary>The element focus returns to when the panel closes (Carbon: <c>launcherButtonRef</c>). Default: the control focused before the panel opened.</summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsSidePanel), null,
        propertyChanged: static (b, _, _) => ((CdsSidePanel)b).Reshape());

    /// <summary>An AI label (<see cref="CdsAILabel"/>, extra small) shown before the close button when the content is generated by AI.</summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(System.Windows.Input.ICommand), typeof(CdsSidePanel), null);

    /// <summary>Runs when the panel closes from its chrome, before <see cref="IsOpen"/> becomes false.</summary>
    public System.Windows.Input.ICommand? RequestCloseCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }

    /// <summary>Bindable property for <see cref="NavigationBackCommand"/>.</summary>
    public static readonly BindableProperty NavigationBackCommandProperty = BindableProperty.Create(
        nameof(NavigationBackCommand), typeof(System.Windows.Input.ICommand), typeof(CdsSidePanel), null);

    /// <summary>Runs when the back button is activated, after <see cref="NavigationBack"/>.</summary>
    public System.Windows.Input.ICommand? NavigationBackCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(NavigationBackCommandProperty);
        set => SetValue(NavigationBackCommandProperty, value);
    }
}
