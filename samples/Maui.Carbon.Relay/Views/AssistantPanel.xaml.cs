using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

public partial class AssistantPanel : ContentView
{
    public AssistantPanel()
    {
        InitializeComponent();
    }

    void OnIssueClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: IssueRow row } && BindingContext is AssistantViewModel vm)
            vm.OpenIssueCommand.Execute(row);
    }
}
