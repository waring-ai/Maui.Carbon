using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

public partial class NotificationsPanel : ContentView
{
    public NotificationsPanel()
    {
        InitializeComponent();
    }

    void OnItemClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: NotificationRow row } && BindingContext is ShellViewModel vm)
            vm.OpenNotificationCommand.Execute(row);
    }
}
