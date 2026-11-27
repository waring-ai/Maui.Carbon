using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Create tearsheet", GalleryCategory.Products, Order = 40, Spec = "CreateTearsheet",
    Summary = "Multi-step and one-step create flows in a tearsheet.")]
public partial class CreateTearsheetPage : GalleryPage
{
    public CreateTearsheetPage()
    {
        InitializeComponent();
        foreach (var (text, value) in new[] { ("1 day", "1d"), ("7 days", "7d"), ("30 days", "30d") })
            Retention.Items.Add(new CdsRadioButton { Label = text, Value = value });
        NameStep.NextAsync = () =>
        {
            var valid = !string.IsNullOrWhiteSpace(TopicName.Text);
            TopicName.IsInvalid = !valid;
            TopicName.InvalidText = valid ? null : "Enter a topic name.";
            return Task.FromResult(valid);
        };
        Wide.SubmitAsync = async () =>
        {
            await Task.Delay(600);
            return true;
        };
    }

    void OnWideClicked(object? sender, EventArgs e) => Wide.IsOpen = true;

    void OnSubmitted(object? sender, EventArgs e) => Status.Text = $"Topic {TopicName.Text} created";

    void OnNarrowClicked(object? sender, EventArgs e) => Narrow.IsOpen = true;

    void OnNarrowSubmit(object? sender, EventArgs e)
    {
        Narrow.IsOpen = false;
        Status.Text = "Partition created";
    }
}
