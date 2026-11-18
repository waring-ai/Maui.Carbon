using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>Converters and templates for the board and backlog screens.</summary>
public static class BoardConverters
{
    /// <summary>A WIP tag is gray until the column goes over its limit, then red (its text says so too).</summary>
    public static IValueConverter WipTag { get; } = new FuncConverter<WipState, CdsTagType>(s => s == WipState.Over ? CdsTagType.Red : CdsTagType.Gray);

    /// <summary>A warning icon on an over-limit WIP tag.</summary>
    public static IValueConverter WipIcon { get; } = new FuncConverter<WipState, CdsIconName?>(s => s == WipState.Over ? CdsIconName.WarningAlt : null);

    /// <summary>An active quick filter is a blue tag with a checkmark; an inactive one is gray.</summary>
    public static IValueConverter FilterTag { get; } = new FuncConverter<bool, CdsTagType>(on => on ? CdsTagType.Blue : CdsTagType.Gray);

    public static IValueConverter FilterIcon { get; } = new FuncConverter<bool, CdsIconName?>(on => on ? CdsIconName.Checkmark : null);

    /// <summary>A met sprint commitment is green, a missed one warm gray (the outcome text says which).</summary>
    public static IValueConverter OutcomeTag { get; } = new FuncConverter<bool, CdsTagType>(met => met ? CdsTagType.Green : CdsTagType.WarmGray);
}

/// <summary>Picks the card template: a plain tile with a menu, or a selectable tile in select mode.</summary>
public sealed class BoardCardTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Tile { get; set; }

    public DataTemplate? Selectable { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        (item is BoardCard { IsSelectMode: true } ? Selectable : Tile) ?? throw new InvalidOperationException("Set both board card templates.");
}
