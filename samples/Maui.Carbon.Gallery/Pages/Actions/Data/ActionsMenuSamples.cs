namespace Maui.Carbon.Gallery.Pages.Actions.Data;

/// <summary>Which sample content a <see cref="MenuPreviewView"/> shows.</summary>
public enum ActionsMenuSample
{
    /// <summary>Icons, shortcuts, a divider and a danger item.</summary>
    Actions,
    /// <summary>Selectable items and a radio group.</summary>
    Selectable,
    /// <summary>An item with a submenu.</summary>
    Submenu,
    /// <summary>Disabled items among enabled ones.</summary>
    Disabled,
    /// <summary>Plain text items.</summary>
    Plain,
}

/// <summary>Builds the sample menus the Actions gallery pages show.</summary>
public static class ActionsMenuSamples
{
    /// <summary>Creates a menu with the sample content.</summary>
    public static CdsMenu Create(ActionsMenuSample sample, CdsSize size = CdsSize.Sm)
    {
        var menu = new CdsMenu { Label = "Object actions", Size = size };
        switch (sample)
        {
            case ActionsMenuSample.Actions:
                menu.Items.Add(new CdsMenuItem("Download", CdsIconName.Download) { Shortcut = "Ctrl+D" });
                menu.Items.Add(new CdsMenuItem("Share link", CdsIconName.Share));
                menu.Items.Add(new CdsMenuItem("Edit tags", CdsIconName.Edit) { Shortcut = "Ctrl+E" });
                menu.Items.Add(new CdsMenuItemDivider());
                menu.Items.Add(new CdsMenuItem("Delete object", CdsIconName.TrashCan) { Kind = CdsMenuItemKind.Danger });
                break;
            case ActionsMenuSample.Selectable:
                menu.Items.Add(new CdsMenuItemSelectable { Text = "Show hidden files", IsSelected = true });
                menu.Items.Add(new CdsMenuItemSelectable { Text = "Show file sizes" });
                menu.Items.Add(new CdsMenuItemDivider());
                var sort = new CdsMenuItemRadioGroup { Label = "Sort by", SelectedItem = "Last modified" };
                sort.Items.Add("Name");
                sort.Items.Add("Last modified");
                sort.Items.Add("Size");
                menu.Items.Add(sort);
                break;
            case ActionsMenuSample.Submenu:
                menu.Items.Add(new CdsMenuItem("Open"));
                var share = new CdsMenuItem("Share");
                share.Items.Add(new CdsMenuItem("Copy link"));
                share.Items.Add(new CdsMenuItem("Invite people"));
                share.Items.Add(new CdsMenuItem("Email a copy"));
                menu.Items.Add(share);
                menu.Items.Add(new CdsMenuItem("Rename"));
                break;
            case ActionsMenuSample.Disabled:
                menu.Items.Add(new CdsMenuItem("Restart cluster"));
                menu.Items.Add(new CdsMenuItem("Scale worker pool") { IsEnabled = false });
                menu.Items.Add(new CdsMenuItem("Upgrade version") { IsEnabled = false });
                menu.Items.Add(new CdsMenuItemDivider());
                menu.Items.Add(new CdsMenuItem("Delete cluster") { Kind = CdsMenuItemKind.Danger, IsEnabled = false });
                break;
            default:
                menu.Items.Add(new CdsMenuItem("Restart cluster"));
                menu.Items.Add(new CdsMenuItem("Scale worker pool"));
                menu.Items.Add(new CdsMenuItemDivider());
                menu.Items.Add(new CdsMenuItem("Delete cluster") { Kind = CdsMenuItemKind.Danger });
                break;
        }
        return menu;
    }
}

/// <summary>Shows a menu surface inline (no overlay), so a gallery page can display a menu without interaction.</summary>
public class MenuPreviewView : ContentView
{
    /// <summary>Bindable property for <see cref="Sample"/>.</summary>
    public static readonly BindableProperty SampleProperty = BindableProperty.Create(
        nameof(Sample), typeof(ActionsMenuSample), typeof(MenuPreviewView), ActionsMenuSample.Actions,
        propertyChanged: static (b, _, _) => ((MenuPreviewView)b).Build());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(MenuPreviewView), CdsSize.Sm,
        propertyChanged: static (b, _, _) => ((MenuPreviewView)b).Build());

    /// <summary>Bindable property for <see cref="Highlight"/>.</summary>
    public static readonly BindableProperty HighlightProperty = BindableProperty.Create(
        nameof(Highlight), typeof(int), typeof(MenuPreviewView), -1,
        propertyChanged: static (b, _, _) => ((MenuPreviewView)b).Build());

    /// <summary>Creates the preview.</summary>
    public MenuPreviewView()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        Padding = new Thickness(0, 0, 8, 8);
        Build();
    }

    /// <summary>The sample content.</summary>
    public ActionsMenuSample Sample
    {
        get => (ActionsMenuSample)GetValue(SampleProperty);
        set => SetValue(SampleProperty, value);
    }

    /// <summary>The menu size.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Index of the highlighted row (counting dividers), or -1.</summary>
    public int Highlight
    {
        get => (int)GetValue(HighlightProperty);
        set => SetValue(HighlightProperty, value);
    }

    void Build() => Content = ActionsMenuSamples.Create(Sample, Size).CreatePreview(Highlight);
}
