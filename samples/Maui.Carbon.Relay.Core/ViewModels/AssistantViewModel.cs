using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>One answer in the assistant panel: the prompt, the generated text and the issues it points at.</summary>
public sealed record AssistantAnswer(string Prompt, string Text, IReadOnlyList<string> Highlights, IReadOnlyList<IssueRow> Issues);

/// <summary>The AI assistant header panel: quick actions that summarize work or find duplicates (mock AI).</summary>
public sealed partial class AssistantViewModel(
    IAssistantService assistant,
    IWorkspace workspace,
    IPeopleService people,
    IProjectService projects,
    IIssueService issues,
    INavigator navigator,
    IClock clock) : ObservableObject
{
    public ObservableCollection<AssistantAnswer> Answers { get; } = [];

    /// <summary>True while an answer is being generated (the panel shows AI skeletons).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SummarizeSprintCommand), nameof(SummarizeWeekCommand), nameof(FindDuplicatesCommand))]
    public partial bool IsThinking { get; set; }

    bool CanAsk => !IsThinking;

    [RelayCommand(CanExecute = nameof(CanAsk))]
    async Task SummarizeSprintAsync()
    {
        IsThinking = true;
        try
        {
            var result = await assistant.SummarizeSprintAsync(workspace.CurrentProject.Id);
            Add($"Summarize the {workspace.CurrentProject.Name} sprint", result);
        }
        finally
        {
            IsThinking = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAsk))]
    async Task SummarizeWeekAsync()
    {
        IsThinking = true;
        try
        {
            Add("Summarize my week", await assistant.SummarizeWeekAsync(people.Me.Id));
        }
        finally
        {
            IsThinking = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAsk))]
    async Task FindDuplicatesAsync()
    {
        IsThinking = true;
        try
        {
            var pairs = await assistant.FindDuplicatesAsync();
            var text = pairs.Count == 0
                ? "I didn't find likely duplicates among open issues."
                : $"I found {pairs.Count} pairs of open issues that may describe the same problem. The closest match is {pairs[0].Issue.Key} and {pairs[0].Duplicate.Key} ({pairs[0].Similarity:P0} similar).";
            var keys = pairs.SelectMany(p => new[] { p.Issue.Key, p.Duplicate.Key }).Distinct().Take(6).ToList();
            Add("Find duplicate issues", new AssistantSummary(text, pairs.Select(p => $"{p.Issue.Key} ↔ {p.Duplicate.Key}").ToList(), keys));
        }
        finally
        {
            IsThinking = false;
        }
    }

    [RelayCommand]
    void Clear() => Answers.Clear();

    [RelayCommand]
    Task OpenIssueAsync(IssueRow row) => navigator.NavigateAsync(Routes.IssueDetail, row.Key);

    void Add(string prompt, AssistantSummary summary)
    {
        var now = clock.Now;
        var rows = summary.IssueKeys.Select(issues.Find).OfType<Models.Issue>().Select(i => IssueRow.From(i, people, projects, now)).ToList();
        Answers.Insert(0, new AssistantAnswer(prompt, summary.Text, summary.Highlights, rows));
    }
}
