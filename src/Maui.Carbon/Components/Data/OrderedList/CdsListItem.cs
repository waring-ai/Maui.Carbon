namespace Maui.Carbon;

/// <summary>
/// One item of a <see cref="CdsOrderedList"/> or <see cref="CdsUnorderedList"/>: its <see cref="Text"/> (wraps under the
/// text, never under the marker), optional <see cref="Body"/> content, and an optional <see cref="NestedList"/> shown below
/// the text. The list draws the marker. Keep items short and grammatically parallel.
/// </summary>
[ContentProperty(nameof(Text))]
public class CdsListItem : ContentView
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsListItem), null,
        propertyChanged: static (b, _, _) => ((CdsListItem)b).Refresh());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsListItem), null,
        propertyChanged: static (b, _, _) => ((CdsListItem)b).Rebuild());

    /// <summary>Bindable property for <see cref="NestedList"/>.</summary>
    public static readonly BindableProperty NestedListProperty = BindableProperty.Create(
        nameof(NestedList), typeof(CdsList), typeof(CdsListItem), null,
        propertyChanged: static (b, _, _) => ((CdsListItem)b).Rebuild());

    readonly Grid _row = new() { ColumnSpacing = 0 };
    readonly CdsText _marker = new() { VerticalOptions = LayoutOptions.Start, LineBreakMode = LineBreakMode.NoWrap };
    readonly CdsText _text = new() { VerticalOptions = LayoutOptions.Start };
    readonly VerticalStackLayout _content = new() { Spacing = 0 };
    bool _expressive;
    int _depth;

    /// <summary>Creates an empty item.</summary>
    public CdsListItem()
    {
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _row.Add(_marker, 0);
        _row.Add(_content, 1);
        AutomationProperties.SetIsInAccessibleTree(_marker, false);
        Content = _row;
        Rebuild();
    }

    /// <summary>Creates an item with text.</summary>
    public CdsListItem(string text) : this()
    {
        Text = text;
    }

    /// <summary>The item text.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Extra content shown under the text (any view).</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>A nested list shown under the text, indented 32. Go no deeper than two levels.</summary>
    public CdsList? NestedList
    {
        get => (CdsList?)GetValue(NestedListProperty);
        set => SetValue(NestedListProperty, value);
    }

    internal void Configure(string marker, double markerWidth, bool expressive, int depth, bool announceMarker, bool markerAtEnd = false)
    {
        _expressive = expressive;
        _depth = depth;
        _marker.Text = marker;
        // Native-style markers end a small gap (the marker's trailing space) before the text.
        _marker.HorizontalTextAlignment = markerAtEnd ? TextAlignment.End : TextAlignment.Start;
        _marker.Padding = markerAtEnd ? new Thickness(0, 0, CdsSpacing.Spacing02, 0) : new Thickness(0);
        _row.ColumnDefinitions[0].Width = new GridLength(markerWidth);
        Refresh();
        if (announceMarker && !string.IsNullOrEmpty(Text)) SemanticProperties.SetDescription(_text, $"{marker} {Text}");
        else _text.ClearValue(SemanticProperties.DescriptionProperty);
    }

    void Refresh()
    {
        var style = CdsListMarkers.TypeStyle(_expressive);
        _marker.TypeStyle = style;
        _text.TypeStyle = style;
        _text.Text = Text;
        _text.IsVisible = !string.IsNullOrEmpty(Text);
        if (NestedList is { } nested) nested.Attach(_depth + 1, _expressive);
    }

    void Rebuild()
    {
        _content.Children.Clear();
        _content.Add(_text);
        if (Body is { } body) _content.Add(body);
        if (NestedList is { } nested) _content.Add(nested);
        Refresh();
    }
}
