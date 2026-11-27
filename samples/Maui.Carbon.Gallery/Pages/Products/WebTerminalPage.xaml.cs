using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Web terminal", GalleryCategory.Products, Order = 60, Spec = "WebTerminal",
    Summary = "A dark terminal panel docked at the end edge, with documentation links and actions.")]
public partial class WebTerminalPage : GalleryPage
{
    public WebTerminalPage()
    {
        InitializeComponent();
        Terminal.PageContent = PageArea;
        Output.Text = string.Join("\n",
            "$ kubectl get pods -n checkout",
            "NAME                            READY   STATUS    RESTARTS   AGE",
            "checkout-api-7d9f8c6b5d-2xk9p   1/1     Running   0          3h",
            "checkout-api-7d9f8c6b5d-8qlz4   1/1     Running   0          3h",
            "checkout-worker-5c7d8f9-mw2rt   1/1     Running   1          3h",
            "$ ");
    }

    void OnToggle(object? sender, EventArgs e) => Terminal.IsOpen = !Terminal.IsOpen;

    void OnCopy(object? sender, EventArgs e) => Status.Text = "Output copied";

    void OnDownload(object? sender, EventArgs e) => Status.Text = "Log downloaded";
}
