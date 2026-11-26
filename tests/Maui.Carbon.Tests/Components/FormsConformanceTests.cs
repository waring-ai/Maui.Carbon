using System.Collections.ObjectModel;

namespace Maui.Carbon.Tests.Components;

/// <summary>Design-conformance fixes of the form fields, choices and list boxes (CA-02 to CA-09, CA-20 to CA-22, CA-35, CA-36).</summary>
public class FormsConformanceTests
{
    static Color Token(CdsThemeName theme, string key) => CdsPalettes.GetTokenColor(theme, key);

    // ---- CA-02: the "!" of warning--alt--filled stays dark ----

    [Theory]
    [InlineData(CdsThemeName.White, CdsColorKeys.IconPrimary, "icon-primary")]
    [InlineData(CdsThemeName.G10, CdsColorKeys.IconPrimary, "icon-primary")]
    [InlineData(CdsThemeName.G90, CdsColorKeys.IconInverse, "icon-inverse")]
    [InlineData(CdsThemeName.G100, CdsColorKeys.IconInverse, "icon-inverse")]
    public void Warning_alt_glyph_has_a_dark_inner_mark_in_every_theme(CdsThemeName theme, string key, string token)
    {
        Assert.Equal(key, CdsIcon.DefaultInnerColorKey(CdsIconName.WarningAltFilled, theme));
        var icon = new CdsIcon(CdsIconName.WarningAltFilled);
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsColorKeys.SupportWarning);
        _ = new CdsTheme { Theme = theme, Body = icon };
        Assert.Equal(Token(theme, token), icon.EffectiveInnerColor);
        // Dark in every theme: the mark is #161616 on the yellow triangle.
        Assert.True(icon.EffectiveInnerColor!.GetLuminosity() < 0.2f);
    }

    // A-03: drawn in any color other than support-warning, the "!" is a hole, as in Carbon; in icon-primary it would
    // otherwise be filled with the triangle's own color and vanish.
    [Theory]
    [InlineData(CdsThemeName.White)]
    [InlineData(CdsThemeName.G100)]
    public void Untinted_warning_alt_glyph_keeps_its_mark_a_hole(CdsThemeName theme)
    {
        var icon = new CdsIcon(CdsIconName.WarningAltFilled);
        _ = new CdsTheme { Theme = theme, Body = icon };
        Assert.Equal(Token(theme, "icon-primary"), icon.Color);
        Assert.Null(icon.EffectiveInnerColor);
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsColorKeys.SupportWarning);
        Assert.NotNull(icon.EffectiveInnerColor);
    }

    [Fact]
    public void Other_glyphs_keep_a_transparent_inner_mark_and_an_explicit_color_wins()
    {
        Assert.Null(CdsIcon.DefaultInnerColorKey(CdsIconName.WarningFilled, CdsThemeName.White));
        Assert.Null(CdsIcon.DefaultInnerColorKey(null, CdsThemeName.G100));
        var icon = new CdsIcon(CdsIconName.WarningAltFilled);
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = icon };
        icon.Icon = CdsIconName.CheckmarkFilled;
        Assert.Equal(0f, icon.EffectiveInnerColor?.Alpha ?? 0f);
        icon.Icon = CdsIconName.WarningAltFilled;
        icon.InnerColor = Colors.Transparent;
        Assert.Equal(Colors.Transparent, icon.EffectiveInnerColor);
    }

    [Fact]
    public void Field_warning_icon_draws_the_dark_mark()
    {
        var input = new CdsTextInput { Label = "Secret value", IsWarning = true, WarnText = "Stored in plain text" };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = input };
        var chrome = Private.Field<CdsFieldChrome>(input, "Chrome");
        var icon = Private.Field<CdsIcon>(chrome, "_statusIcon");
        Assert.Equal(CdsIconName.WarningAltFilled, icon.Icon);
        Assert.Equal(Token(CdsThemeName.White, "icon-primary"), icon.EffectiveInnerColor);
    }

    // ---- CA-03: a radio group is one Tab stop ----

    static CdsRadioButtonGroup RadioGroup(out CdsRadioButton a, out CdsRadioButton b, out CdsRadioButton c)
    {
        a = new CdsRadioButton("Single zone", "single");
        b = new CdsRadioButton("Multizone", "multi");
        c = new CdsRadioButton("Edge", "edge");
        var group = new CdsRadioButtonGroup { Legend = "Cluster type", Orientation = CdsChoiceOrientation.Vertical };
        group.Items.Add(a);
        group.Items.Add(b);
        group.Items.Add(c);
        return group;
    }

    static bool[] TabStops(params CdsRadioButton[] radios) => [.. radios.Select(r => r.HitTarget.IsTabStop)];

    [Fact]
    public void Radio_group_has_one_tab_stop_on_the_first_radio_when_nothing_is_selected()
    {
        var group = RadioGroup(out var a, out var b, out var c);
        Assert.Equal([true, false, false], TabStops(a, b, c));
        Assert.Same(a, group.TabStopRadio);
    }

    [Fact]
    public void Radio_group_tab_stop_follows_the_selection()
    {
        var group = RadioGroup(out var a, out var b, out var c);
        group.SelectedValue = "multi";
        Assert.Equal([false, true, false], TabStops(a, b, c));
        group.SelectedValue = "edge";
        Assert.Equal([false, false, true], TabStops(a, b, c));
    }

    [Fact]
    public void Radio_group_tab_stop_skips_disabled_radios()
    {
        var group = RadioGroup(out var a, out var b, out var c);
        a.IsEnabled = false;
        Assert.Equal([false, true, false], TabStops(a, b, c));
        group.SelectedValue = "edge";
        c.IsEnabled = false;
        Assert.Equal([false, true, false], TabStops(a, b, c));
        group.IsEnabled = false;
        Assert.Equal([false, false, false], TabStops(a, b, c));
        Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
    }

    [Fact]
    public void Arrow_keys_move_the_tab_stop_and_the_selection()
    {
        var group = RadioGroup(out var a, out var b, out var c);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("multi", group.SelectedValue);
        Assert.Equal([false, true, false], TabStops(a, b, c));
    }

    [Fact]
    public void Read_only_group_arrows_move_the_focus_but_not_the_selection()
    {
        var group = RadioGroup(out var a, out var b, out var c);
        group.SelectedValue = "single";
        group.IsReadOnly = true;
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("single", group.SelectedValue);
        Assert.Equal([false, true, false], TabStops(a, b, c));
    }

    [Fact]
    public void A_radio_removed_from_its_group_is_a_tab_stop_again()
    {
        var group = RadioGroup(out var a, out var b, out _);
        group.Items.Remove(b);
        Assert.True(b.HitTarget.IsTabStop);
        Assert.True(a.HitTarget.IsTabStop);
    }

    // ---- CA-04: number input steppers are not Tab stops ----

    [Fact]
    public void Number_input_steppers_are_not_tab_stops_but_stay_named()
    {
        var input = new CdsNumberInput { Label = "Replicas" };
        Assert.False(input.DownButton.HitTarget.IsTabStop);
        Assert.False(input.UpButton.HitTarget.IsTabStop);
        Assert.False(string.IsNullOrEmpty(SemanticProperties.GetDescription(input.DownButton.HitTarget)));
        Assert.False(string.IsNullOrEmpty(SemanticProperties.GetDescription(input.UpButton.HitTarget)));
    }

    [Fact]
    public void Password_toggle_stays_a_tab_stop()
    {
        var input = new CdsPasswordInput { Label = "Password" };
        Assert.True(Private.Field<CdsFieldIconButton>(input, "_toggle").HitTarget.IsTabStop);
    }

    // ---- CA-05: read-only is reported by checkbox, radio, toggle and dropdown ----

    [Fact]
    public void Read_only_checkbox_reports_read_only_own_or_from_its_group()
    {
        var box = new CdsCheckbox("Auto-scaling") { IsReadOnly = true };
        Assert.True(box.HitTarget.IsReadOnly);
        Assert.True(box.HitTarget.IsEnabled);
        box.IsReadOnly = false;
        Assert.False(box.HitTarget.IsReadOnly);

        var group = new CdsCheckboxGroup { Legend = "Channels", IsReadOnly = true };
        var member = new CdsCheckbox("Email");
        group.Items.Add(member);
        Assert.True(member.HitTarget.IsReadOnly);
        group.IsEnabled = false;
        Assert.False(member.HitTarget.IsReadOnly);
    }

    [Fact]
    public void Read_only_radio_reports_read_only_own_or_from_its_group()
    {
        var group = RadioGroup(out var a, out _, out _);
        Assert.False(a.HitTarget.IsReadOnly);
        group.IsReadOnly = true;
        Assert.True(a.HitTarget.IsReadOnly);
        group.IsReadOnly = false;
        a.IsReadOnly = true;
        Assert.True(a.HitTarget.IsReadOnly);
    }

    [Fact]
    public void Read_only_toggle_reports_read_only()
    {
        var toggle = new CdsToggle { LabelText = "Auto-scaling", IsReadOnly = true };
        var hit = Private.Field<CdsHitTarget>(toggle, "_hit");
        Assert.True(hit.IsReadOnly);
        toggle.IsReadOnly = false;
        Assert.False(hit.IsReadOnly);
    }

    [Fact]
    public void Read_only_dropdown_trigger_reports_read_only_and_stays_enabled()
    {
        var dropdown = new CdsDropdown { Label = "Region", ItemsSource = new[] { "Dallas", "Frankfurt" }, IsReadOnly = true };
        var hit = Private.Field<CdsHitTarget>(dropdown, "_hit");
        Assert.True(hit.IsReadOnly);
        Assert.True(hit.IsEnabled);
        dropdown.IsReadOnly = false;
        Assert.False(hit.IsReadOnly);
    }

    // ---- CA-06: errors and warnings are announced when they appear ----

    [Fact]
    public void Field_error_is_announced_once_when_it_appears_on_screen()
    {
        using var host = FormsTestHost.Load(() => new CdsTextInput { Label = "Cluster name", InvalidText = "Enter a name" }, out var input);
        using var heard = CdsAnnouncer.Capture();
        input.IsInvalid = true;
        Assert.Equal(new CdsAnnouncement("Enter a name", CdsAnnouncePriority.Assertive), Assert.Single(heard.Announcements));

        input.Label = "Name";   // re-applies the chrome: same message, not repeated
        Assert.Single(heard.Announcements);

        input.InvalidText = "Use lowercase letters";
        Assert.Equal("Use lowercase letters", heard.Announcements[^1].Text);

        input.IsInvalid = false;
        input.IsWarning = true;
        input.WarnText = "Names are public";
        Assert.Equal(new CdsAnnouncement("Names are public", CdsAnnouncePriority.Assertive), heard.Announcements[^1]);
        Assert.Equal(3, heard.Announcements.Count);
    }

    [Fact]
    public void Helper_text_and_messages_present_before_loading_are_not_announced()
    {
        using var heard = CdsAnnouncer.Capture();
        var input = new CdsTextInput { Label = "Name", IsInvalid = true, InvalidText = "Enter a name" };
        input.HelperText = "Lowercase";
        Assert.Empty(heard.Announcements);
        var area = new CdsTextArea { Label = "Notes", HelperText = "Optional" };
        Assert.NotNull(area);
        Assert.Empty(heard.Announcements);
    }

    [Fact]
    public void Clearing_an_error_rearms_it()
    {
        using var host = FormsTestHost.Load(() => new CdsNumberInput { Label = "Replicas", InvalidText = "Too many" }, out var input);
        using var heard = CdsAnnouncer.Capture();
        input.IsInvalid = true;
        input.IsInvalid = false;
        input.IsInvalid = true;
        Assert.Equal(2, heard.Announcements.Count(a => a.Text == "Too many"));
    }

    [Fact]
    public void Group_and_choice_messages_are_announced_when_they_appear()
    {
        using var host = FormsTestHost.Load(() =>
        {
            var stack = new VerticalStackLayout();
            var group = new CdsCheckboxGroup { Legend = "Channels", InvalidText = "Select at least one channel" };
            group.Items.Add(new CdsCheckbox("Email"));
            stack.Add(group);
            stack.Add(new CdsRadioButtonGroup { Legend = "Theme", WarnText = "Applies to every user" });
            return stack;
        }, out var stack);
        var checkboxes = (CdsCheckboxGroup)stack[0];
        var radios = (CdsRadioButtonGroup)stack[1];
        using var heard = CdsAnnouncer.Capture();
        checkboxes.IsInvalid = true;
        radios.IsWarning = true;
        Assert.Equal(
            [new CdsAnnouncement("Select at least one channel", CdsAnnouncePriority.Assertive), new CdsAnnouncement("Applies to every user", CdsAnnouncePriority.Assertive)],
            heard.Announcements);
    }

    [Fact]
    public void Time_picker_format_error_is_announced()
    {
        using var host = FormsTestHost.Load(() => new CdsTimePicker { Label = "Start" }, out var picker);
        using var heard = CdsAnnouncer.Capture();
        picker.Text = "25:99";
        Assert.False(picker.Validate());
        Assert.Equal(new CdsAnnouncement("Invalid time format.", CdsAnnouncePriority.Assertive), Assert.Single(heard.Announcements));
    }

    // ---- CA-07: inline helper text sits under the label ----

    [Fact]
    public void Inline_helper_text_sits_in_the_label_column_and_errors_under_the_field()
    {
        var input = new CdsTextInput { Label = "Name", HelperText = "Lowercase only", IsInline = true, InvalidText = "Enter a name" };
        var chrome = Private.Field<CdsFieldChrome>(input, "Chrome");
        Assert.Equal(0, chrome.MessageColumn);
        input.IsInvalid = true;
        Assert.Equal(1, chrome.MessageColumn);
        input.IsInvalid = false;
        Assert.Equal(0, chrome.MessageColumn);
        input.IsInline = false;
        Assert.Equal(0, chrome.MessageColumn);
    }

    // ---- CA-08: a form label names its control ----

    [Fact]
    public void Form_label_names_a_custom_control_and_leaves_its_own_text_out_of_the_tree()
    {
        var control = new Entry();
        var label = new CdsFormLabel { Text = "Cluster name", For = control };
        Assert.Equal("Cluster name", SemanticProperties.GetDescription(control));
        Assert.False(AutomationProperties.GetIsInAccessibleTree(Private.Field<CdsText>(label, "_text")));
        label.Text = "Name";
        Assert.Equal("Name", SemanticProperties.GetDescription(control));
    }

    [Fact]
    public void Form_label_names_a_carbon_control_on_its_hit_target()
    {
        var toggle = new CdsToggle();
        _ = new CdsFormLabel { Text = "Auto-scaling", For = toggle };
        Assert.Equal("Auto-scaling", SemanticProperties.GetDescription(Private.Field<CdsHitTarget>(toggle, "_hit")));
    }

    [Fact]
    public void Form_label_keeps_an_existing_name_and_hands_its_name_back_when_retargeted()
    {
        var named = new Entry();
        SemanticProperties.SetDescription(named, "Own name");
        var first = new Entry();
        var label = new CdsFormLabel { Text = "Cluster name", For = named };
        Assert.Equal("Own name", SemanticProperties.GetDescription(named));
        Assert.True(AutomationProperties.GetIsInAccessibleTree(Private.Field<CdsText>(label, "_text")) ?? true);

        label.For = first;
        Assert.Equal("Cluster name", SemanticProperties.GetDescription(first));
        var second = new Entry();
        label.For = second;
        Assert.Null(SemanticProperties.GetDescription(first));
        Assert.Equal("Cluster name", SemanticProperties.GetDescription(second));
        label.For = null;
        Assert.Null(SemanticProperties.GetDescription(second));
    }

    // ---- CA-09: collapsed expandable search reports its expanded state ----

    [Fact]
    public void Expandable_search_magnifier_is_an_expander_that_reports_its_state()
    {
        var search = new CdsSearch { Label = "Search clusters", IsExpandable = true };
        var magnifier = Private.Field<CdsFieldIconButton>(search, "_collapsed");
        Assert.Equal(CdsAccessibilityRole.Expander, magnifier.HitTarget.Role);
        Assert.False(magnifier.HitTarget.IsExpanded);
        search.IsExpanded = true;
        Assert.True(magnifier.HitTarget.IsExpanded);
        search.IsExpandable = false;
        Assert.Null(magnifier.HitTarget.IsExpanded);
    }

    // ---- CA-20: the keyboard highlight is announced ----

    [Fact]
    public void Row_announcements_carry_text_state_and_position()
    {
        Assert.Equal("Frankfurt, 3 of 4", ListBoxLogic.RowAnnouncement(new("eu-de", "Frankfurt"), 2, 4, chosen: false, multi: false));
        Assert.Equal("Frankfurt, selected, 3 of 4", ListBoxLogic.RowAnnouncement(new("eu-de", "Frankfurt"), 2, 4, chosen: true, multi: false));
        Assert.Equal("Email, checked, 1 of 2", ListBoxLogic.RowAnnouncement(new("e", "Email"), 0, 2, chosen: true, multi: true));
        Assert.Equal("Email, not checked, 1 of 2", ListBoxLogic.RowAnnouncement(new("e", "Email"), 0, 2, chosen: false, multi: true));
        Assert.Equal("Tokyo, unavailable, 2 of 2", ListBoxLogic.RowAnnouncement(new("t", "Tokyo", true), 1, 2, chosen: false, multi: false));
        Assert.Equal("No options available", ListBoxLogic.CountAnnouncement(0));
        Assert.Equal("1 option available", ListBoxLogic.CountAnnouncement(1));
        Assert.Equal("4 options available", ListBoxLogic.CountAnnouncement(4));
    }

    static readonly string[] Cities = ["Dallas", "Frankfurt", "London", "Tokyo"];

    [Fact]
    public void Dropdown_announces_the_highlighted_row_on_open_and_on_every_move()
    {
        using var host = FormsTestHost.Load(() => new CdsDropdown { Label = "Region", ItemsSource = Cities, SelectedItem = "London" }, out var dropdown);
        FormsTestHost.SizeField(dropdown);
        using var heard = CdsAnnouncer.Capture();

        Assert.True(dropdown.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.NotNull(dropdown.HighlightAnnouncement());
        Assert.Equal(new CdsAnnouncement("London, selected, 3 of 4", CdsAnnouncePriority.Assertive), Assert.Single(heard.Announcements));

        Assert.True(host.Host.RouteKey(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("Tokyo, 4 of 4", heard.Announcements[^1].Text);
        Assert.True(host.Host.RouteKey(new CdsKeyEventArgs(CdsKey.Home)));
        Assert.Equal("Dallas, 1 of 4", heard.Announcements[^1].Text);
        Assert.Equal(3, heard.Announcements.Count);

        host.Host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape));
        Assert.False(dropdown.IsOpen);
        Assert.Null(dropdown.HighlightAnnouncement());
    }

    [Fact]
    public void Dropdown_opened_by_pointer_announces_the_selected_row_or_the_option_count()
    {
        using var host = FormsTestHost.Load(() =>
        {
            var stack = new VerticalStackLayout();
            stack.Add(new CdsDropdown { Label = "Region", ItemsSource = Cities });
            stack.Add(new CdsDropdown { Label = "Zone", ItemsSource = Cities, SelectedItem = "Tokyo" });
            return stack;
        }, out var stack);
        var plain = (CdsDropdown)stack[0];
        var chosen = (CdsDropdown)stack[1];
        FormsTestHost.SizeField(plain);
        FormsTestHost.SizeField(chosen);
        using var heard = CdsAnnouncer.Capture();
        plain.IsOpen = true;
        Assert.Equal("4 options available", heard.Announcements[^1].Text);
        plain.IsOpen = false;
        chosen.IsOpen = true;
        Assert.Equal("Tokyo, selected, 4 of 4", heard.Announcements[^1].Text);
    }

    [Fact]
    public void Multi_select_announces_checked_state_as_rows_toggle()
    {
        using var host = FormsTestHost.Load(() => new CdsMultiSelect { Label = "Channels", ItemsSource = new[] { "Email", "SMS", "Slack" } }, out var multi);
        FormsTestHost.SizeField(multi);
        using var heard = CdsAnnouncer.Capture();
        multi.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down));
        Assert.Equal("Email, not checked, 1 of 3", heard.Announcements[^1].Text);
        host.Host.RouteKey(new CdsKeyEventArgs(CdsKey.Enter));
        Assert.Contains(new CdsAnnouncement("Email, checked, 1 of 3", CdsAnnouncePriority.Assertive), heard.Announcements);
    }

    [Fact]
    public void Combo_box_announces_the_first_match_or_that_nothing_matches_as_you_type()
    {
        using var host = FormsTestHost.Load(() => new CdsComboBox { Label = "City", ItemsSource = Cities }, out var combo);
        FormsTestHost.SizeField(combo);
        var entry = Private.Field<CdsNativeEntry>(combo, "Entry");
        using var heard = CdsAnnouncer.Capture();
        entry.Text = "o";
        Assert.Equal(new CdsAnnouncement("London, 1 of 2", CdsAnnouncePriority.Assertive), Assert.Single(heard.Announcements));
        entry.Text = "ok";
        Assert.Equal("Tokyo, 1 of 1", heard.Announcements[^1].Text);
        entry.Text = "okz";
        Assert.Equal("No options available", heard.Announcements[^1].Text);
        Assert.Equal(3, heard.Announcements.Count);
    }

    // ---- CA-21: type-ahead reaches the focused list box ----

    [Fact]
    public void Typed_letters_reach_the_focused_dropdown_as_type_ahead()
    {
        using var host = FormsTestHost.Load(() => new CdsDropdown { Label = "Region", ItemsSource = Cities }, out var dropdown);
        FormsTestHost.SizeField(dropdown);
        CdsFocusTracker.SetFocused(dropdown);
        try
        {
            using var heard = CdsAnnouncer.Capture();
            Assert.True(host.Host.RouteCharacter('f'));
            Assert.True(dropdown.IsOpen);
            Assert.Equal("Frankfurt", dropdown.Model.HighlightedRow?.Text);
            Assert.Equal("Frankfurt, 2 of 4", Assert.Single(heard.Announcements).Text);
            Assert.False(host.Host.RouteCharacter(' '));
            Assert.False(host.Host.RouteCharacter('\t'));
        }
        finally
        {
            CdsFocusTracker.ClearFocused(dropdown);
        }
    }

    [Fact]
    public void Typed_letters_are_not_taken_from_text_fields_or_unfocused_controls()
    {
        using var host = FormsTestHost.Load(() => new CdsComboBox { Label = "City", ItemsSource = Cities }, out var combo);
        CdsFocusTracker.SetFocused(combo);
        try
        {
            Assert.False(host.Host.RouteCharacter('f'));
        }
        finally
        {
            CdsFocusTracker.ClearFocused(combo);
        }
        Assert.False(new CdsTextInput().OnCharacter('f'));
    }

    // ---- CA-22: the checkbox group's message reaches every checkbox ----

    [Fact]
    public void Checkbox_group_message_is_part_of_every_checkbox_hint()
    {
        var group = new CdsCheckboxGroup { Legend = "Channels", HelperText = "Where alerts go", InvalidText = "Select at least one channel" };
        var email = new CdsCheckbox("Email");
        var sms = new CdsCheckbox("SMS") { HelperText = "Charges apply" };
        group.Items.Add(email);
        group.Items.Add(sms);
        Assert.Equal("Where alerts go", email.AccessibilityHint);
        Assert.Equal("Charges apply. Where alerts go", sms.AccessibilityHint);
        group.IsInvalid = true;
        Assert.Equal("Select at least one channel", email.AccessibilityHint);
        group.IsInvalid = false;
        group.HelperText = null;
        Assert.Null(email.AccessibilityHint);
        Assert.Equal("Charges apply", sms.AccessibilityHint);
    }

    [Fact]
    public void Radio_group_message_is_part_of_every_radio_hint()
    {
        var group = RadioGroup(out var a, out _, out var c);
        group.HelperText = "Change it later in settings";
        Assert.Equal("Change it later in settings", SemanticProperties.GetHint(a.HitTarget));
        group.Items.Remove(c);
        Assert.Null(SemanticProperties.GetHint(c.HitTarget));
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("own", null, "own")]
    [InlineData(null, "group", "group")]
    [InlineData("own", "group", "own. group")]
    [InlineData("same", "same", "same")]
    public void Checkbox_hint_combines_its_own_and_its_groups_message(string? own, string? group, string? expected) =>
        Assert.Equal(expected, CdsCheckbox.CombineHint(own, group));

    // ---- CA-35: select and time picker sizes are sm..lg ----

    [Theory]
    [InlineData(CdsSize.Xs, CdsSize.Sm)]
    [InlineData(CdsSize.Sm, CdsSize.Sm)]
    [InlineData(CdsSize.Lg, CdsSize.Lg)]
    [InlineData(CdsSize.Xl, CdsSize.Lg)]
    [InlineData(CdsSize.Xxl, CdsSize.Lg)]
    public void Select_and_time_picker_clamp_their_size(CdsSize size, CdsSize expected)
    {
        var select = new CdsSelect { Label = "Region", Size = size };
        Assert.Equal(expected, Private.Field<CdsFieldChrome>(select, "Chrome").Size);
        Assert.Equal(expected.Height(), Private.Field<CdsFieldChrome>(select, "Chrome").FieldHeight);
        var picker = new CdsTimePicker { Label = "Start", Size = size };
        Assert.Equal(expected, Private.Field<CdsFieldChrome>(picker, "Chrome").Size);
    }

    // ---- CA-36: a read-only select stays focusable ----

    [Fact]
    public void Read_only_select_keeps_its_picker_interactive_and_reverts_changes()
    {
        var select = new CdsSelect { Label = "Region", IsReadOnly = true };
        select.Items.Add(new CdsSelectItem { Value = "a", Text = "Dallas" });
        select.Items.Add(new CdsSelectItem { Value = "b", Text = "Frankfurt" });
        select.SelectedIndex = 0;
        var picker = Private.Field<CdsNativePicker>(select, "Picker");
        Assert.False(picker.InputTransparent);
        Assert.True(picker.IsEnabled);
        picker.SelectedIndex = 1;
        Assert.Equal(0, picker.SelectedIndex);
        Assert.Equal(0, select.SelectedIndex);
    }
}
