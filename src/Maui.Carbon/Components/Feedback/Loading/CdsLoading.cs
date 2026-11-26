namespace Maui.Carbon;

/// <summary>What the overlay of a <see cref="CdsLoading"/> covers.</summary>
public enum CdsLoadingOverlayScope
{
    /// <summary>The whole page: a modal overlay opened through the page host (the default).</summary>
    Page,
    /// <summary>
    /// Only the region the control is placed in: it fills its cell with the <c>overlay</c> scrim and the spinner, and the
    /// views beside it in the same layout stop taking input and leave the accessibility tree while it is active.
    /// </summary>
    Region,
}

/// <summary>
/// A spinner that shows the app or a region is busy, optionally covering the page with a dimmed overlay. The large
/// spinner is 88 (a 10-unit <c>interactive</c> arc, one turn every 690ms) for full-page takeovers; the small one is 16
/// with a 48% arc over a <c>layer-accent-01</c> track, for use next to a trigger. With
/// <see cref="WithOverlay"/> the control draws nothing itself: while <see cref="IsActive"/> it opens a modal overlay with
/// the spinner centered (it does not trap focus; disable the content beneath yourself). To cover only the region that is
/// blocked (Carbon: "overlay Loading covers only the region that is blocked"), set <see cref="OverlayScope"/> to
/// <see cref="CdsLoadingOverlayScope.Region"/> and place the control over the region, in the same Grid cell as its
/// content. Without the overlay it renders only the spinner so you can place it inside a region. Reduced motion shows a
/// static arc. Never show several spinners at once; for measurable progress use <see cref="CdsProgressBar"/>.
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

    /// <summary>Bindable property for <see cref="OverlayScope"/>.</summary>
    public static readonly BindableProperty OverlayScopeProperty = BindableProperty.Create(
        nameof(OverlayScope), typeof(CdsLoadingOverlayScope), typeof(CdsLoading), CdsLoadingOverlayScope.Page,
        propertyChanged: static (b, _, _) => ((CdsLoading)b).OnOverlayScopeChanged());

    readonly CdsSpinnerView _spinner = new() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
    readonly BoxView _scrim = new() { IsVisible = false };
    readonly RegionRoot _root = new();
    readonly List<BlockedView> _blocked = [];
    CdsOverlay? _overlay;
    CdsSpinnerView? _overlaySpinner;
    bool _announced;

    /// <summary>Creates an active large spinner with an overlay.</summary>
    public CdsLoading()
    {
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        CdsTokens.SetColor(_scrim, BoxView.ColorProperty, CdsColorKeys.Overlay);
        AutomationProperties.SetExcludedWithChildren(_scrim, true);
        _root.Add(_scrim);
        _root.Add(_spinner);
        SetChrome(_root);
        Loaded += (_, _) => Update();
        Unloaded += (_, _) =>
        {
            CloseOverlay();
            UnblockRegion();
        };
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

    /// <summary>
    /// What the overlay covers while <see cref="WithOverlay"/> is on: the whole page (default) or only the region the
    /// control is placed in (<see cref="CdsLoadingOverlayScope.Region"/>: place it in the same Grid cell as the region's
    /// content; it fills the cell, and the other views in that layout ignore input and are hidden from screen readers while
    /// it is active).
    /// </summary>
    public CdsLoadingOverlayScope OverlayScope
    {
        get => (CdsLoadingOverlayScope)GetValue(OverlayScopeProperty);
        set => SetValue(OverlayScopeProperty, value);
    }

    /// <summary>Whether the region overlay is drawn now (region scope, overlay on, active).</summary>
    internal bool IsRegionOverlayShown => _scrim.IsVisible;

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
        var region = WithOverlay && OverlayScope == CdsLoadingOverlayScope.Region;
        var page = WithOverlay && !region;
        ConfigureSpinner(_spinner, spinning: IsActive && !page);
        // With a page overlay the control itself draws nothing (the overlay shows the spinner). A region overlay draws the
        // scrim and spinner over its cell while active, and nothing (and lets input through) while inactive.
        _spinner.IsVisible = region ? IsActive : !page;
        _scrim.IsVisible = region && IsActive;
        _root.IsVisible = !region || IsActive;
        InputTransparent = region && !IsActive;
        if (page && IsActive && IsLoaded) OpenOverlay();
        else CloseOverlay();
        if (region && IsActive && IsLoaded) BlockRegion();
        else UnblockRegion();

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
        CdsAnnouncer.Announce(Description);
    }

    void OnOverlayScopeChanged()
    {
        // A region overlay fills the cell it covers; the spinner alone is centered. The root asks for the whole space it is
        // offered while measuring, so the app's HorizontalOptions and VerticalOptions are never overwritten.
        _root.FillsSpace = OverlayScope == CdsLoadingOverlayScope.Region;
        Update();
    }

    // The views beside the control in its layout: the region it covers. They stop taking input (the scrim blocks
    // pointers, this blocks keyboard focus) and leave the accessibility tree, as the page does under a page overlay.
    void BlockRegion()
    {
        if (_blocked.Count > 0 || Parent is not Layout layout) return;
        foreach (var child in layout.Children)
        {
            if (child is not VisualElement view || ReferenceEquals(view, this)) continue;
            _blocked.Add(new BlockedView(new WeakReference<VisualElement>(view), view.InputTransparent, AutomationProperties.GetExcludedWithChildren(view)));
            view.InputTransparent = true;
            AutomationProperties.SetExcludedWithChildren(view, true);
        }
    }

    void UnblockRegion()
    {
        foreach (var blocked in _blocked)
        {
            if (!blocked.View.TryGetTarget(out var view)) continue;
            view.InputTransparent = blocked.InputTransparent;
            AutomationProperties.SetExcludedWithChildren(view, blocked.ExcludedWithChildren);
        }
        _blocked.Clear();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        UnblockRegion();
        base.OnParentSet();
        Update();
    }

    readonly record struct BlockedView(WeakReference<VisualElement> View, bool InputTransparent, bool? ExcludedWithChildren);

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsLoading)b).Update();

    /// <summary>Whether the control asks for all the space it is offered (region scope; for tests).</summary>
    internal bool FillsSpace => _root.FillsSpace;

    // The chrome root: in region scope it measures to the whole finite space it is offered, so the control covers its
    // cell under any alignment (an unbounded direction falls back to the spinner's size).
    sealed class RegionRoot : Grid
    {
        bool _fills;

        public bool FillsSpace
        {
            get => _fills;
            set
            {
                if (_fills == value) return;
                _fills = value;
                InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            var size = base.MeasureOverride(widthConstraint, heightConstraint);
            if (!_fills) return size;
            return new Size(
                double.IsFinite(widthConstraint) ? widthConstraint : size.Width,
                double.IsFinite(heightConstraint) ? heightConstraint : size.Height);
        }
    }
}
