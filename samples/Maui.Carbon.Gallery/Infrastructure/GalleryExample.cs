namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>A titled example on a gallery page: heading, optional description, then the live content.</summary>
[ContentProperty(nameof(Body))]
public class GalleryExample : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(GalleryExample), null,
        propertyChanged: static (b, _, n) => ((GalleryExample)b)._title.Text = (string?)n);

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(GalleryExample), null,
        propertyChanged: static (b, _, n) =>
        {
            var e = (GalleryExample)b;
            e._description.Text = (string?)n;
            e._description.IsVisible = !string.IsNullOrEmpty((string?)n);
        });

    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(GalleryExample), null,
        propertyChanged: static (b, _, n) => ((GalleryExample)b)._stage.Content = (View?)n);

    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Heading03 };
    readonly CdsText _description = new() { TypeStyle = CdsTypeStyle.Body01, Tone = CdsTextTone.Secondary, IsVisible = false, MaximumWidthRequest = 720, HorizontalOptions = LayoutOptions.Start };
    readonly ContentView _stage = new() { Padding = new Thickness(0, CdsSpacing.Spacing03, 0, 0) };

    public GalleryExample()
    {
        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03 };
        stack.Add(_title);
        stack.Add(_description);
        stack.Add(_stage);
        Content = stack;
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }
}
