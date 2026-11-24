using System.Reflection;
using Microsoft.Maui.Dispatching;

namespace Maui.Carbon.Tests.Components;

/// <summary>
/// Puts a control on a loaded page with a Carbon overlay host, headlessly, so list boxes can open and announcements that
/// wait for the control to be on screen are made. The dispatcher runs work inline and exists only for the calling test's
/// logical context; other tests keep the platform-less default.
/// </summary>
internal sealed class FormsTestHost : IDisposable
{
    static readonly object s_gate = new();
    static bool s_installed;
    readonly IDispatcher? _previous;

    FormsTestHost(View content)
    {
        Install();
        _previous = ScopedDispatcherProvider.Scoped.Value;
        ScopedDispatcherProvider.Scoped.Value = new InlineDispatcher();
        Host = new CdsOverlayHost { Body = content };
        Window = new Window(new ContentPage { Content = Host });
    }

    public CdsOverlayHost Host { get; }

    public Window Window { get; }

    /// <summary>Builds <paramref name="content"/> on a loaded page. Create the controls inside <paramref name="build"/> so they get the dispatcher.</summary>
    public static FormsTestHost Load<T>(Func<T> build, out T control) where T : View
    {
        Install();
        var previous = ScopedDispatcherProvider.Scoped.Value;
        ScopedDispatcherProvider.Scoped.Value = new InlineDispatcher();
        try
        {
            control = build();
        }
        finally
        {
            ScopedDispatcherProvider.Scoped.Value = previous;
        }
        return new FormsTestHost(control);
    }

    /// <summary>Gives a list box's field a size, so its list can open (the list matches the field width).</summary>
    public static void SizeField(CdsFormField field)
    {
        var chrome = (CdsFieldChrome)typeof(CdsFormField).GetField("Chrome", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(field)!;
        ((IView)chrome.Field).Arrange(new Rect(0, 0, 320, 40));
    }

    public void Dispose() => ScopedDispatcherProvider.Scoped.Value = _previous;

    static void Install()
    {
        lock (s_gate)
        {
            if (s_installed) return;
            s_installed = true;
            DispatcherProvider.SetCurrent(new ScopedDispatcherProvider(DispatcherProvider.Current));
        }
    }

    sealed class ScopedDispatcherProvider(IDispatcherProvider? fallback) : IDispatcherProvider
    {
        public static readonly AsyncLocal<IDispatcher?> Scoped = new();

        public IDispatcher? GetForCurrentThread() => Scoped.Value ?? fallback?.GetForCurrentThread();
    }

    sealed class InlineDispatcher : IDispatcher
    {
        public bool IsDispatchRequired => false;

        public bool Dispatch(Action action)
        {
            action();
            return true;
        }

        public bool DispatchDelayed(TimeSpan delay, Action action)
        {
            action();
            return true;
        }

        public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
    }
}

/// <summary>Reads private fields of controls under test.</summary>
internal static class Private
{
    public static T Field<T>(object target, string name)
    {
        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            if (type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public) is { } field)
                return (T)field.GetValue(target)!;
        }
        throw new MissingFieldException(target.GetType().Name, name);
    }
}
