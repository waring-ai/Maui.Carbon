namespace Maui.Carbon.Tests.Components;

public class EditInPlaceTests
{
    [Theory]
    [InlineData(CdsSize.Xs, CdsSize.Sm)]
    [InlineData(CdsSize.Sm, CdsSize.Sm)]
    [InlineData(CdsSize.Md, CdsSize.Md)]
    [InlineData(CdsSize.Lg, CdsSize.Lg)]
    [InlineData(CdsSize.Xl, CdsSize.Lg)]
    [InlineData(CdsSize.Xxl, CdsSize.Lg)]
    public void Sizes_clamp_to_sm_md_lg(CdsSize size, CdsSize expected) => Assert.Equal(expected, EditInPlaceModel.Clamp(size));

    [Theory]
    [InlineData(CdsSize.Sm, 32, 40)]
    [InlineData(CdsSize.Md, 40, 48)]
    [InlineData(CdsSize.Lg, 48, 64)]
    [InlineData(CdsSize.Xs, 32, 40)]
    public void Toolbar_squares_are_one_step_above_the_input(CdsSize size, double input, double side)
    {
        Assert.Equal(input, EditInPlaceModel.InputHeight(size));
        Assert.Equal(side, EditInPlaceModel.Side(size));
    }

    [Theory]
    [InlineData("default", "default", false)]
    [InlineData(" default ", "default", false)]
    [InlineData("default", "  default", false)]
    [InlineData("prod-eu-de-1", "default", true)]
    [InlineData("", "default", true)]
    [InlineData(null, "", false)]
    [InlineData(null, null, false)]
    public void Change_compares_trimmed_values(string? value, string? initial, bool changed)
    {
        Assert.Equal(changed, EditInPlaceModel.HasChanged(value, initial));
        Assert.Equal(changed, EditInPlaceModel.CanCancel(value, initial));
        Assert.Equal(changed, EditInPlaceModel.CanSave(value, initial, invalid: false));
    }

    [Fact]
    public void Invalid_blocks_save_but_not_cancel()
    {
        Assert.False(EditInPlaceModel.CanSave("new", "old", invalid: true));
        Assert.True(EditInPlaceModel.CanCancel("new", "old"));
    }

    [Theory]
    [InlineData(CdsSize.Sm, false, false, false, 40)]
    [InlineData(CdsSize.Sm, true, false, false, 80)]
    [InlineData(CdsSize.Sm, true, true, false, 112)]
    [InlineData(CdsSize.Sm, false, true, false, 72)]
    [InlineData(CdsSize.Md, true, false, false, 96)]
    [InlineData(CdsSize.Lg, true, false, false, 128)]
    [InlineData(CdsSize.Lg, true, false, true, 64)]
    [InlineData(CdsSize.Md, false, true, true, 80)]
    public void Toolbar_width(CdsSize size, bool editing, bool invalid, bool readOnly, double width) =>
        Assert.Equal(width, EditInPlaceModel.ToolbarWidth(size, editing, invalid, readOnly));

    [Fact]
    public void Blur_outcome()
    {
        Assert.Equal(EditInPlaceOutcome.Custom, EditInPlaceModel.Blur(true, true));
        Assert.Equal(EditInPlaceOutcome.Custom, EditInPlaceModel.Blur(true, false));
        Assert.Equal(EditInPlaceOutcome.Save, EditInPlaceModel.Blur(false, true));
        Assert.Equal(EditInPlaceOutcome.Cancel, EditInPlaceModel.Blur(false, false));
    }

    [Fact]
    public void Enter_saves_only_a_savable_change()
    {
        Assert.Equal(EditInPlaceOutcome.Save, EditInPlaceModel.Enter(true));
        Assert.Equal(EditInPlaceOutcome.Leave, EditInPlaceModel.Enter(false));
    }

    [Fact]
    public void Tokens_fill_on_hover_and_edit_only()
    {
        Assert.Null(CdsEditInPlaceTokens.Background(CdsInteractionState.None, editing: false));
        Assert.Equal(CdsColorKeys.Field01, CdsEditInPlaceTokens.Background(CdsInteractionState.Hover, editing: false));
        Assert.Equal(CdsColorKeys.Field01, CdsEditInPlaceTokens.Background(CdsInteractionState.None, editing: true));
        Assert.Null(CdsEditInPlaceTokens.Background(CdsInteractionState.Hover | CdsInteractionState.Disabled, editing: false));
    }

    [Fact]
    public void Tokens_outline_focus_over_error()
    {
        Assert.Equal(CdsColorKeys.Focus, CdsEditInPlaceTokens.Outline(inputFocused: true, editing: true, invalid: true));
        Assert.Equal(CdsColorKeys.SupportError, CdsEditInPlaceTokens.Outline(false, false, true));
        Assert.Null(CdsEditInPlaceTokens.Outline(false, true, true));
        Assert.Null(CdsEditInPlaceTokens.Outline(false, false, false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsEditInPlaceTokens.Text(true));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsEditInPlaceTokens.Text(false));
        Assert.Equal(CdsColorKeys.TextPlaceholder, CdsEditInPlaceTokens.Placeholder(false));
        Assert.Equal(CdsColorKeys.SupportError, CdsEditInPlaceTokens.Invalid);
    }

    [Fact]
    public void Defaults_and_property_setting_do_not_throw()
    {
        var field = new CdsEditInPlace();
        Assert.Equal(CdsSize.Sm, field.Size);
        Assert.Equal("Edit", field.EditLabel);
        Assert.Equal("Save", field.SaveLabel);
        Assert.Equal("Cancel", field.CancelLabel);
        Assert.Equal("Edit off", field.ReadOnlyLabel);
        Assert.Equal(CdsPlacement.Top, field.TooltipPlacement);
        Assert.Equal(CdsTypeStyle.BodyCompact01, field.TypeStyle);
        Assert.False(field.IsEditing);

        field.Value = "prod-eu-de-1";
        field.Label = "Cluster name";
        field.Placeholder = "Name the cluster";
        field.IsInvalid = true;
        field.InvalidText = "Enter a name";
        field.IsReadOnly = true;
        field.ReadOnlyLabel = "Locked";
        field.ReadOnlyToggletipText = "Ask an administrator to rename it";
        field.ToggletipPlacement = CdsPlacement.Bottom;
        field.IsReadOnly = false;
        foreach (var size in Enum.GetValues<CdsSize>()) field.Size = size;
        field.TypeStyle = CdsTypeStyle.Heading03;
        field.EditAlwaysVisible = true;
        field.EditLabel = "Rename";
        field.SaveLabel = "Keep";
        field.CancelLabel = "Discard";
        field.TooltipPlacement = CdsPlacement.Bottom;
        field.EditTooltipPlacement = CdsPlacement.Right;
        field.SaveTooltipPlacement = CdsPlacement.Right;
        field.CancelTooltipPlacement = CdsPlacement.Left;
        field.SaveCommand = new Command(() => { });
        field.CancelCommand = new Command(() => { });
        field.IsEnabled = false;
        field.IsEnabled = true;
        field.IsInvalid = false;
        Assert.Equal("Cluster name", SemanticProperties.GetDescription(field.Input));
    }

    [Fact]
    public void Value_syncs_with_the_input_both_ways()
    {
        var field = new CdsEditInPlace { Value = "default" };
        Assert.Equal("default", field.Input.Text);
        Assert.Equal("default", field.InitialValue);
        string? changed = null;
        field.ValueChanged += (_, e) => changed = e.NewTextValue;
        field.BeginEditing();
        field.Input.Text = "prod-eu-de-1";
        Assert.Equal("prod-eu-de-1", field.Value);
        Assert.Equal("prod-eu-de-1", changed);
        // Typing does not move the starting point.
        Assert.Equal("default", field.InitialValue);
    }

    [Fact]
    public void Editing_shows_cancel_and_save_enabled_by_the_change()
    {
        var field = new CdsEditInPlace { Value = "default" };
        Assert.Null(field.Buttons.Cancel);
        field.BeginEditing();
        Assert.True(field.IsEditing);
        var (edit, cancel, save) = field.Buttons;
        Assert.False(edit.IsVisible);
        Assert.NotNull(cancel);
        Assert.NotNull(save);
        Assert.False(cancel!.IsEnabled);
        Assert.False(save!.IsEnabled);

        field.Input.Text = "default ";   // only spaces: no change
        Assert.False(save.IsEnabled);
        field.Input.Text = "prod";
        Assert.True(cancel.IsEnabled);
        Assert.True(save.IsEnabled);
        field.IsInvalid = true;
        Assert.False(save.IsEnabled);
        Assert.True(cancel.IsEnabled);
    }

    [Fact]
    public void Save_keeps_the_value_and_moves_the_starting_point()
    {
        var command = new CountingCommand();
        var field = new CdsEditInPlace { Value = "default", SaveCommand = command };
        CdsEditInPlaceEventArgs? saved = null;
        field.Saved += (_, e) => saved = e;
        field.BeginEditing();
        field.Input.Text = "prod-eu-de-1";
        field.Save(exit: false);
        Assert.Equal("prod-eu-de-1", saved!.Value);
        Assert.Equal("default", saved.InitialValue);
        Assert.Equal("prod-eu-de-1", field.InitialValue);
        Assert.Equal("prod-eu-de-1", command.Parameter);
        Assert.True(field.IsEditing);
        Assert.False(field.CanSave);
    }

    [Fact]
    public void Cancel_restores_the_starting_value()
    {
        var command = new CountingCommand();
        var field = new CdsEditInPlace { Value = "default", CancelCommand = command };
        CdsEditInPlaceEventArgs? canceled = null;
        field.Canceled += (_, e) => canceled = e;
        field.BeginEditing();
        field.Input.Text = "typo";
        field.Cancel(exit: true);
        Assert.Equal("default", field.Value);
        Assert.Equal("default", field.Input.Text);
        Assert.Equal("typo", canceled!.Value);
        Assert.Equal("default", canceled.InitialValue);
        Assert.Equal("default", command.Parameter);
        Assert.False(field.IsEditing);
    }

    [Fact]
    public void Blur_saves_a_valid_change_and_cancels_otherwise()
    {
        var field = new CdsEditInPlace { Value = "default" };
        var saves = 0;
        var cancels = 0;
        field.Saved += (_, _) => saves++;
        field.Canceled += (_, _) => cancels++;

        field.BeginEditing();
        field.Input.Text = "prod";
        field.HandleBlur();
        Assert.Equal(1, saves);
        Assert.False(field.IsEditing);
        Assert.Equal("prod", field.Value);

        field.BeginEditing();
        field.Input.Text = "bad name";
        field.IsInvalid = true;
        field.HandleBlur();
        Assert.Equal(1, cancels);
        Assert.Equal("prod", field.Value);
    }

    [Fact]
    public void A_blur_handler_replaces_the_default()
    {
        var field = new CdsEditInPlace { Value = "default" };
        CdsEditInPlaceEventArgs? blurred = null;
        var saves = 0;
        field.Saved += (_, _) => saves++;
        field.Blurred += (_, e) => blurred = e;
        field.BeginEditing();
        field.Input.Text = "prod";
        field.HandleBlur();
        Assert.Equal(0, saves);
        Assert.Equal("prod", blurred!.Value);
        Assert.Equal("default", blurred.InitialValue);
        Assert.Equal("prod", field.Value);
        Assert.False(field.IsEditing);
    }

    [Fact]
    public void Escape_restores_while_editing()
    {
        var field = new CdsEditInPlace { Value = "default" };
        Assert.False(field.WantsEscapeKey);
        field.BeginEditing();
        field.Input.Text = "typo";
        Assert.True(field.WantsEscapeKey);
        Assert.True(field.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.Equal("default", field.Value);
        Assert.False(field.OnKeyDown(new CdsKeyEventArgs(CdsKey.Enter)));
    }

    [Fact]
    public void Read_only_shows_the_edit_off_toggletip_and_no_editing_buttons()
    {
        var field = new CdsEditInPlace { Value = "default", IsReadOnly = true, ReadOnlyToggletipText = "Managed by policy" };
        Assert.NotNull(field.ReadOnlyToggletip);
        Assert.Equal(CdsIconName.EditOff, field.ReadOnlyToggletip!.Icon);
        Assert.Equal("Managed by policy", field.ReadOnlyToggletip.Text);
        Assert.Equal("Edit off", field.ReadOnlyToggletip.Label);
        Assert.True(field.Input.IsReadOnly);
        field.BeginEditing();
        Assert.Null(field.Buttons.Cancel);
        Assert.False(field.WantsEscapeKey);
    }

    [Fact]
    public void Invalid_shows_its_message()
    {
        var field = new CdsEditInPlace { Value = "", IsInvalid = true, InvalidText = "Enter a name" };
        Assert.NotNull(field.Message);
        Assert.True(field.Message!.IsVisible);
        Assert.Equal("Enter a name", field.Message.Text);
        Assert.Equal("Enter a name", SemanticProperties.GetHint(field.Input));
        field.IsInvalid = false;
        Assert.False(field.Message.IsVisible);
        Assert.Null(SemanticProperties.GetHint(field.Input));
    }

    sealed class CountingCommand : System.Windows.Input.ICommand
    {
        public object? Parameter { get; private set; }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => Parameter = parameter;
    }
}
