namespace Maui.Carbon;

/// <summary>The states of a <see cref="CdsInlineLoading"/>.</summary>
public enum CdsInlineLoadingStatus
{
    /// <summary>Winds the spinner down to nothing and keeps the text.</summary>
    Inactive,
    /// <summary>A small <c>interactive</c> spinner (the default).</summary>
    Active,
    /// <summary>A <c>checkmark--filled</c> in <c>support-success</c>.</summary>
    Finished,
    /// <summary>An <c>error--filled</c> in <c>support-error</c>.</summary>
    Error,
}

/// <summary>How a live region announces its changes (the web's <c>aria-live</c>).</summary>
public enum CdsLiveMode
{
    /// <summary>Interrupts what the screen reader is saying (<c>aria-live="assertive"</c>). The default of inline loading.</summary>
    Assertive,
    /// <summary>Queued after what the screen reader is saying (<c>aria-live="polite"</c>): background refreshes and progress polls.</summary>
    Polite,
    /// <summary>Not announced (<c>aria-live="off"</c>).</summary>
    Off,
}

/// <summary>What a status draws (pure, tested).</summary>
/// <param name="Spinner">Show the spinner.</param>
/// <param name="Icon">The status icon, if any.</param>
/// <param name="IconColor">The color key of the icon.</param>
/// <param name="DefaultIconDescription">The default accessible name of the icon.</param>
public readonly record struct CdsInlineLoadingVisuals(bool Spinner, CdsIconName? Icon, string? IconColor, string DefaultIconDescription);

/// <summary>Maps <see cref="CdsInlineLoadingStatus"/> to what it draws.</summary>
public static class CdsInlineLoadingTokens
{
    /// <summary>Resolves a status: spinner, <c>checkmark--filled</c> in <c>support-success</c>, or <c>error--filled</c> in <c>support-error</c>.</summary>
    public static CdsInlineLoadingVisuals Resolve(CdsInlineLoadingStatus status) => status switch
    {
        CdsInlineLoadingStatus.Active => new(true, null, null, "loading"),
        CdsInlineLoadingStatus.Finished => new(false, CdsIconName.CheckmarkFilled, CdsColorKeys.SupportSuccess, "finished"),
        CdsInlineLoadingStatus.Error => new(false, CdsIconName.ErrorFilled, CdsColorKeys.SupportError, "error"),
        _ => new(false, null, null, "not loading"),
    };
}

/// <summary>
/// A small spinner with status text for a short action in progress, usually in place of the button that started it,
/// which settles to a success or error icon. Active shows the 16px spinner, finished a success check, error an error
/// icon; inactive winds the spinner down and keeps the text. <see cref="Success"/> is raised
/// <see cref="SuccessDelay"/> milliseconds after the status becomes finished. Disable the other actions of the form
/// while it is active, and move focus somewhere sensible when the button it replaced comes back. Status text changes
/// are announced while it is shown, assertively by default; set <see cref="LiveMode"/> to
/// <see cref="CdsLiveMode.Polite"/> for background refreshes (a progress poll) or <see cref="CdsLiveMode.Off"/>.
/// </summary>
public class CdsInlineLoading : CdsControl
{
    /// <summary>Bindable property for <see cref="Status"/>.</summary>
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(CdsInlineLoadingStatus), typeof(CdsInlineLoading), CdsInlineLoadingStatus.Active,
        propertyChanged: static (b, _, _) => ((CdsInlineLoading)b).OnStatusChanged());

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsInlineLoading), null,
        propertyChanged: static (b, _, _) => ((CdsInlineLoading)b).OnDescriptionChanged());

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsInlineLoading), null,
        propertyChanged: static (b, _, _) => ((CdsInlineLoading)b).Update());

    /// <summary>Bindable property for <see cref="SuccessDelay"/>.</summary>
    public static readonly BindableProperty SuccessDelayProperty = BindableProperty.Create(
        nameof(SuccessDelay), typeof(int), typeof(CdsInlineLoading), 1500, coerceValue: static (_, v) => Math.Max(0, (int)v));

    /// <summary>Bindable property for <see cref="LiveMode"/>.</summary>
    public static readonly BindableProperty LiveModeProperty = BindableProperty.Create(
        nameof(LiveMode), typeof(CdsLiveMode), typeof(CdsInlineLoading), CdsLiveMode.Assertive);

    readonly CdsSpinnerView _spinner = new() { IsSmall = true };
    readonly CdsIcon _icon = new() { IsVisible = false };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.Label02, Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center };
    readonly Grid _glyph = new() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center };
    readonly CdsCoalescedAction _announce;
    IDispatcherTimer? _timer;
    (string? Text, CdsInlineLoadingStatus Status)? _announced;

    /// <summary>Creates an active indicator.</summary>
    public CdsInlineLoading()
    {
        _announce = new CdsCoalescedAction(this, AnnounceNow);
        CdsTokens.SetColor(_spinner, CdsSpinnerView.TrackColorProperty, CdsColorKeys.LayerAccent01);
        _glyph.Add(_spinner);
        _glyph.Add(_icon);
        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03, MinimumHeightRequest = CdsSizing.Small };
        row.Add(_glyph);
        row.Add(_text);
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        SetChrome(row);
        Unloaded += (_, _) =>
        {
            StopTimer();
            _announced = null;
        };
        Update();
    }

    /// <summary>Creates an indicator with a status and text.</summary>
    public CdsInlineLoading(CdsInlineLoadingStatus status, string? description) : this()
    {
        Status = status;
        Description = description;
    }

    /// <summary>Raised <see cref="SuccessDelay"/> ms after <see cref="Status"/> becomes finished: reset the form, close the modal, refetch.</summary>
    public event EventHandler? Success;

    /// <summary>The state. Default <see cref="CdsInlineLoadingStatus.Active"/>. Without a <see cref="Success"/> handler the finished state stays until you change it.</summary>
    public CdsInlineLoadingStatus Status
    {
        get => (CdsInlineLoadingStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>The status text: the active text names the action ("Saving..."), the finished text confirms it ("Saved"), the error text says it failed.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Accessible name of the icon. Defaults to "loading", "not loading", "finished" or "error"; give a spinner-only instance one that says what is happening.</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>
    /// How status text changes are announced (Carbon's <c>aria-live</c>): <see cref="CdsLiveMode.Assertive"/> (default),
    /// <see cref="CdsLiveMode.Polite"/> for background refreshes, or <see cref="CdsLiveMode.Off"/>. A status and text
    /// changed together are announced once, and nothing is announced while the indicator is hidden.
    /// </summary>
    public CdsLiveMode LiveMode
    {
        get => (CdsLiveMode)GetValue(LiveModeProperty);
        set => SetValue(LiveModeProperty, value);
    }

    /// <summary>Milliseconds between finishing and <see cref="Success"/>. Default 1500.</summary>
    public int SuccessDelay
    {
        get => (int)GetValue(SuccessDelayProperty);
        set => SetValue(SuccessDelayProperty, value);
    }

    void Update()
    {
        var v = CdsInlineLoadingTokens.Resolve(Status);
        var description = IconDescription ?? v.DefaultIconDescription;
        _spinner.IsSpinning = v.Spinner;
        SemanticProperties.SetDescription(_spinner, description);
        AutomationProperties.SetIsInAccessibleTree(_spinner, v.Spinner);
        _icon.IsVisible = v.Icon is not null;
        _icon.Icon = v.Icon;
        _icon.Description = v.Icon is null ? null : description;
        if (v.IconColor is not null) CdsTokens.SetColor(_icon, CdsIcon.ColorProperty, v.IconColor);
        _text.Text = Description;
        _text.IsVisible = !string.IsNullOrEmpty(Description);
    }

    void OnDescriptionChanged()
    {
        Update();
        AnnounceText();
    }

    void OnStatusChanged()
    {
        Update();
        StopTimer();
        AnnounceText();
        if (Status == CdsInlineLoadingStatus.Finished) StartTimer();
    }

    void StartTimer()
    {
        IDispatcher? dispatcher;
        try
        {
            dispatcher = Dispatcher;
        }
        catch (InvalidOperationException)
        {
            return; // No dispatcher yet (not created on the UI thread): the success timer cannot run.
        }
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(1, SuccessDelay));
        _timer.IsRepeating = false;
        _timer.Tick += OnTimer;
        _timer.Start();
    }

    void StopTimer()
    {
        if (_timer is null) return;
        _timer.Stop();
        _timer.Tick -= OnTimer;
        _timer = null;
    }

    void OnTimer(object? sender, EventArgs e)
    {
        StopTimer();
        if (Status == CdsInlineLoadingStatus.Finished) Success?.Invoke(this, EventArgs.Empty);
    }

    void AnnounceText() => _announce.Request();

    void AnnounceNow()
    {
        if (!IsLoaded || !CdsLoop.IsShown(this) || string.IsNullOrEmpty(Description)) return;
        if (AnnouncementPriority(LiveMode) is not { } priority) return;
        // The same text in the same status says nothing new (a poll re-setting the status).
        (string? Text, CdsInlineLoadingStatus Status) announcement = (Description, Status);
        if (_announced == announcement) return;
        _announced = announcement;
        CdsAnnouncer.Announce(Description, priority);
    }

    /// <summary>The announcement priority of a live mode, or <c>null</c> when it is off.</summary>
    internal static CdsAnnouncePriority? AnnouncementPriority(CdsLiveMode mode) => mode switch
    {
        CdsLiveMode.Assertive => CdsAnnouncePriority.Assertive,
        CdsLiveMode.Polite => CdsAnnouncePriority.Polite,
        _ => null,
    };
}
