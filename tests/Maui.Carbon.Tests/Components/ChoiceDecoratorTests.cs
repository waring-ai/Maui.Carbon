using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Tests.Components;

/// <summary>The AI label decorator of the checkbox, checkbox group, radio button and radio button group (Carbon's decorator slot).</summary>
[Collection(FocusTrackerCollection.Name)]
public class ChoiceDecoratorTests
{
    static CdsDecoratedRow RowOf(object control, string field = "_row") => Private.Field<CdsDecoratedRow>(control, field);

    static CdsHitTarget HitOf(CdsCheckbox box) => box.HitTarget;

    static IEnumerable<IVisualTreeElement> Walk(IVisualTreeElement root)
    {
        yield return root;
        foreach (var child in root.GetVisualChildren())
            foreach (var d in Walk(child)) yield return d;
    }

    static int Position(IVisualTreeElement root, object target) => Walk(root).ToList().FindIndex(e => ReferenceEquals(e, target));

    static CdsRadioButtonGroup RadioGroup(out CdsRadioButton a, out CdsRadioButton b, out CdsAILabel labelOfA)
    {
        a = new CdsRadioButton("Frankfurt", "eu-de") { Decorator = labelOfA = new CdsAILabel() };
        b = new CdsRadioButton("London", "eu-gb");
        var group = new CdsRadioButtonGroup { Legend = "Region", Orientation = CdsChoiceOrientation.Vertical };
        group.Items.Add(a);
        group.Items.Add(b);
        return group;
    }

    // ---- sizing: Carbon clones the AI label with a forced size (and kind, in groups) ----

    [Fact]
    public void A_checkbox_shows_an_ai_label_at_mini_and_an_inline_one_at_md()
    {
        var box = new CdsCheckbox("Enable autoscaling");
        var label = new CdsAILabel { Size = CdsAILabelSize.Lg };
        box.Decorator = label;
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        Assert.Equal(CdsAILabelKind.Default, label.Kind);

        var inline = new CdsAILabel { Kind = CdsAILabelKind.Inline };
        box.Decorator = inline;
        Assert.Equal(CdsAILabelSize.Md, inline.Size);
        Assert.Equal(CdsAILabelKind.Inline, inline.Kind);
        // The inline kind sits 1px higher (margin-block-start: -1px).
        Assert.Equal(new Thickness(CdsSpacing.Spacing03, -1, 0, 0), inline.Margin);
    }

    [Fact]
    public void A_radio_button_shows_an_ai_label_at_mini_and_an_inline_one_at_md()
    {
        var label = new CdsAILabel { Size = CdsAILabelSize.Xl };
        var radio = new CdsRadioButton("Frankfurt") { Decorator = label };
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        var inline = new CdsAILabel { Kind = CdsAILabelKind.Inline };
        radio.Decorator = inline;
        Assert.Equal(CdsAILabelSize.Md, inline.Size);
        Assert.Equal(CdsAILabelKind.Inline, inline.Kind);
    }

    [Fact]
    public void A_size_or_kind_set_later_is_normalised_again()
    {
        var box = new CdsCheckbox("Enable autoscaling");
        var label = new CdsAILabel();
        box.Decorator = label;
        label.Size = CdsAILabelSize.Lg;
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        label.Kind = CdsAILabelKind.Inline;
        Assert.Equal(CdsAILabelSize.Md, label.Size);
        label.Kind = CdsAILabelKind.Default;
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        Assert.Equal(0, label.Margin.Top);
    }

    [Fact]
    public void Groups_force_the_mini_size_and_the_default_kind()
    {
        var label = new CdsAILabel { Size = CdsAILabelSize.Lg, Kind = CdsAILabelKind.Inline };
        _ = new CdsCheckboxGroup { Legend = "Notify me by", Decorator = label };
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        Assert.Equal(CdsAILabelKind.Default, label.Kind);

        var other = new CdsAILabel { Size = CdsAILabelSize.Lg, Kind = CdsAILabelKind.Inline };
        _ = new CdsRadioButtonGroup { Legend = "Region", Decorator = other };
        Assert.Equal(CdsAILabelSize.Mini, other.Size);
        Assert.Equal(CdsAILabelKind.Default, other.Kind);
        label.Kind = CdsAILabelKind.Inline;
        Assert.Equal(CdsAILabelKind.Default, label.Kind);
    }

    [Fact]
    public void Another_view_is_placed_without_a_size_change()
    {
        var tip = new CdsToggletip();
        var box = new CdsCheckbox("Enable autoscaling") { Decorator = tip };
        Assert.Same(tip, RowOf(box).Decorator);
        Assert.Equal(CdsSpacing.Spacing03, tip.Margin.Left);
    }

    // ---- placement ----

    [Fact]
    public void The_checkbox_decorator_follows_the_label_with_spacing_03_and_is_not_part_of_the_hit_target()
    {
        var box = new CdsCheckbox("Enable autoscaling");
        var label = new CdsAILabel();
        box.Decorator = label;
        var row = RowOf(box);
        Assert.Equal(CdsSpacing.Spacing03, label.Margin.Left);
        Assert.Same(label, row.Children[1]);
        // The box, label and hit target are in the first child, the decorator is its sibling (on top of the hit area).
        var main = Assert.IsType<Grid>(row.Children[0]);
        Assert.Contains(box.HitTarget, main.Children);
        Assert.DoesNotContain(label, Walk(box.HitTarget).OfType<CdsAILabel>());
        Assert.Same(row, label.Parent);
        Assert.True(label.IsVisible);
        Assert.False(label.InputTransparent);
        // Tab order: the checkbox first, then its AI label.
        Assert.True(Position(box, box.HitTarget) < Position(box, label));
    }

    [Fact]
    public void A_label_set_after_the_decorator_goes_under_the_hit_target()
    {
        var box = new CdsCheckbox { Decorator = new CdsAILabel() };
        box.Label = "Enable autoscaling";
        var main = Assert.IsType<Grid>(RowOf(box).Children[0]);
        var label = Assert.IsType<CdsText>(Private.Field<CdsText?>(box, "_label"));
        Assert.True(main.Children.IndexOf(label) < main.Children.IndexOf(box.HitTarget));
        Assert.Equal("Enable autoscaling", label.Text);
        Assert.Equal(box.HitTarget, main.Children[^1]);
    }

    [Fact]
    public void A_checkbox_without_a_decorator_keeps_the_flat_tree()
    {
        var box = new CdsCheckbox("Email");
        Assert.Null(Private.Field<CdsDecoratedRow?>(box, "_row"));
        box.Decorator = null;
        Assert.Null(Private.Field<CdsDecoratedRow?>(box, "_row"));
    }

    [Fact]
    public void Removing_or_replacing_the_decorator_detaches_the_old_one()
    {
        var box = new CdsCheckbox("Enable autoscaling");
        var first = new CdsAILabel();
        var second = new CdsAILabel();
        box.Decorator = first;
        box.Decorator = second;
        Assert.Null(first.Parent);
        Assert.Same(second, RowOf(box).Decorator);
        Assert.Equal(2, RowOf(box).Children.Count);
        box.Decorator = null;
        Assert.Null(second.Parent);
        Assert.Single(RowOf(box).Children);
        Assert.Null(box.Decorator);
    }

    [Fact]
    public void A_hidden_label_still_shows_the_decorator()
    {
        var box = new CdsCheckbox { HideLabel = true, Label = "Select row", Decorator = new CdsAILabel() };
        Assert.True(box.Decorator!.IsVisible);
        var radio = new CdsRadioButton("Frankfurt") { HideLabel = true, Decorator = new CdsAILabel() };
        Assert.True(radio.Decorator!.IsVisible);
    }

    [Fact]
    public void A_radio_decorator_shows_only_while_the_radio_has_a_label()
    {
        var radio = new CdsRadioButton { Decorator = new CdsAILabel() };
        Assert.False(radio.Decorator!.IsVisible);
        radio.Label = "Frankfurt";
        Assert.True(radio.Decorator.IsVisible);
        radio.Label = null;
        Assert.False(radio.Decorator.IsVisible);
    }

    [Theory]
    [InlineData(CdsRadioLabelPosition.Right)]
    [InlineData(CdsRadioLabelPosition.Left)]
    public void The_radio_decorator_is_after_the_circle_and_label_whichever_side_the_label_is_on(CdsRadioLabelPosition position)
    {
        var label = new CdsAILabel();
        var radio = new CdsRadioButton("Frankfurt") { LabelPosition = position, Decorator = label };
        var row = RowOf(radio);
        var main = Assert.IsType<Grid>(row.Children[0]);
        Assert.Same(label, row.Children[1]);
        Assert.Contains(radio.HitTarget, main.Children);
        Assert.DoesNotContain(label, main.Children);
        Assert.Same(row, radio.Content);
        Assert.True(Position(radio, radio.HitTarget) < Position(radio, label));
        // Carbon centers the AI label on the radio's line.
        var layout = Private.Field<bool>(row, "_centersDecorator");
        Assert.True(layout);
    }

    [Fact]
    public void A_group_decorator_shares_the_legend_row_and_needs_a_legend()
    {
        var label = new CdsAILabel();
        var group = new CdsCheckboxGroup { Decorator = label };
        var row = RowOf(group, "_legendRow");
        Assert.Same(label, row.Children[1]);
        Assert.False(row.IsVisible);
        group.Legend = "Notify me by";
        Assert.True(row.IsVisible);
        Assert.Equal(CdsSpacing.Spacing03, label.Margin.Left);
        group.Legend = null;
        Assert.False(row.IsVisible);

        var radios = new CdsRadioButtonGroup { Decorator = new CdsAILabel() };
        Assert.False(RowOf(radios, "_legendRow").IsVisible);
        radios.Legend = "Region";
        Assert.True(RowOf(radios, "_legendRow").IsVisible);
    }

    [Fact]
    public void The_radio_group_legend_keeps_its_gap_below_the_header_row()
    {
        var group = new CdsRadioButtonGroup { Legend = "Region" };
        Assert.Equal(CdsSpacing.Spacing03, Private.Field<CdsText>(group, "_legend").Margin.Bottom);
        group.Decorator = new CdsAILabel();
        var row = RowOf(group, "_legendRow");
        Assert.Equal(CdsSpacing.Spacing03, row.Margin.Bottom);
        Assert.Equal(0, Private.Field<CdsText>(group, "_legend").Margin.Bottom);
        Assert.Same(Private.Field<CdsText>(group, "_legend"), row.Children[0]);
    }

    [Fact]
    public void A_group_decorator_does_not_change_the_messages_the_boxes_read()
    {
        var a = new CdsCheckbox("Email");
        var group = new CdsCheckboxGroup { Legend = "Notify me by", IsInvalid = true, InvalidText = "Select at least one" };
        group.Items.Add(a);
        group.Decorator = new CdsAILabel();
        Assert.Equal("Select at least one", a.AccessibilityHint);
        Assert.Contains(a, group.Items);
    }

    // ---- the AI label is never disabled, and is not the control's ----

    static bool Enabled(View view) => view.IsEnabled;

    [Fact]
    public void Spike_a_disabled_control_leaves_its_ai_label_enabled()
    {
        var box = new CdsCheckbox("Enable autoscaling") { Decorator = new CdsAILabel(), IsEnabled = false };
        var group = new CdsCheckboxGroup { Legend = "Notify me by", Decorator = new CdsAILabel(), IsEnabled = false };
        var radio = new CdsRadioButton("Frankfurt") { Decorator = new CdsAILabel(), IsEnabled = false };
        var radios = new CdsRadioButtonGroup { Legend = "Region", Decorator = new CdsAILabel(), IsEnabled = false };
        var inGroup = RadioGroup(out _, out _, out var labelOfA);
        inGroup.IsEnabled = false;
        foreach (var label in new[] { box.Decorator, group.Decorator, radio.Decorator, radios.Decorator, labelOfA })
        {
            Assert.True(Enabled(label!), "the decorator's own IsEnabled stays true");
            Assert.False(label!.Parent is null);
        }
        // The checkbox's own hit target is disabled; the AI label's is not.
        Assert.False(box.HitTarget.IsEnabled);
        Assert.True(Private.Field<CdsHitTarget>(box.Decorator!, "_hit").IsEnabled);
        Assert.True(Private.Field<CdsHitTarget>(labelOfA, "_hit").IsEnabled);
    }

    [Fact]
    public void Spike_maui_cascades_a_disabled_parent_to_its_children_and_the_row_stops_it()
    {
        // The reason the decorator row exists as an enabled boundary: without it a disabled control turns its AI label off.
        var plain = new CdsAILabel();
        var layout = new VerticalStackLayout { IsEnabled = false };
        layout.Add(plain);
        Assert.False(plain.IsEnabled);

        var inRow = new CdsAILabel();
        var main = new Fixed(10, 10);
        var row = new CdsDecoratedRow(main, allowInline: true, centersDecorator: false) { Decorator = inRow };
        var disabledParent = new VerticalStackLayout { IsEnabled = false };
        disabledParent.Add(row);
        Assert.True(row.IsEnabled);
        Assert.True(inRow.IsEnabled);
        inRow.IsEnabled = false;
        Assert.False(inRow.IsEnabled);
    }

    [Fact]
    public void Read_only_controls_leave_the_ai_label_active()
    {
        var box = new CdsCheckbox("Enable autoscaling") { Decorator = new CdsAILabel(), IsReadOnly = true };
        var group = RadioGroup(out _, out _, out var labelOfA);
        group.IsReadOnly = true;
        Assert.True(Enabled(box.Decorator!));
        Assert.True(Private.Field<CdsHitTarget>(box.Decorator!, "_hit").IsEnabled);
        Assert.True(Private.Field<CdsHitTarget>(labelOfA, "_hit").IsEnabled);
    }

    [Fact]
    public void Activating_the_checkbox_ai_label_does_not_toggle_the_checkbox()
    {
        var label = new CdsAILabel { Callout = new CdsAILabelContent() };
        var box = new CdsCheckbox("Enable autoscaling") { Decorator = label };
        var changes = 0;
        box.CheckedChanged += (_, _) => changes++;
        Private.Field<CdsHitTarget>(label, "_hit").RaiseKeyboardActivation();
        Assert.True(label.IsOpen);
        Assert.False(box.IsChecked);
        Assert.Equal(0, changes);

        box.HitTarget.RaiseKeyboardActivation();
        Assert.True(box.IsChecked);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void Activating_the_radio_ai_label_does_not_select_the_radio()
    {
        var group = RadioGroup(out var a, out var b, out var label);
        label.Callout = new CdsAILabelContent();
        Private.Field<CdsHitTarget>(label, "_hit").RaiseKeyboardActivation();
        Assert.True(label.IsOpen);
        Assert.False(a.IsChecked);
        Assert.Null(group.SelectedValue);
        b.HitTarget.RaiseKeyboardActivation();
        Assert.Equal("eu-gb", group.SelectedValue);
    }

    [Fact]
    public void An_ai_label_in_revert_state_changes_nothing_on_the_control()
    {
        var label = new CdsAILabel();
        var box = new CdsCheckbox("Enable autoscaling") { IsChecked = true, Decorator = label };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = box };
        var before = Private.Field<CdsCheckGlyph>(box, "_box").BorderColor;
        label.IsRevertActive = true;
        Assert.Equal(before, Private.Field<CdsCheckGlyph>(box, "_box").BorderColor);
        Assert.True(box.IsChecked);
    }

    // ---- roving focus with a foreign focusable inside a radio group ----

    [Fact]
    public void Spike_arrow_keys_on_a_focused_ai_label_do_not_move_the_selection()
    {
        var group = RadioGroup(out var a, out var b, out var label);
        group.SelectedValue = "eu-de";
        try
        {
            CdsFocusTracker.SetFocused(label);
            Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
            Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
            Assert.Equal("eu-de", group.SelectedValue);

            // Focus on a radio of the group: the arrows work as before.
            CdsFocusTracker.SetFocused(a);
            Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
            Assert.Equal("eu-gb", group.SelectedValue);
        }
        finally
        {
            CdsFocusTracker.ClearFocused(label);
            CdsFocusTracker.ClearFocused(a);
        }
    }

    [Fact]
    public void Spike_the_group_decorator_does_not_take_part_in_the_roving_focus()
    {
        var group = RadioGroup(out var a, out var b, out _);
        var legendLabel = new CdsAILabel();
        group.Decorator = legendLabel;
        group.SelectedValue = "eu-gb";
        try
        {
            CdsFocusTracker.SetFocused(legendLabel);
            Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
            Assert.Equal("eu-gb", group.SelectedValue);
        }
        finally
        {
            CdsFocusTracker.ClearFocused(legendLabel);
        }
        // One Tab stop for the radios, unchanged by the AI labels; the AI labels keep their own.
        Assert.Same(b, group.TabStopRadio);
        Assert.Equal([false, true], new[] { a.HitTarget.IsTabStop, b.HitTarget.IsTabStop });
        Assert.True(Private.Field<CdsHitTarget>(legendLabel, "_hit").IsTabStop);
        Assert.True(Private.Field<CdsHitTarget>(a.Decorator!, "_hit").IsTabStop);
        // Tab order: the legend's AI label, then the radios with each AI label after its radio.
        Assert.True(Position(group, legendLabel) < Position(group, a.HitTarget));
        Assert.True(Position(group, a.HitTarget) < Position(group, a.Decorator!));
        Assert.True(Position(group, a.Decorator!) < Position(group, b.HitTarget));
    }

    [Fact]
    public void Arrow_keys_still_select_among_radios_and_skip_the_decorators()
    {
        var group = RadioGroup(out var a, out var b, out _);
        var c = new CdsRadioButton("Paris", "eu-fr") { Decorator = new CdsAILabel() };
        group.Items.Add(c);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("eu-gb", group.SelectedValue);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("eu-fr", group.SelectedValue);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("eu-de", group.SelectedValue);
        Assert.Same(a, group.TabStopRadio);
    }

    [Fact]
    public void A_group_with_decorators_keeps_its_messages_and_hints()
    {
        var group = RadioGroup(out var a, out _, out _);
        group.Decorator = new CdsAILabel();
        group.IsInvalid = true;
        group.InvalidText = "Choose a region";
        Assert.Equal("Choose a region", SemanticProperties.GetHint(a.HitTarget));
    }

    // ---- layout: Carbon's wrapping flex row ----

    // A view with a known size and no handler (the margin counts in the desired size, as in MAUI).
    sealed class Fixed(double width, double height) : BoxView
    {
        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            DesiredSize = new Size(Math.Min(width, widthConstraint) + Margin.HorizontalThickness, height + Margin.VerticalThickness);
            return DesiredSize;
        }
    }

    static (CdsDecoratedRow Row, Fixed Main, Fixed Decorator) Row(double mainWidth, bool centers = false)
    {
        var main = new Fixed(mainWidth, 20);
        var decorator = new Fixed(16, 16);
        var row = new CdsDecoratedRow(main, allowInline: true, centersDecorator: centers) { Decorator = decorator };
        return (row, main, decorator);
    }

    [Fact]
    public void The_decorator_sits_right_after_the_main_view_when_there_is_room()
    {
        var (row, main, decorator) = Row(100);
        var size = ((ICrossPlatformLayout)row).CrossPlatformMeasure(400, double.PositiveInfinity);
        Assert.Equal(100 + CdsSpacing.Spacing03 + 16, size.Width, 1);
        Assert.Equal(20, size.Height, 1);
        ((ICrossPlatformLayout)row).CrossPlatformArrange(new Rect(0, 0, 400, 20));
        Assert.Equal(0, main.Frame.X, 1);
        Assert.Equal(100, main.Frame.Width, 1);
        Assert.Equal(100 + CdsSpacing.Spacing03, decorator.Frame.X, 1);
        // Top aligned (checkbox, legends): the decorator starts at the top of the line.
        Assert.Equal(0, decorator.Frame.Y, 1);
    }

    [Fact]
    public void A_centered_decorator_is_centered_on_the_line()
    {
        var (row, _, decorator) = Row(100, centers: true);
        ((ICrossPlatformLayout)row).CrossPlatformMeasure(400, double.PositiveInfinity);
        ((ICrossPlatformLayout)row).CrossPlatformArrange(new Rect(0, 0, 400, 20));
        Assert.Equal(2, decorator.Frame.Y, 1);
    }

    [Fact]
    public void The_decorator_drops_to_the_next_line_when_the_main_view_leaves_no_room()
    {
        var (row, main, decorator) = Row(100);
        // 100 + 8 + 16 = 124 does not fit 110.
        var size = ((ICrossPlatformLayout)row).CrossPlatformMeasure(110, double.PositiveInfinity);
        Assert.Equal(20 + 16, size.Height, 1);
        ((ICrossPlatformLayout)row).CrossPlatformArrange(new Rect(0, 0, 110, size.Height));
        Assert.Equal(100, main.Frame.Width, 1);
        // Carbon (flex-wrap: wrap): on its own line at the start edge, still 8px in from its margin.
        Assert.Equal(CdsSpacing.Spacing03, decorator.Frame.X, 1);
        Assert.Equal(20, decorator.Frame.Y, 1);
    }

    [Fact]
    public void A_main_view_wider_than_the_row_is_capped_to_it_and_the_decorator_stays_inside()
    {
        var (row, main, decorator) = Row(500);
        var size = ((ICrossPlatformLayout)row).CrossPlatformMeasure(200, double.PositiveInfinity);
        Assert.True(size.Width <= 200 + 0.01);
        ((ICrossPlatformLayout)row).CrossPlatformArrange(new Rect(0, 0, 200, size.Height));
        Assert.Equal(200, main.Frame.Width, 1);
        Assert.True(decorator.Frame.Right <= 200 + 0.01);
    }

    [Fact]
    public void A_collapsed_decorator_takes_no_room()
    {
        var (row, _, decorator) = Row(100);
        decorator.IsVisible = false;
        Assert.Equal(100, ((ICrossPlatformLayout)row).CrossPlatformMeasure(400, double.PositiveInfinity).Width, 1);
    }

    [Fact]
    public void Every_control_can_be_constructed_with_a_decorator_and_every_property_set()
    {
        var box = new CdsCheckbox("Email") { Decorator = new CdsAILabel(), HelperText = "help", IsInvalid = true, InvalidText = "bad", IsReadOnly = true };
        var group = new CdsCheckboxGroup { Legend = "Notify me by", Decorator = new CdsAILabel(), HelperText = "help", Orientation = CdsChoiceOrientation.Horizontal };
        group.Items.Add(box);
        var radios = RadioGroup(out var a, out _, out _);
        radios.Decorator = new CdsAILabel();
        radios.LabelPosition = CdsRadioLabelPosition.Left;
        a.LabelPosition = CdsRadioLabelPosition.Left;
        Assert.NotNull(box.Decorator);
        Assert.NotNull(group.Decorator);
        Assert.NotNull(radios.Decorator);
        Assert.NotNull(a.Decorator);
        group.Decorator = null;
        radios.Decorator = null;
        Assert.Null(group.Decorator);
        Assert.Null(radios.Decorator);
    }
}
