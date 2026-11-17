namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>
/// Base page for gallery pages: a title and summary from <see cref="GalleryPageAttribute"/>, then <see cref="Body"/>
/// in a scrolling column. Put <see cref="GalleryExample"/>s in the body.
/// </summary>
[ContentProperty(nameof(Body))]
public class GalleryPage : ContentPage
{
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(GalleryPage), null,
        propertyChanged: static (b, _, n) => ((GalleryPage)b)._body.Content = (View?)n);

    readonly ContentView _body = new();

    public GalleryPage()
    {
        var info = GalleryRegistry.InfoFor(GetType());
        Title = info?.Title ?? GetType().Name;

        var header = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03 };
        header.Add(new CdsText(info?.Category.ToString() ?? "", CdsTypeStyle.Label01, CdsTextTone.Secondary));
        header.Add(new CdsText(Title, CdsTypeStyle.Heading05));
        if (info?.Summary is { } summary)
            header.Add(new CdsText(summary, CdsTypeStyle.Body02, CdsTextTone.Secondary) { MaximumWidthRequest = 720, HorizontalOptions = LayoutOptions.Start });

        var column = new VerticalStackLayout
        {
            Spacing = CdsSpacing.Spacing09,
            Padding = new Thickness(CdsSpacing.Spacing07, CdsSpacing.Spacing07, CdsSpacing.Spacing07, CdsSpacing.Spacing10),
        };
        column.Add(header);
        column.Add(_body);
        Content = new ScrollView { Content = column };
    }

    /// <summary>The page content.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }
}
