namespace Maui.Carbon;

/// <summary>Carbon control sizes. Not every control supports every size; each control documents its range.</summary>
public enum CdsSize
{
    /// <summary>xs: 24.</summary>
    Xs,
    /// <summary>sm: 32.</summary>
    Sm,
    /// <summary>md: 40.</summary>
    Md,
    /// <summary>lg: 48.</summary>
    Lg,
    /// <summary>xl: 64.</summary>
    Xl,
    /// <summary>2xl: 80.</summary>
    Xxl,
}

/// <summary>Helpers for <see cref="CdsSize"/>.</summary>
public static class CdsSizeExtensions
{
    /// <summary>The control height of a size.</summary>
    public static double Height(this CdsSize size) => size switch
    {
        CdsSize.Xs => CdsSizing.XSmall,
        CdsSize.Sm => CdsSizing.Small,
        CdsSize.Md => CdsSizing.Medium,
        CdsSize.Lg => CdsSizing.Large,
        CdsSize.Xl => CdsSizing.XLarge,
        _ => CdsSizing.X2Large,
    };
}
