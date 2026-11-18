using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// Tracks the breakpoint that applies to a shell part: the enclosing <see cref="CdsUIShell"/>'s (which can force one),
/// else the page's (<see cref="CdsContextKeys.Breakpoint"/>), else the window width.
/// </summary>
internal sealed class CdsBreakpointWatcher
{
    static readonly ConditionalWeakTable<BindableObject, CdsBreakpointWatcher> s_watchers = [];

    static readonly BindableProperty ContextProperty = BindableProperty.CreateAttached(
        "CdsContextBreakpoint", typeof(CdsBreakpoint?), typeof(CdsBreakpointWatcher), null,
        propertyChanged: static (b, _, _) =>
        {
            if (s_watchers.TryGetValue(b, out var watcher)) watcher.Raise();
        });

    readonly VisualElement _owner;
    readonly Action _changed;
    CdsUIShell? _shell;
    CdsWeakSubscription<CdsBreakpointWatcher>? _subscription;

    public CdsBreakpointWatcher(VisualElement owner, Action changed)
    {
        _owner = owner;
        _changed = changed;
        s_watchers.AddOrUpdate(owner, this);
        owner.SetDynamicResource(ContextProperty, CdsContextKeys.Breakpoint);
        owner.ParentChanged += (_, _) => Relink();
        owner.Loaded += (_, _) => Relink();
        // Removing a whole screen from the shell changes the parent of the screen only, not of the parts deep inside it,
        // so ParentChanged alone would leave them subscribed to the shell. Unloaded reaches every part.
        owner.Unloaded += (_, _) => Link(null);
        owner.SizeChanged += (_, _) => Raise();
    }

    /// <summary>The breakpoint in effect.</summary>
    public CdsBreakpoint Current
    {
        get
        {
            // The part can be parented before its ancestors reach the shell (XAML builds bottom up): look again lazily.
            if (_shell is null) Link(CdsShellContext.FindShell(_owner));
            if (_shell is not null) return _shell.EffectiveBreakpoint;
            if (_owner.GetValue(ContextProperty) is CdsBreakpoint context) return context;
            var width = Application.Current?.Windows.FirstOrDefault()?.Width ?? 0;
            return width > 0 ? CdsBreakpointExtensions.FromWidth(width) : CdsBreakpoint.Lg;
        }
    }

    /// <summary>The enclosing shell, if any.</summary>
    public CdsUIShell? Shell
    {
        get
        {
            if (_shell is null) Link(CdsShellContext.FindShell(_owner));
            return _shell;
        }
    }

    void Relink()
    {
        if (Link(CdsShellContext.FindShell(_owner))) Raise();
    }

    bool Link(CdsUIShell? found)
    {
        if (ReferenceEquals(found, _shell)) return false;
        _subscription?.Dispose();
        _subscription = null;
        _shell = found;
        // Weak as well: the shell outlives the screens in it, and a part that is dropped without ever being unloaded (built
        // but never shown) must not be kept alive by the shell.
        if (found is not null)
            _subscription = new CdsWeakSubscription<CdsBreakpointWatcher>(
                this, static w => w.Raise(), h => found.ShellChanged += h, h => found.ShellChanged -= h);
        return true;
    }

    void Raise() => _changed();
}

/// <summary>Finds the shell parts a view belongs to.</summary>
internal static class CdsShellContext
{
    public static CdsUIShell? FindShell(Element? element)
    {
        for (var e = element; e is not null; e = e.Parent)
            if (e is CdsUIShell shell) return shell;
        return null;
    }

    public static CdsSideNav? FindSideNav(Element? element)
    {
        for (var e = element; e is not null; e = e.Parent)
            if (e is CdsSideNav nav) return nav;
        return null;
    }
}

/// <summary>
/// The base of the clickable parts of the UI shell (header links and actions, side nav rows, switcher items): an
/// <see cref="IsActive"/> flag for the current item, <see cref="Clicked"/> and a command. Navigation is yours: handle
/// <see cref="Clicked"/> or bind <see cref="Command"/>, swap the shell's body or call your navigation, and set
/// <see cref="IsActive"/> on the destination.
/// </summary>
public abstract class CdsShellItem : CdsControl
{
    /// <summary>Bindable property for <see cref="IsActive"/>.</summary>
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(CdsShellItem), false,
        propertyChanged: static (b, _, _) => ((CdsShellItem)b).OnActiveChanged());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsShellItem), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsShellItem), null, propertyChanged: OnCommandChanged);

    /// <summary>Raised on activation (click, tap, Enter or Space), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>Raised on activation, for the menu that hosts the item (it closes itself).</summary>
    internal event EventHandler? Activated;

    /// <summary>Marks the current page or section (<c>aria-current="page"</c>): the item shows the active treatment.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Executed on activation; <c>CanExecute</c> drives <c>IsEnabled</c>.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Called when <see cref="IsActive"/> changes.</summary>
    protected virtual void OnActiveChanged() => RefreshVisuals();

    /// <summary>Runs the item's own behavior before <see cref="Clicked"/> (toggling a menu, a switch).</summary>
    protected virtual void OnItemActivated()
    {
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        OnItemActivated();
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        Activated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Activates the item as if it was clicked (used by a hosting menu's keyboard handling).</summary>
    internal void Activate()
    {
        if (IsEnabled) OnActivated();
    }

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var item = (CdsShellItem)b;
        item.SetCommand(item.Command, item.CommandParameter);
    }
}
