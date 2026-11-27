namespace Maui.Carbon.Tests.Components;

public class ExportModalTests
{
    [Theory]
    [InlineData("Sample02.pdf", true, false)]
    [InlineData("Sample02.PDF", true, false)]
    [InlineData("Sample02", true, true)]
    [InlineData("Sample02.csv", true, true)]
    [InlineData("Sample02.csv", false, false)]
    [InlineData("", true, true)]
    [InlineData(null, true, true)]
    [InlineData("archive.tar.pdf", true, false)]
    public void Extension_validation_needs_a_valid_extension_once_dirty(string? name, bool dirty, bool expected) =>
        Assert.Equal(expected, ExportModalModel.HasInvalidExtension(name, ["pdf"], dirty));

    [Fact]
    public void No_valid_extensions_accepts_any_name()
    {
        Assert.False(ExportModalModel.HasInvalidExtension("Sample02", [], true));
        Assert.False(ExportModalModel.HasInvalidExtension("Sample02", null, true));
        Assert.False(ExportModalModel.HasInvalidExtension("Sample02.csv", [".csv"], true));
    }

    [Fact]
    public void Submit_name_uses_the_chosen_format_in_lowercase()
    {
        Assert.Equal("test.yaml", ExportModalModel.SubmitName("test", "ignored", "YAML"));
        Assert.Equal("Sample02.pdf", ExportModalModel.SubmitName("Sample", "Sample02.pdf", null));
        Assert.Equal("Sample02.pdf", ExportModalModel.SubmitName("Sample", "Sample02.pdf", ""));
        Assert.Equal(string.Empty, ExportModalModel.SubmitName("Sample", null, null));
    }

    [Fact]
    public void Primary_and_submitted_states()
    {
        Assert.True(ExportModalModel.IsPrimaryDisabled(true, "a.pdf", false));
        Assert.True(ExportModalModel.IsPrimaryDisabled(false, "", false));
        Assert.True(ExportModalModel.IsPrimaryDisabled(false, "a", true));
        Assert.False(ExportModalModel.IsPrimaryDisabled(false, "a.pdf", false));
        Assert.False(ExportModalModel.IsSubmitted(false, false, false));
        Assert.True(ExportModalModel.IsSubmitted(true, false, false));
        Assert.True(ExportModalModel.IsSubmitted(false, true, false));
        Assert.True(ExportModalModel.IsSubmitted(false, false, true));
    }

    [Fact]
    public void Formats_default_to_the_first_and_label_with_their_description()
    {
        Assert.Null(ExportModalModel.DefaultExtension(null));
        Assert.Null(ExportModalModel.DefaultExtension([]));
        Assert.Equal("YAML", ExportModalModel.DefaultExtension([new("YAML", "best for IBM managed cloud"), new("BAR")]));
        Assert.Equal("YAML (best for IBM managed cloud)", ExportModalModel.ExtensionLabel(new("YAML", "best for IBM managed cloud")));
        Assert.Equal("BAR", ExportModalModel.ExtensionLabel(new("BAR")));
    }

    [Fact]
    public void Constructs_and_takes_every_property()
    {
        var modal = new CdsExportModal
        {
            Title = "Export",
            BodyText = "File must be exported in a PDF format.",
            Filename = "Sample02.pdf",
            InputLabel = "File name",
            InputType = CdsExportModalInputType.Password,
            InvalidInputText = "File must have valid extension .pdf",
            PreformattedExtensionsLabel = "Choose an export format",
            ShowPasswordLabel = "Show name",
            HidePasswordLabel = "Hide name",
            PrimaryButtonText = "Export",
            SecondaryButtonText = "Cancel",
            LoadingMessage = "Exporting file...",
            SuccessMessage = "The file has been exported.",
            ErrorMessage = "Server error 500",
            RequestSubmitCommand = new Command<string>(_ => { }),
        };
        modal.ValidExtensions.Add("pdf");
        modal.PreformattedExtensions.Add(new CdsExportExtension("YAML", "best for IBM managed cloud"));
        modal.IsLoading = true;
        modal.IsLoading = false;
        modal.IsError = true;
        modal.IsSuccessful = true;
        modal.IsInline = true;
        Assert.IsType<CdsPasswordInput>(modal.NameInput);
    }

    [Fact]
    public void Typed_name_is_submitted_and_an_invalid_extension_blocks_it()
    {
        var modal = new CdsExportModal { Filename = "Sample02.pdf", PrimaryButtonText = "Export", SecondaryButtonText = "Cancel", InvalidInputText = "File must have valid extension .pdf" };
        modal.ValidExtensions.Add("pdf");
        string? submitted = null;
        modal.RequestSubmit += (_, e) => submitted = e.FileName;
        Assert.Equal("Sample02.pdf", modal.NameInput.Text);
        modal.Submit();
        Assert.Equal("Sample02.pdf", submitted);

        submitted = null;
        modal.NameInput.Text = "Sample02";
        Assert.False(modal.NameInput.IsInvalid);
        Assert.False(modal.Footer!.PrimaryButtonDisabled);
        modal.MarkDirty();
        Assert.True(modal.NameInput.IsInvalid);
        Assert.Equal("File must have valid extension .pdf", modal.NameInput.InvalidText);
        Assert.True(modal.Footer.PrimaryButtonDisabled);
        modal.Submit();
        Assert.Null(submitted);
        modal.NameInput.Text = "Sample02.pdf";
        Assert.False(modal.NameInput.IsInvalid);
    }

    [Fact]
    public void Formats_replace_the_input_and_name_the_file()
    {
        var modal = new CdsExportModal { Filename = "test", PrimaryButtonText = "Export", SecondaryButtonText = "Cancel" };
        modal.PreformattedExtensions.Add(new CdsExportExtension("YAML", "best for IBM managed cloud"));
        modal.PreformattedExtensions.Add(new CdsExportExtension("BAR", "best for integration server"));
        Assert.True(modal.FormatGroup.IsVisible);
        Assert.False(modal.NameInput.IsVisible);
        Assert.Equal(2, modal.FormatGroup.Items.Count);
        Assert.Equal("YAML", modal.CurrentExtension);
        string? submitted = null;
        modal.RequestSubmit += (_, e) => submitted = e.FileName;
        modal.Submit();
        Assert.Equal("test.yaml", submitted);
        modal.FormatGroup.SelectedValue = "BAR";
        modal.Submit();
        Assert.Equal("test.bar", submitted);
    }

    [Fact]
    public void Submitted_states_hide_the_fields_and_footer()
    {
        var modal = new CdsExportModal { Filename = "Sample02.pdf", PrimaryButtonText = "Export", SecondaryButtonText = "Cancel", BodyText = "Pick a name." };
        Assert.True(modal.Footer!.HasContent);
        modal.IsLoading = true;
        Assert.False(modal.NameInput.IsVisible);
        Assert.False(modal.Footer.HasContent);
        modal.IsLoading = false;
        modal.IsSuccessful = true;
        Assert.False(modal.Footer.HasContent);
        modal.IsSuccessful = false;
        Assert.True(modal.Footer.HasContent);
        Assert.True(modal.NameInput.IsVisible);
    }

    [Fact]
    public void Opening_resets_the_name_to_the_filename()
    {
        var modal = new CdsExportModal { Filename = "Sample02.pdf", PrimaryButtonText = "Export", SecondaryButtonText = "Cancel" };
        modal.NameInput.Text = "other.pdf";
        Assert.Equal("other.pdf", modal.CurrentName);
        modal.IsOpen = true;
        Assert.Equal("Sample02.pdf", modal.CurrentName);
        modal.IsOpen = false;
    }
}
