using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// A gradient strip that fades content into a surface color (<c>layer-01</c>): the bottom fade of a scrolling modal body
/// and the end fade of a single-line code snippet. The color follows the theme and layer like any token.
/// </summary>
internal sealed class CdsEdgeFade : Grid
{
    static readonly BindableProperty FadeColorProperty = BindableProperty.Create(
        "FadeColor", typeof(Color), typeof(CdsEdgeFade), null,
        propertyChanged: static (b, _, _) => ((CdsEdgeFade)b).Rebuild());

    readonly bool _vertical;
    readonly double _ramp;
    readonly double _solid;

    /// <param name="colorKey">The surface color key.</param>
    /// <param name="vertical"><c>true</c>: transparent at the top, solid at the bottom; <c>false</c>: transparent at the start, solid at the end.</param>
    /// <param name="ramp">Length of the gradient.</param>
    /// <param name="solid">Length of the solid part after the gradient.</param>
    public CdsEdgeFade(string colorKey, bool vertical, double ramp, double solid)
    {
        _vertical = vertical;
        _ramp = ramp;
        _solid = solid;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        if (vertical)
        {
            HeightRequest = ramp + solid;
            VerticalOptions = LayoutOptions.End;
        }
        else
        {
            WidthRequest = ramp + solid;
            HorizontalOptions = LayoutOptions.End;
        }
        this.SetDynamicResource(FadeColorProperty, colorKey);
    }

    void Rebuild()
    {
        if (GetValue(FadeColorProperty) is not Color color) return;
        var total = _ramp + _solid;
        var split = total <= 0 ? 1 : _ramp / total;
        Background = new LinearGradientBrush(
            [new GradientStop(color.WithAlpha(0), 0), new GradientStop(color, (float)split), new GradientStop(color, 1)],
            new Point(0, 0), _vertical ? new Point(0, 1) : new Point(1, 0));
    }
}

/// <summary>Stacks a scroller over a fade strip pinned to its bottom; sized by the scroller.</summary>
internal sealed class CdsScrollFadeLayout : Layout
{
    readonly ScrollView _scroller;
    readonly CdsEdgeFade _fade;

    public CdsScrollFadeLayout(ScrollView scroller, CdsEdgeFade fade)
    {
        _scroller = scroller;
        _fade = fade;
        Children.Add(scroller);
        Children.Add(fade);
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsScrollFadeLayout owner) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var size = ((IView)owner._scroller).Measure(widthConstraint, heightConstraint);
            ((IView)owner._fade).Measure(size.Width, size.Height);
            return size;
        }

        public Size ArrangeChildren(Rect bounds)
        {
            ((IView)owner._scroller).Arrange(new Rect(0, 0, bounds.Width, bounds.Height));
            var fadeHeight = Math.Min(CdsModalSizing.FadeHeight, bounds.Height);
            ((IView)owner._fade).Arrange(new Rect(0, bounds.Height - fadeHeight, bounds.Width, fadeHeight));
            return bounds.Size;
        }
    }
}

/// <summary>
/// The modal container's layout: header and footer take their natural height; the body takes what is left under the
/// container's maximum height and scrolls inside it.
/// </summary>
internal sealed class CdsModalPanel : Layout
{
    View? _header;
    View? _body;
    View? _footer;
    double _maxHeight = double.PositiveInfinity;

    /// <summary>The maximum height of the whole container.</summary>
    public double MaxPanelHeight
    {
        get => _maxHeight;
        set
        {
            if (Math.Abs(_maxHeight - value) < 0.01) return;
            _maxHeight = value;
            InvalidateMeasure();
        }
    }

    /// <summary>Replaces the parts.</summary>
    public void SetParts(View? header, View? body, View? footer)
    {
        Release();
        _header = header;
        _body = body;
        _footer = footer;
        if (header is not null) Children.Add(header);
        if (body is not null) Children.Add(body);
        if (footer is not null) Children.Add(footer);
    }

    /// <summary>Removes the parts so they can be shown again elsewhere.</summary>
    public void Release()
    {
        Children.Clear();
        _header = _body = _footer = null;
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsModalPanel owner) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var limit = Math.Min(heightConstraint, owner._maxHeight);
            var width = double.IsInfinity(widthConstraint) ? 400 : widthConstraint;
            var headerHeight = owner._header is null ? 0 : ((IView)owner._header).Measure(width, double.PositiveInfinity).Height;
            var footerHeight = owner._footer is null ? 0 : ((IView)owner._footer).Measure(width, double.PositiveInfinity).Height;
            var bodyHeight = 0d;
            if (owner._body is not null)
                bodyHeight = ((IView)owner._body).Measure(width, Math.Max(0, limit - headerHeight - footerHeight)).Height;
            return new Size(width, headerHeight + bodyHeight + footerHeight);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var headerHeight = owner._header is null ? 0 : ((IView)owner._header).Measure(bounds.Width, double.PositiveInfinity).Height;
            var footerHeight = owner._footer is null ? 0 : ((IView)owner._footer).Measure(bounds.Width, double.PositiveInfinity).Height;
            var bodyHeight = Math.Max(0, bounds.Height - headerHeight - footerHeight);
            if (owner._header is not null) ((IView)owner._header).Arrange(new Rect(0, 0, bounds.Width, headerHeight));
            if (owner._body is not null) ((IView)owner._body).Arrange(new Rect(0, headerHeight, bounds.Width, bodyHeight));
            if (owner._footer is not null) ((IView)owner._footer).Arrange(new Rect(0, bounds.Height - footerHeight, bounds.Width, footerHeight));
            return bounds.Size;
        }
    }
}

/// <summary>Announces text to screen readers, ignoring platforms (and unit tests) without an implementation.</summary>
internal static class CdsAnnouncer
{
    public static void Announce(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            SemanticScreenReader.Announce(text);
        }
        catch (Exception)
        {
            // No screen reader implementation on this platform.
        }
    }
}
