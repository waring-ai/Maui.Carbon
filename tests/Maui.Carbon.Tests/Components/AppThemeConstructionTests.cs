namespace Maui.Carbon.Tests.Components;

/// <summary>
/// Controls built while an app-level theme is already in place: CdsControl's constructor resolves the theme context at once,
/// before a tile variant has built its parts (the Relay exercise caught a NullReferenceException here in G100).
/// Sets Application.Current, so it runs in the non-parallel collection.
/// </summary>
[Collection(FocusTrackerCollection.Name)]
public class AppThemeConstructionTests
{
    [Theory]
    [InlineData(CdsThemeName.White)]
    [InlineData(CdsThemeName.G10)]
    [InlineData(CdsThemeName.G90)]
    [InlineData(CdsThemeName.G100)]
    public void Every_control_can_be_built_under_an_app_theme(CdsThemeName theme)
    {
        var types = typeof(CdsControl).Assembly.GetExportedTypes()
            .Where(t => typeof(View).IsAssignableFrom(t) && !t.IsAbstract && !t.ContainsGenericParameters && t.GetConstructor(Type.EmptyTypes) is not null)
            .ToList();
        Assert.NotEmpty(types);
        var previous = Application.Current;
        Application.Current = new Application { Resources = CdsPalettes.CreateThemeDictionary(theme) };
        var failures = new List<string>();
        try
        {
            foreach (var type in types)
            {
                try
                {
                    var view = (View)Activator.CreateInstance(type)!;
                    view.IsEnabled = false;
                    view.IsEnabled = true;
                }
                catch (Exception e)
                {
                    failures.Add($"{type.Name}: {(e.InnerException ?? e).GetType().Name} {(e.InnerException ?? e).Message}");
                }
            }
        }
        finally
        {
            Application.Current = previous;
        }
        Assert.Empty(failures);
    }

    [Theory]
    [InlineData(CdsThemeName.G100)]
    [InlineData(CdsThemeName.G10)]
    public void Tiles_can_be_built_under_an_app_theme(CdsThemeName theme)
    {
        var previous = Application.Current;
        Application.Current = new Application { Resources = CdsPalettes.CreateThemeDictionary(theme) };
        try
        {
            var clickable = new CdsClickableTile { Body = new CdsText { Text = "Open" } };
            var radio = new CdsRadioTile { Body = new CdsText { Text = "Small" } };
            var selectable = new CdsSelectableTile { Body = new CdsText { Text = "Pick" } };
            var tile = new CdsTile { Body = new CdsText { Text = "Plain" } };
            radio.IsEnabled = false;
            Assert.False(radio.IsEnabled);
            Assert.NotNull(clickable.Content);
            Assert.NotNull(selectable.Content);
            Assert.NotNull(tile.Content);
        }
        finally
        {
            Application.Current = previous;
        }
    }
}
