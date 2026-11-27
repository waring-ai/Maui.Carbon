namespace Maui.Carbon;

/// <summary>Which illustration a <see cref="CdsFullPageError"/> shows (<c>kind</c>).</summary>
public enum CdsFullPageErrorKind
{
    /// <summary>A generic error drawing, for any other error (the default, <c>custom</c>).</summary>
    Custom,
    /// <summary>The 403 drawing: people lack permission to see the page (<c>403</c>).</summary>
    Forbidden,
    /// <summary>The 404 drawing: the page does not exist (<c>404</c>).</summary>
    NotFound,
}
