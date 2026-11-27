using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Layout.Data;

namespace Maui.Carbon.Gallery.Pages.Layout;

[GalleryPage("Grid", GalleryCategory.Layout, Order = 10,
    Summary = "The 16-column responsive layout grid: spans and offsets per breakpoint, wide, narrow and condensed gutters, nested subgrids.")]
public partial class GridPage : GalleryPage
{
    public GridPage()
    {
        InitializeComponent();

        for (var i = 1; i <= 16; i++) LgGrid.Add(LayoutDemos.Column(i.ToString(), span: 1));
        for (var i = 1; i <= 8; i++) MdGrid.Add(LayoutDemos.Column(i.ToString(), span: 1));
        for (var i = 1; i <= 4; i++) SmGrid.Add(LayoutDemos.Column(i.ToString(), span: 1));

        foreach (var grid in new[] { SpanSmGrid, SpanMdGrid, SpanLgGrid })
        {
            grid.Add(LayoutDemos.Column("sm 4, md 8, lg 8", sm: 4, md: 8, lg: 8));
            grid.Add(LayoutDemos.Column("sm 2, md 4, lg 4", sm: 2, md: 4, lg: 4));
            grid.Add(LayoutDemos.Column("sm 2, md 4, lg 4", sm: 2, md: 4, lg: 4));
            grid.Add(LayoutDemos.Column("Hidden below md", sm: 0, md: 8, lg: 16));
        }

        OffsetGrid.Add(LayoutDemos.Column("Span 8, offset 4 (starts at column 5)", span: 8, offset: 4));
        OffsetGrid.Add(LayoutDemos.Column("Span 4, offset 2", span: 4, offset: 2));
        OffsetGrid.Add(LayoutDemos.Column("Span 4, offset 12", span: 4, offset: 12));

        foreach (var grid in new[] { WideGrid, NarrowGrid, CondensedGrid })
            for (var i = 0; i < 4; i++) grid.Add(LayoutDemos.Column("lg 4", lg: 4, sm: 2, md: 4));

        for (var i = 1; i <= 16; i++) NestedGrid.Add(LayoutDemos.Column(i.ToString(), span: 1));
        var wideSubgrid = new CdsGrid();
        for (var i = 0; i < 2; i++) wideSubgrid.Add(LayoutDemos.Column("Subgrid, span 4", span: 4));
        NestedGrid.Add(new CdsColumn { Span = 8, Content = wideSubgrid, Margin = new Thickness(0, 8, 0, 0) });
        var narrowSubgrid = new CdsGrid { Mode = CdsGridMode.Narrow };
        for (var i = 0; i < 4; i++) narrowSubgrid.Add(LayoutDemos.Column("Narrow, 2", span: 2));
        NestedGrid.Add(new CdsColumn { Span = 8, Content = narrowSubgrid, Margin = new Thickness(0, 8, 0, 0) });

        var title = new CdsStack { Gap = 2 };
        title.Add(new CdsText("Clusters", CdsTypeStyle.Heading04));
        title.Add(new CdsText("Manage worker pools and access for every cluster.", CdsTypeStyle.Body01, CdsTextTone.Secondary));
        PageGrid.Add(new CdsColumn { Content = title, Span = 16 });

        var filters = new CdsStack { Gap = 5, Padding = new Thickness(16) };
        filters.Add(new CdsText("Filters", CdsTypeStyle.HeadingCompact01));
        filters.Add(new CdsText("Region: all", CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary));
        filters.Add(new CdsText("Status: running", CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary));
        CdsTokens.SetColor(filters, VisualElement.BackgroundColorProperty, CdsColorKeys.Layer01);
        PageGrid.Add(new CdsColumn { Content = filters, SpanSm = 4, SpanMd = 8, SpanLg = 4, Margin = new Thickness(0, 24, 0, 0) });

        var content = new CdsStack { Gap = 5, Padding = new Thickness(16) };
        content.Add(new CdsText("prod-eu-de-1", CdsTypeStyle.Heading02));
        content.Add(new CdsText("3 worker pools, 12 nodes, last scaled 2 hours ago.", CdsTypeStyle.Body01, CdsTextTone.Secondary));
        CdsTokens.SetColor(content, VisualElement.BackgroundColorProperty, CdsColorKeys.Layer01);
        PageGrid.Add(new CdsColumn { Content = content, SpanSm = 4, SpanMd = 8, SpanLg = 12, Margin = new Thickness(0, 24, 0, 0) });
    }
}
