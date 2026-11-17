using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.AI;

[GalleryPage("Chat button", GalleryCategory.AI, Order = 30, Spec = "ChatButton",
    Summary = "A rounded (pill) button for chat and conversational UIs, plus the outlined quick action variant for suggested prompts.")]
public partial class ChatButtonPage : GalleryPage
{
    public ChatButtonPage()
    {
        InitializeComponent();
    }

    void OnPromptClicked(object? sender, EventArgs e)
    {
        if (sender is not CdsChatButton clicked) return;
        foreach (var prompt in Prompts.Children.OfType<CdsChatButton>())
            prompt.IsSelected = ReferenceEquals(prompt, clicked);
        Status.Text = $"Selected prompt: {clicked.Text}";
    }
}
