using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Layouts;
using GSize = Microsoft.Maui.Graphics.Size;

namespace Maui.Carbon;

/// <summary>
/// The row of full-height buttons that closes a side panel, tearsheet or full-page flow (Carbon for IBM Products'
/// <c>ActionSet</c>). Add up to four <see cref="CdsButton"/>s in any order: the set sorts them by kind (ghost at the
/// start, then tertiary, secondary, danger, primary at the end), sizes them for <see cref="Size"/> and stacks them,
/// primary on top, in narrow containers. Use one primary and at most one ghost; a stacked set holds three buttons and no
/// ghost beside others. For a row of ordinary buttons inside content use <see cref="CdsButtonSet"/>.
/// </summary>
[ContentProperty(nameof(Actions))]
public class CdsActionSet : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsActionSetSize), typeof(CdsActionSet), CdsActionSetSize.Md,
        propertyChanged: static (b, _, _) => ((CdsActionSet)b).Rebuild());

    /// <summary>Bindable property for <see cref="ButtonSize"/>.</summary>
    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        nameof(ButtonSize), typeof(CdsSize), typeof(CdsActionSet), CdsSize.Xl,
        propertyChanged: static (b, _, _) => ((CdsActionSet)b).Rebuild());

    /// <summary>Bindable property for <see cref="DisableStacking"/>.</summary>
    public static readonly BindableProperty DisableStackingProperty = BindableProperty.Create(
        nameof(DisableStacking), typeof(bool), typeof(CdsActionSet), false,
        propertyChanged: static (b, _, _) => ((CdsActionSet)b).Rebuild());

    readonly ActionSetLayout _layout;
    readonly CdsItemCollection<CdsButton> _actions = [];
    readonly CdsCoalescedAction _rebuild;
    CdsButton[] _shown = [];

    /// <summary>Creates an empty medium action set.</summary>
    public CdsActionSet()
    {
        _layout = new ActionSetLayout(this);
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        _actions.CollectionChanged += OnActionsChanged;
        SetChrome(_layout, fillsWidth: true);
        RefreshVisuals();
    }

    /// <summary>
    /// The buttons, in any order (the set shows them sorted by kind). Their <see cref="CdsButton.Size"/> and alignment
    /// are set by the set; their kind, text, icon, command and <c>IsEnabled</c> are yours.
    /// </summary>
    public IList<CdsButton> Actions => _actions;

    /// <summary>The width class of the container the set closes. Default <see cref="CdsActionSetSize.Md"/>.</summary>
    public CdsActionSetSize Size
    {
        get => (CdsActionSetSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The height of each button: <see cref="CdsSize.Xl"/> (64, the default) or <see cref="CdsSize.Lg"/> (48) for condensed sets.</summary>
    public CdsSize ButtonSize
    {
        get => (CdsSize)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    /// <summary>Keep the buttons in a row even where the set would stack them.</summary>
    public bool DisableStacking
    {
        get => (bool)GetValue(DisableStackingProperty);
        set => SetValue(DisableStackingProperty, value);
    }

    /// <summary>The fill behind the buttons (<c>layer-01</c>; a wide tearsheet uses <c>background</c>).</summary>
    internal string FillKey
    {
        get => _fillKey;
        set
        {
            _fillKey = value;
            RefreshVisuals();
        }
    }

    string _fillKey = CdsColorKeys.Layer01;

    /// <summary>True when the buttons are stacked (primary on top).</summary>
    public bool IsStacking => !DisableStacking && ActionSetModel.WillStack(Size, _actions.Count);

    /// <summary>The buttons in the order they are shown (start to end, or top to bottom when stacked).</summary>
    internal IReadOnlyList<CdsButton> Shown => _shown;

    /// <inheritdoc/>
    protected override void ApplyVisuals() => SetColor(_layout, BackgroundColorProperty, _fillKey);

    void OnActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsButton old in e.OldItems) old.PropertyChanged -= OnActionPropertyChanged;
        if (e.NewItems is not null)
            foreach (CdsButton added in e.NewItems) added.PropertyChanged += OnActionPropertyChanged;
        _rebuild.Request();
    }

    void OnActionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == CdsButton.KindProperty.PropertyName) _rebuild.Request();
    }

    void Rebuild()
    {
        var kinds = _actions.Select(a => a.Kind).ToArray();
        foreach (var problem in ActionSetModel.Problems(kinds, Size))
            System.Diagnostics.Debug.WriteLine($"CdsActionSet: {problem}.");
        var order = ActionSetModel.Arrange(kinds, IsStacking);
        _shown = [.. order.Select(i => _actions[i])];
        foreach (var button in _shown)
        {
            button.Size = ButtonSize;
            button.HorizontalOptions = LayoutOptions.Fill;
        }
        // Keep the buttons already in place (each re-add costs a native view) and only move what changed order.
        for (var i = _layout.Children.Count - 1; i >= 0; i--)
            if (_layout.Children[i] is CdsButton b && Array.IndexOf(_shown, b) < 0) _layout.Children.RemoveAt(i);
        for (var i = 0; i < _shown.Length; i++)
        {
            var at = _layout.Children.IndexOf(_shown[i]);
            if (at == i) continue;
            if (at >= 0) _layout.Children.RemoveAt(at);
            _layout.Children.Insert(i, _shown[i]);
        }
        _layout.IsVisible = _shown.Length > 0;
        _layout.InvalidateMeasure();
    }

    sealed class ActionSetLayout(CdsActionSet owner) : Layout
    {
        protected override ILayoutManager CreateLayoutManager() => new Manager(owner);

        sealed class Manager(CdsActionSet owner) : ILayoutManager
        {
            public GSize Measure(double widthConstraint, double heightConstraint)
            {
                var buttons = owner._shown;
                if (buttons.Length == 0) return GSize.Zero;
                var width = double.IsFinite(widthConstraint) ? widthConstraint : buttons.Length * ActionSetModel.MaxQuarterWidth;
                if (owner.IsStacking)
                {
                    double total = 0;
                    foreach (var b in buttons) total += b.Measure(width, double.PositiveInfinity).Height;
                    return new GSize(width, total);
                }
                var widths = ActionSetModel.RowWidths([.. buttons.Select(b => b.Kind)], owner.Size, width);
                double height = 0;
                for (var i = 0; i < buttons.Length; i++)
                    height = Math.Max(height, buttons[i].Measure(widths[i], double.PositiveInfinity).Height);
                return new GSize(width, height);
            }

            public GSize ArrangeChildren(Rect bounds)
            {
                var buttons = owner._shown;
                if (owner.IsStacking)
                {
                    var y = bounds.Top;
                    foreach (var b in buttons)
                    {
                        var h = b.DesiredSize.Height;
                        ((IView)b).Arrange(new Rect(bounds.Left, y, bounds.Width, h));
                        y += h;
                    }
                    return bounds.Size;
                }
                var widths = ActionSetModel.RowWidths([.. buttons.Select(b => b.Kind)], owner.Size, bounds.Width);
                var x = bounds.Right - widths.Sum();
                for (var i = 0; i < buttons.Length; i++)
                {
                    ((IView)buttons[i]).Arrange(new Rect(x, bounds.Top, widths[i], bounds.Height));
                    x += widths[i];
                }
                return bounds.Size;
            }
        }
    }
}
