using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// A footer button of an interstitial screen: a Carbon modal-footer button (64 high, label at the top start corner, no
/// gap or radius) with the optional 16px arrow pinned 16 from the end, and an inline loading in its place while the
/// button's asynchronous action runs.
/// </summary>
internal sealed class CdsInterstitialScreenButton : CdsControl
{
    readonly Grid _root = new() { HeightRequest = CdsModalSizing.FooterHeight };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0, InputTransparent = true };
    readonly Grid _face = new() { InputTransparent = true };
    // 15 and 14 inside the 1px border: Carbon's 16/15 padding of a 4rem modal-footer button.
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(15, 14, 15, 0), InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Button);
    readonly CdsHitTarget _hit;
    CdsIcon? _icon;
    CdsInlineLoading? _loading;
    bool _hasIcon;

    public CdsInterstitialScreenButton(CdsInterstitialScreenAction action)
    {
        Action = action;
        HorizontalOptions = LayoutOptions.Fill;
        _hit = CreateHitTarget();
        AutomationProperties.SetExcludedWithChildren(_box, true);
        _face.Add(_label);
        _box.Content = _face;
        _root.Add(_box);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        RefreshVisuals();
    }

    public event EventHandler? Clicked;

    public CdsInterstitialScreenAction Action { get; }

    public string? Text
    {
        get => _label.Text;
        set
        {
            _label.Text = value;
            SemanticProperties.SetDescription(_hit, value);
        }
    }

    /// <summary>Shows the arrow at the end.</summary>
    public bool HasIcon
    {
        get => _hasIcon;
        set
        {
            _hasIcon = value;
            if (value && _icon is null)
            {
                _icon = new CdsIcon(CdsIconName.ArrowRight, 16) { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 15, 15, 0), InputTransparent = true };
                _face.Add(_icon);
            }
            if (_icon is not null) _icon.IsVisible = value && _loading is null;
            UpdateLabelMargin();
            RefreshVisuals();
        }
    }

    /// <summary>Whether the inline loading is shown (the button's action is running).</summary>
    public bool IsLoading
    {
        get => _loading is not null;
        set
        {
            if (value == IsLoading) return;
            if (value)
            {
                _loading = new CdsInlineLoading { Status = CdsInlineLoadingStatus.Active, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0), InputTransparent = true };
                _face.Add(_loading);
            }
            else
            {
                _face.Remove(_loading!);
                _loading = null;
            }
            if (_icon is not null) _icon.IsVisible = _hasIcon && _loading is null;
            UpdateLabelMargin();
            RefreshVisuals();
        }
    }

    internal CdsHitTarget HitTarget => _hit;

    void UpdateLabelMargin()
    {
        var end = _hasIcon || _loading is not null ? 15 + CdsSpacing.Spacing05 + CdsSpacing.Spacing03 : 15;
        _label.Margin = new Thickness(15, 14, end, 0);
    }

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsInterstitialScreenTokens.Button(Action, State);
        SetColor(_box, BackgroundColorProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, v.Border);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        if (_icon is not null) SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

/// <summary>
/// Lays the footer buttons out on one 64px row (<see cref="InterstitialScreenModel.FooterWidths"/>): equal shares below
/// md, else 240 for back, next and start at the end and the rest for skip. Decided while measuring, from its own width.
/// </summary>
internal sealed class CdsInterstitialFooterLayout : Layout
{
    InterstitialFooterState _footer;

    /// <summary>The shown buttons and the state they reflect.</summary>
    public void SetButtons(IReadOnlyList<CdsInterstitialScreenButton> buttons, InterstitialFooterState footer)
    {
        _footer = footer;
        for (var i = Children.Count - 1; i >= 0; i--)
            if (Children[i] is CdsInterstitialScreenButton b && !buttons.Contains(b)) Children.RemoveAt(i);
        for (var i = 0; i < buttons.Count; i++)
        {
            var index = Children.IndexOf(buttons[i]);
            if (index == i) continue;
            if (index >= 0) Children.RemoveAt(index);
            Children.Insert(i, buttons[i]);
        }
        InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsInterstitialFooterLayout owner) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var width = double.IsInfinity(widthConstraint) ? InterstitialScreenModel.ActionButtonWidth * Math.Max(1, owner.Children.Count) : widthConstraint;
            var (_, widths) = InterstitialScreenModel.FooterWidths(width, owner._footer);
            for (var i = 0; i < owner.Children.Count; i++)
                owner.Children[i].Measure(i < widths.Length ? widths[i] : 0, CdsModalSizing.FooterHeight);
            return new Size(width, owner.Children.Count == 0 ? 0 : CdsModalSizing.FooterHeight);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var (lead, widths) = InterstitialScreenModel.FooterWidths(bounds.Width, owner._footer);
            var x = bounds.X + lead;
            for (var i = 0; i < owner.Children.Count; i++)
            {
                var w = i < widths.Length ? widths[i] : 0;
                owner.Children[i].Arrange(new Rect(x, bounds.Y, w, CdsModalSizing.FooterHeight));
                x += w;
            }
            return bounds.Size;
        }
    }
}

/// <summary>Pads the interstitial header by its own width (<see cref="InterstitialScreenModel.HeaderPadding"/>), decided while measuring.</summary>
internal sealed class CdsInterstitialHeaderLayout : Layout
{
    public bool IsModal { get; set; }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsInterstitialHeaderLayout owner) : ILayoutManager
    {
        Thickness Padding(double width) => InterstitialScreenModel.HeaderPadding(InterstitialScreenModel.IsCompact(width), owner.IsModal);

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var width = double.IsInfinity(widthConstraint) ? 400 : widthConstraint;
            var p = Padding(width);
            var height = 0d;
            foreach (var child in owner.Children)
                if (child.Visibility != Visibility.Collapsed)
                    height = Math.Max(height, child.Measure(Math.Max(0, width - p.HorizontalThickness), double.PositiveInfinity).Height);
            return new Size(width, height == 0 ? 0 : height + p.VerticalThickness);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var p = Padding(bounds.Width);
            foreach (var child in owner.Children)
                child.Arrange(new Rect(bounds.X + p.Left, bounds.Y + p.Top, Math.Max(0, bounds.Width - p.HorizontalThickness), child.DesiredSize.Height));
            return bounds.Size;
        }
    }
}
