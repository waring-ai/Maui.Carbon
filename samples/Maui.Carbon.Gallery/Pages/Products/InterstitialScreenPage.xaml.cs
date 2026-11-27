using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Interstitial screen", GalleryCategory.Products, Order = 30, Spec = "InterstitialScreen",
    Summary = "An onboarding screen shown the first time someone reaches a new experience, as a modal or full screen.")]
public partial class InterstitialScreenPage : GalleryPage
{
    public InterstitialScreenPage()
    {
        InitializeComponent();
        AsyncScreen.ActionHandler = CheckSetupAsync;
        UpdateCustomFooter();
    }

    void OnSingleClicked(object? sender, EventArgs e) => SingleScreen.IsOpen = true;

    void OnMultiClicked(object? sender, EventArgs e) => MultiScreen.IsOpen = true;

    void OnFullClicked(object? sender, EventArgs e) => FullScreen.IsOpen = true;

    void OnAsyncClicked(object? sender, EventArgs e) => AsyncScreen.IsOpen = true;

    void OnCustomClicked(object? sender, EventArgs e) => CustomScreen.IsOpen = true;

    void OnRequestClose(object? sender, CdsInterstitialScreenCloseEventArgs e) => Status.Text = $"Closed with {e.Action}";

    void OnInlineRequestClose(object? sender, CdsInterstitialScreenCloseEventArgs e)
    {
        // Keep the inline previews on the page.
        e.Cancel = true;
        Status.Text = $"{e.Action} (the inline preview stays open)";
    }

    void OnStepChanged(object? sender, ValueChangedEventArgs e)
    {
        if (sender is CdsInterstitialScreen screen) Status.Text = $"Step {screen.ShownStep + 1} of {screen.StepCount}";
    }

    async Task CheckSetupAsync(CdsInterstitialScreenActionEventArgs args)
    {
        // Skip leaves at once; every other button waits for a (simulated) check first.
        if (args.Action == CdsInterstitialScreenAction.Skip) return;
        Status.Text = $"Checking before {args.Action}...";
        await Task.Delay(1500);
        Status.Text = $"Checked step {args.Step + 1}";
    }

    void OnCustomStepChanged(object? sender, ValueChangedEventArgs e)
    {
        UpdateCustomFooter();
        OnStepChanged(sender, e);
    }

    void UpdateCustomFooter()
    {
        CustomPrevious.IsVisible = CustomScreen.ShownStep > 0;
        CustomNext.Text = CustomScreen.ShownStep == CustomScreen.StepCount - 1 ? "Start" : "Next";
    }

    void OnCustomPrevious(object? sender, EventArgs e) => CustomScreen.CurrentStep = CustomScreen.ShownStep - 1;

    void OnCustomNext(object? sender, EventArgs e)
    {
        if (CustomScreen.ShownStep == CustomScreen.StepCount - 1)
        {
            CustomScreen.IsOpen = false;
            Status.Text = "Started";
            return;
        }
        CustomScreen.CurrentStep = CustomScreen.ShownStep + 1;
    }

    void OnExploreClicked(object? sender, EventArgs e)
    {
        CustomScreen.IsOpen = false;
        Status.Text = "Exploring on my own";
    }
}
