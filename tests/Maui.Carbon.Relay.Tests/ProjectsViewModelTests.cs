namespace Maui.Carbon.Relay.Tests;

public class ProjectsViewModelTests
{
    readonly TestWorld _w = new();

    ProjectsViewModel Create() => new(_w.Projects, _w.Issues, _w.People, _w.Workspace, _w.Navigator, _w.Feedback, _w.Clock);

    async Task<ProjectsViewModel> Activated()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        return vm;
    }

    [Fact]
    public async Task Activation_lists_active_projects_starred_first()
    {
        var vm = await Activated();
        Assert.False(vm.IsLoading);
        Assert.Equal(_w.Projects.All.Count(p => !p.IsArchived), vm.Cards.Count);
        Assert.DoesNotContain(vm.Cards, c => c.IsArchived);
        Assert.True(vm.Cards.TakeWhile(c => c.IsStarred).Count() == vm.Cards.Count(c => c.IsStarred));
        Assert.False(vm.IsEmpty);
    }

    [Fact]
    public async Task Cards_count_open_issues_and_name_the_lead()
    {
        var vm = await Activated();
        var atlas = vm.Cards.Single(c => c.Key == "ATL");
        Assert.Equal(_w.Issues.All.Count(i => i.ProjectId == "atl" && i.IsOpen), atlas.OpenIssues);
        Assert.Equal("Daniel Okafor", atlas.LeadName);
        Assert.StartsWith("Target ", atlas.TargetText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Search_matches_name_key_and_description()
    {
        var vm = await Activated();
        vm.SearchText = "por";
        Assert.Equal(["POR"], vm.Cards.Select(c => c.Key));
        vm.SearchText = "billing";
        Assert.Equal(["BIL"], vm.Cards.Select(c => c.Key));
        vm.SearchText = "tokens";
        Assert.Equal(["HBR"], vm.Cards.Select(c => c.Key));
        Assert.True(vm.HasFilters);
    }

    [Fact]
    public async Task Category_starred_and_archived_filters_combine()
    {
        var vm = await Activated();
        vm.ToggleCategory(ProjectCategory.Platform, true);
        Assert.Equal(["BIL"], vm.Cards.Select(c => c.Key));
        vm.ShowArchived = true;
        Assert.Equal(["BIL", "LEG"], vm.Cards.Select(c => c.Key));
        vm.StarredOnly = true;
        Assert.Equal(["BIL"], vm.Cards.Select(c => c.Key));
    }

    [Fact]
    public async Task No_match_shows_the_empty_state_and_clear_filters_restores()
    {
        var vm = await Activated();
        var reset = 0;
        vm.FiltersReset += (_, _) => reset++;
        vm.SearchText = "quantum";
        Assert.True(vm.IsEmpty);
        vm.ClearFiltersCommand.Execute(null);
        Assert.False(vm.IsEmpty);
        Assert.False(vm.HasFilters);
        Assert.Equal(1, reset);
    }

    [Fact]
    public async Task Star_toggles_through_the_service_and_toasts()
    {
        var vm = await Activated();
        var portal = vm.Cards.Single(c => c.Key == "POR");
        Assert.False(portal.IsStarred);
        vm.ToggleStarCommand.Execute(portal);
        Assert.True(_w.Projects.Find("por")!.IsStarred);
        Assert.True(vm.Cards.Single(c => c.Key == "POR").IsStarred);
        Assert.Contains(_w.Feedback.Toasts, t => t.Title == "Customer portal starred");
    }

    [Fact]
    public async Task Opening_a_card_scopes_the_workspace_and_shows_the_dashboard()
    {
        var vm = await Activated();
        await vm.OpenProjectCommand.ExecuteAsync(vm.Cards.Single(c => c.Key == "HBR"));
        Assert.Equal("hbr", _w.Workspace.CurrentProject.Id);
        Assert.Equal(Routes.Dashboard, _w.Navigator.CurrentRoute);
    }

    [Theory]
    [InlineData("Mobile release train", "MRT")]
    [InlineData("Payments", "PAY")]
    [InlineData("Atlas mobile", "AM")]
    [InlineData("Billing", "BIL2")]
    public void Key_is_suggested_from_the_name(string name, string expected)
    {
        var key = ProjectsQuery.SuggestKey(name, _w.Projects.IsKeyAvailable);
        if (expected == "BIL2")
        {
            // BIL is taken: a longer, free key is suggested instead.
            Assert.NotEqual("BIL", key);
            Assert.StartsWith("BIL", key, StringComparison.Ordinal);
            Assert.True(_w.Projects.IsKeyAvailable(key));
        }
        else
        {
            Assert.Equal(expected, key);
        }
    }

    [Fact]
    public async Task Typing_a_name_suggests_a_key_until_the_key_is_edited()
    {
        var vm = await Activated();
        vm.NewProjectCommand.Execute(null);
        Assert.True(vm.IsCreateOpen);
        vm.NewName = "Field service";
        Assert.Equal("FS", vm.NewKey);
        vm.NewKey = "fsv";
        Assert.Equal("FSV", vm.NewKey);
        vm.NewName = "Field service app";
        Assert.Equal("FSV", vm.NewKey);
    }

    [Fact]
    public async Task Key_validation_rejects_bad_shapes_and_keys_in_use()
    {
        var vm = await Activated();
        vm.NewProjectCommand.Execute(null);
        vm.NewName = "Something";
        vm.NewKey = "ATL";
        Assert.True(vm.IsKeyInvalid);
        Assert.Equal("ATL is already used by Atlas mobile.", vm.KeyError);
        vm.NewKey = "A";
        Assert.Equal("Use 2 to 5 capital letters, A to Z.", vm.KeyError);
        vm.NewKey = "TOOLONG";
        Assert.NotNull(vm.KeyError);
        vm.NewKey = "AB1";
        Assert.NotNull(vm.KeyError);
        vm.NewKey = "SMT";
        Assert.Null(vm.KeyError);
        Assert.False(vm.CreateDisabled);
    }

    [Fact]
    public async Task Create_is_blocked_until_the_form_is_valid()
    {
        var vm = await Activated();
        vm.NewProjectCommand.Execute(null);
        Assert.True(vm.CreateDisabled);
        Assert.False(vm.IsNameInvalid);
        await vm.CreateProjectCommand.ExecuteAsync(null);
        Assert.True(vm.IsCreateOpen);
        Assert.True(vm.IsNameInvalid);
        Assert.Equal(6, _w.Projects.All.Count);
    }

    [Fact]
    public async Task Create_adds_the_project_toasts_and_refreshes()
    {
        var vm = await Activated();
        vm.NewProjectCommand.Execute(null);
        vm.NewName = "Field service";
        vm.NewDescription = "Scheduling and dispatch for field technicians.";
        vm.NewCategory = vm.CategoryOptions.Single(c => c.Value == ProjectCategory.Web);
        vm.NewLead = _w.People.Find("p08");
        vm.NewTargetDate = TestWorld.Now.Date.AddDays(60);
        vm.SetMembers(_w.People.All.Take(3));
        vm.RepositoryQuery = "portal";
        Assert.Equal(["relay/portal-web"], vm.RepositoryMatches);
        vm.NewRepository = "relay/portal-web";

        await vm.CreateProjectCommand.ExecuteAsync(null);

        Assert.False(vm.IsCreateOpen);
        var created = _w.Projects.All.Single(p => p.Key == "FS");
        Assert.Equal("Field service", created.Name);
        Assert.Equal(ProjectCategory.Web, created.Category);
        Assert.Equal("p08", created.LeadId);
        Assert.Contains(vm.Cards, c => c.Key == "FS");
        var toast = Assert.Single(_w.Feedback.Toasts);
        Assert.Equal(FeedbackKind.Success, toast.Kind);
        Assert.Equal("Field service created", toast.Title);
        Assert.Contains("3 members", toast.Subtitle, StringComparison.Ordinal);
        Assert.Contains("relay/portal-web", toast.Subtitle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Opening_the_form_again_starts_from_a_clean_draft()
    {
        var vm = await Activated();
        vm.NewProjectCommand.Execute(null);
        vm.NewName = "Draft";
        vm.CancelCreateCommand.Execute(null);
        Assert.False(vm.IsCreateOpen);
        vm.NewProjectCommand.Execute(null);
        Assert.Equal("", vm.NewName);
        Assert.Equal("", vm.NewKey);
        Assert.Equal(_w.People.Me, vm.NewLead);
    }
}
