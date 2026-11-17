using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Form", GalleryCategory.Forms, Order = 10, Spec = "Form",
    Summary = "The structural pieces of a Carbon form: Form, FormGroup, FormItem and FormLabel.")]
public partial class FormPage : GalleryPage
{
    public FormPage()
    {
        InitializeComponent();
        ReplicaLabel.For = ReplicaInput;
        ReplicaLabel.Body = new CdsIcon(CdsIconName.Information, 16);
    }

    void OnCreate(object? sender, EventArgs e)
    {
        if (CreateButton.Parent?.Parent is CdsForm form) form.Submit();
        Status.Text = "Submitted: cluster created";
    }
}
