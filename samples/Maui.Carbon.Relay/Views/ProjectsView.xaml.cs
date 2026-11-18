using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The projects grid. Bindings carry the values; this code-behind builds the category chips (one per enum value) and
/// adapts the card, star and member multi-select events to view model calls.
/// </summary>
public partial class ProjectsView : ContentView
{
    readonly Dictionary<ProjectCategory, CdsSelectableTag> _categoryTags = [];
    ProjectsViewModel? _vm;
    bool _syncing;

    public ProjectsView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);

        var index = 0;
        foreach (var category in Enum.GetValues<ProjectCategory>())
        {
            var tag = new CdsSelectableTag { Text = category.Text(), Icon = CarbonMap.Icon(category), Margin = new Thickness(0, 0, 8, 8) };
            tag.SelectedChanged += (_, e) =>
            {
                if (!_syncing) _vm?.ToggleCategory(category, e.Value);
            };
            _categoryTags[category] = tag;
            FilterTags.Children.Insert(index++, tag);
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null)
        {
            _vm.FiltersReset -= OnFiltersReset;
            _vm.DraftReset -= OnDraftReset;
        }
        _vm = BindingContext as ProjectsViewModel;
        if (_vm is null) return;
        _vm.FiltersReset += OnFiltersReset;
        _vm.DraftReset += OnDraftReset;
        SyncCategoryTags();
    }

    void OnFiltersReset(object? sender, EventArgs e) => SyncCategoryTags();

    void SyncCategoryTags()
    {
        if (_vm is null) return;
        _syncing = true;
        try
        {
            foreach (var (category, tag) in _categoryTags) tag.IsSelected = _vm.SelectedCategories.Contains(category);
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnDraftReset(object? sender, EventArgs e)
    {
        _syncing = true;
        try
        {
            MembersSelect.SelectedItems = new List<object>();
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnMembersChanged(object? sender, CdsListBoxSelectionChangedEventArgs e)
    {
        if (!_syncing) _vm?.SetMembers(e.SelectedItems.OfType<Person>());
    }

    void OnCardClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: ProjectCard card }) _vm?.OpenProjectCommand.Execute(card);
    }

    void OnStarClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: ProjectCard card }) _vm?.ToggleStarCommand.Execute(card);
    }
}
