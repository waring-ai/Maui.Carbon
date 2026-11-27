namespace Maui.Carbon.Tests.Components;

public class APIKeyModalTests
{
    static APIKeyModalInputs S(string? key = null, bool loading = false, bool editing = false, bool editSuccess = false, int steps = 0,
        int step = 0, bool? valid = null, bool nameRequired = false, string? name = null, bool copyError = false, bool error = false) =>
        new(key, loading, editing, editSuccess, steps, step, valid, nameRequired, name, false, false, copyError, error);

    [Fact]
    public void Primary_action_moves_through_steps_then_copy()
    {
        Assert.Equal(APIKeyPrimaryAction.Generate, APIKeyModalModel.PrimaryAction(S()));
        Assert.Equal(APIKeyPrimaryAction.Edit, APIKeyModalModel.PrimaryAction(S(editing: true)));
        Assert.Equal(APIKeyPrimaryAction.Copy, APIKeyModalModel.PrimaryAction(S(key: "111")));
        Assert.Equal(APIKeyPrimaryAction.Generate, APIKeyModalModel.PrimaryAction(S(key: "111", loading: true)));
        Assert.Equal(APIKeyPrimaryAction.NextStep, APIKeyModalModel.PrimaryAction(S(steps: 2, step: 0)));
        Assert.Equal(APIKeyPrimaryAction.Generate, APIKeyModalModel.PrimaryAction(S(steps: 2, step: 1)));
    }

    [Fact]
    public void Secondary_goes_back_only_between_steps()
    {
        Assert.False(APIKeyModalModel.SecondaryGoesBack(S()));
        Assert.False(APIKeyModalModel.SecondaryGoesBack(S(steps: 3, step: 0)));
        Assert.True(APIKeyModalModel.SecondaryGoesBack(S(steps: 3, step: 2)));
        Assert.False(APIKeyModalModel.SecondaryGoesBack(S(key: "111", steps: 3, step: 2)));
    }

    [Fact]
    public void Primary_disabled_rules()
    {
        Assert.True(APIKeyModalModel.IsPrimaryDisabled(S(loading: true)));
        Assert.True(APIKeyModalModel.IsPrimaryDisabled(S(nameRequired: true)));
        Assert.False(APIKeyModalModel.IsPrimaryDisabled(S(nameRequired: true, name: "ci-deploy")));
        Assert.True(APIKeyModalModel.IsPrimaryDisabled(S(steps: 2, valid: false)));
        Assert.False(APIKeyModalModel.IsPrimaryDisabled(S(steps: 2, valid: true)));
        Assert.False(APIKeyModalModel.IsPrimaryDisabled(S(steps: 2, nameRequired: true)));
    }

    [Fact]
    public void Body_parts_follow_the_state()
    {
        Assert.True(APIKeyModalModel.ShowsStepContent(S(steps: 2)));
        Assert.False(APIKeyModalModel.ShowsStepContent(S(steps: 2, key: "111")));
        Assert.True(APIKeyModalModel.ShowsKey(S(key: "111")));
        Assert.True(APIKeyModalModel.ShowsKey(S(key: "111", loading: true)));
        Assert.False(APIKeyModalModel.ShowsKey(S(key: "111", editing: true)));
        Assert.True(APIKeyModalModel.ShowsNameInput(S(nameRequired: true)));
        Assert.False(APIKeyModalModel.ShowsNameInput(S(nameRequired: true, key: "111")));
        Assert.True(APIKeyModalModel.ShowsNameInput(S(editing: true)));
        Assert.True(APIKeyModalModel.ShowsError(S(error: true)));
        Assert.True(APIKeyModalModel.ShowsError(S(copyError: true)));
        Assert.False(APIKeyModalModel.ShowsError(S(steps: 2, error: true)));
        Assert.True(APIKeyModalModel.ShowsInformation(S(key: "111")));
        Assert.False(APIKeyModalModel.ShowsInformation(S()));
        Assert.True(APIKeyModalModel.ShowsSuccess(S(key: "111"), "API key successfully created"));
        Assert.False(APIKeyModalModel.ShowsSuccess(S(key: "111"), null));
        Assert.True(APIKeyModalModel.ShowsSuccess(S(editing: true, editSuccess: true), "API key successfully saved"));
    }

    [Fact]
    public void Title_and_success_message()
    {
        Assert.Equal("Generate", APIKeyModalModel.Title(S(), "Generate", "Step"));
        Assert.Equal("Step", APIKeyModalModel.Title(S(steps: 2), "Generate", "Step"));
        Assert.Equal("Generate", APIKeyModalModel.Title(S(steps: 2, key: "111"), "Generate", "Step"));
        Assert.Equal("Generate", APIKeyModalModel.Title(S(steps: 2, editing: true, editSuccess: true), "Generate", "Step"));
        Assert.Equal("Saved", APIKeyModalModel.SuccessMessage(S(editing: true, editSuccess: true), "Saved", "Created"));
        Assert.Equal("Created", APIKeyModalModel.SuccessMessage(S(key: "111"), "Saved", "Created"));
        Assert.Null(APIKeyModalModel.SuccessMessage(S(), "Saved", "Created"));
    }

    [Theory]
    [InlineData(0, 1, 3, 1)]
    [InlineData(2, 1, 3, 2)]
    [InlineData(0, -1, 3, 0)]
    [InlineData(5, 0, 0, 0)]
    public void Steps_stay_in_range(int current, int delta, int count, int expected) =>
        Assert.Equal(expected, APIKeyModalModel.MoveStep(current, delta, count));

    [Fact]
    public void Downloaded_file_names_types_and_content()
    {
        Assert.Equal("apikey.json", APIKeyDownloaderModel.FileName(null, CdsAPIKeyFileType.Json));
        Assert.Equal("apikey.json", APIKeyDownloaderModel.FileName("  ", CdsAPIKeyFileType.Json));
        Assert.Equal("ci-deploy.txt", APIKeyDownloaderModel.FileName("ci-deploy", CdsAPIKeyFileType.Txt));
        Assert.Equal("application/json", APIKeyDownloaderModel.ContentType(CdsAPIKeyFileType.Json));
        Assert.Equal("text/plain", APIKeyDownloaderModel.ContentType(CdsAPIKeyFileType.Txt));
        Assert.Equal("111-111", APIKeyDownloaderModel.Content("111-111", CdsAPIKeyFileType.Txt));
        Assert.Equal("{\"apiKey\":\"111-111\"}", APIKeyDownloaderModel.Content("111-111", CdsAPIKeyFileType.Json));
        Assert.Equal("{\"apiKey\":\"a\\\"b\"}", APIKeyDownloaderModel.Content("a\"b", CdsAPIKeyFileType.Json));
        Assert.Equal("{\"apiKey\":\"\"}", APIKeyDownloaderModel.Content(null, CdsAPIKeyFileType.Json));
    }

    [Fact]
    public void Constructs_and_takes_every_property()
    {
        var modal = new CdsAPIKeyModal
        {
            ApiKey = "123-456-789",
            ApiKeyLabel = "API key",
            ApiKeyName = "ci-deploy",
            BodyText = "Keys let scripts call the API.",
            ModalLabel = "An example of Generate API key",
            GenerateTitle = "Generate an API key",
            GenerateButtonText = "Generate API key",
            GenerateSuccessMessage = "API key successfully created",
            GenerateSuccessBody = "This is your unique API key and is non-recoverable.",
            CopyButtonText = "Copy",
            CopyIconDescription = "Copy",
            CopyErrorText = "Copy failed",
            CloseButtonText = "Close",
            IsEditing = false,
            EditButtonText = "Save",
            IsEditSuccess = false,
            EditSuccessMessage = "API key successfully saved",
            IsLoading = false,
            LoadingText = "Generating...",
            IsError = false,
            ErrorText = "An error occurred. Please try again.",
            IsNameRequired = true,
            NameLabel = "Name of the API key",
            NamePlaceholder = "ci-deploy",
            NameHelperText = "Say where the key is used",
            HasAPIKeyVisibilityToggle = true,
            ShowAPIKeyLabel = "Show key",
            HideAPIKeyLabel = "Hide key",
            HelperText = "This is your unique API key and is non-recoverable.",
            HasDownloadLink = true,
            DownloadBodyText = "This API key can't be shown again. Save it now.",
            DownloadFileName = "apikey",
            DownloadFileType = CdsAPIKeyFileType.Txt,
            DownloadLinkText = "Download as text",
            DownloadLinkLabel = "Download the API key as a text file",
            NextStepButtonText = "Next",
            PreviousStepButtonText = "Back",
            RequestGenerateCommand = new Command<string>(_ => { }),
            RequestEditCommand = new Command<string>(_ => { }),
            CopyCommand = new Command<string>(_ => { }),
        };
        modal.CustomSteps.Add(new CdsAPIKeyModalStep { Title = "Name the key", Content = new CdsTextInput(), IsValid = true });
        modal.CurrentStep = 0;
        modal.IsInline = true;
        Assert.True(modal.HiddenKeyInput.IsVisible);
        Assert.True(modal.HiddenKeyInput.IsReadOnly);
        Assert.Equal("Copy", modal.Footer!.PrimaryButtonText);
        Assert.False(modal.Header.ShowsCloseButton);
    }

    [Fact]
    public async Task Generate_hands_over_the_name_then_the_key_shows_with_copy()
    {
        var modal = new CdsAPIKeyModal
        {
            GenerateTitle = "Generate an API key",
            GenerateButtonText = "Generate API key",
            CopyButtonText = "Copy",
            CloseButtonText = "Close",
            IsNameRequired = true,
            NameLabel = "Name of the API key",
            GenerateSuccessMessage = "API key successfully created",
        };
        Assert.True(modal.Footer!.PrimaryButtonDisabled);
        Assert.True(modal.NameInput.IsVisible);
        string? requested = null;
        modal.RequestGenerate += (_, e) => requested = e.Value;
        modal.NameInput.Text = "ci-deploy";
        Assert.False(modal.Footer.PrimaryButtonDisabled);
        await modal.SubmitAsync();
        Assert.Equal("ci-deploy", requested);

        modal.IsLoading = true;
        Assert.True(modal.Footer.PrimaryButtonDisabled);
        Assert.False(modal.NameInput.IsEnabled);
        modal.ApiKey = "111-111-111-111";
        modal.IsLoading = false;
        Assert.Equal("Copy", modal.Footer.PrimaryButtonText);
        Assert.True(modal.ShownKeyInput.IsVisible);
        Assert.False(modal.NameInput.IsVisible);

        string? copied = null;
        modal.CopyRequested += (_, e) => copied = e.Value;
        await modal.SubmitAsync();
        Assert.Equal("111-111-111-111", copied);
    }

    [Fact]
    public async Task Without_a_copy_handler_a_failed_clipboard_shows_the_copy_error()
    {
        var modal = new CdsAPIKeyModal { ApiKey = "111", CopyButtonText = "Copy", CopyErrorText = "Copy failed" };
        await modal.SubmitAsync();
        // Plain .NET has no clipboard: the copy fails and the error line shows.
        Assert.True(modal.HasCopyError);
    }

    [Fact]
    public async Task Custom_steps_go_next_and_back()
    {
        var first = new CdsAPIKeyModalStep { Title = "Name the key", Content = new CdsTextInput(), IsValid = false };
        var second = new CdsAPIKeyModalStep { Title = "Set permissions", Content = new CdsCheckbox() };
        var modal = new CdsAPIKeyModal { NextStepButtonText = "Next", PreviousStepButtonText = "Back", GenerateButtonText = "Generate", CloseButtonText = "Close" };
        modal.CustomSteps.Add(first);
        modal.CustomSteps.Add(second);
        Assert.Equal("Name the key", modal.Header.Title);
        Assert.Equal("Next", modal.Footer!.PrimaryButtonText);
        Assert.Equal("Close", modal.Footer.SecondaryButtonText);
        Assert.True(modal.Footer.PrimaryButtonDisabled);
        first.IsValid = true;
        Assert.False(modal.Footer.PrimaryButtonDisabled);
        await modal.SubmitAsync();
        Assert.Equal(1, modal.CurrentStep);
        Assert.Equal("Set permissions", modal.Header.Title);
        Assert.Equal("Generate", modal.Footer.PrimaryButtonText);
        Assert.Equal("Back", modal.Footer.SecondaryButtonText);
        modal.Back();
        Assert.Equal(0, modal.CurrentStep);
        var closes = 0;
        modal.RequestClose += (_, _) => closes++;
        modal.Back();
        Assert.Equal(1, closes);
    }

    [Fact]
    public async Task Editing_saves_the_name()
    {
        var modal = new CdsAPIKeyModal { IsEditing = true, ApiKeyName = "ci-deploy", EditButtonText = "Save", EditSuccessMessage = "API key successfully saved" };
        modal.IsOpen = true;
        Assert.Equal("ci-deploy", modal.CurrentName);
        Assert.Equal("Save", modal.Footer!.PrimaryButtonText);
        string? saved = null;
        modal.RequestEdit += (_, e) => saved = e.Value;
        modal.NameInput.Text = "ci-release";
        await modal.SubmitAsync();
        Assert.Equal("ci-release", saved);
        modal.IsOpen = false;
        Assert.Equal(string.Empty, modal.CurrentName);
    }

    [Fact]
    public async Task Downloader_hands_the_file_to_a_save_handler()
    {
        var downloader = new CdsAPIKeyDownloader { ApiKey = "111", FileName = "ci", FileType = CdsAPIKeyFileType.Json, LinkText = "Download as JSON", BodyText = "Save it now." };
        CdsAPIKeySaveEventArgs? args = null;
        var done = 0;
        downloader.SaveRequested += (_, e) =>
        {
            args = e;
            e.Handled = true;
        };
        downloader.Downloaded += (_, _) => done++;
        await downloader.DownloadAsync();
        Assert.Equal("ci.json", args!.FileName);
        Assert.Equal("{\"apiKey\":\"111\"}", args.Content);
        Assert.Equal("application/json", args.ContentType);
        Assert.Equal(1, done);
    }

    [Fact]
    public void Downloader_names_its_link()
    {
        var downloader = new CdsAPIKeyDownloader { LinkText = "Download as JSON", LinkLabel = "Download the API key as a JSON file", BodyText = "Save it now." };
        var link = Assert.Single(downloader.Paragraph.LinkTargets);
        Assert.Equal("Download the API key as a JSON file", SemanticProperties.GetDescription(link));
    }
}
