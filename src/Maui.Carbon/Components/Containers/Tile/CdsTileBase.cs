using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The shared surface of every tile: a square <c>layer-01</c> fill with 16px padding, at least 64 high and 128 wide,
/// optionally a 1px border, and a 2px inset focus ring. Tiles use only <c>-01</c> tokens, so a <see cref="CdsLayer"/>
/// around them steps the fill up one layer. Derive to build a tile variant. A <see cref="Decorator"/> that is an
/// <see cref="CdsAILabel"/> gives the tile Carbon's AI presence (aura, gradient border, shadow).
/// </summary>
public abstract partial class CdsTileBase : CdsContainer
{
    /// <summary>The tile surface: fill and 1px border.</summary>
    protected readonly Border Surface = new()
    {
        StrokeShape = new Rectangle(),
        StrokeThickness = 0,
        Padding = 0,
        MinimumHeightRequest = 64,
        MinimumWidthRequest = 128,
    };

    /// <summary>The single-cell grid inside the surface. Variants add their glyphs and hit target after the body, ring first.</summary>
    protected readonly Grid Stack = new() { IsClippedToBounds = true };

    /// <summary>Hosts <see cref="CdsContainer.Body"/> with the tile padding.</summary>
    protected readonly ContentView BodyHost = new() { Padding = CdsSpacing.Spacing05 };

    /// <summary>The 2px inset focus ring.</summary>
    protected readonly CdsFocusRing Ring = new(CdsFocusRingStyle.Inset);

    /// <summary>Creates the tile surface.</summary>
    protected CdsTileBase()
    {
        Stack.Add(BodyHost);
        Stack.Add(Ring);
        Surface.Content = Stack;
        SetChrome(Surface);
    }

    /// <summary>Which variant's tokens apply.</summary>
    protected abstract CdsTileKind TileKind { get; }

    /// <summary>The state flags the tokens see; variants add <see cref="CdsInteractionState.Selected"/>.</summary>
    protected virtual CdsInteractionState TokenState => State;

    /// <summary>The border thickness: 0, or 1 for the selectable variants.</summary>
    protected virtual double BorderThickness => 0;

    /// <summary>Makes the body ignore input and hide from screen readers, because the tile's hit target is the control.</summary>
    protected void SealBody()
    {
        BodyHost.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(BodyHost, true);
    }

    /// <summary>The tile's text as one accessible name: all labels of the body, in order.</summary>
    protected string? BodyText() => CdsTileText.Collect(Body);

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        BodyHost.Content = newBody;
        OnBodyReplaced();
    }

    /// <summary>Called after the body changed (variants refresh their accessible name).</summary>
    protected virtual void OnBodyReplaced()
    {
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = TokenState;
        var v = CdsTileTokens.Resolve(TileKind, state);
        ApplyContentTone(state.HasFlag(CdsInteractionState.Disabled));
        if (IsAiPresence) ApplyAiSurface(state);
        else ApplyFlatSurface(v);
        Ring.IsActive = Is(CdsInteractionState.FocusVisible);
        ApplyTileVisuals(v);
    }

    /// <summary>Applies variant parts (icons) from the resolved colors.</summary>
    protected virtual void ApplyTileVisuals(CdsTileVisuals visuals)
    {
    }

    /// <inheritdoc/>
    // Only the disabled content tone depends on the theme; the surface colors follow their dynamic resources. This runs
    // from CdsControl's constructor when an app theme is already in place, before a variant has built its parts, so it
    // must not reach ApplyTileVisuals.
    protected override void OnContextChanged()
    {
        if (_contentToneApplied is { } applied) ApplyContentTone(applied.Disabled);
    }

    // A disabled tile's content uses text-disabled and icon-disabled (Tile, SelectableTile and RadioTile READMEs). On the
    // web the color is inherited; here every CdsText and CdsIcon binds its own token, so the tile re-maps the text and
    // icon tokens for everything inside it, the way CdsLayer re-maps the layer tokens. Created on the first disable only.
    ResourceDictionary? _contentTone;
    (CdsThemeName Theme, bool Disabled)? _contentToneApplied;

    void ApplyContentTone(bool disabled)
    {
        if (_contentTone is null)
        {
            if (!disabled) return;
            _contentTone = [];
            Stack.Resources = _contentTone;
        }
        var applied = (ContextTheme, disabled);
        if (_contentToneApplied == applied) return;
        _contentToneApplied = applied;
        foreach (var (key, disabledKey) in CdsTileTokens.DisabledContentKeys)
            _contentTone[key] = CdsPalettes.GetColor(applied.ContextTheme, disabled ? disabledKey : key);
    }
}

/// <summary>Collects the text of a view tree, for the accessible name of a tile.</summary>
internal static class CdsTileText
{
    /// <summary>All label texts under <paramref name="root"/> in visual order, joined by a space; <c>null</c> when there is none.</summary>
    public static string? Collect(Element? root)
    {
        var parts = new List<string>();
        Walk(root, parts);
        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    static void Walk(Element? element, List<string> parts)
    {
        switch (element)
        {
            case null:
                return;
            case Label { Text: { Length: > 0 } text }:
                parts.Add(text);
                return;
            case Label { FormattedText: { } formatted }:
                var joined = string.Concat(formatted.Spans.Select(s => s.Text));
                if (joined.Length > 0) parts.Add(joined);
                return;
        }
        if (element is IVisualTreeElement tree)
            foreach (var child in tree.GetVisualChildren())
                Walk(child as Element, parts);
    }

    /// <summary>The visible elements under <paramref name="root"/> (breadth is irrelevant; order is document order).</summary>
    public static IEnumerable<Element> Descendants(Element? root)
    {
        if (root is not IVisualTreeElement tree) yield break;
        foreach (var child in tree.GetVisualChildren())
        {
            if (child is not Element element) continue;
            yield return element;
            foreach (var nested in Descendants(element)) yield return nested;
        }
    }
}
