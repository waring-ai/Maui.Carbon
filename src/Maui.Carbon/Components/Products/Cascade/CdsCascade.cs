using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Staggered entrance of its <see cref="Items"/> (Carbon for IBM Products' <c>Cascade</c>): when it first appears, each
/// item fades in and moves up from 48 below, 60 ms after the one before it (the eighth and later start together). Use it
/// for cards, tiles and similar blocks in the area that is the main focus of a page, to lead the eye through the most
/// important information; never on secondary content, where the motion distracts. Items stack vertically
/// (<see cref="Spacing"/> apart), or with <see cref="IsGrid"/> they are placed in a <see cref="CdsGrid"/> and take its
/// attached spans. When the system asks for reduced motion the items simply show.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsCascade : ContentView
{
    /// <summary>Bindable property for <see cref="IsGrid"/>.</summary>
    public static readonly BindableProperty IsGridProperty = BindableProperty.Create(
        nameof(IsGrid), typeof(bool), typeof(CdsCascade), false,
        propertyChanged: static (b, _, _) => ((CdsCascade)b).Rehost());

    /// <summary>Bindable property for <see cref="Spacing"/>.</summary>
    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing), typeof(double), typeof(CdsCascade), 0d,
        propertyChanged: static (b, _, n) => ((CdsCascade)b)._stack.Spacing = (double)n);

    const string AnimationName = "CdsCascade";

    readonly CdsItemCollection<View> _items = [];
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    CdsGrid? _grid;
    bool _settingContent;
    bool _played;
    int _generation;

    /// <summary>Creates an empty cascade.</summary>
    public CdsCascade()
    {
        _items.CollectionChanged += OnItemsChanged;
        SetHost(_stack);
        Loaded += (_, _) =>
        {
            if (_played) return;
            _played = true;
            Play();
        };
    }

    /// <summary>The items that enter one after another, in order.</summary>
    public IList<View> Items => _items;

    /// <summary>
    /// Place the items in a <see cref="CdsGrid"/> (Carbon's <c>grid</c>): give them <c>CdsGrid.Span</c> attached values (or
    /// use <see cref="CdsColumn"/>s) and they wrap into rows; they still enter in order.
    /// </summary>
    public bool IsGrid
    {
        get => (bool)GetValue(IsGridProperty);
        set => SetValue(IsGridProperty, value);
    }

    /// <summary>Space between stacked items (not used with <see cref="IsGrid"/>). Default 0, as Carbon's block flow.</summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>The grid the items sit in while <see cref="IsGrid"/> (set its mode or breakpoint here), else <c>null</c>.</summary>
    public CdsGrid? Grid => IsGrid ? _grid : null;

    /// <summary>Whether the entrance has started (it runs once, the first time the cascade is loaded, and on <see cref="Replay"/>).</summary>
    public bool HasPlayed => _played;

    /// <summary>Runs the entrance again from the start (after new content arrived, say). Does nothing under reduced motion.</summary>
    public void Replay()
    {
        _played = true;
        Play();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == ContentProperty.PropertyName && !_settingContent)
            throw new InvalidOperationException($"{nameof(CdsCascade)} lays out its own content. Put views in {nameof(Items)} instead of Content.");
    }

    void SetHost(View host)
    {
        _settingContent = true;
        try
        {
            Content = host;
        }
        finally
        {
            _settingContent = false;
        }
    }

    Layout Host => IsGrid ? (_grid ??= new CdsGrid()) : _stack;

    void Rehost()
    {
        var from = IsGrid ? (Layout)_stack : _grid;
        from?.Clear();
        var to = Host;
        to.Clear();
        foreach (var item in _items) to.Add(item);
        SetHost(to);
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var host = Host;
        if (e.OldItems is not null)
        {
            foreach (View item in e.OldItems)
            {
                host.Remove(item);
                item.AbortAnimation(AnimationName);
                item.Opacity = 1;
                item.TranslationY = 0;
            }
        }
        if (e.NewItems is not null)
        {
            var index = e.NewStartingIndex < 0 ? _items.Count - e.NewItems.Count : e.NewStartingIndex;
            foreach (View item in e.NewItems)
            {
                if (index <= host.Count) host.Insert(index, item);
                else host.Add(item);
                // Hidden from the start (no flash before the first frame), unless the entrance already ran and the item
                // arrives later without the cascade on screen.
                if (!CdsMotion.IsReducedMotion && (!_played || IsLoaded))
                {
                    Prepare(item);
                    if (_played && IsLoaded) Schedule(item, index, _generation);
                }
                index++;
            }
        }
    }

    static void Prepare(View item)
    {
        var (opacity, offset) = CascadeModel.At(0);
        item.AbortAnimation(AnimationName);
        item.Opacity = opacity;
        item.TranslationY = offset;
    }

    void Play()
    {
        var generation = ++_generation;
        if (CdsMotion.IsReducedMotion)
        {
            foreach (var item in _items)
            {
                item.AbortAnimation(AnimationName);
                item.Opacity = 1;
                item.TranslationY = 0;
            }
            return;
        }
        for (var i = 0; i < _items.Count; i++)
        {
            Prepare(_items[i]);
            Schedule(_items[i], i, generation);
        }
    }

    void Schedule(View item, int index, int generation)
    {
        var delay = TimeSpan.FromMilliseconds(CascadeModel.Delay(index));
        if (Handler is null || Dispatcher is not { } dispatcher)
        {
            Show(item);
            return;
        }
        dispatcher.DispatchDelayed(delay, () =>
        {
            if (generation != _generation || !ReferenceEquals(item.Parent, Host)) return;
            _ = CdsMotion.AnimateAsync(item, AnimationName, p =>
            {
                var (opacity, offset) = CascadeModel.At(p);
                item.Opacity = opacity;
                item.TranslationY = offset;
            }, 0, 1, CascadeModel.Duration, CdsEasing.StandardProductive);
        });
    }

    static void Show(View item)
    {
        item.Opacity = 1;
        item.TranslationY = 0;
    }
}
