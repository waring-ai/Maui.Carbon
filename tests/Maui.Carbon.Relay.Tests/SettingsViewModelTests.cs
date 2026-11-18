namespace Maui.Carbon.Relay.Tests;

public class SettingsViewModelTests
{
    readonly TestWorld _w = new();

    SettingsViewModel Create() => new(_w.Settings, _w.Projects, _w.Theme, _w.Feedback, _w.Clock);

    async Task<SettingsViewModel> Activated()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        return vm;
    }

    [Fact]
    public async Task Loads_the_saved_settings_clean()
    {
        var vm = await Activated();
        Assert.Equal("Maya Chen", vm.DisplayName);
        Assert.Equal("maya.chen@relay.dev", vm.Email);
        Assert.Equal("America/Toronto", vm.TimeZone?.Id);
        Assert.Equal("atl", vm.DefaultProject.Id);
        Assert.Equal(8, vm.DigestHour);
        Assert.False(vm.IsDirty);
        Assert.False(vm.SaveCommand.CanExecute(null));
        Assert.DoesNotContain(vm.ProjectOptions, o => o.Id == "leg");
    }

    [Fact]
    public async Task Editing_marks_dirty_and_undoing_the_edit_clears_it()
    {
        var vm = await Activated();
        vm.EmailOnStatus = true;
        Assert.True(vm.IsDirty);
        Assert.True(vm.SaveCommand.CanExecute(null));
        vm.EmailOnStatus = false;
        Assert.False(vm.IsDirty);
        vm.DigestHour = 17;
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public async Task Save_writes_the_copy_back_and_toasts()
    {
        var vm = await Activated();
        vm.DisplayName = "Maya C.";
        vm.TimeZone = vm.TimeZones.Single(t => t.Id == "Europe/Berlin");
        vm.DigestHour = 17.4;
        vm.SlackChannel = "#billing-alerts";
        // The service's settings object is untouched until Save.
        Assert.Equal("Maya Chen", _w.Settings.Current.DisplayName);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.False(vm.IsSaving);
        Assert.False(vm.IsDirty);
        Assert.Equal("Maya C.", _w.Settings.Current.DisplayName);
        Assert.Equal("Europe/Berlin", _w.Settings.Current.TimeZone);
        Assert.Equal(17, _w.Settings.Current.DigestHour);
        Assert.Equal("#billing-alerts", _w.Settings.Current.SlackChannel);
        Assert.Contains(_w.Feedback.Toasts, t => t is { Kind: FeedbackKind.Success, Title: "Settings saved" });
    }

    [Fact]
    public async Task Discard_reloads_the_saved_values()
    {
        var vm = await Activated();
        vm.CompactTables = true;
        vm.DisplayName = "Someone else";
        vm.DiscardCommand.Execute(null);
        Assert.False(vm.CompactTables);
        Assert.Equal("Maya Chen", vm.DisplayName);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public async Task Invalid_fields_block_save()
    {
        var vm = await Activated();
        vm.DisplayName = "  ";
        Assert.True(vm.IsDirty);
        Assert.True(vm.IsDisplayNameInvalid);
        Assert.False(vm.SaveCommand.CanExecute(null));
        vm.DisplayName = "Maya";
        vm.SlackChannel = "atlas-dev";
        Assert.True(vm.IsSlackChannelInvalid);
        Assert.True(vm.SaveDisabled);
        vm.SlackEnabled = false;
        Assert.False(vm.IsSlackChannelInvalid);
        Assert.False(vm.SaveDisabled);
    }

    [Fact]
    public async Task Unsaved_edits_survive_leaving_and_coming_back()
    {
        var vm = await Activated();
        vm.ReduceMotion = true;
        vm.Deactivate();
        await vm.ActivateAsync(null);
        Assert.True(vm.ReduceMotion);
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public async Task Theme_applies_at_once_and_is_not_an_unsaved_change()
    {
        var vm = await Activated();
        vm.Theme = ThemeChoice.G100;
        Assert.Equal(ThemeChoice.G100, _w.Theme.Current);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public async Task Theme_changed_elsewhere_is_shown_without_reapplying()
    {
        var vm = await Activated();
        _w.Theme.Apply(ThemeChoice.G90);
        vm.SyncTheme();
        Assert.Equal(ThemeChoice.G90, vm.Theme);
        Assert.Empty(_w.Feedback.Toasts);
    }

    [Fact]
    public async Task Regenerate_issues_a_new_token_without_dirtying()
    {
        var vm = await Activated();
        var old = vm.ApiToken;
        await vm.RegenerateTokenCommand.ExecuteAsync(null);
        Assert.NotEqual(old, vm.ApiToken);
        Assert.StartsWith("rly_live_", vm.ApiToken, StringComparison.Ordinal);
        Assert.Equal(_w.Settings.Current.ApiToken, vm.ApiToken);
        Assert.False(vm.IsDirty);
        Assert.Single(_w.Feedback.Toasts);
    }

    [Fact]
    public async Task Webhook_payload_reflects_the_integrations()
    {
        var vm = await Activated();
        Assert.Contains("\"slack\": \"#atlas-dev\"", vm.WebhookPayload, StringComparison.Ordinal);
        vm.SlackEnabled = false;
        Assert.Contains("\"slack\": null", vm.WebhookPayload, StringComparison.Ordinal);
        Assert.Contains("hooks.relay.dev", vm.WebhookPayload, StringComparison.Ordinal);
    }

    [Fact]
    public void Digest_hour_formats_as_clock_time()
    {
        Assert.Equal("12:00 AM", SettingsViewModel.FormatHour(0));
        Assert.Equal("8:00 AM", SettingsViewModel.FormatHour(8));
        Assert.Equal("5:00 PM", SettingsViewModel.FormatHour(17.2));
        Assert.Equal("11:00 PM", SettingsViewModel.FormatHour(23));
    }

    [Fact]
    public async Task Delete_needs_the_exact_workspace_name()
    {
        var vm = await Activated();
        vm.RequestDeleteCommand.Execute(null);
        Assert.True(vm.IsDeleteOpen);
        Assert.True(vm.DeleteDisabled);
        Assert.False(vm.ConfirmDeleteCommand.CanExecute(null));

        vm.DeleteConfirmText = "relay";
        Assert.True(vm.DeleteDisabled);
        vm.DeleteConfirmText = "Rela";
        Assert.False(vm.CanConfirmDelete);
        vm.DeleteConfirmText = "Relay";
        Assert.False(vm.DeleteDisabled);
        Assert.True(vm.ConfirmDeleteCommand.CanExecute(null));

        vm.ConfirmDeleteCommand.Execute(null);
        Assert.False(vm.IsDeleteOpen);
        Assert.Equal("", vm.DeleteConfirmText);
        Assert.Contains(_w.Feedback.Toasts, t => t.Kind == FeedbackKind.Warning);
    }

    [Fact]
    public async Task Cancelling_delete_clears_the_typed_name()
    {
        var vm = await Activated();
        vm.RequestDeleteCommand.Execute(null);
        vm.DeleteConfirmText = "Relay";
        vm.CancelDeleteCommand.Execute(null);
        Assert.False(vm.IsDeleteOpen);
        vm.RequestDeleteCommand.Execute(null);
        Assert.True(vm.DeleteDisabled);
        Assert.Empty(_w.Feedback.Toasts);
    }
}
