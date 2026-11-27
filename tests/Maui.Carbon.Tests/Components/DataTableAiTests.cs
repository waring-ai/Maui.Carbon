using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

/// <summary>
/// AI presence in a data table (Carbon's AI label on a column, on a row and on the whole table): the column decorator, the per-row
/// decorator with the AI row fills, and the table decorator with the whole-table treatment.
/// </summary>
public class DataTableAiTests
{
    sealed class Cluster(string name, bool ai) : System.ComponentModel.INotifyPropertyChanged
    {
        bool _ai = ai;
        bool _edited;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; } = name;

        public bool IsAi
        {
            get => _ai;
            set { _ai = value; PropertyChanged?.Invoke(this, new(nameof(IsAi))); }
        }

        public bool IsEdited
        {
            get => _edited;
            set { _edited = value; PropertyChanged?.Invoke(this, new(nameof(IsEdited))); }
        }
    }

    // An element holds its parent weakly: keep the theme alive as long as the view.
    static readonly System.Runtime.CompilerServices.ConditionalWeakTable<View, CdsTheme> s_themes = [];

    static T Themed<T>(T view) where T : View
    {
        s_themes.Add(view, new CdsTheme { Theme = CdsThemeName.White, Body = view });
        return view;
    }

    static Color Token(string key) => CdsPalettes.GetTokenColor(CdsThemeName.White, key);

    static List<Cluster> Clusters(int count, Func<int, bool>? ai = null) =>
        Enumerable.Range(0, count).Select(i => new Cluster($"cluster-{i:D2}", ai?.Invoke(i) ?? i % 2 == 0)).ToList();

    static DataTemplate RowLabel() => new(() =>
    {
        var label = new CdsAILabel();
        label.SetBinding(VisualElement.IsVisibleProperty, nameof(Cluster.IsAi));
        label.SetBinding(CdsAILabel.IsRevertActiveProperty, nameof(Cluster.IsEdited));
        return label;
    });

    static CdsDataTable Table(int rows = 4, bool rowLabels = true, bool sticky = false, Func<int, bool>? ai = null)
    {
        var table = new CdsDataTable { StickyHeader = sticky, RowKeySelector = r => ((Cluster)r).Name, RowNameSelector = r => ((Cluster)r).Name };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", ValueSelector = r => ((Cluster)r).Name });
        table.Headers.Add(new CdsDataTableHeader { Key = "score", Header = "Score", ValueSelector = r => ((Cluster)r).Name.Length });
        if (rowLabels) table.RowDecoratorTemplate = RowLabel();
        table.ItemsSource = Clusters(rows, ai);
        return Themed(table);
    }

    static IEnumerable<T> All<T>(Element root) where T : Element => ContainerSemanticsAuditTests.Walk(root).OfType<T>();

    // ----- tokens and gradients (pure) -----

    [Fact]
    public void An_ai_row_fill_is_layer_01_under_the_aura_hover_background_when_hovered_and_layer_selected_when_selected()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsTableTokens.AiRowFill(selected: false, hover: false));
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsTableTokens.AiRowFill(selected: true, hover: false));
        Assert.Equal(CdsColorKeys.AiAuraHoverBackground, CdsTableTokens.AiRowFill(selected: false, hover: true));
        // Carbon's hover rules win over the selected ones.
        Assert.Equal(CdsColorKeys.AiAuraHoverBackground, CdsTableTokens.AiRowFill(selected: true, hover: true));
    }

    [Fact]
    public void The_rule_of_a_hovered_or_selected_ai_row_is_border_subtle_and_other_rows_keep_theirs()
    {
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsTableTokens.RowRule(selected: true, hover: false, zebra: false, zebraStripe: false, ai: true));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsTableTokens.RowRule(selected: false, hover: true, zebra: false, zebraStripe: false, ai: true));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsTableTokens.RowRule(false, false, false, false, ai: true));
        // Not an AI row: unchanged.
        foreach (var (selected, hover, zebra, stripe) in new[] { (true, false, false, false), (false, true, false, false), (false, false, true, true), (false, false, true, false), (false, false, false, false) })
            Assert.Equal(CdsTableTokens.RowRule(selected, hover, zebra, stripe), CdsTableTokens.RowRule(selected, hover, zebra, stripe, ai: false));
    }

    [Fact]
    public void The_decorator_column_is_16_wide_and_pushes_a_child_rows_content_by_its_width()
    {
        Assert.Equal(16, CdsTableTokens.DecoratorColumnWidth);
        Assert.Equal(56, CdsTableTokens.ChildContentInset(56, decoratorColumn: false));
        Assert.Equal(72, CdsTableTokens.ChildContentInset(56, decoratorColumn: true));
        Assert.Equal(104, CdsTableTokens.ChildContentInset(CdsTableTokens.ChildInset(CdsDataTableSelection.Multi), decoratorColumn: true));
    }

    [Fact]
    public void The_row_gradient_is_the_aura_at_the_start_edge_fading_to_nothing_by_the_middle()
    {
        var aura = Color.FromUint(0x294589FF);
        var brush = CdsAiTableGradient.Row(aura);
        Assert.Equal(new Point(0, 0), brush.StartPoint);
        Assert.Equal(new Point(1, 0), brush.EndPoint);
        var stops = brush.GradientStops;
        Assert.Equal(0f, stops[0].Offset);
        Assert.Equal(aura.Alpha, stops[0].Color.Alpha, 3);
        Assert.Equal(0.5f, stops[1].Offset);
        Assert.Equal(0f, stops[1].Color.Alpha);
        Assert.All(stops, s => { Assert.Equal(aura.Red, s.Color.Red); Assert.Equal(aura.Blue, s.Color.Blue); });
        Assert.Equal(0f, stops[^1].Color.Alpha);
    }

    [Fact]
    public void The_hover_gradient_reaches_half_its_alpha_at_the_15_percent_hint_and_nothing_at_50()
    {
        var aura = Color.FromUint(0x664589FF);
        var stops = CdsAiTableGradient.RowHover(aura).GradientStops;
        Assert.Equal(aura.Alpha, stops[0].Color.Alpha, 3);
        var hint = stops.First(s => Math.Abs(s.Offset - 0.15f) < 0.001f);
        Assert.Equal(aura.Alpha / 2, hint.Color.Alpha, 2);
        Assert.Equal(0f, stops.First(s => Math.Abs(s.Offset - 0.5f) < 0.001f).Color.Alpha, 3);
        // Monotonic: the aura only fades.
        for (var i = 1; i < stops.Count; i++) Assert.True(stops[i].Color.Alpha <= stops[i - 1].Color.Alpha + 0.0001f);
    }

    [Fact]
    public void The_whole_table_gradient_rises_from_the_bottom_edge()
    {
        var brush = CdsAiTableGradient.FullTable(Color.FromUint(0x1A4589FF));
        Assert.Equal(new Point(0, 1), brush.StartPoint);
        Assert.Equal(new Point(0, 0), brush.EndPoint);
        Assert.Equal(0.5f, brush.GradientStops[1].Offset);
        Assert.Equal(0f, brush.GradientStops[1].Color.Alpha);
    }

    // ----- column decorator -----

    [Fact]
    public void An_ai_label_in_a_column_header_is_mini_sits_at_the_end_of_the_cell_and_tints_the_column()
    {
        var table = Table(rowLabels: false);
        var label = new CdsAILabel { Size = CdsAILabelSize.Lg };
        table.Headers[1].Decorator = label;

        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        var host = Assert.IsType<Grid>(label.Parent);
        Assert.Equal(1, Grid.GetColumn(label));
        Assert.Equal(LayoutOptions.End, label.HorizontalOptions);
        var (layer, line) = table.Table.HeaderAiOf(1);
        Assert.NotNull(layer);
        Assert.NotNull(line);
        Assert.IsType<LinearGradientBrush>(layer!.Background);
        Assert.True(layer.InputTransparent);
        Assert.Equal(1, line!.HeightRequest);
        Assert.Equal(Token("ai-border-strong"), line.Color);
        Assert.Equal((null, null), table.Table.HeaderAiOf(0));
        Assert.Same(host.Parent, layer.Parent);
    }

    [Fact]
    public void The_body_cells_of_an_ai_column_carry_the_gradient_and_the_others_do_not()
    {
        var table = Table(rowLabels: false);
        table.Headers[0].Decorator = new CdsAILabel();
        foreach (var index in new[] { 0, 1, 3 })
        {
            var hosts = table.Table.AiHostsOf(index);
            var host = Assert.Single(hosts);
            Assert.IsType<LinearGradientBrush>(host.Background);
            Assert.Contains(All<CdsText>(host), t => t.Text == $"cluster-{index:D2}");
        }
    }

    [Fact]
    public void The_ai_column_styling_stays_while_the_label_shows_its_revert_button()
    {
        var table = Table(rowLabels: false);
        var label = new CdsAILabel();
        table.Headers[1].Decorator = label;
        label.IsRevertActive = true;
        Assert.NotNull(table.Table.HeaderAiOf(1).Layer);
        Assert.IsType<LinearGradientBrush>(table.Table.HeaderAiOf(1).Layer!.Background);
        Assert.Single(table.Table.AiHostsOf(0));
    }

    [Fact]
    public void A_header_decorator_that_is_not_an_ai_label_is_placed_without_ai_styling()
    {
        var table = Table(rowLabels: false);
        var tip = new CdsToggletip();
        table.Headers[1].Decorator = tip;
        Assert.Equal(1, Grid.GetColumn(tip));
        Assert.Equal((null, null), table.Table.HeaderAiOf(1));
        Assert.Empty(table.Table.AiHostsOf(0));
    }

    [Fact]
    public void The_decorator_of_a_sortable_header_sits_above_the_sort_button_and_leaves_its_hit_area_alone()
    {
        var table = Table(rowLabels: false);
        table.IsSortable = true;
        var label = new CdsAILabel();
        table.Headers[1].Decorator = label;
        var host = (Grid)label.Parent!;
        var hit = host.Children.OfType<CdsHitTarget>().Single();
        Assert.Equal(0, Grid.GetColumn(hit));
        Assert.True(host.Children.IndexOf(label) > host.Children.IndexOf(hit));
        // Activating the sort button still sorts.
        hit.RaiseKeyboardActivation();
        Assert.Equal("score", table.SortKey);
    }

    [Fact]
    public void Removing_the_decorator_takes_the_column_styling_away()
    {
        var table = Table(rowLabels: false);
        var label = new CdsAILabel();
        table.Headers[1].Decorator = label;
        table.Headers[1].Decorator = null;
        Assert.Equal((null, null), table.Table.HeaderAiOf(1));
        Assert.Empty(table.Table.AiHostsOf(0));
    }

    // ----- row decorator -----

    [Fact]
    public void A_row_decorator_template_adds_a_16_wide_column_first_and_leaves_the_cells_alone()
    {
        var table = Table();
        Assert.True(table.Table.HasDecoratorColumn);
        Assert.Equal(3, table.Table.HeaderCount);
        var row = table.Table.Rows[0];
        Assert.Equal(2, row.Cells.Count);
        Assert.Equal("cluster-00", row.Cells[0].Text);
        var label = Assert.IsType<CdsAILabel>(row.Decorator);
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        var host = Assert.IsType<Grid>(label.Parent);
        var body = (Grid)host.Parent!;
        var columns = body.ColumnDefinitions;
        Assert.Equal(new GridLength(16), columns[0].Width);
        Assert.Equal(0, Grid.GetColumn(host));
    }

    [Fact]
    public void Without_a_template_the_table_has_no_decorator_column_and_builds_nothing_for_ai()
    {
        var table = Table(rowLabels: false);
        Assert.False(table.Table.HasDecoratorColumn);
        Assert.Equal(2, table.Table.HeaderCount);
        for (var i = 0; i < 4; i++)
        {
            Assert.Null(table.Table.AiLayerOf(i));
            Assert.Empty(table.Table.AiHostsOf(i));
        }
        Assert.Null(table.Table.AiOverlay);
    }

    [Fact]
    public void Rows_with_a_shown_label_are_ai_rows_and_the_others_are_not()
    {
        var table = Table();
        for (var i = 0; i < 4; i++)
        {
            var layer = table.Table.AiLayerOf(i);
            if (i % 2 == 0)
            {
                Assert.NotNull(layer);
                Assert.True(layer!.IsVisible);
                Assert.IsType<LinearGradientBrush>(layer.Background);
                Assert.True(layer.InputTransparent);
                Assert.Equal(1, Assert.IsType<BoxView>(Assert.Single(layer.Children)).WidthRequest);
            }
            else
            {
                Assert.True(layer is null or { IsVisible: false });
            }
        }
    }

    [Fact]
    public void Showing_or_hiding_the_label_of_a_bound_row_updates_the_row()
    {
        var table = Table();
        var rows = table.Table.Rows;
        Assert.True(table.Table.AiLayerOf(1) is null or { IsVisible: false });
        ((CdsAILabel)rows[1].Decorator!).IsVisible = true;
        Assert.True(table.Table.AiLayerOf(1)!.IsVisible);
        ((CdsAILabel)rows[0].Decorator!).IsVisible = false;
        Assert.False(table.Table.AiLayerOf(0)!.IsVisible);
    }

    [Fact]
    public void A_row_stays_an_ai_row_while_its_label_shows_the_revert_button()
    {
        var table = Table();
        var label = (CdsAILabel)table.Table.Rows[0].Decorator!;
        label.IsRevertActive = true;
        Assert.True(table.Table.AiLayerOf(0)!.IsVisible);
        Assert.Equal(Token("layer-01"), table.Table.FillOf(0)!.Color);
    }

    [Fact]
    public void Ai_rows_use_the_ai_fills_for_hover_and_selected_and_the_edge_color_is_ai_border_strong()
    {
        var table = Table();
        table.Selection = CdsDataTableSelection.Multi;
        var items = (List<Cluster>)table.ItemsSource!;
        var surface = table.Table;
        Assert.Equal(Token("layer-01"), surface.FillOf(0)!.Color);
        Assert.Equal(Token("border-subtle-01"), surface.RuleOf(0)!.Color);
        Assert.Equal(Token("ai-border-strong"), ((BoxView)surface.AiLayerOf(0)!.Children[0]).Color);

        table.SelectedRows = new List<object> { items[0] };
        Assert.Equal(Token("layer-selected-01"), surface.FillOf(0)!.Color);
        Assert.Equal(Token("border-subtle-01"), surface.RuleOf(0)!.Color);

        surface.HoverRow(0);
        Assert.Equal(Token("ai-aura-hover-background"), surface.FillOf(0)!.Color);
        var hover = ((LinearGradientBrush)surface.AiLayerOf(0)!.Background).GradientStops;
        Assert.Contains(hover, s => Math.Abs(s.Offset - 0.15f) < 0.001f);

        // A row that is not AI keeps the plain hover fill.
        surface.HoverRow(1);
        Assert.Equal(Token("layer-hover-01"), surface.FillOf(1)!.Color);
        Assert.True(surface.AiLayerOf(1) is null or { IsVisible: false });
    }

    [Fact]
    public void An_ai_row_ignores_the_zebra_stripe()
    {
        var table = Table(ai: i => i == 1 || i == 2);
        table.UseZebraStyles = true;
        Assert.Equal(Token("layer-01"), table.Table.FillOf(1)!.Color); // odd row: would be striped
        Assert.Equal(Token("layer-accent-01"), table.Table.FillOf(3)!.Color);
        Assert.Equal(Token("layer-01"), table.Table.FillOf(2)!.Color);
    }

    [Fact]
    public void The_child_row_of_an_expanded_ai_row_takes_the_ai_tint()
    {
        var table = Table();
        table.IsExpandable = true;
        table.ExpandedRowTemplate = new DataTemplate(() => new CdsText { Text = "details" });
        var items = (List<Cluster>)table.ItemsSource!;
        table.ExpandedRows = new List<object> { items[0], items[1] };
        Assert.True(table.Table.ChildAiOf(0)!.IsVisible);
        Assert.IsType<LinearGradientBrush>(table.Table.ChildAiOf(0)!.Background);
        Assert.True(table.Table.ChildAiOf(1) is null or { IsVisible: false });
        // The content starts after the decorator column as well.
        Assert.Equal(56 + 16, table.Table.ChildHostOf(0)!.Padding.Left);
    }

    [Fact]
    public void The_row_label_is_named_by_its_row()
    {
        var table = Table();
        var label = (CdsAILabel)table.Table.Rows[2].Decorator!;
        var description = SemanticProperties.GetDescription(label);
        Assert.Contains("cluster-02", description);
        Assert.StartsWith("AI", description);
        // Not on the row's grid or the table: that would make each one VoiceOver element.
        Assert.True(string.IsNullOrEmpty(SemanticProperties.GetDescription(table)));
        Assert.True(string.IsNullOrEmpty(SemanticProperties.GetDescription((Element)label.Parent!.Parent!)));
    }

    [Fact]
    public void Sorting_re_binds_the_labels_to_the_rows_they_show()
    {
        var table = Table(ai: i => i == 0);
        table.IsSortable = true;
        table.SortKey = "name";
        table.SortDirection = CdsSortDirection.Descending;
        // cluster-00 is the last row now.
        Assert.False(table.Table.AiLayerOf(0) is { IsVisible: true });
        Assert.True(table.Table.AiLayerOf(3)!.IsVisible);
        Assert.Contains("cluster-00", SemanticProperties.GetDescription(table.Table.Rows[3].Decorator!));
    }

    // ----- virtualization -----

    [Fact]
    public void A_pooled_slot_re_bound_from_an_ai_row_to_another_row_clears_the_ai_styling()
    {
        var table = Table(rows: 200, sticky: true, ai: i => i < 5);
        Assert.True(table.Table.IsVirtual);
        var slot = table.Table.RealizedRows.First(r => r.Index == 0).Row;
        Assert.True(table.Table.AiLayerOf(0)!.IsVisible);

        table.Table.RealizeAt(table.Table.RowTop(150));

        Assert.DoesNotContain(table.Table.RealizedRows, r => r.Index == 0);
        var rebound = table.Table.RealizedRows.First(r => ReferenceEquals(r.Row, slot));
        Assert.True(rebound.Index >= 100);
        Assert.True(table.Table.AiLayerOf(rebound.Index) is null or { IsVisible: false });
        Assert.Equal(Token("layer-01"), table.Table.FillOf(rebound.Index)!.Color);

        table.Table.RealizeAt(0);
        Assert.True(table.Table.AiLayerOf(0)!.IsVisible);
    }

    [Fact]
    public void A_virtualized_table_builds_ai_views_only_for_the_slots_that_show_an_ai_row()
    {
        var table = Table(rows: 300, sticky: true, ai: i => i == 2);
        var realized = table.Table.RealizedRows;
        var withLayer = realized.Count(r => table.Table.AiLayerOf(r.Index) is not null);
        Assert.Equal(1, withLayer);
    }

    [Fact]
    public void Re_binding_slots_keeps_one_decorator_per_slot()
    {
        var table = Table(rows: 300, sticky: true, ai: _ => true);
        var before = table.Table.RealizedRows.Select(r => r.Row.Decorator).ToHashSet();
        for (var scroll = 0; scroll < 6; scroll++) table.Table.RealizeAt(table.Table.RowTop(40 * scroll + 20));
        var after = table.Table.RealizedRows.Select(r => r.Row.Decorator).ToHashSet();
        Assert.Subset(after, before);
        Assert.All(after, d => Assert.IsType<CdsAILabel>(d));
    }

    // ----- whole table -----

    [Fact]
    public void The_table_decorator_sits_at_the_end_of_the_title_block_without_turning_on_the_ai_styling()
    {
        var table = Table(rowLabels: false);
        table.Title = "Recommendations";
        var label = new CdsAILabel { Size = CdsAILabelSize.Sm };
        table.Decorator = label;
        var block = (Grid)label.Parent!;
        Assert.Equal(1, Grid.GetColumn(label));
        Assert.Equal(LayoutOptions.End, label.HorizontalOptions);
        Assert.Equal(LayoutOptions.Start, label.VerticalOptions);
        Assert.Equal(CdsAILabelSize.Sm, label.Size);
        Assert.True(block.IsVisible);
        // The decorator is not aiEnabled: Carbon keeps them apart.
        Assert.False(table.IsAiEnabled);
        Assert.Null(table.Table.AiOverlay);
        Assert.Equal(new Thickness(0), Private.Field<VerticalStackLayout>(table, "_root").Padding);
    }

    [Fact]
    public void A_decorator_alone_shows_the_title_block_at_the_end_and_removing_it_hides_the_block()
    {
        var table = Table(rowLabels: false);
        var block = Private.Field<Grid>(table, "_titleBlock");
        Assert.False(block.IsVisible);
        var label = new CdsAILabel();
        table.Decorator = label;
        Assert.True(block.IsVisible);
        Assert.False(Private.Field<VerticalStackLayout>(table, "_titleStack").IsVisible);
        table.Decorator = null;
        Assert.False(block.IsVisible);
        Assert.DoesNotContain(label, block.Children);
    }

    [Fact]
    public void An_ai_enabled_table_has_a_gradient_border_and_the_glow_over_its_body()
    {
        var table = Table(rowLabels: false);
        table.IsAiEnabled = true;
        var root = Private.Field<VerticalStackLayout>(table, "_root");
        Assert.Equal(new Thickness(1), root.Padding);
        var border = Assert.IsType<LinearGradientBrush>(root.Background);
        Assert.Equal(Token("ai-border-start"), border.GradientStops[0].Color);
        Assert.Equal(Token("ai-border-end"), border.GradientStops[^1].Color);

        var overlay = table.Table.AiOverlay;
        Assert.NotNull(overlay);
        Assert.True(overlay!.InputTransparent);
        Assert.IsType<LinearGradientBrush>(overlay.Background);
        // Over the rows, not the header: starts at the first body row and spans them all.
        Assert.Equal(1, Grid.GetRow(overlay));
        Assert.Equal(4, Grid.GetRowSpan(overlay));

        table.IsAiEnabled = false;
        Assert.Null(table.Table.AiOverlay);
        Assert.Equal(new Thickness(0), root.Padding);
        Assert.True(Brush.IsNullOrEmpty(root.Background));
    }

    [Fact]
    public void The_glow_stays_over_the_rows_when_rows_are_added()
    {
        var table = Table(rowLabels: false);
        table.IsAiEnabled = true;
        ((List<Cluster>)table.ItemsSource!).Add(new Cluster("cluster-99", false));
        table.ItemsSource = ((List<Cluster>)table.ItemsSource!).ToList();
        var overlay = table.Table.AiOverlay!;
        var grid = (Grid)overlay.Parent!;
        Assert.Same(overlay, grid.Children[^1]);
        Assert.Equal(5, Grid.GetRowSpan(overlay));
    }

    [Fact]
    public void In_a_virtualized_body_the_glow_is_the_last_child_and_covers_it()
    {
        var table = Table(rows: 100, rowLabels: false, sticky: true);
        table.IsAiEnabled = true;
        table.Table.RealizeAt(table.Table.RowTop(60));
        var overlay = table.Table.AiOverlay!;
        var body = (AbsoluteLayout)overlay.Parent!;
        Assert.Same(overlay, body.Children[^1]);
        Assert.Equal(new Rect(0, 0, 1, 1), AbsoluteLayout.GetLayoutBounds(overlay));
        Assert.True(overlay.InputTransparent);
    }

    // ----- the declarative table -----

    [Fact]
    public void A_declarative_table_takes_a_column_and_a_row_decorator()
    {
        var table = new CdsTable();
        var head = new CdsTableHead();
        head.Headers.Add(new CdsTableHeader { Text = "Name" });
        head.Headers.Add(new CdsTableHeader { Text = "Risk", Decorator = new CdsAILabel() });
        table.Head = head;
        var ai = new CdsTableRow("a", "1") { Decorator = new CdsAILabel() };
        var plain = new CdsTableRow("b", "2");
        table.Rows.Add(ai);
        table.Rows.Add(plain);
        Themed(table);

        Assert.True(table.HasDecoratorColumn);
        Assert.Equal(3, table.HeaderCount);
        Assert.NotNull(table.HeaderAiOf(2).Layer);
        Assert.Null(table.HeaderAiOf(1).Layer);
        Assert.True(table.AiLayerOf(0)!.IsVisible);
        Assert.Null(table.AiLayerOf(1));
        Assert.Single(table.AiHostsOf(0));
        Assert.Equal(CdsAILabelSize.Mini, ((CdsAILabel)ai.Decorator!).Size);
    }

    [Fact]
    public void A_row_decorator_that_is_not_an_ai_label_draws_nothing_and_is_not_ai()
    {
        var table = new CdsTable();
        var head = new CdsTableHead();
        head.Headers.Add(new CdsTableHeader { Text = "Name" });
        table.Head = head;
        var tip = new CdsToggletip();
        var row = new CdsTableRow("a") { Decorator = tip };
        table.Rows.Add(row);
        Themed(table);
        Assert.True(table.HasDecoratorColumn);
        Assert.False(row.IsAi);
        Assert.Null(tip.Parent);
        Assert.Null(table.AiLayerOf(0));
    }
}
