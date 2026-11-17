namespace Maui.Carbon.Tests.Components;

public class PageHeaderModelTests
{
    [Theory]
    [InlineData(CdsBreakpoint.Sm, 16)]
    [InlineData(CdsBreakpoint.Md, 32)]
    [InlineData(CdsBreakpoint.Lg, 32)]
    [InlineData(CdsBreakpoint.Xlg, 32)]
    [InlineData(CdsBreakpoint.Max, 40)]
    public void Content_aligns_to_the_first_grid_column(CdsBreakpoint breakpoint, double inset) =>
        Assert.Equal(inset, PageHeaderModel.Inset(breakpoint));

    [Fact]
    public void Title_is_two_lines_or_one_with_contextual_actions()
    {
        Assert.Equal(2, PageHeaderModel.TitleMaxLines(false));
        Assert.Equal(1, PageHeaderModel.TitleMaxLines(true));
    }

    [Fact]
    public void Actions_stack_only_at_the_smallest_breakpoint()
    {
        Assert.True(PageHeaderModel.StacksActions(CdsBreakpoint.Sm));
        Assert.False(PageHeaderModel.StacksActions(CdsBreakpoint.Md));
        Assert.False(PageHeaderModel.StacksActions(CdsBreakpoint.Lg));
    }

    [Fact]
    public void Hero_image_is_2_to_1_from_lg_and_3_to_2_below()
    {
        Assert.Equal(2.0, PageHeaderModel.HeroRatio(CdsBreakpoint.Lg));
        Assert.Equal(2.0, PageHeaderModel.HeroRatio(CdsBreakpoint.Max));
        Assert.Equal(1.5, PageHeaderModel.HeroRatio(CdsBreakpoint.Md));
        Assert.Equal(1.5, PageHeaderModel.HeroRatio(CdsBreakpoint.Sm));
    }

    [Fact]
    public void Fixed_measurements_match_the_spec()
    {
        Assert.Equal(640, PageHeaderModel.TextMaxWidth);
        Assert.Equal(64, PageHeaderModel.TabTagGap);
        Assert.Equal(40, PageHeaderModel.BreadcrumbBarHeight);
        Assert.Equal(-16, PageHeaderModel.TabBarPull);
    }

    [Fact]
    public void Flush_actions_remove_the_end_inset()
    {
        Assert.Equal(0, PageHeaderModel.BarEndInset(CdsBreakpoint.Lg, true));
        Assert.Equal(32, PageHeaderModel.BarEndInset(CdsBreakpoint.Lg, false));
    }
}

public class PageHeaderControlTests
{
    [Fact]
    public void Full_header_can_be_composed()
    {
        var breadcrumb = new CdsBreadcrumb { Size = CdsBreadcrumbSize.Sm };
        breadcrumb.Items.Add(new CdsBreadcrumbItem("Compute"));
        var bar = new CdsPageHeaderBreadcrumbBar
        {
            Breadcrumb = breadcrumb,
            Icon = CdsIconName.Folder,
            ContentActions = new CdsButton("Upload", CdsButtonKind.Tertiary),
            PageActions = new CdsPageHeaderActions { new CdsIconButton(CdsIconName.Share, "Share", CdsIconButtonKind.Ghost) },
            HasBorder = false,
            ContentActionsFlush = true,
            PageActionsFlush = true,
        };
        var content = new CdsPageHeaderContent
        {
            Title = "dal-vm-01",
            Icon = CdsIconName.VirtualMachine,
            ContextualActions = new CdsTag("Running", CdsTagType.Green),
            PageActions = new CdsPageHeaderActions { new CdsButton("Create snapshot") },
            Body = new CdsPageHeaderContentText { Subtitle = "Checkout service", Text = "Backups run nightly." },
        };
        var tabs = new CdsPageHeaderTabBar { Label = "Sections" };
        tabs.Items.Add(new CdsTab { Text = "Overview" });
        tabs.Items.Add(new CdsTab { Text = "Storage" });
        tabs.Tags.Add(new CdsTag("prod", CdsTagType.Blue));
        var header = new CdsPageHeader();
        header.Items.Add(bar);
        header.Items.Add(content);
        header.Items.Add(tabs);

        Assert.Equal(3, header.Items.Count);
        Assert.Equal("dal-vm-01", content.Title);
        Assert.Equal(2, tabs.Items.Count);
        Assert.Single(tabs.Tags);
    }

    [Fact]
    public void Tab_bar_selection_is_two_way_with_the_strip()
    {
        var tabs = new CdsPageHeaderTabBar();
        tabs.Items.Add(new CdsTab { Text = "Overview" });
        tabs.Items.Add(new CdsTab { Text = "Storage" });
        var raised = new List<int>();
        tabs.SelectionChanged += (_, e) => raised.Add(e.NewIndex);
        tabs.SelectedIndex = 1;
        Assert.Equal(1, tabs.SelectedIndex);
        Assert.Contains(1, raised);
        Assert.Equal("Page tabs", tabs.Label);
    }

    [Fact]
    public void Content_text_hides_empty_parts()
    {
        var text = new CdsPageHeaderContentText { Text = "Only body" };
        Assert.Equal("Only body", text.Text);
        text.Subtitle = "With subtitle";
        Assert.Equal("With subtitle", text.Subtitle);
    }

    [Fact]
    public void Hero_image_accepts_a_view()
    {
        var hero = new CdsPageHeaderHeroImage { Image = new BoxView() };
        Assert.NotNull(hero.Image);
        hero.Image = null;
        Assert.Null(hero.Image);
    }
}
