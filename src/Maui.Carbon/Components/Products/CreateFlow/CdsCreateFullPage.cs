namespace Maui.Carbon;

/// <summary>
/// A multi-step create flow that fills a page (Carbon for IBM Products' <c>CreateFullPage</c>): an optional title and
/// breadcrumb, a progress column on the start side, the current <see cref="CdsCreateStep"/> on the page background and
/// a full-width footer with Cancel, Back and Next (the last step's Next submits). Cancel asks for confirmation in a danger
/// modal before <see cref="RequestClose"/> fires. Use it for a large, many-step setup that deserves its own page; for a
/// smaller flow over the current page use <see cref="CdsCreateTearsheet"/>.
/// </summary>
[ContentProperty(nameof(Steps))]
public class CdsCreateFullPage : CdsControl
{
    /// <summary>Bindable property for <see cref="BackButtonText"/>.</summary>
    public static readonly BindableProperty BackButtonTextProperty = BindableProperty.Create(
        nameof(BackButtonText), typeof(string), typeof(CdsCreateFullPage), "Back",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The Back button label.</summary>
    public string BackButtonText
    {
        get => (string)GetValue(BackButtonTextProperty);
        set => SetValue(BackButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="CancelButtonText"/>.</summary>
    public static readonly BindableProperty CancelButtonTextProperty = BindableProperty.Create(
        nameof(CancelButtonText), typeof(string), typeof(CdsCreateFullPage), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The Cancel button label.</summary>
    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="NextButtonText"/>.</summary>
    public static readonly BindableProperty NextButtonTextProperty = BindableProperty.Create(
        nameof(NextButtonText), typeof(string), typeof(CdsCreateFullPage), "Next",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The primary button label before the last step.</summary>
    public string NextButtonText
    {
        get => (string)GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SubmitButtonText"/>.</summary>
    public static readonly BindableProperty SubmitButtonTextProperty = BindableProperty.Create(
        nameof(SubmitButtonText), typeof(string), typeof(CdsCreateFullPage), "Create",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The primary button label on the last step: the final action ("Create pipeline").</summary>
    public string SubmitButtonText
    {
        get => (string)GetValue(SubmitButtonTextProperty);
        set => SetValue(SubmitButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="InitialStep"/>.</summary>
    public static readonly BindableProperty InitialStepProperty = BindableProperty.Create(
        nameof(InitialStep), typeof(int), typeof(CdsCreateFullPage), 0);

    /// <summary>The step to open on, counting included steps from 1 (0: the first).</summary>
    public int InitialStep
    {
        get => (int)GetValue(InitialStepProperty);
        set => SetValue(InitialStepProperty, value);
    }

    /// <summary>Bindable property for <see cref="SubmitAsync"/>.</summary>
    public static readonly BindableProperty SubmitAsyncProperty = BindableProperty.Create(
        nameof(SubmitAsync), typeof(Func<Task<bool>>), typeof(CdsCreateFullPage), null);

    /// <summary>Runs when the person submits from the last step (Carbon: <c>onRequestSubmit</c>): create the resource here and return false (or throw) to stay. The primary button is disabled while it runs.</summary>
    public Func<Task<bool>>? SubmitAsync
    {
        get => (Func<Task<bool>>?)GetValue(SubmitAsyncProperty);
        set => SetValue(SubmitAsyncProperty, value);
    }

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsCreateFullPage), null,
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The page title above the flow.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="Breadcrumb"/>.</summary>
    public static readonly BindableProperty BreadcrumbProperty = BindableProperty.Create(
        nameof(Breadcrumb), typeof(View), typeof(CdsCreateFullPage), null,
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>A <see cref="CdsBreadcrumb"/> above the title.</summary>
    public View? Breadcrumb
    {
        get => (View?)GetValue(BreadcrumbProperty);
        set => SetValue(BreadcrumbProperty, value);
    }

    /// <summary>Bindable property for <see cref="SecondaryTitle"/>.</summary>
    public static readonly BindableProperty SecondaryTitleProperty = BindableProperty.Create(
        nameof(SecondaryTitle), typeof(string), typeof(CdsCreateFullPage), null,
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>A heading above the progress indicator.</summary>
    public string? SecondaryTitle
    {
        get => (string?)GetValue(SecondaryTitleProperty);
        set => SetValue(SecondaryTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="ModalTitle"/>.</summary>
    public static readonly BindableProperty ModalTitleProperty = BindableProperty.Create(
        nameof(ModalTitle), typeof(string), typeof(CdsCreateFullPage), "Cancel creating?",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The heading of the cancel confirmation.</summary>
    public string ModalTitle
    {
        get => (string)GetValue(ModalTitleProperty);
        set => SetValue(ModalTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="ModalDescription"/>.</summary>
    public static readonly BindableProperty ModalDescriptionProperty = BindableProperty.Create(
        nameof(ModalDescription), typeof(string), typeof(CdsCreateFullPage), "Your progress will be lost.",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The body of the cancel confirmation.</summary>
    public string? ModalDescription
    {
        get => (string?)GetValue(ModalDescriptionProperty);
        set => SetValue(ModalDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="ModalDangerButtonText"/>.</summary>
    public static readonly BindableProperty ModalDangerButtonTextProperty = BindableProperty.Create(
        nameof(ModalDangerButtonText), typeof(string), typeof(CdsCreateFullPage), "Cancel creating",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The confirm (danger) button of the cancel confirmation.</summary>
    public string ModalDangerButtonText
    {
        get => (string)GetValue(ModalDangerButtonTextProperty);
        set => SetValue(ModalDangerButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="ModalSecondaryButtonText"/>.</summary>
    public static readonly BindableProperty ModalSecondaryButtonTextProperty = BindableProperty.Create(
        nameof(ModalSecondaryButtonText), typeof(string), typeof(CdsCreateFullPage), "Return to form",
        propertyChanged: static (b, _, _) => ((CdsCreateFullPage)b).SyncTexts());

    /// <summary>The button that keeps the person in the flow.</summary>
    public string ModalSecondaryButtonText
    {
        get => (string)GetValue(ModalSecondaryButtonTextProperty);
        set => SetValue(ModalSecondaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(System.Windows.Input.ICommand), typeof(CdsCreateFullPage), null);

    /// <summary>Runs when the person confirms cancelling, after <see cref="RequestClose"/>.</summary>
    public System.Windows.Input.ICommand? RequestCloseCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }


    readonly CreateFlowController _flow;
    readonly CdsActionSet _actionSet = new() { Size = CdsActionSetSize.Xxl, ButtonSize = CdsSize.Xxl, FillKey = CdsColorKeys.Background };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Heading04, LineBreakMode = LineBreakMode.WordWrap };
    readonly ContentView _breadcrumbHost = new();
    readonly VerticalStackLayout _header = new() { Spacing = CdsSpacing.Spacing03, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing06) };
    readonly CdsText _secondaryTitle = new() { TypeStyle = CdsTypeStyle.HeadingCompact02, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(CdsSpacing.Spacing06, CdsSpacing.Spacing06, CdsSpacing.Spacing06, 0) };
    readonly CdsModal _confirm = new() { Size = CdsModalSize.Sm, IsDanger = true };
    readonly CdsText _confirmText = new() { TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.WordWrap };

    /// <summary>Creates a create full page.</summary>
    public CdsCreateFullPage()
    {
        _flow = new CreateFlowController(this, _actionSet.Actions)
        {
            CancelRequested = () => _confirm.IsOpen = true,
            Submitted = () => Submitted?.Invoke(this, EventArgs.Empty),
            StepChanged = () => CurrentStepChanged?.Invoke(this, EventArgs.Empty),
        };
        _flow.Progress.StepRequested += (_, e) => OnProgressStepRequested(e.Index);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);
        SemanticProperties.SetHeadingLevel(_secondaryTitle, SemanticHeadingLevel.Level2);
        _header.Add(_breadcrumbHost);
        _header.Add(_title);

        var influencer = new Grid { WidthRequest = 257, RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)] };
        influencer.Add(_secondaryTitle, 0, 0);
        var progress = new ScrollView { Content = new ContentView { Content = _flow.Progress, Padding = new Thickness(CdsSpacing.Spacing06) } };
        influencer.Add(progress, 0, 1);
        var influencerEdge = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(influencerEdge, BoxView.ColorProperty, CdsColorKeys.LayerAccent01);
        influencer.Add(influencerEdge, 0, 0);
        Grid.SetRowSpan(influencerEdge, 2);
        CdsTokens.SetColor(influencer, BackgroundColorProperty, CdsColorKeys.Layer01);

        var content = new ScrollView { Content = new ContentView { Content = _flow.StepsHost, Padding = new Thickness(CdsSpacing.Spacing07, CdsSpacing.Spacing06, CdsSpacing.Spacing07, CdsSpacing.Spacing07) } };
        CdsTokens.SetColor(content, BackgroundColorProperty, CdsColorKeys.Background);
        var buttonRule = new BoxView { HeightRequest = 1 };
        CdsTokens.SetColor(buttonRule, BoxView.ColorProperty, CdsColorKeys.LayerAccent01);
        var buttons = new VerticalStackLayout { Spacing = 0 };
        buttons.Add(buttonRule);
        buttons.Add(_actionSet);
        var main = new Grid { RowDefinitions = [new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)] };
        main.Add(content, 0, 0);
        main.Add(buttons, 0, 1);

        var body = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)] };
        body.Add(influencer, 0);
        body.Add(main, 1);

        var root = new Grid { RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)] };
        root.Add(_header, 0, 0);
        root.Add(body, 0, 1);
        root.Add(_confirm, 0, 0);

        _confirm.Body = _confirmText;
        _confirm.InitialFocusElement = null;
        _confirm.RequestSubmit += (_, _) =>
        {
            _confirm.IsOpen = false;
            RequestClose?.Invoke(this, EventArgs.Empty);
            if (RequestCloseCommand?.CanExecute(null) == true) RequestCloseCommand.Execute(null);
        };
        SetChrome(root, fillsWidth: true);
        VerticalOptions = LayoutOptions.Fill;
        SyncTexts();
        Loaded += (_, _) => { if (_flow.Current == 0 || !_started) Restart(); };
    }

    bool _started;

    /// <summary>Raised when the person confirms cancelling. Leave the page here.</summary>
    public event EventHandler? RequestClose;

    /// <summary>Raised after a successful submit.</summary>
    public event EventHandler? Submitted;

    /// <summary>Raised when the current step changes.</summary>
    public event EventHandler? CurrentStepChanged;

    /// <summary>
    /// Raised when a step in the progress indicator is clicked (Carbon: <c>onClickInfluencerStep</c>), with the step
    /// number counting every step from 1. While it has a handler the progress steps are clickable; move with
    /// <see cref="GoToStep"/> if the person may go there.
    /// </summary>
    public event EventHandler<int>? InfluencerStepClicked
    {
        add
        {
            _influencerStepClicked += value;
            _flow.Progress.IsClickable = _influencerStepClicked is not null;
        }
        remove
        {
            _influencerStepClicked -= value;
            _flow.Progress.IsClickable = _influencerStepClicked is not null;
        }
    }

    EventHandler<int>? _influencerStepClicked;

    /// <summary>The steps, in order.</summary>
    public IList<CdsCreateStep> Steps => _flow.Steps;

    /// <summary>The current step, counting every step from 1.</summary>
    public int CurrentStep => _flow.Current;

    /// <summary>The flow engine (for tests).</summary>
    internal CreateFlowController Flow => _flow;

    /// <summary>The cancel confirmation (for tests).</summary>
    internal CdsModal Confirmation => _confirm;

    /// <summary>Goes back to the opening step (<see cref="InitialStep"/>), as when the flow is shown again.</summary>
    public void Restart()
    {
        _started = true;
        _flow.Reset(InitialStep);
    }

    /// <summary>Moves to a step (counting every step from 1) if it is included.</summary>
    public void GoToStep(int step) => _flow.GoTo(step);

    void OnProgressStepRequested(int progressIndex)
    {
        var listed = CreateFlowModel.ProgressSteps([.. _flow.Steps.Select(s => s.Info)]);
        if (progressIndex >= 0 && progressIndex < listed.Count) _influencerStepClicked?.Invoke(this, listed[progressIndex]);
    }

    void SyncTexts()
    {
        if (_flow is null) return;
        _flow.BackText = BackButtonText;
        _flow.CancelText = CancelButtonText;
        _flow.NextText = NextButtonText;
        _flow.SubmitText = SubmitButtonText;
        _flow.Submit = SubmitAsync;
        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _breadcrumbHost.Content = Breadcrumb;
        _breadcrumbHost.IsVisible = Breadcrumb is not null;
        _header.IsVisible = _title.IsVisible || _breadcrumbHost.IsVisible;
        _secondaryTitle.Text = SecondaryTitle;
        _secondaryTitle.IsVisible = !string.IsNullOrEmpty(SecondaryTitle);
        _confirm.Heading = ModalTitle;
        _confirmText.Text = ModalDescription;
        _confirm.PrimaryButtonText = ModalDangerButtonText;
        _confirm.SecondaryButtonText = ModalSecondaryButtonText;
        _flow.Refresh();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SubmitAsyncProperty.PropertyName && _flow is not null) _flow.Submit = SubmitAsync;
    }
}
