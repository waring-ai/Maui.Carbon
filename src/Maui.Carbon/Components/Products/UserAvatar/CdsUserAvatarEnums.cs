namespace Maui.Carbon;

/// <summary>
/// The twelve avatar background colors, in the order Carbon for IBM Products assigns them: six mid tones, then six deep
/// tones of the same hues. Give the people in one view different colors; the initials or photo carry the identity.
/// </summary>
public enum CdsUserAvatarColor
{
    /// <summary>Cyan (the default).</summary>
    Order1Cyan,
    /// <summary>Gray.</summary>
    Order2Gray,
    /// <summary>Green.</summary>
    Order3Green,
    /// <summary>Magenta.</summary>
    Order4Magenta,
    /// <summary>Purple.</summary>
    Order5Purple,
    /// <summary>Teal.</summary>
    Order6Teal,
    /// <summary>Deep cyan.</summary>
    Order7Cyan,
    /// <summary>Deep gray.</summary>
    Order8Gray,
    /// <summary>Deep green.</summary>
    Order9Green,
    /// <summary>Deep magenta.</summary>
    Order10Magenta,
    /// <summary>Deep purple.</summary>
    Order11Purple,
    /// <summary>Deep teal.</summary>
    Order12Teal,
}

/// <summary>What a <see cref="CdsUserAvatar"/> draws, in order of precedence.</summary>
public enum CdsUserAvatarContent
{
    /// <summary>The photo (<see cref="CdsUserAvatar.Image"/>).</summary>
    Image,
    /// <summary>The chosen icon (<see cref="CdsUserAvatar.Icon"/>).</summary>
    Icon,
    /// <summary>Initials from <see cref="CdsUserAvatar.Name"/>.</summary>
    Initials,
    /// <summary>The default <c>user</c> icon.</summary>
    DefaultIcon,
}
