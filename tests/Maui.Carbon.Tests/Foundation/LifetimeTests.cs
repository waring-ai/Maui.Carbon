using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Carbon.Tests.Foundation;

/// <summary>Long-lived sources (view model commands, the shell) must not keep the views that listen to them alive.</summary>
public class LifetimeTests
{
    sealed class Source
    {
        public event EventHandler? Raised;

        public int Count => Raised?.GetInvocationList().Length ?? 0;

        public void Raise() => Raised?.Invoke(this, EventArgs.Empty);
    }

    sealed class Listener
    {
        public int Calls;
    }

    sealed class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public int Subscribers => CanExecuteChanged?.GetInvocationList().Length ?? 0;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    static void Collect()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    [Fact]
    public void A_weak_subscription_notifies_while_the_subscriber_lives_and_can_be_disposed()
    {
        var source = new Source();
        var listener = new Listener();
        var subscription = new CdsWeakSubscription<Listener>(listener, static l => l.Calls++, h => source.Raised += h, h => source.Raised -= h);
        source.Raise();
        Assert.Equal(1, listener.Calls);
        subscription.Dispose();
        source.Raise();
        Assert.Equal(1, listener.Calls);
        Assert.Equal(0, source.Count);
    }

    [Fact]
    public void A_weak_subscription_lets_the_subscriber_go_and_then_removes_itself()
    {
        var source = new Source();
        var listener = Subscribe(source);
        Collect();
        Assert.False(listener.TryGetTarget(out _));
        Assert.Equal(1, source.Count);
        source.Raise();
        Assert.Equal(0, source.Count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference<Listener> Subscribe(Source source)
    {
        var listener = new Listener();
        _ = new CdsWeakSubscription<Listener>(listener, static l => l.Calls++, h => source.Raised += h, h => source.Raised -= h);
        return new WeakReference<Listener>(listener);
    }

    [Fact]
    public void A_control_bound_to_a_long_lived_command_can_be_collected()
    {
        var command = new Command();
        var item = Bind(command);
        Collect();
        Assert.False(item.TryGetTarget(out _));
        command.RaiseCanExecuteChanged();
        Assert.Equal(0, command.Subscribers);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference<CdsHeaderMenuItem> Bind(Command command) =>
        new(new CdsHeaderMenuItem { Text = "Project", Command = command });

    [Fact]
    public void A_control_still_follows_its_command_and_lets_go_of_a_replaced_one()
    {
        var first = new Command();
        var second = new Command();
        var item = new CdsHeaderMenuItem { Text = "Project", Command = first };
        Assert.Equal(1, first.Subscribers);
        item.Command = second;
        Assert.Equal(0, first.Subscribers);
        Assert.Equal(1, second.Subscribers);
        item.Command = null;
        Assert.Equal(0, second.Subscribers);
    }

    [Fact]
    public void A_part_inside_a_screen_that_left_the_shell_can_be_collected()
    {
        var shell = new CdsUIShell { Body = new ContentView() };
        var header = Show(shell);
        // Replacing the screen re-parents the screen, not the page header deep inside it.
        shell.Body = new ContentView();
        Collect();
        Assert.False(header.TryGetTarget(out _));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference<CdsPageHeaderContent> Show(CdsUIShell shell)
    {
        var screen = new VerticalStackLayout();
        shell.Body = new ContentView { Content = screen };
        // Added once the screen is in the shell, so the header's watcher links to the shell.
        var header = new CdsPageHeaderContent { Title = "Issue" };
        var pageHeader = new CdsPageHeader();
        screen.Add(pageHeader);
        pageHeader.Items.Add(header);
        Assert.True(ShellSubscribers(shell) > 0);
        return new WeakReference<CdsPageHeaderContent>(header);
    }

    static int ShellSubscribers(CdsUIShell shell) =>
        (typeof(CdsUIShell).GetField(nameof(CdsUIShell.ShellChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(shell) as Delegate)?.GetInvocationList().Length ?? 0;

    sealed class Painter : CdsControl
    {
        public void Paint(BoxView view) => SetColor(view, BoxView.ColorProperty, CdsColorKeys.Layer01);
    }

    [Fact]
    public void A_view_a_control_colored_and_dropped_can_be_collected()
    {
        var painter = new Painter();
        var view = Paint(painter);
        Collect();
        Assert.False(view.TryGetTarget(out _));
        GC.KeepAlive(painter);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference<BoxView> Paint(Painter painter)
    {
        var view = new BoxView();
        painter.Paint(view);
        return new WeakReference<BoxView>(view);
    }
}
