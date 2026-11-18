#if WINDOWS
using Microsoft.Maui.LifecycleEvents;
#endif
using Serilog;
using Serilog.Events;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Relay's log file, written with Serilog so a test run can be checked for errors afterwards. Everything logged through
/// <see cref="Microsoft.Extensions.Logging.ILogger"/> lands in it (navigation, toasts, MAUI's own warnings such as
/// binding failures), as do unhandled and unobserved exceptions. One line per event, with the level as
/// <c>[ERR]</c>, <c>[FTL]</c>, <c>[WRN]</c>, so <c>grep -E '\[(ERR|FTL)\]'</c> finds the failures.
/// <para>
/// The file is <c>relay-&lt;yyyyMMdd&gt;.log</c> (a new one each day, the last 7 kept) in <c>RELAY_LOG_DIR</c> or the
/// <c>--log-dir</c> argument, else the screenshot directory in screenshot mode, else <c>Logs/&lt;app id&gt;</c> under the
/// app data directory (<c>~/Library/Logs/ai.waring.mauicarbon.relay</c> on Mac Catalyst). <see cref="Directory"/> says which, and the first line of each launch repeats it.
/// </para>
/// </summary>
public static class AppLog
{
    public const string FilePrefix = "relay-";

    /// <summary>Where this launch writes its log.</summary>
    public static string Directory { get; private set; } = "";

    public static void Configure(MauiAppBuilder builder)
    {
        Directory = LaunchArgs.Get("--log-dir", "RELAY_LOG_DIR")
            ?? LaunchArgs.Get("--snapshot", "CARBON_SNAPSHOT_DIR")
            // Unsandboxed Mac Catalyst's app data directory is ~/Library itself, so this is ~/Library/Logs/<app id>
            // there (where Console.app looks); a folder of the app's own on iOS, Android and Windows.
            ?? Path.Combine(FileSystem.AppDataDirectory, "Logs", AppInfo.PackageName);
        System.IO.Directory.CreateDirectory(Directory);

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            // MAUI and the hosting stack are chatty below Warning; their warnings (binding failures) still come through.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(Directory, FilePrefix + ".log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                // Unbuffered (the default): every line is flushed, so the file is complete even if the app crashes.
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger, dispose: true);
        CatchUnhandled(builder);

        Log.ForContext(typeof(AppLog)).Information(
            "Relay {Version} starting on {Platform} {OSVersion}; log directory {LogDirectory}",
            AppInfo.VersionString, DeviceInfo.Platform, DeviceInfo.VersionString, Directory);
    }

    static void CatchUnhandled(MauiAppBuilder builder)
    {
        var log = Log.ForContext(typeof(AppLog));

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            log.Fatal(e.ExceptionObject as Exception, "Unhandled exception (terminating: {IsTerminating})", e.IsTerminating);
            if (e.IsTerminating) Log.CloseAndFlush();
        };
        // A faulted fire-and-forget task (for example `_ = navigator.NavigateAsync(...)`) surfaces only here, at GC.
        TaskScheduler.UnobservedTaskException += (_, e) => log.Error(e.Exception, "Unobserved task exception");

#if IOS || MACCATALYST
        // A managed exception escaping into a UIKit callback is reported here before the process aborts.
        ObjCRuntime.Runtime.MarshalManagedException += (_, e) =>
        {
            log.Fatal(e.Exception, "Managed exception crossing into Objective-C ({Mode})", e.ExceptionMode);
        };
#elif ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            log.Fatal(e.Exception, "Unhandled Android exception (handled: {Handled})", e.Handled);
        };
#elif WINDOWS
        builder.ConfigureLifecycleEvents(events => events.AddWindows(windows => windows.OnLaunched((app, _) =>
            app.UnhandledException += (_, e) => log.Fatal(e.Exception, "Unhandled WinUI exception (handled: {Handled})", e.Handled))));
#endif
    }
}

/// <summary>Launch settings from a <c>--name value</c> argument or, failing that, an environment variable.</summary>
static class LaunchArgs
{
    public static string? Get(string name, string env)
    {
        var args = Environment.GetCommandLineArgs();
        var i = Array.IndexOf(args, name);
        var value = i >= 0 && i + 1 < args.Length ? args[i + 1] : Environment.GetEnvironmentVariable(env);
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
