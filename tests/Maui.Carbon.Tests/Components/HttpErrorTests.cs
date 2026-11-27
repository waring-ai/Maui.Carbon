namespace Maui.Carbon.Tests.Components;

public class HttpErrorModelTests
{
    [Theory]
    [InlineData(CdsBreakpoint.Sm, 500 - 150)]
    [InlineData(CdsBreakpoint.Md, 500 - 200)]
    [InlineData(CdsBreakpoint.Lg, 500 - 100)]
    [InlineData(CdsBreakpoint.Max, 500 - 100)]
    public void Text_is_centered_and_moves_up_on_small_screens(CdsBreakpoint breakpoint, double top) =>
        Assert.Equal(top, HttpErrorModel.ContentTop(breakpoint, 1000, 200));

    [Fact]
    public void Below_md_the_illustration_grows_and_moves_toward_the_end()
    {
        var frame = HttpErrorModel.ArtFrame(CdsBreakpoint.Sm, new Rect(0, 0, 400, 800));
        Assert.Equal(new Rect(40 - 100, -200, 600, 1200), frame);
        Assert.Equal(new Rect(0, 0, 1200, 800), HttpErrorModel.ArtFrame(CdsBreakpoint.Lg, new Rect(0, 0, 1200, 800)));
    }

    [Fact]
    public void Spacing_matches_the_stylesheet()
    {
        Assert.Equal(4, HttpErrorModel.LabelGap);
        Assert.Equal(12, HttpErrorModel.TitleGap);
        Assert.Equal(24, HttpErrorModel.DescriptionGap);
        Assert.Equal(4, HttpErrorModel.LinkGap);
        Assert.Equal(916, HttpErrorModel.NaturalHeight(IllustrationArt.HttpError404, 1584), 3);
    }
}

public class HttpErrorTests
{
    public static TheoryData<Func<CdsHttpError>, string> Pages => new()
    {
        { () => new CdsHttpError403(), "HttpError403" },
        { () => new CdsHttpError404(), "HttpError404" },
        { () => new CdsHttpErrorOther(), "HttpErrorOther" },
    };

    [Theory]
    [MemberData(nameof(Pages))]
    public void Each_page_draws_its_illustration_and_takes_every_property(Func<CdsHttpError> create, string art)
    {
        var page = create();
        page.ErrorCodeLabel = "Error 404";
        page.Title = "Page not found";
        page.Description = "The page you are looking for was not found.";
        Assert.Equal(art, page.CurrentArt!.Name);
        Assert.Equal(CdsTypeStyle.Heading05, page.TitleLabel.TypeStyle);
        Assert.Equal(SemanticHeadingLevel.Level1, SemanticProperties.GetHeadingLevel(page.TitleLabel));
        page.Title = null;
        Assert.False(page.TitleLabel.IsVisible);
    }

    [Fact]
    public void Links_are_placed_in_order_and_removed()
    {
        var page = new CdsHttpError404();
        var docs = new CdsLink("Carbon Design System", "https://www.carbondesignsystem.com");
        var repo = new CdsLink("Carbon for IBM Products component library", "https://github.com/carbon-design-system/ibm-products");
        page.Links.Add(docs);
        page.Links.Add(repo);
        Assert.Equal([docs, repo], page.PlacedLinks);
        Assert.Equal(new Thickness(0, 0, 0, 4), docs.Margin);
        page.Links.Remove(docs);
        page.Links.Insert(1, docs);
        Assert.Equal([repo, docs], page.PlacedLinks);
        page.Links.Clear();
        Assert.Empty(page.PlacedLinks);
    }
}
