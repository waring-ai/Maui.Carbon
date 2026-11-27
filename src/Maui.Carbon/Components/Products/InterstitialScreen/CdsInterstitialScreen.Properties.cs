namespace Maui.Carbon;

public partial class CdsInterstitialScreen
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsInterstitialScreen), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsInterstitialScreen)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="IsFullScreen"/>.</summary>
    public static readonly BindableProperty IsFullScreenProperty = BindableProperty.Create(
        nameof(IsFullScreen), typeof(bool), typeof(CdsInterstitialScreen), false,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).Reshow());

    /// <summary>Bindable property for <see cref="IsInline"/>.</summary>
    public static readonly BindableProperty IsInlineProperty = BindableProperty.Create(
        nameof(IsInline), typeof(bool), typeof(CdsInterstitialScreen), false,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).Reshow());

    /// <summary>Bindable property for <see cref="CurrentStep"/>.</summary>
    public static readonly BindableProperty CurrentStepProperty = BindableProperty.Create(
        nameof(CurrentStep), typeof(int), typeof(CdsInterstitialScreen), 0, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsInterstitialScreen)b).OnCurrentStepChanged((int)o, (int)n));

    /// <summary>Bindable property for <see cref="HeaderTitle"/>.</summary>
    public static readonly BindableProperty HeaderTitleProperty = BindableProperty.Create(
        nameof(HeaderTitle), typeof(string), typeof(CdsInterstitialScreen), null,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="HeaderSubtitle"/>.</summary>
    public static readonly BindableProperty HeaderSubtitleProperty = BindableProperty.Create(
        nameof(HeaderSubtitle), typeof(string), typeof(CdsInterstitialScreen), null,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="HeaderContent"/>.</summary>
    public static readonly BindableProperty HeaderContentProperty = BindableProperty.Create(
        nameof(HeaderContent), typeof(View), typeof(CdsInterstitialScreen), null,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="HideProgressIndicator"/>.</summary>
    public static readonly BindableProperty HideProgressIndicatorProperty = BindableProperty.Create(
        nameof(HideProgressIndicator), typeof(bool), typeof(CdsInterstitialScreen), false,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="CloseIconDescription"/>.</summary>
    public static readonly BindableProperty CloseIconDescriptionProperty = BindableProperty.Create(
        nameof(CloseIconDescription), typeof(string), typeof(CdsInterstitialScreen), "Close",
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="SkipButtonLabel"/>.</summary>
    public static readonly BindableProperty SkipButtonLabelProperty = BindableProperty.Create(
        nameof(SkipButtonLabel), typeof(string), typeof(CdsInterstitialScreen), "Skip",
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="PreviousButtonLabel"/>.</summary>
    public static readonly BindableProperty PreviousButtonLabelProperty = BindableProperty.Create(
        nameof(PreviousButtonLabel), typeof(string), typeof(CdsInterstitialScreen), "Back",
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="NextButtonLabel"/>.</summary>
    public static readonly BindableProperty NextButtonLabelProperty = BindableProperty.Create(
        nameof(NextButtonLabel), typeof(string), typeof(CdsInterstitialScreen), "Next",
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="StartButtonLabel"/>.</summary>
    public static readonly BindableProperty StartButtonLabelProperty = BindableProperty.Create(
        nameof(StartButtonLabel), typeof(string), typeof(CdsInterstitialScreen), "Get started",
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="FooterContent"/>.</summary>
    public static readonly BindableProperty FooterContentProperty = BindableProperty.Create(
        nameof(FooterContent), typeof(View), typeof(CdsInterstitialScreen), null,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="DisabledButtons"/>.</summary>
    public static readonly BindableProperty DisabledButtonsProperty = BindableProperty.Create(
        nameof(DisabledButtons), typeof(CdsInterstitialScreenButtons), typeof(CdsInterstitialScreen), CdsInterstitialScreenButtons.None,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreen)b).OnPropertyAffectingScreen());

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsInterstitialScreen), null);

    /// <summary>Bindable property for <see cref="ActionHandler"/>.</summary>
    public static readonly BindableProperty ActionHandlerProperty = BindableProperty.Create(
        nameof(ActionHandler), typeof(Func<CdsInterstitialScreenActionEventArgs, Task>), typeof(CdsInterstitialScreen), null);

    /// <summary>Bindable property for <see cref="CloseCommand"/>.</summary>
    public static readonly BindableProperty CloseCommandProperty = BindableProperty.Create(
        nameof(CloseCommand), typeof(System.Windows.Input.ICommand), typeof(CdsInterstitialScreen), null);

    /// <summary>Open state (two-way). Opening it from a user action or on the first visit; closing it returns to the first step.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Cover the whole page (Carbon's <c>isFullScreen</c>) instead of the large modal over a scrim. The full-screen variant has no close button: the footer and Escape close it.</summary>
    public bool IsFullScreen
    {
        get => (bool)GetValue(IsFullScreenProperty);
        set => SetValue(IsFullScreenProperty, value);
    }

    /// <summary>Draw the screen where it is declared (filling its slot) instead of in the page's overlay host: a first-run pane inside your own frame, or a preview. Escape still closes it while it has focus.</summary>
    public bool IsInline
    {
        get => (bool)GetValue(IsInlineProperty);
        set => SetValue(IsInlineProperty, value);
    }

    /// <summary>The zero-based step shown (two-way; Carbon's <c>progStep</c> and <c>handleGotoStep</c>). Kept inside the steps; back and next move it, closing resets it to 0.</summary>
    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    /// <summary>The header title (<c>productive-heading-04</c>): what the new experience is.</summary>
    public string? HeaderTitle
    {
        get => (string?)GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    /// <summary>The header subtitle (<c>productive-heading-03</c>) under the title.</summary>
    public string? HeaderSubtitle
    {
        get => (string?)GetValue(HeaderSubtitleProperty);
        set => SetValue(HeaderSubtitleProperty, value);
    }

    /// <summary>Your own header content, after the titles and before the progress indicator.</summary>
    public View? HeaderContent
    {
        get => (View?)GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    /// <summary>Hide the progress indicator a multi-step screen shows in its header.</summary>
    public bool HideProgressIndicator
    {
        get => (bool)GetValue(HideProgressIndicatorProperty);
        set => SetValue(HideProgressIndicatorProperty, value);
    }

    /// <summary>Tooltip and accessible name of the modal variant's close button.</summary>
    public string CloseIconDescription
    {
        get => (string)GetValue(CloseIconDescriptionProperty);
        set => SetValue(CloseIconDescriptionProperty, value);
    }

    /// <summary>The skip button label (multi-step only). Empty hides the button.</summary>
    public string SkipButtonLabel
    {
        get => (string)GetValue(SkipButtonLabelProperty);
        set => SetValue(SkipButtonLabelProperty, value);
    }

    /// <summary>The back button label.</summary>
    public string PreviousButtonLabel
    {
        get => (string)GetValue(PreviousButtonLabelProperty);
        set => SetValue(PreviousButtonLabelProperty, value);
    }

    /// <summary>The next button label.</summary>
    public string NextButtonLabel
    {
        get => (string)GetValue(NextButtonLabelProperty);
        set => SetValue(NextButtonLabelProperty, value);
    }

    /// <summary>The start button label, on the last (or only) step.</summary>
    public string StartButtonLabel
    {
        get => (string)GetValue(StartButtonLabelProperty);
        set => SetValue(StartButtonLabelProperty, value);
    }

    /// <summary>Your own footer buttons instead of skip, back, next and start (Carbon's <c>actionButtonRenderer</c>): drive them with <see cref="CurrentStep"/>, <see cref="StepCount"/> and <see cref="IsOpen"/>.</summary>
    public View? FooterContent
    {
        get => (View?)GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    /// <summary>Footer buttons that are disabled (Carbon's <c>disableActionButton</c>), for example <c>Next</c> until a choice on the step is made.</summary>
    public CdsInterstitialScreenButtons DisabledButtons
    {
        get => (CdsInterstitialScreenButtons)GetValue(DisabledButtonsProperty);
        set => SetValue(DisabledButtonsProperty, value);
    }

    /// <summary>The element that opened the screen; focus returns to it on close (Carbon's <c>launcherButtonRef</c>).</summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>An asynchronous step run when a footer button is activated, after <see cref="ActionRequested"/> (Carbon's async <c>onAction</c>): the button shows an inline loading until the task ends, then the default follows unless the handler set <see cref="CdsInterstitialScreenActionEventArgs.Cancel"/>.</summary>
    public Func<CdsInterstitialScreenActionEventArgs, Task>? ActionHandler
    {
        get => (Func<CdsInterstitialScreenActionEventArgs, Task>?)GetValue(ActionHandlerProperty);
        set => SetValue(ActionHandlerProperty, value);
    }

    /// <summary>Runs when the screen closes (after <see cref="RequestClose"/>), with the <see cref="CdsInterstitialScreenAction"/> as parameter.</summary>
    public System.Windows.Input.ICommand? CloseCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}
