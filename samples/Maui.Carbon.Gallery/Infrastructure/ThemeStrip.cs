namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>
/// Renders <see cref="Template"/> four times side by side, once in each Carbon theme zone, so one screenshot shows
/// every theme. Set <see cref="Layered"/> to also wrap each copy in a <see cref="CdsLayer"/>.
/// </summary>
public class ThemeStrip : ContentView
{
    public static readonly BindableProperty TemplateProperty = BindableProperty.Create(
        nameof(Template), typeof(DataTemplate), typeof(ThemeStrip), null,
        propertyChanged: static (b, _, _) => ((ThemeStrip)b).Build());

    public static readonly BindableProperty LayeredProperty = BindableProperty.Create(
        nameof(Layered), typeof(bool), typeof(ThemeStrip), false,
        propertyChanged: static (b, _, _) => ((ThemeStrip)b).Build());

    public DataTemplate? Template
    {
        get => (DataTemplate?)GetValue(TemplateProperty);
        set => SetValue(TemplateProperty, value);
    }

    public bool Layered
    {
        get => (bool)GetValue(LayeredProperty);
        set => SetValue(LayeredProperty, value);
    }

    void Build()
    {
        if (Template is null)
        {
            Content = null;
            return;
        }
        var grid = new Grid { ColumnSpacing = 0 };
        var themes = Enum.GetValues<CdsThemeName>();
        for (var i = 0; i < themes.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var body = (View)Template.CreateContent();
            var column = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05, Padding = new Thickness(CdsSpacing.Spacing05) };
            column.Add(new CdsText(themes[i] switch
            {
                CdsThemeName.White => "White",
                CdsThemeName.G10 => "Gray 10",
                CdsThemeName.G90 => "Gray 90",
                _ => "Gray 100",
            }, CdsTypeStyle.Label01, CdsTextTone.Secondary));
            column.Add(Layered ? new CdsLayer { WithBackground = true, Body = new ContentView { Padding = CdsSpacing.Spacing05, Content = body } } : body);
            var zone = new CdsTheme { Theme = themes[i], Body = column };
            grid.Add(zone, i);
        }
        Content = grid;
    }
}
