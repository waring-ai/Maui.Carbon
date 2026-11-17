namespace Maui.Carbon;

/// <summary>
/// A spinner that shows the app or a region is busy, optionally covering the page with a dimmed overlay. The large
/// spinner is 88 (a 10-unit <c>interactive</c> arc, one turn every 690ms) for full-page takeovers; the small one is 16
/// with a 48% arc over a <c>layer-accent-01</c> track, for use next to a trigger. With
/// <see cref="WithOverlay"/> the control draws nothing itself: while <see cref="IsActive"/> it opens a modal overlay with
/// the spinner centered (it does not trap focus; disable the content beneath yourself). Without the overlay it renders
/// only the spinner so you can place it inside a region. Reduced motion shows a static arc. Never show several spinners
/// at once; for measurable progress use <see cref="CdsProgressBar"/>.
/// </summary>
public class CdsLoading : CdsControl
{
    /// <summary>Bindable property for <see cref="IsActive"/>.</summary>
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(CdsLoading), true, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsSmall"/>.</summary>
    public static readonly BindableProperty IsSmallProperty = BindableProperty.Create(
        nameof(IsSmall), typeof(bool), typeof(CdsLoading), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="WithOverlay"/>.</summary>
    public static readonly BindableProperty WithOverlayProperty = BindableProperty.Create(
        nameof(WithOverlay), typeof(bool), typeof(CdsLoading), true, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsLoading), "loading", propertyChanged: OnChanged);

    readonly CdsSpinnerView _spinner = new();
    CdsOverlay? _overlay;
    CdsSpinnerView? _overlaySpinner;
    bool _announced;

    /// <summary>Creates an active large spinner with an overlay.</summary>
    public CdsLoading()
    {
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        SetChrome(_spinner);
        Loaded += (_, _) => Update();
        Unloaded += (_, _) => CloseOverlay();
        Update();
    }

    /// <summary>Creates a spinner with an accessible description.</summary>
    public CdsLoading(string description, bool small = false, bool withOverlay = true) : this()
    {
        Description = description;
        IsSmall = small;
        WithOverlay = withOverlay;
    }

    /// <summary><c>true</c> (default) spins; <c>false</c> winds the spinner down and stops it, and closes the overlay.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>The 16px spinner next to a label instead of the 88 one.</summary>
    public bool IsSmall
    {
        get => (bool)GetValue(IsSmallProperty);
        set => SetValue(IsSmallProperty, value);
    }

    /// <summary>
    /// Cover the page with the <c>overlay</c> scrim while active (default <c>true</c>). Turn it off to place the spinner
    /// inside a region.
    /// </summary>
    public bool WithOverlay
    {
        get => (bool)GetValue(WithOverlayProperty);
        set => SetValue(WithOverlayProperty, value);
    }

    /// <summary>The accessible name and the announcement while active: name it after what is loading ("Recalculating billing summary"). Default "loading".</summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The overlay while one is open.</summary>
    public CdsOverlay? Overlay => _overlay;

    void Update()
    {
        ConfigureSpinner(_spinner, spinning: IsActive && !WithOverlay);
        // With an overlay the control itself draws nothing (the overlay shows the spinner).
        _spinner.IsVisible = !WithOverlay;
        if (WithOverlay && IsActive && IsLoaded) OpenOverlay();
        else CloseOverlay();

        if (IsActive && IsLoaded)
        {
            if (!_announced)
            {
                _announced = true;
                Announce();
            }
        }
        else
        {
            _announced = false;
        }
    }

    void ConfigureSpinner(CdsSpinnerView spinner, bool spinning)
    {
        spinner.IsSmall = IsSmall;
        if (IsSmall) CdsTokens.SetColor(spinner, CdsSpinnerView.TrackColorProperty, CdsColorKeys.LayerAccent01);
        else spinner.TrackColor = null;
        SemanticProperties.SetDescription(spinner, Description);
        AutomationProperties.SetIsInAccessibleTree(spinner, true);
        spinner.IsSpinning = spinning;
    }

    void OpenOverlay()
    {
        if (_overlay is { IsOpen: true })
        {
            if (_overlaySpinner is not null) ConfigureSpinner(_overlaySpinner, spinning: true);
            return;
        }
        _overlaySpinner = new CdsSpinnerView();
        ConfigureSpinner(_overlaySpinner, spinning: true);
        try
        {
            _overlay = CdsOverlayService.Open(_overlaySpinner,
                new CdsOverlayOptions { IsModal = true, CloseOnEscape = false, LightDismiss = false, ReturnFocus = false }, this);
            _overlay.Closed += OnOverlayClosed;
        }
        catch (InvalidOperationException)
        {
            // No page host (not on a page yet): nothing to cover.
            _overlay = null;
            _overlaySpinner = null;
        }
    }

    void CloseOverlay()
    {
        var overlay = _overlay;
        _overlay = null;
        _overlaySpinner = null;
        if (overlay is null) return;
        overlay.Closed -= OnOverlayClosed;
        overlay.Close();
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        _overlay = null;
        _overlaySpinner = null;
    }

    void Announce()
    {
        try
        {
            SemanticScreenReader.Announce(Description);
        }
        catch (Exception)
        {
            // No screen reader service on this platform.
        }
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsLoading)b).Update();
}
