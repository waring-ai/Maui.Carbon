using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Settings", GalleryCategory.Patterns, Order = 30, Spec = "PatternSettings",
    Summary = "An account settings page: tabs for setting areas, a form section that saves on submit, toggles that apply immediately with save feedback, read-only identifiers and a separated destructive action.")]
public partial class PatternSettingsPage : GalleryPage
{
    // The saved values; the form opens with an edited name so Save changes and Cancel are enabled.
    const string SavedName = "Northwind production";
    const string SavedRegion = "eu-de";
    const string SavedContact = "platform-team@northwind.dev";

    string _savedName = SavedName;
    string _savedRegion = SavedRegion;
    string _savedContact = SavedContact;
    bool _saving;
    readonly bool _ready;

    public PatternSettingsPage()
    {
        InitializeComponent();

        CdsTokens.SetColor(Rule1, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(Rule2, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);

        AddPreference("Require multifactor authentication", "Everyone in the account must set up a second factor at their next login.", on: true, savedAtOpen: true);
        AddPreference("Restrict console access by IP address", "Only the address ranges in Access > Network rules can open the console.", on: false, failsWhenOn: true);
        AddPreference("Allow users to create API keys", "When off, only administrators can create keys. Existing keys keep working.", on: true);

        UpdateDirty();
        _ready = true;
    }

    bool IsDirty => (NameInput.Text ?? string.Empty) != _savedName
        || (RegionSelect.SelectedItem as string ?? string.Empty) != _savedRegion
        || (ContactInput.Text ?? string.Empty) != _savedContact;

    void UpdateDirty()
    {
        if (!_ready && SaveButton is null) return;
        var dirty = IsDirty && !_saving;
        SaveButton.IsEnabled = dirty;
        CancelButton.IsEnabled = dirty;
        if (IsDirty) Saved.IsVisible = false;
    }

    void OnDetailsChanged(object? sender, TextChangedEventArgs e) => UpdateDirty();

    void OnDetailsSelectChanged(object? sender, SelectedItemChangedEventArgs e) => UpdateDirty();

    void OnSaveClicked(object? sender, EventArgs e) => DetailsForm.Submit();

    async void OnDetailsSubmitted(object? sender, EventArgs e)
    {
        if (!IsDirty || _saving) return;

        // Saving takes the button's place, then "Changes saved" appears beside the actions.
        _saving = true;
        SaveButton.IsVisible = false;
        Saving.IsVisible = true;
        CancelButton.IsEnabled = false;
        Status.Text = "Saving account details...";

        await Task.Delay(900);

        _savedName = NameInput.Text ?? string.Empty;
        _savedRegion = RegionSelect.SelectedItem as string ?? string.Empty;
        _savedContact = ContactInput.Text ?? string.Empty;
        _saving = false;
        Saving.IsVisible = false;
        SaveButton.IsVisible = true;
        Saved.IsVisible = true;
        UpdateDirty();
        Status.Text = $"Saved account details for {_savedName}.";
    }

    void OnCancelClicked(object? sender, EventArgs e)
    {
        NameInput.Text = _savedName;
        RegionSelect.SelectedItem = _savedRegion;
        ContactInput.Text = _savedContact;
        UpdateDirty();
        Status.Text = "Changes discarded.";
    }

    void OnCloseAccountClicked(object? sender, EventArgs e) =>
        Status.Text = "Closing the account opens a confirmation first (see the confirmation pattern); nothing was closed.";

    /// <summary>A preference row: title and description on the start, a toggle with its save feedback on the end.</summary>
    void AddPreference(string title, string description, bool on, bool savedAtOpen = false, bool failsWhenOn = false)
    {
        var rule = new BoxView { HeightRequest = 1 };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);

        var text = new VerticalStackLayout { Spacing = CdsSpacing.Spacing02 };
        text.Add(new CdsText(title, CdsTypeStyle.BodyCompact01));
        text.Add(new CdsText(description, CdsTypeStyle.HelperText01, CdsTextTone.Helper) { MaximumWidthRequest = 360, HorizontalOptions = LayoutOptions.Start });

        var toggle = new CdsToggle { LabelText = title, HideLabel = true, Size = CdsToggleSize.Sm, IsToggled = on, HorizontalOptions = LayoutOptions.End };
        var feedback = new CdsInlineLoading { Description = "Saved", Status = CdsInlineLoadingStatus.Finished, SuccessDelay = 600000, IsVisible = savedAtOpen, HorizontalOptions = LayoutOptions.End, MinimumHeightRequest = CdsSpacing.Spacing05 };

        var end = new VerticalStackLayout { Spacing = CdsSpacing.Spacing02, MinimumWidthRequest = 160, VerticalOptions = LayoutOptions.Start };
        end.Add(toggle);
        end.Add(feedback);

        var row = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], ColumnSpacing = CdsSpacing.Spacing06, Padding = new Thickness(0, CdsSpacing.Spacing05) };
        row.Add(text, 0, 0);
        row.Add(end, 1, 0);

        var reverting = false;
        toggle.Toggled += async (_, e) =>
        {
            if (reverting) return;

            feedback.IsVisible = true;
            feedback.Description = "Saving...";
            feedback.Status = CdsInlineLoadingStatus.Active;
            Status.Text = $"{title}: saving...";
            await Task.Delay(700);

            if (failsWhenOn && e.Value)
            {
                // A failed save puts the toggle back and says why.
                reverting = true;
                toggle.IsToggled = false;
                reverting = false;
                feedback.Description = "Couldn't save. Add a network rule first.";
                feedback.Status = CdsInlineLoadingStatus.Error;
                Status.Text = $"{title}: not saved, the toggle was reverted.";
                return;
            }

            feedback.Description = "Saved";
            feedback.Status = CdsInlineLoadingStatus.Finished;
            Status.Text = $"{title}: {(e.Value ? "on" : "off")}, saved.";
            await Task.Delay(1500);
            if (feedback.Status == CdsInlineLoadingStatus.Finished) feedback.IsVisible = false;
        };

        PrefsHost.Add(rule);
        PrefsHost.Add(row);
    }
}
