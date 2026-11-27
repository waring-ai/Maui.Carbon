namespace Maui.Carbon;

/// <summary>
/// A multi-step create flow in a wide tearsheet (Carbon for IBM Products' <c>CreateTearsheet</c>): the steps
/// (<see cref="CdsCreateStep"/>) show one at a time, a vertical progress indicator lists them in the influencer column,
/// and the footer has Cancel, Back and Next, which becomes <see cref="SubmitButtonText"/> on the last step. Use it to
/// create something complex in a few steps without leaving the page; a single short form belongs in a
/// <see cref="CdsCreateTearsheetNarrow"/>, <see cref="CdsCreateSidePanel"/> or modal.
/// </summary>
[ContentProperty(nameof(Steps))]
public class CdsCreateTearsheet : CdsTearsheetShell
{
    /// <summary>Bindable property for <see cref="BackButtonText"/>.</summary>
    public static readonly BindableProperty BackButtonTextProperty = BindableProperty.Create(
        nameof(BackButtonText), typeof(string), typeof(CdsCreateTearsheet), "Back",
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheet)b).SyncTexts());

    /// <summary>The Back button label.</summary>
    public string BackButtonText
    {
        get => (string)GetValue(BackButtonTextProperty);
        set => SetValue(BackButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="CancelButtonText"/>.</summary>
    public static readonly BindableProperty CancelButtonTextProperty = BindableProperty.Create(
        nameof(CancelButtonText), typeof(string), typeof(CdsCreateTearsheet), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheet)b).SyncTexts());

    /// <summary>The Cancel button label.</summary>
    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="NextButtonText"/>.</summary>
    public static readonly BindableProperty NextButtonTextProperty = BindableProperty.Create(
        nameof(NextButtonText), typeof(string), typeof(CdsCreateTearsheet), "Next",
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheet)b).SyncTexts());

    /// <summary>The primary button label before the last step.</summary>
    public string NextButtonText
    {
        get => (string)GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SubmitButtonText"/>.</summary>
    public static readonly BindableProperty SubmitButtonTextProperty = BindableProperty.Create(
        nameof(SubmitButtonText), typeof(string), typeof(CdsCreateTearsheet), "Create",
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheet)b).SyncTexts());

    /// <summary>The primary button label on the last step: the final action ("Create pipeline").</summary>
    public string SubmitButtonText
    {
        get => (string)GetValue(SubmitButtonTextProperty);
        set => SetValue(SubmitButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="InitialStep"/>.</summary>
    public static readonly BindableProperty InitialStepProperty = BindableProperty.Create(
        nameof(InitialStep), typeof(int), typeof(CdsCreateTearsheet), 0);

    /// <summary>The step to open on, counting included steps from 1 (0: the first).</summary>
    public int InitialStep
    {
        get => (int)GetValue(InitialStepProperty);
        set => SetValue(InitialStepProperty, value);
    }

    /// <summary>Bindable property for <see cref="SubmitAsync"/>.</summary>
    public static readonly BindableProperty SubmitAsyncProperty = BindableProperty.Create(
        nameof(SubmitAsync), typeof(Func<Task<bool>>), typeof(CdsCreateTearsheet), null);

    /// <summary>Runs when the person submits from the last step (Carbon: <c>onRequestSubmit</c>): create the resource here and return false (or throw) to stay. The primary button is disabled while it runs.</summary>
    public Func<Task<bool>>? SubmitAsync
    {
        get => (Func<Task<bool>>?)GetValue(SubmitAsyncProperty);
        set => SetValue(SubmitAsyncProperty, value);
    }

    /// <summary>Bindable property for <see cref="HasError"/>.</summary>
    public static readonly BindableProperty HasErrorProperty = BindableProperty.Create(
        nameof(HasError), typeof(bool), typeof(CdsCreateTearsheet), false,
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheet)b).OnHasErrorChanged());

    /// <summary>Set when submitting failed and the tearsheet shows an error: focus moves back into it.</summary>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /// <summary>Bindable property for <see cref="InfluencerWidth"/>.</summary>
    public static readonly BindableProperty InfluencerWidthProperty = BindableProperty.Create(
        nameof(InfluencerWidth), typeof(CdsTearsheetInfluencerWidth), typeof(CdsCreateTearsheet), CdsTearsheetInfluencerWidth.Narrow,
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheet)b).OnPartsChanged());

    /// <summary>The width of the progress column: 256 (default) or 320.</summary>
    public CdsTearsheetInfluencerWidth InfluencerWidth
    {
        get => (CdsTearsheetInfluencerWidth)GetValue(InfluencerWidthProperty);
        set => SetValue(InfluencerWidthProperty, value);
    }


    readonly CreateFlowController _flow;
    readonly ContentView _influencer;

    /// <summary>Creates a closed create tearsheet.</summary>
    public CdsCreateTearsheet()
    {
        _flow = new CreateFlowController(this, Actions)
        {
            CancelRequested = () => TryClose(),
            Submitted = () =>
            {
                Submitted?.Invoke(this, EventArgs.Empty);
                IsOpen = false;
            },
            StepChanged = () =>
            {
                CurrentStepChanged?.Invoke(this, EventArgs.Empty);
                ClaimFocus();
            },
        };
        _influencer = new ContentView { Content = _flow.Progress, Padding = new Thickness(CdsSpacing.Spacing06) };
        Body = new ContentView { Content = _flow.StepsHost, Padding = new Thickness(CdsSpacing.Spacing07, CdsSpacing.Spacing06) };
        SyncTexts();
    }

    /// <summary>Raised after a successful submit, just before the tearsheet closes.</summary>
    public event EventHandler? Submitted;

    /// <summary>Raised when the current step changes.</summary>
    public event EventHandler? CurrentStepChanged;

    /// <summary>The steps, in order.</summary>
    public IList<CdsCreateStep> Steps => _flow.Steps;

    /// <summary>The current step, counting every step from 1.</summary>
    public int CurrentStep => _flow.Current;

    /// <summary>The flow engine (for tests).</summary>
    internal CreateFlowController Flow => _flow;

    private protected override bool IsWide => true;

    private protected override View? InfluencerView => _influencer;

    private protected override CdsTearsheetInfluencerWidth InfluencerSize => InfluencerWidth;

    private protected override void OnOpening() => _flow.Reset(InitialStep);

    void SyncTexts()
    {
        if (_flow is null) return;
        _flow.BackText = BackButtonText;
        _flow.CancelText = CancelButtonText;
        _flow.NextText = NextButtonText;
        _flow.SubmitText = SubmitButtonText;
        _flow.Submit = SubmitAsync;
        _flow.Refresh();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SubmitAsyncProperty.PropertyName && _flow is not null) _flow.Submit = SubmitAsync;
    }

    void OnHasErrorChanged()
    {
        if (HasError) ClaimFocus();
    }
}
