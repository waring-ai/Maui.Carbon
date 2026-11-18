using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The 48 high application header: <c>background</c> with a 1px <c>border-subtle-00</c> bottom border (it follows the theme; there is no
/// inverse bar). Add, in order, an optional <see cref="CdsSkipToContent"/>, a <see cref="CdsHeaderMenuButton"/>, the
/// <see cref="CdsHeaderName"/>, a <see cref="CdsHeaderNavigation"/> and a <see cref="CdsHeaderGlobalBar"/> (which takes the
/// remaining width, right-aligned). Put it in <see cref="CdsUIShell.Header"/>, or on its own at the top of a page. A banner
/// landmark: give it a <see cref="Label"/> ("Platform").
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsHeader : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsHeader), null,
        propertyChanged: static (b, _, n) => SemanticProperties.SetDescription((CdsHeader)b, (string?)n));

    readonly CdsItemCollection<View> _items = [];
    readonly Grid _row = new() { ColumnSpacing = 0 };
    readonly Grid _layers = new() { HeightRequest = UIShellModel.HeaderHeight };
    readonly CdsBreakpointWatcher _watcher;
    double _measuredFor = -1;

    /// <summary>Creates an empty header.</summary>
    public CdsHeader()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        var fill = new BoxView { InputTransparent = true };
        CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsUIShellTokens.Surface);
        var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsUIShellTokens.Divider);
        _layers.Add(fill);
        _layers.Add(_row);
        _layers.Add(rule);
        SetChrome(_layers);
        _items.CollectionChanged += (_, _) => Rebuild();
        _watcher = new CdsBreakpointWatcher(this, Sync);
        // While a window is resized live, MAUI can arrange the row at the new width from a measure taken at an earlier
        // one: the star column then holds the global bar at its old, narrow measure, right-aligned, with its actions past
        // the window edge. A new width re-measures the row.
        SizeChanged += (_, _) =>
        {
            if (Math.Abs(Width - _measuredFor) < 0.5) return;
            _measuredFor = Width;
            ((IView)_row).InvalidateMeasure();
        };
    }

    /// <summary>The accessible name of the banner landmark.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The header parts, in visual order.</summary>
    public IList<View> Items => _items;

    void Rebuild()
    {
        foreach (var view in _row.Children.OfType<View>().ToArray()) _row.Remove(view);
        _row.ColumnDefinitions.Clear();
        foreach (var v in _layers.Children.OfType<CdsSkipToContent>().ToArray()) _layers.Remove(v);

        var column = 0;
        foreach (var item in _items)
        {
            if (item is CdsSkipToContent skip)
            {
                _layers.Add(skip);
                continue;
            }
            var grow = item is CdsHeaderGlobalBar;
            _row.ColumnDefinitions.Add(new ColumnDefinition(grow ? GridLength.Star : GridLength.Auto));
            _row.Add(item, column++);
        }
        if (!_row.ColumnDefinitions.Any(c => c.Width.IsStar)) _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        Sync();
    }

    void Sync()
    {
        if (_watcher is null) return;
        // The name sits 8 (not 16) from a visible menu button.
        var afterMenuButton = false;
        foreach (var item in _items)
        {
            switch (item)
            {
                case CdsHeaderMenuButton button:
                    afterMenuButton = button.IsShown;
                    break;
                case CdsHeaderName name:
                    name.SetAfterMenuButton(afterMenuButton);
                    afterMenuButton = false;
                    break;
            }
        }
    }
}
