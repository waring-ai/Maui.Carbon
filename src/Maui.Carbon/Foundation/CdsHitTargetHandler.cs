using Microsoft.Maui.Handlers;

namespace Maui.Carbon;

/// <summary>Handler for <see cref="CdsHitTarget"/>: a native button with every bit of native chrome removed.</summary>
public partial class CdsHitTargetHandler : ButtonHandler
{
    /// <summary>The property mapper.</summary>
    public static readonly IPropertyMapper<CdsHitTarget, CdsHitTargetHandler> HitTargetMapper =
        new PropertyMapper<CdsHitTarget, CdsHitTargetHandler>(ButtonHandler.Mapper)
        {
            [nameof(IView.Background)] = MapChrome,
            [nameof(CdsHitTarget.Role)] = MapAccessibility,
            [nameof(CdsHitTarget.IsChecked)] = MapAccessibility,
            [nameof(CdsHitTarget.IsSelected)] = MapAccessibility,
            [nameof(CdsHitTarget.IsExpanded)] = MapAccessibility,
            [nameof(IView.IsEnabled)] = MapIsEnabled,
        };

    /// <summary>Creates the handler.</summary>
    public CdsHitTargetHandler() : base(HitTargetMapper, CommandMapper)
    {
    }

    static void MapChrome(CdsHitTargetHandler handler, CdsHitTarget target) => handler.StripChrome();

    static void MapAccessibility(CdsHitTargetHandler handler, CdsHitTarget target) => handler.UpdateAccessibility(target);

    static void MapIsEnabled(CdsHitTargetHandler handler, CdsHitTarget target)
    {
        ViewHandler.MapIsEnabled(handler, target);
        handler.UpdateAccessibility(target);
    }

    partial void StripChrome();

    partial void UpdateAccessibility(CdsHitTarget target);
}
