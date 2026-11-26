namespace Maui.Carbon.Tests.Components;

public class NetworkDiagramTests
{
    static List<CdsNetworkNode> Pipeline() =>
    [
        new("repo", 128, 60, "Repository", "main branch") { Color = CdsColorKeys.SupportInfo },
        new("build", 408, 60, "Build", "4 min 12 s"),
        new("test", 408, 180, "Test suite", "1,284 passed") { Label = "Stacked: 3 shards", IsStacked = true },
        new("stage", 688, 60, "Staging", "v2.14.0") { IsClickable = true },
    ];

    static List<CdsNetworkEdge> Edges() =>
    [
        new("repo", "build", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight),
        new("build", "test", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight),
        new("build", "stage", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight) { Variant = CdsNetworkEdgeVariant.DashMd, Label = "Promote" },
    ];

    static CdsNetworkDiagram Diagram() => new() { Data = Pipeline(), Edges = Edges() };

    static readonly ChartPaint Halo = ChartPaint.OfKey(CdsColorKeys.LayerBackground01);

    [Fact]
    public void Shape_node_text_has_a_halo_of_the_surface_so_edges_never_cross_it()
    {
        var nodes = new List<CdsNetworkNode>
        {
            new("a", 40, 40, "App server 1") { Kind = CdsNetworkNodeKind.Shape, Subtitle = "10.0.1.4" },
            new("b", 300, 40, "Database") { Kind = CdsNetworkNodeKind.Shape },
        };
        foreach (var width in new[] { 400f, 200f })
        {
            var scene = new CdsNetworkDiagram { Data = nodes, Edges = [new("a", "b")] }.RenderScene(width, 200);
            foreach (var label in new[] { "App server 1", "10.0.1.4", "Database" })
            {
                var copies = scene.Texts.Where(t => t.Text == label).ToList();
                Assert.Equal(9, copies.Count);
                var text = copies[^1];
                Assert.Equal(ChartPaint.OfKey(CdsColorKeys.TextPrimary), text.Color);
                // The halo is drawn first, around the label at its size.
                Assert.All(copies[..^1], h =>
                {
                    Assert.Equal(Halo, h.Color);
                    Assert.Equal(text.Font, h.Font);
                    Assert.True(MathF.Abs(h.X - text.X) <= 1.51f && MathF.Abs(h.Y - text.Y) <= 1.51f);
                });
            }
        }
        // Card text sits on the card's own fill and has no halo.
        Assert.Single(Diagram().RenderScene(900, 300).Texts, t => t.Text == "Repository");
    }

    [Fact]
    public void A_heavily_scaled_card_keeps_its_text_inside_the_card()
    {
        var nodes = new List<CdsNetworkNode>
        {
            new("prod", 128, 60, "Production", "Awaiting approval") { Color = CdsColorKeys.SupportWarning },
            new("stage", 688, 60, "Staging environment for release candidates", "v2.14.0"),
        };
        var scene = new CdsNetworkDiagram { Data = nodes }.RenderScene(300, 300);
        var k = 300f / 812;
        var cards = scene.Shapes.OfType<ChartRect>().Where(r => r.Fill == ChartPaint.OfKey(CdsColorKeys.Layer01)).Select(r => r.Rect).ToList();
        Assert.Equal(2, cards.Count);
        Assert.NotEmpty(scene.Texts);
        foreach (var t in scene.Texts)
        {
            var card = cards.Single(c => t.X >= c.Left && t.X <= c.Right);
            Assert.True(t.X + t.Width <= card.Right - 16 * k + 0.5f, $"{t.Text} overflows its card");
            Assert.True(t.Font.Size >= 12);
        }
        // The long title is cut with an ellipsis; the card keeps its first line, and lines below the card are dropped.
        Assert.Contains(scene.Texts, t => t.Text.StartsWith("Staging", StringComparison.Ordinal) && t.Text.EndsWith('…'));
        Assert.DoesNotContain(scene.Texts, t => t.Text == "Awaiting approval");
        Assert.Equal(2, scene.Texts.Count);
        // Unscaled, nothing is cut.
        var full = new CdsNetworkDiagram { Data = nodes }.RenderScene(900, 300);
        Assert.Contains(full.Texts, t => t.Text == "Awaiting approval");
        Assert.Equal("Production", CdsNetworkDiagram.Ellipsize("Production", new ChartFont(ChartFontFamily.SansSemiBold, 14), 500));
        Assert.Equal("…", CdsNetworkDiagram.Ellipsize("Production", new ChartFont(ChartFontFamily.SansSemiBold, 14), 1));
    }

    [Fact]
    public void Cards_are_layer_01_with_a_four_pixel_left_border_and_their_text()
    {
        var scene = Diagram().RenderScene(900, 300);
        Assert.Equal(7, scene.Marks.Count);
        Assert.Equal("4 nodes, 3 edges", scene.Summary);
        var cards = scene.Shapes.OfType<ChartRect>().Where(r => r.Fill == ChartPaint.OfKey(CdsColorKeys.Layer01)).ToList();
        Assert.Equal(4, cards.Count);
        Assert.Equal(new RectF(20, 26, 216, 68), cards[0].Rect);
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.Rect == new RectF(20, 26, 4, 68) && r.Fill == ChartPaint.OfKey(CdsColorKeys.SupportInfo));
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.Rect.Width == 4 && r.Fill == ChartPaint.OfKey(CdsColorKeys.BorderInverse));
        var title = scene.Texts.Single(t => t.Text == "Repository");
        Assert.Equal(40, title.X);
        Assert.Equal(42, title.Y);
        Assert.Equal(new ChartFont(ChartFontFamily.SansSemiBold, 14), title.Font);
        Assert.Equal(108, NetworkHeight(scene, "Test suite"));
        Assert.Equal(4, scene.Shapes.OfType<ChartRect>().Count(r => r.Fill == ChartPaint.OfKey(CdsColorKeys.LayerAccent01)));
        Assert.Equal("Repository, main branch", scene.Marks[0].SpokenText);
    }

    static float NetworkHeight(ChartScene scene, string title)
    {
        var t = scene.Texts.Single(x => x.Text == title);
        return scene.Shapes.OfType<ChartRect>().First(r => r.Fill == ChartPaint.OfKey(CdsColorKeys.Layer01) && r.Rect.Contains(t.X, t.Y + 1)).Rect.Height;
    }

    [Fact]
    public void Elbow_edges_join_facing_sides_and_end_markers_touch_the_target()
    {
        var scene = Diagram().RenderScene(900, 300);
        var inner = scene.Shapes.OfType<ChartPath>().Where(p => p.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderStrong01)).ToList();
        Assert.Equal(3, inner.Count);
        Assert.All(inner, p => Assert.Equal(2.5f, p.HoverStrokeWidth));
        Assert.Equal([4f, 4f], inner[2].Dash!);
        var arrow = scene.Shapes.OfType<ChartPath>().First(p => p.Fill == ChartPaint.OfKey(CdsColorKeys.BorderInverse));
        var bounds = arrow.Path.GetBoundsByFlattening();
        // repo's right side is at 236, build's left side at 300: the arrow tip stops half a pixel short.
        Assert.Equal(299.5, bounds.Right, 0.05);
        Assert.Equal(56, bounds.Top, 0.05);
        Assert.Equal(64, bounds.Bottom, 0.05);
        var (_, points) = CdsNetworkDiagram.EdgeGeometry(Edges()[1], CdsNetworkDiagram.Box(Pipeline()[1]), CdsNetworkDiagram.Box(Pipeline()[2]));
        Assert.Equal(new PointF(408, 94), points[0]);
        Assert.Equal(new PointF(408, 126), points[^1]);
        var promote = scene.Marks.Single(m => m.Title == "Promote");
        Assert.Equal("Build to Staging: Promote", promote.SpokenText);
    }

    [Fact]
    public void Straight_edges_end_at_outlines_and_shape_nodes_draw_icons()
    {
        var nodes = new List<CdsNetworkNode>
        {
            new() { Id = "user", X = 60, Y = 120, Title = "Users", Kind = CdsNetworkNodeKind.Shape, Icon = CdsIconName.User },
            new() { Id = "lb", X = 230, Y = 120, Title = "Load balancer", Subtitle = "10.0.0.1", Kind = CdsNetworkNodeKind.Shape, Shape = CdsNetworkNodeShape.RoundedSquare, Icon = CdsIconName.ArrowsVertical },
        };
        var edge = new CdsNetworkEdge("user", "lb") { Variant = CdsNetworkEdgeVariant.Tunnel, MarkerStart = CdsNetworkMarker.Circle, MarkerEnd = CdsNetworkMarker.Diamond };
        var (_, pts) = CdsNetworkDiagram.EdgeGeometry(edge, CdsNetworkDiagram.Box(nodes[0]), CdsNetworkDiagram.Box(nodes[1]));
        Assert.Equal(new PointF(84, 120), pts[0]);
        Assert.Equal(new PointF(206, 120), pts[1]);
        var scene = new CdsNetworkDiagram { Data = nodes, Edges = [edge] }.RenderScene(400, 250);
        Assert.Contains(scene.Shapes.OfType<ChartCircle>(), c => c.Radius == 24 && c.Fill == ChartPaint.OfKey(CdsColorKeys.Layer01));
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.CornerRadius == 8);
        Assert.Contains(scene.Shapes.OfType<ChartPath>(), p => p.StrokeWidth == 6 && p.Stroke == ChartPaint.OfKey(CdsColorKeys.LayerAccent01));
        Assert.True(scene.Shapes.OfType<ChartPath>().Count(p => p.Fill == ChartPaint.OfKey(CdsColorKeys.TextPrimary)) >= 2);
        Assert.Contains(scene.Shapes.OfType<ChartCircle>(), c => c.Fill == ChartPaint.OfKey(CdsColorKeys.BorderInverse));
        var title = scene.Texts.Single(t => t.Text == "Load balancer" && t.Color != Halo);
        Assert.Equal(ChartTextAnchor.Middle, title.Anchor);
        Assert.Equal(148, title.Y);
        Assert.Equal(167, scene.Texts.Single(t => t.Text == "10.0.0.1" && t.Color != Halo).Y);
    }

    [Fact]
    public void Double_edges_and_free_edges()
    {
        var free = new CdsNetworkEdge { Start = new Point(8, 12), End = new Point(118, 12), Variant = CdsNetworkEdgeVariant.Double, MarkerEnd = CdsNetworkMarker.ArrowRight };
        var scene = new CdsNetworkDiagram { Edges = [free] }.RenderScene(240, 24);
        Assert.Single(scene.Marks);
        Assert.Contains(scene.Shapes.OfType<ChartPath>(), p => p.StrokeWidth == 4.5f && p.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderInverse));
        Assert.Contains(scene.Shapes.OfType<ChartPath>(), p => p.StrokeWidth == 1 && p.Stroke == ChartPaint.OfKey(CdsColorKeys.Background));
        var arrow = scene.Shapes.OfType<ChartPath>().Single(p => p.Fill == ChartPaint.OfKey(CdsColorKeys.BorderInverse));
        // A building-block marker is centred on the end.
        Assert.Equal(118 + 7 - 4.25, arrow.Path.GetBoundsByFlattening().Right, 0.05);
    }

    [Fact]
    public void A_diagram_wider_than_the_chart_scales_down_to_fit()
    {
        var full = Diagram().RenderScene(900, 300);
        var half = Diagram().RenderScene(398, 300);
        var k = 398f / 796;
        Assert.Equal(full.Shapes.OfType<ChartRect>().First().Rect.Width * k, half.Shapes.OfType<ChartRect>().First().Rect.Width, 0.01);
        // Text shrinks no further than 12px.
        Assert.Equal(12, half.Texts.Single(t => t.Text == "Repository").Font.Size, 0.01);
        Assert.Equal(full.Marks[0].Bounds.Right * k, half.Marks[0].Bounds.Right, 0.01);
        Assert.True(half.Texts.All(t => t.X <= 398));
        // Casing and line share one path, scaled once.
        var tunnel = new CdsNetworkDiagram { Data = Pipeline(), Edges = [new("repo", "build") { Variant = CdsNetworkEdgeVariant.Tunnel }] }.RenderScene(398, 300);
        var casing = tunnel.Shapes.OfType<ChartPath>().First(p => p.StrokeWidth == 6 * k);
        Assert.Equal(236 * k, casing.Path.GetBoundsByFlattening().Left, 0.05);
    }

    [Fact]
    public void Clickable_nodes_hover_and_raise_node_clicked()
    {
        var diagram = Diagram();
        CdsNetworkNode? clicked = null;
        diagram.NodeClicked += (_, e) => clicked = e.Node;
        var scene = diagram.RenderScene(900, 300);
        var stage = scene.Marks[3];
        Assert.Equal("Staging, v2.14.0, button", stage.SpokenText);
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.Mark == stage.Id && r.HoverFill == ChartPaint.OfKey(CdsColorKeys.LayerHover01));
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.Mark == stage.Id && r.HoverOnly && r.Stroke == ChartPaint.OfKey(CdsColorKeys.Focus));
        diagram.ActivateMark(scene.Marks[0].Id);
        Assert.Null(clicked);
        diagram.ActivateMark(stage.Id);
        Assert.Equal("stage", clicked?.Id);
    }

    [Fact]
    public void Content_size_table_and_every_property()
    {
        var diagram = Diagram();
        Assert.Equal(new Size(812, 250), diagram.ContentSize());
        var table = diagram.RenderTable();
        Assert.Equal(["Source", "Target", "Label"], table.Headers);
        Assert.Equal(["Build", "Staging", "Promote"], table.Rows[2]);
        Assert.Equal("No data", new CdsNetworkDiagram().RenderScene(300, 200).Summary);
        var node = new CdsNetworkNode { Id = "a", X = 100, Y = 50, Kind = CdsNetworkNodeKind.Card, Title = "A", Subtitle = "b", Label = "c", Description = "d", Shape = CdsNetworkNodeShape.Square, Icon = CdsIconName.Catalog, Size = 40, Width = 180, Height = 90, Color = "not-a-key", IsStacked = true, IsClickable = true, Tag = 1 };
        var edge = new CdsNetworkEdge { Source = "a", Target = "missing", Path = CdsNetworkEdgePath.Bezier, BendPercent = 0.3, Variant = CdsNetworkEdgeVariant.DashXl, MarkerStart = CdsNetworkMarker.Tee, MarkerEnd = CdsNetworkMarker.Square, Color = CdsColorKeys.SupportError, Label = "x" };
        var scene = new CdsNetworkDiagram { Data = new[] { node }, Edges = [edge], Title = "Graph" }.RenderScene(400, 200);
        Assert.Single(scene.Marks);
    }

    [Fact]
    public void Edges_into_a_stacked_card_end_at_its_outermost_outline()
    {
        var scene = Diagram().RenderScene(900, 300);
        // build (408, 60) to the stacked test card (408, 180), 108 tall: its top is at 126 and its outlines rise 11 above it.
        var arrow = scene.Shapes.OfType<ChartPath>().Single(p => p.Mark == 5 && p.Fill == ChartPaint.OfKey(CdsColorKeys.BorderInverse));
        Assert.Equal(126 - 11 - 0.5, arrow.Path.GetBoundsByFlattening().Bottom, 0.05);
        var box = CdsNetworkDiagram.EdgeBox(Pipeline()[2]);
        Assert.Equal(115, box.Top);
        Assert.Equal(408 + 108 + 11, box.Right);
        Assert.Equal(300, box.Left);
        Assert.Equal(234, box.Bottom);
    }

    [Fact]
    public void A_scaled_diagram_is_centred_and_keeps_text_lines_apart()
    {
        var nodes = new List<CdsNetworkNode>
        {
            new("users", 40, 40, "Users") { Kind = CdsNetworkNodeKind.Shape, Shape = CdsNetworkNodeShape.Circle, Description = "10.0.1.4" },
            new("db", 760, 40, "Database") { Kind = CdsNetworkNodeKind.Shape, Description = "10.0.1.5" },
        };
        var scene = new CdsNetworkDiagram { Data = nodes, Edges = [new("users", "db")] }.RenderScene(392, 300);
        var k = 392f / 784;
        var circle = scene.Shapes.OfType<ChartCircle>().First();
        Assert.Equal(24 * k, circle.Radius, 0.01);
        // Centred vertically: the content (16 to 120) scaled by k leaves equal room above and below.
        Assert.Equal(300 - 120 * k - (150 - 68 * k), 16 * k + (150 - 68 * k), 0.5);
        var title = scene.Texts.Single(t => t.Text == "Users" && t.Color != Halo);
        var ip = scene.Texts.Single(t => t.Text == "10.0.1.4" && t.Color != Halo);
        Assert.Equal(12, title.Font.Size, 0.01);
        Assert.Equal(12, ip.Font.Size, 0.01);
        Assert.True(ip.Y - title.Y >= title.Font.LineHeight);
        Assert.Equal(circle.Center.Y + circle.Radius + 4, title.Y, 0.01);
    }
}
