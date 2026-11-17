namespace Maui.Carbon.Tests.Components;

public class ContentSwitcherTokenTests
{
    [Fact]
    public void High_contrast_selected_inverts()
    {
        var v = CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.None, selected: true);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, v.Indicator);
        Assert.Equal(CdsColorKeys.TextInverse, v.Foreground);
        Assert.Equal(CdsColorKeys.IconInverse, v.Icon);
        Assert.Null(v.IndicatorBorder);
    }

    [Fact]
    public void Unselected_is_secondary_and_hover_and_press_use_layer_hover()
    {
        var rest = CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.None, false);
        Assert.Equal(CdsColorKeys.TextSecondary, rest.Foreground);
        Assert.Null(rest.Background);
        var hover = CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.Hover, false);
        Assert.Equal(CdsColorKeys.LayerHover01, hover.Background);
        Assert.Equal(CdsColorKeys.TextPrimary, hover.Foreground);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.Pressed, false).Background);
    }

    [Fact]
    public void Focused_unselected_segment_fills_with_focus_inset()
    {
        Assert.Equal(CdsColorKeys.FocusInset, CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.FocusVisible, false).Background);
        Assert.Null(CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.FocusVisible, true).Background);
    }

    [Fact]
    public void Low_contrast_selected_sits_on_a_layer_with_a_subtle_edge()
    {
        var v = CdsContentSwitcherTokens.Resolve(true, CdsInteractionState.None, selected: true);
        Assert.Equal(CdsColorKeys.Layer01, v.Indicator);
        Assert.Equal(CdsColorKeys.BorderSubtle01, v.IndicatorBorder);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Foreground);
    }

    [Fact]
    public void Disabled_segments()
    {
        var plain = CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.Disabled, false);
        Assert.Equal(CdsColorKeys.TextDisabled, plain.Foreground);
        Assert.Null(plain.Indicator);
        var selected = CdsContentSwitcherTokens.Resolve(false, CdsInteractionState.Disabled, true);
        Assert.Equal(CdsColorKeys.ButtonDisabled, selected.Indicator);
        Assert.Null(CdsContentSwitcherTokens.Resolve(true, CdsInteractionState.Disabled, true).Indicator);
    }

    [Fact]
    public void Dividers()
    {
        Assert.Null(CdsContentSwitcherTokens.Divider(true, false, false, false));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsContentSwitcherTokens.Divider(false, false, false, false));
        Assert.Null(CdsContentSwitcherTokens.Divider(false, true, false, false));
        Assert.Equal(CdsColorKeys.BorderDisabled, CdsContentSwitcherTokens.Divider(false, true, true, false));
        Assert.Null(CdsContentSwitcherTokens.Divider(false, false, true, true));
    }

    [Fact]
    public void Outline()
    {
        Assert.Equal(CdsColorKeys.BorderInverse, CdsContentSwitcherTokens.Outline(false, false));
        Assert.Equal(CdsColorKeys.BorderDisabled, CdsContentSwitcherTokens.Outline(false, true));
        Assert.Null(CdsContentSwitcherTokens.Outline(true, true));
    }

    [Fact]
    public void Sizes()
    {
        Assert.Equal([32d, 40d, 48d], [CdsContentSwitcherTokens.Height(CdsSize.Sm), CdsContentSwitcherTokens.Height(CdsSize.Md), CdsContentSwitcherTokens.Height(CdsSize.Lg)]);
        Assert.Equal(40, CdsContentSwitcherTokens.Height(CdsSize.Xl));
        Assert.Equal([16d, 16d, 14d], [CdsContentSwitcherTokens.InlinePadding(CdsSize.Sm), CdsContentSwitcherTokens.InlinePadding(CdsSize.Md), CdsContentSwitcherTokens.InlinePadding(CdsSize.Lg)]);
    }
}

public class ContentSwitcherControlTests
{
    static CdsContentSwitcher Make() => new()
    {
        Items =
        {
            new CdsSwitch { Text = "Last hour", Name = "1h" },
            new CdsSwitch { Text = "24 hours", Name = "24h" },
            new CdsSwitch { Text = "7 days", Name = "7d", IsEnabled = false },
            new CdsSwitch { Text = "30 days", Name = "30d" },
        },
    };

    [Fact]
    public void First_segment_is_selected_by_default() => Assert.Equal(0, Make().SelectedIndex);

    [Fact]
    public void Selection_raises_the_event_with_name_and_text()
    {
        var switcher = Make();
        CdsSwitchSelectionChangedEventArgs? args = null;
        switcher.SelectionChanged += (_, e) => args = e;
        switcher.SelectedIndex = 1;
        Assert.Equal((1, "24h", "24 hours"), (args?.Index, args?.Name, args?.Text));
    }

    [Fact]
    public void Disabled_segments_cannot_be_selected()
    {
        var switcher = Make();
        switcher.OnSegmentActivated(2);
        Assert.Equal(0, switcher.SelectedIndex);
        switcher.OnSegmentActivated(3);
        Assert.Equal(3, switcher.SelectedIndex);
        switcher.SelectedIndex = 9;
        Assert.Equal(3, switcher.SelectedIndex);
    }

    [Fact]
    public void Arrow_keys_select_in_automatic_mode_skip_disabled_and_wrap()
    {
        var switcher = Make();
        Assert.True(switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal(1, switcher.SelectedIndex);
        switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(3, switcher.SelectedIndex);
        switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(0, switcher.SelectedIndex);
        switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Left));
        Assert.Equal(3, switcher.SelectedIndex);
        Assert.False(switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Home)));
        Assert.False(switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
    }

    [Fact]
    public void Manual_mode_only_moves_focus()
    {
        var switcher = Make();
        switcher.Activation = CdsActivation.Manual;
        switcher.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(0, switcher.SelectedIndex);
    }

    [Fact]
    public void Variants_construct_without_a_platform()
    {
        var switcher = Make();
        switcher.Size = CdsSize.Sm;
        switcher.IsLowContrast = true;
        switcher.Label = "Time range";
        var icons = new CdsContentSwitcher
        {
            IsIconOnly = true,
            Size = CdsSize.Lg,
            Items =
            {
                new CdsSwitch { Text = "List", Icon = CdsIconName.Menu },
                new CdsSwitch { Text = "Grid", Icon = CdsIconName.Dashboard },
            },
        };
        Assert.Equal(2, icons.Items.Count);
        Assert.True(switcher.WantsNavigationKeys);
    }
}
