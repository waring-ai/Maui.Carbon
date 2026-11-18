using System.Reflection;
using System.Windows.Input;
using Maui.Carbon.Relay.Pages;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Counts subscribers on the long-lived events a control can be kept alive by: the shell's <c>ShellChanged</c>,
/// <see cref="CdsThemeManager.ThemeChanged"/>, and <c>CanExecuteChanged</c> on every command of the singleton view
/// models. Keyed by event and subscriber type, so growth across a leak scenario names who is holding on.
/// </summary>
static class RetainerProbe
{
    public static Dictionary<string, int> Snapshot(IServiceProvider services)
    {
        var counts = new Dictionary<string, int>();
        void Add(string source, Delegate? d)
        {
            if (d is null) return;
            foreach (var h in d.GetInvocationList())
            {
                var key = $"{source} -> {h.Target?.GetType().Name ?? "static"}";
                counts[key] = counts.GetValueOrDefault(key) + 1;
            }
        }

        if (services.GetRequiredService<MainPage>().FindByName("Shell") is CdsUIShell shell)
            Add("CdsUIShell.ShellChanged", EventField(typeof(CdsUIShell), shell, "ShellChanged"));
        Add("CdsThemeManager.ThemeChanged", EventField(typeof(CdsThemeManager), null, "ThemeChanged"));

        foreach (var vmType in new[] { typeof(ShellViewModel), typeof(AssistantViewModel), typeof(DashboardViewModel), typeof(IssuesViewModel), typeof(BoardViewModel), typeof(BacklogViewModel), typeof(ProjectsViewModel), typeof(TeamViewModel), typeof(ReportsViewModel), typeof(SettingsViewModel) })
        {
            var vm = services.GetRequiredService(vmType);
            foreach (var p in vmType.GetProperties().Where(p => typeof(ICommand).IsAssignableFrom(p.PropertyType)))
            {
                if (p.GetValue(vm) is not { } command) continue;
                Add($"{vmType.Name}.{p.Name}.CanExecuteChanged", EventField(command.GetType(), command, "CanExecuteChanged"));
            }
        }
        return counts;
    }

    static Delegate? EventField(Type type, object? target, string name)
    {
        for (var t = type; t is not null; t = t.BaseType)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | (target is null ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.DeclaredOnly;
            if (t.GetField(name, flags) is { } f) return f.GetValue(target) as Delegate;
        }
        return null;
    }
}
