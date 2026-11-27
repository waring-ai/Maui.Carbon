using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("User avatar", GalleryCategory.Products, Order = 140, Spec = "UserAvatar",
    Summary = "A circle that stands for a person: their initials, a photo, or an icon, on one of twelve background colors, with an optional tooltip.")]
public partial class UserAvatarPage : GalleryPage
{
    public UserAvatarPage()
    {
        InitializeComponent();
    }

    void OnNameChanged(object? sender, TextChangedEventArgs e)
    {
        Live.Name = e.NewTextValue;
        Status.Text = Live.CurrentInitials is { } initials ? $"Initials: {initials}" : "No name: showing the user icon";
    }

    void OnNextColorClicked(object? sender, EventArgs e)
    {
        var count = Enum.GetValues<CdsUserAvatarColor>().Length;
        Live.AvatarColor = (CdsUserAvatarColor)(((int)Live.AvatarColor + 1) % count);
        Status.Text = $"Color: {Live.AvatarColor}";
    }
}
