using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// A stack of views of which one, <see cref="Active"/>, is shown and laid out; the others stay attached (handlers, native
/// views, state) but are hidden on the platform and skipped by layout. Use it for screens or panels that are switched often
/// and are costly to build: the screens of an app shell, the panes of a large settings page.
/// <para>
/// Switching costs only the two views involved. Hiding a view with <c>IsVisible</c> makes MAUI push the effective
/// visibility to every descendant (one native update each, hundreds of milliseconds for a large screen), and taking it out
/// of the window and back costs more. The deck instead hides the inactive views' own native view, which hides everything
/// in it. An inactive view keeps <c>IsVisible="True"</c>: Carbon treats it as hidden (animations park, focus and audit walks
/// skip it); check <see cref="IsHidden(Element)"/> where code of your own asks whether something is on screen.
/// </para>
/// </summary>
public class CdsDeck : Layout
{
    /// <summary>Bindable property for <see cref="Active"/>.</summary>
    public static readonly BindableProperty ActiveProperty = BindableProperty.Create(
        nameof(Active), typeof(View), typeof(CdsDeck), null,
        propertyChanged: static (b, o, n) => ((CdsDeck)b).OnActiveChanged((View?)o, (View?)n));

    /// <summary>The shown child, or <c>null</c> for none. Set it to a child of the deck.</summary>
    public View? Active
    {
        get => (View?)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    /// <summary>
    /// Whether <paramref name="element"/> is hidden: not visible itself, or an inactive child of a deck (not its ancestors;
    /// see <see cref="IsShown"/> for the whole chain).
    /// </summary>
    public static bool IsHidden(Element element) =>
        element is VisualElement { IsVisible: false } || (element.Parent is CdsDeck deck && !ReferenceEquals(deck.Active, element));

    /// <summary>Whether <paramref name="element"/> and every ancestor are shown (none is hidden or an inactive deck child).</summary>
    public static bool IsShown(Element element)
    {
        for (Element? e = element; e is not null; e = e.Parent)
            if (IsHidden(e)) return false;
        return true;
    }

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    /// <inheritdoc/>
    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (child is not View view) return;
        view.HandlerChanged += OnChildHandlerChanged;
        ApplyHidden(view);
    }

    /// <inheritdoc/>
    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        if (child is not View view) return;
        view.HandlerChanged -= OnChildHandlerChanged;
        // Shown wherever it goes next.
        SetPlatformHidden(view, false);
        if (ReferenceEquals(Active, view)) Active = null;
    }

    void OnActiveChanged(View? old, View? next)
    {
        if (old is not null && ReferenceEquals(old.Parent, this)) SetPlatformHidden(old, true);
        if (next is not null)
        {
            if (!ReferenceEquals(next.Parent, this)) throw new ArgumentException("The active view must be a child of the deck.", nameof(Active));
            SetPlatformHidden(next, false);
            // It was skipped by layout while inactive: measure it at the deck's current size.
            ((IView)next).InvalidateMeasure();
        }
        ((IView)this).InvalidateMeasure();
    }

    // MAUI maps visibility when a handler connects, which would show an inactive child again.
    void OnChildHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is View view) ApplyHidden(view);
    }

    void ApplyHidden(View view) => SetPlatformHidden(view, !ReferenceEquals(Active, view));

    static void SetPlatformHidden(View view, bool hidden)
    {
#if IOS || MACCATALYST
        if (view.Handler is IPlatformViewHandler h && (h.ContainerView ?? h.PlatformView) is { } native)
        {
            native.Hidden = hidden;
            native.AccessibilityElementsHidden = hidden;
        }
#elif ANDROID
        if (view.Handler is IPlatformViewHandler h && (h.ContainerView ?? h.PlatformView) is { } native)
        {
            native.Visibility = hidden ? Android.Views.ViewStates.Invisible : Android.Views.ViewStates.Visible;
            native.ImportantForAccessibility = hidden ? Android.Views.ImportantForAccessibility.NoHideDescendants : Android.Views.ImportantForAccessibility.Auto;
        }
#elif WINDOWS
        if (view.Handler is IPlatformViewHandler h && (h.ContainerView ?? h.PlatformView) is { } native)
            native.Visibility = hidden ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
#endif
    }

    sealed class Manager(CdsDeck deck) : LayoutManager(deck)
    {
        IView? Shown => deck.Active is IView { Visibility: not Microsoft.Maui.Visibility.Collapsed } active && ReferenceEquals(deck.Active.Parent, deck) ? active : null;

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var padding = deck.Padding;
            if (Shown is not { } child) return new Size(padding.HorizontalThickness, padding.VerticalThickness);
            var size = child.Measure(widthConstraint - padding.HorizontalThickness, heightConstraint - padding.VerticalThickness);
            return new Size(size.Width + padding.HorizontalThickness, size.Height + padding.VerticalThickness);
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            Shown?.Arrange(bounds.Inset(deck.Padding));
            return bounds.Size;
        }
    }
}

static file class RectExtensions
{
    public static Rect Inset(this Rect r, Thickness t) =>
        new(r.X + t.Left, r.Y + t.Top, Math.Max(0, r.Width - t.HorizontalThickness), Math.Max(0, r.Height - t.VerticalThickness));
}
