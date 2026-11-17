namespace Maui.Carbon;

/// <summary>Carbon 2x grid breakpoints.</summary>
public enum CdsBreakpoint
{
    /// <summary>Below 672: 4 columns, no outer margin.</summary>
    Sm,
    /// <summary>672 and up: 8 columns, 16 margins.</summary>
    Md,
    /// <summary>1056 and up: 16 columns, 16 margins.</summary>
    Lg,
    /// <summary>1312 and up: 16 columns, 16 margins.</summary>
    Xlg,
    /// <summary>1584 and up: 16 columns, 24 margins.</summary>
    Max,
}

/// <summary>Breakpoint helpers.</summary>
public static class CdsBreakpointExtensions
{
    /// <summary>The breakpoint for a viewport width.</summary>
    public static CdsBreakpoint FromWidth(double width) => width switch
    {
        >= CdsBreakpoints.Max => CdsBreakpoint.Max,
        >= CdsBreakpoints.Xlg => CdsBreakpoint.Xlg,
        >= CdsBreakpoints.Lg => CdsBreakpoint.Lg,
        >= CdsBreakpoints.Md => CdsBreakpoint.Md,
        _ => CdsBreakpoint.Sm,
    };

    /// <summary>Number of grid columns at a breakpoint (4, 8 or 16).</summary>
    public static int Columns(this CdsBreakpoint bp) => bp switch
    {
        CdsBreakpoint.Sm => 4,
        CdsBreakpoint.Md => 8,
        _ => 16,
    };

    /// <summary>Outer grid margin at a breakpoint.</summary>
    public static double Margin(this CdsBreakpoint bp) => bp switch
    {
        CdsBreakpoint.Sm => 0,
        CdsBreakpoint.Max => 24,
        _ => 16,
    };
}
