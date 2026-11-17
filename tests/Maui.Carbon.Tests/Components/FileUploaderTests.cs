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
        Assert.Equal(["public.plain-text", "public.png", "public.image"], FileUploaderModel.UniformTypes(accept));
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
}
