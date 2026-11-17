namespace Maui.Carbon;

/// <summary>Carbon tag colors. Color is decoration; the label carries the meaning.</summary>
public enum CdsTagType
{
    /// <summary>Red.</summary>
    Red,
    /// <summary>Magenta.</summary>
    Magenta,
    /// <summary>Purple.</summary>
    Purple,
    /// <summary>Blue.</summary>
    Blue,
    /// <summary>Cyan.</summary>
    Cyan,
    /// <summary>Teal.</summary>
    Teal,
    /// <summary>Green.</summary>
    Green,
    /// <summary>Gray (default).</summary>
    Gray,
    /// <summary>Cool gray.</summary>
    CoolGray,
    /// <summary>Warm gray.</summary>
    WarmGray,
    /// <summary>Highest emphasis: <c>background-inverse</c> with <c>text-inverse</c>.</summary>
    HighContrast,
    /// <summary>Lowest emphasis: transparent with a 1px <c>background-inverse</c> outline.</summary>
    Outline,
}

/// <summary>Tag heights: sm 18, md 24 (default), lg 32.</summary>
public enum CdsTagSize
{
    /// <summary>18: dense rows and table cells.</summary>
    Sm,
    /// <summary>24: the default.</summary>
    Md,
    /// <summary>32: beside 32 controls or as a focal element.</summary>
    Lg,
}
