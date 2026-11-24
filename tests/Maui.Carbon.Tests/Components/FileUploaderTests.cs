using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

public class FileUploaderTests
{
    [Fact]
    public void ParsesAcceptLists()
    {
        Assert.Empty(FileUploaderModel.ParseAccept(null));
        Assert.Empty(FileUploaderModel.ParseAccept("  "));
        Assert.Equal([".csv", "image/*", ".log"], FileUploaderModel.ParseAccept(".csv, image/* ;.log,"));
    }

    [Theory]
    [InlineData("report.csv", ".csv", true)]
    [InlineData("REPORT.CSV", ".csv", true)]
    [InlineData("report.csv.bak", ".csv", false)]
    [InlineData("photo.png", "image/*", true)]
    [InlineData("photo.JPG", "image/*", true)]
    [InlineData("notes.txt", "image/*", false)]
    [InlineData("data.csv", "text/csv", true)]
    [InlineData("data.txt", "text/csv", false)]
    [InlineData("mystery.xyz", "image/*", false)]
    [InlineData("anything.bin", "", true)]
    public void MatchesTheAcceptList(string file, string accept, bool expected) =>
        Assert.Equal(expected, FileUploaderModel.Matches(file, FileUploaderModel.ParseAccept(accept)));

    [Fact]
    public void MapsAcceptToPlatformTypes()
    {
        var accept = FileUploaderModel.ParseAccept(".log, .txt, .png, image/*, .weird");
        Assert.Equal([".log", ".txt", ".png", ".weird"], FileUploaderModel.Extensions(accept));
        Assert.Equal(["text/plain", "image/png", "image/*", "*/*"], FileUploaderModel.MimeTypes(accept));
        // ".weird" has no known type: it stays an extension, which the picker resolves to that extension's dynamic type.
        Assert.Equal(["public.plain-text", "public.png", "public.image", ".weird"], FileUploaderModel.UniformTypes(accept));
    }

    [Fact]
    public void MimeTypesOfExtensionsAreKnownForCommonFiles()
    {
        Assert.Equal("text/csv", FileUploaderModel.MimeOf("a.CSV"));
        Assert.Null(FileUploaderModel.MimeOf("a.unknown"));
        Assert.Null(FileUploaderModel.MimeOf("noextension"));
    }

    [Theory]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void RowHeightsFollowTheFieldSizes(CdsSize size, double height) => Assert.Equal(height, FileUploaderModel.RowHeight(size));

    [Fact]
    public void InvalidRowSpacingMatchesCarbon()
    {
        Assert.Equal((4, 7), (FileUploaderModel.InvalidRowPadding(CdsSize.Sm), FileUploaderModel.InvalidMessageGap(CdsSize.Sm)));
        Assert.Equal((8, 11), (FileUploaderModel.InvalidRowPadding(CdsSize.Md), FileUploaderModel.InvalidMessageGap(CdsSize.Md)));
        Assert.Equal((12, 16), (FileUploaderModel.InvalidRowPadding(CdsSize.Lg), FileUploaderModel.InvalidMessageGap(CdsSize.Lg)));
    }

    [Fact]
    public void BuildsLabels()
    {
        Assert.Equal("Remove file - a.csv", FileUploaderModel.RemoveLabel("Remove file", "a.csv"));
        Assert.Equal("Remove file", FileUploaderModel.RemoveLabel(null, null));
        Assert.Equal("a.csv added", FileUploaderModel.AddedAnnouncement(["a.csv"]));
        Assert.Equal("3 files added", FileUploaderModel.AddedAnnouncement(["a", "b", "c"]));
        Assert.Equal("", FileUploaderModel.AddedAnnouncement([]));
    }

    [Fact]
    public void UploadedFileRaisesPropertyChanged()
    {
        var file = new CdsUploadedFile("a.csv");
        var names = new List<string?>();
        file.PropertyChanged += (_, e) => names.Add(e.PropertyName);
        file.Status = CdsFileStatus.Uploading;
        file.Status = CdsFileStatus.Uploading; // unchanged: silent
        file.IsInvalid = true;
        file.ErrorSubject = "Too large";
        file.ErrorBody = "Under 10 MB";
        file.Name = "b.csv";
        Assert.Equal(["Status", "IsInvalid", "ErrorSubject", "ErrorBody", "Name"], names);
    }

    [Fact]
    public void UploaderRowsFollowTheFilesCollection()
    {
        var uploader = new CdsFileUploader { LabelTitle = "Upload logs", LabelDescription = "Max 500 MB.", Variant = CdsFileUploaderVariant.Drop, Accept = ".log" };
        var a = new CdsUploadedFile("a.log") { Status = CdsFileStatus.Uploading };
        var b = new CdsUploadedFile("b.log") { Status = CdsFileStatus.Complete };
        uploader.Files.Add(a);
        uploader.Files.Add(b);
        b.IsInvalid = true;
        b.ErrorSubject = "File is too large";
        uploader.Files.Remove(a);
        Assert.Single(uploader.Files);
        uploader.Files.Clear();
        Assert.Empty(uploader.Files);
    }

    [Fact]
    public void ItemSetsEveryPropertyWithoutThrowing()
    {
        var item = new CdsFileUploaderItem
        {
            Name = "very-long-file-name-that-needs-an-ellipsis.csv", Status = CdsFileStatus.Uploading, IsInvalid = true, ErrorSubject = "Too large",
            ErrorBody = "Choose a smaller file", Size = CdsSize.Sm, IconDescription = "Remove",
        };
        item.Status = CdsFileStatus.Complete;
        item.IsEnabled = false;
        Assert.Equal(CdsSize.Sm, item.Size);
    }

    [Fact]
    public void UploaderSetsEveryPropertyWithoutThrowing()
    {
        var uploader = new CdsFileUploader
        {
            LabelTitle = "T", LabelDescription = "D", ButtonLabel = "Add files", DropLabel = "Drop", Accept = ".csv", Multiple = false,
            Variant = CdsFileUploaderVariant.Drop, ButtonKind = CdsButtonKind.Tertiary, Size = CdsSize.Md, FilenameStatus = CdsFileStatus.Complete,
            IconDescription = "Remove",
        };
        uploader.IsEnabled = false;
        Assert.Equal(CdsSize.Md, uploader.Size);
    }

    static IEnumerable<Element> Tree(Element e) => ContainerSemanticsAuditTests.Walk(e);

    static void OnScreen(View view) => _ = new Window(new ContentPage { Content = view });

    // ---- CA-33: the invalid glyph ----

    [Fact]
    public void Invalid_rows_use_the_warning_filled_glyph_like_every_other_form()
    {
        var item = new CdsFileUploaderItem { Name = "a.csv", IsInvalid = true, ErrorSubject = "Too large" };
        var icons = Tree(item).OfType<CdsIcon>().ToList();
        Assert.Contains(icons, i => i.Icon == CdsIconName.WarningFilled && i.IsVisible);
        Assert.DoesNotContain(icons, i => i.Icon == CdsIconName.ErrorFilled);
    }

    // ---- CA-31: the complete check is not color alone ----

    [Fact]
    public void The_complete_check_is_an_image_named_complete()
    {
        var item = new CdsFileUploaderItem { Name = "a.csv", Status = CdsFileStatus.Complete };
        var check = Tree(item).OfType<CdsIcon>().Single(i => i.Icon == CdsIconName.CheckmarkFilled);
        Assert.True(check.IsVisible);
        Assert.Equal("Complete", check.Description);
        Assert.Equal("Complete", SemanticProperties.GetDescription(check));
    }

    [Fact]
    public void Finishing_an_upload_announces_it()
    {
        var item = new CdsFileUploaderItem { Name = "a.csv", Status = CdsFileStatus.Uploading };
        OnScreen(item);
        using var heard = CdsAnnouncer.Capture();
        item.Status = CdsFileStatus.Complete;
        Assert.Equal(new CdsAnnouncement("a.csv uploaded", CdsAnnouncePriority.Polite), Assert.Single(heard.Announcements));
        item.Status = CdsFileStatus.Edit;
        item.Status = CdsFileStatus.Complete; // not from uploading: nothing finished
        Assert.Single(heard.Announcements);
    }

    // ---- CA-30: the invalid message is an alert ----

    [Fact]
    public void A_row_that_turns_invalid_announces_its_error_assertively()
    {
        var uploader = new CdsFileUploader();
        var file = new CdsUploadedFile("screen-recording.mov");
        uploader.Files.Add(file);
        OnScreen(uploader);
        using var heard = CdsAnnouncer.Capture();
        file.IsInvalid = true;
        file.ErrorSubject = "File size exceeds limit";
        file.ErrorBody = "500 MB max file size.";
        Assert.Equal(
            [new CdsAnnouncement("screen-recording.mov: File size exceeds limit", CdsAnnouncePriority.Assertive),
             new CdsAnnouncement("screen-recording.mov: File size exceeds limit. 500 MB max file size.", CdsAnnouncePriority.Assertive)],
            heard.Announcements);
    }

    [Fact]
    public void An_invalid_file_added_to_the_list_announces_its_whole_error_once()
    {
        var uploader = new CdsFileUploader();
        OnScreen(uploader);
        using var heard = CdsAnnouncer.Capture();
        uploader.Files.Add(new CdsUploadedFile("big.mov") { IsInvalid = true, ErrorSubject = "Too large", ErrorBody = "Under 10 MB" });
        Assert.Equal(new CdsAnnouncement("big.mov: Too large. Under 10 MB", CdsAnnouncePriority.Assertive), Assert.Single(heard.Announcements));
    }

    [Fact]
    public void A_row_built_invalid_announces_when_it_comes_on_screen()
    {
        using var heard = CdsAnnouncer.Capture();
        var item = new CdsFileUploaderItem { Name = "a.csv", IsInvalid = true, ErrorSubject = "Too large" };
        Assert.Empty(heard.Announcements);
        OnScreen(item);
        Assert.Equal(new CdsAnnouncement("a.csv: Too large", CdsAnnouncePriority.Assertive), Assert.Single(heard.Announcements));
    }

    [Fact]
    public void Without_a_remove_button_the_error_describes_the_name()
    {
        var item = new CdsFileUploaderItem { Name = "a.csv", Status = CdsFileStatus.Complete, IsInvalid = true, ErrorSubject = "Too large", ErrorBody = "Under 10 MB" };
        var name = Tree(item).OfType<CdsText>().Single(t => t.Text == "a.csv");
        Assert.Equal("Too large. Under 10 MB", SemanticProperties.GetHint(name));
        item.Status = CdsFileStatus.Edit;
        Assert.Null(SemanticProperties.GetHint(name)); // the remove button carries it now
        item.IsInvalid = false;
        Assert.Null(SemanticProperties.GetHint(name));
    }

    [Fact]
    public void Error_texts()
    {
        Assert.Null(FileUploaderModel.ErrorText(false, "a", "b"));
        Assert.Null(FileUploaderModel.ErrorText(true, null, ""));
        Assert.Equal("a", FileUploaderModel.ErrorText(true, "a", null));
        Assert.Equal("a. b", FileUploaderModel.ErrorText(true, "a", "b"));
        Assert.Equal("x.csv: a", FileUploaderModel.InvalidAnnouncement("x.csv", "a"));
        Assert.Equal("a", FileUploaderModel.InvalidAnnouncement(null, "a"));
        Assert.Null(FileUploaderModel.InvalidAnnouncement("x.csv", null));
        Assert.Equal("x.csv uploaded", FileUploaderModel.CompletedAnnouncement("x.csv"));
    }

    // ---- CA-32: one file by default for the button ----

    [Fact]
    public void Multiple_defaults_to_one_file_for_the_button_and_several_for_the_drop_container()
    {
        Assert.False(new CdsFileUploader().Multiple);
        Assert.True(new CdsFileUploader { Variant = CdsFileUploaderVariant.Drop }.Multiple);
        var uploader = new CdsFileUploader { Variant = CdsFileUploaderVariant.Drop };
        uploader.Variant = CdsFileUploaderVariant.Button;
        Assert.False(uploader.Multiple);
    }

    [Fact]
    public void An_explicit_multiple_wins_over_the_variant_default_in_either_order()
    {
        Assert.False(new CdsFileUploader { Variant = CdsFileUploaderVariant.Drop, Multiple = false }.Multiple);
        Assert.False(new CdsFileUploader { Multiple = false, Variant = CdsFileUploaderVariant.Drop }.Multiple);
        Assert.True(new CdsFileUploader { Multiple = true }.Multiple);
        var uploader = new CdsFileUploader { Multiple = true, Variant = CdsFileUploaderVariant.Drop };
        uploader.Variant = CdsFileUploaderVariant.Button;
        Assert.True(uploader.Multiple);
    }

    [Fact]
    public void A_single_file_uploader_keeps_only_the_latest_file()
    {
        var uploader = new CdsFileUploader();
        uploader.AddResults([new FileResult("/tmp/a.csv"), new FileResult("/tmp/b.csv")]);
        Assert.Equal("a.csv", Assert.Single(uploader.Files).Name);
        uploader.AddResults([new FileResult("/tmp/c.csv")]);
        Assert.Equal("c.csv", Assert.Single(uploader.Files).Name);
    }

    // ---- CA-34: skeleton ----

    [Fact]
    public void The_skeleton_is_hidden_from_assistive_technology()
    {
        var skeleton = new CdsFileUploaderSkeleton();
        Assert.False(AutomationProperties.GetIsInAccessibleTree(skeleton) ?? true);
        Assert.DoesNotContain(Tree(skeleton), e => e is CdsHitTarget);
        Assert.Equal(3, Tree(skeleton).Count(e => e.GetType().Name == "CdsShimmerView"));
    }
}
