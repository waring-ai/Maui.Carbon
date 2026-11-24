using System.Collections.Specialized;
using System.ComponentModel;

namespace Maui.Carbon;

/// <summary>
/// Running text with inline links: a paragraph whose <see cref="FormattedText"/> mixes plain <see cref="Span"/>s and
/// <see cref="CdsLinkSpan"/>s (Carbon's <c>inline</c> links), with every link reachable by keyboard and screen readers. A span
/// is not a view, so on its own it can be tapped but never focused or named; this paragraph adds one native link per enabled
/// <see cref="CdsLinkSpan"/>, in text order and named by the span's text. Tab moves through the links, Enter or Space follow the
/// focused one, and the focused link shows the <c>highlight</c> background (a span cannot draw the 1px focus outline). Screen
/// readers read the paragraph, then list its links. Taps still go to the spans themselves. Use it for any paragraph that holds
/// links; keep the number of inline links low.
/// </summary>
[ContentProperty(nameof(FormattedText))]
public class CdsParagraph : CdsControl
{
    /// <summary>Bindable property for <see cref="FormattedText"/>.</summary>
    public static readonly BindableProperty FormattedTextProperty = BindableProperty.Create(
        nameof(FormattedText), typeof(FormattedString), typeof(CdsParagraph), null,
        propertyChanged: static (b, o, n) => ((CdsParagraph)b).OnFormattedTextChanged((FormattedString?)o, (FormattedString?)n));

    /// <summary>Bindable property for <see cref="TypeStyle"/>.</summary>
    public static readonly BindableProperty TypeStyleProperty = BindableProperty.Create(
        nameof(TypeStyle), typeof(CdsTypeStyle), typeof(CdsParagraph), CdsTypeStyle.Body01,
        propertyChanged: static (b, _, n) => ((CdsParagraph)b)._label.TypeStyle = (CdsTypeStyle)n);

    /// <summary>Bindable property for <see cref="Tone"/>.</summary>
    public static readonly BindableProperty ToneProperty = BindableProperty.Create(
        nameof(Tone), typeof(CdsTextTone), typeof(CdsParagraph), CdsTextTone.Primary,
        propertyChanged: static (b, _, n) => ((CdsParagraph)b)._label.Tone = (CdsTextTone)n);

    readonly Grid _root = new();
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap };
    readonly HorizontalStackLayout _links = new() { Spacing = 0, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
    readonly List<(CdsLinkSpan Span, CdsHitTarget Hit)> _targets = [];

    /// <summary>Creates an empty paragraph in <c>body-01</c>.</summary>
    public CdsParagraph()
    {
        _label.TypeStyle = CdsTypeStyle.Body01;
        _root.Add(_label);
        _root.Add(_links);
        SetChrome(_root);
    }

    /// <summary>The text: plain <see cref="Span"/>s and <see cref="CdsLinkSpan"/>s.</summary>
    public FormattedString? FormattedText
    {
        get => (FormattedString?)GetValue(FormattedTextProperty);
        set => SetValue(FormattedTextProperty, value);
    }

    /// <summary>The type style. Default <c>body-01</c> (inline links take the paragraph's size).</summary>
    public CdsTypeStyle TypeStyle
    {
        get => (CdsTypeStyle)GetValue(TypeStyleProperty);
        set => SetValue(TypeStyleProperty, value);
    }

    /// <summary>The color of the plain text. Default <see cref="CdsTextTone.Primary"/>.</summary>
    public CdsTextTone Tone
    {
        get => (CdsTextTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    /// <summary>The native link of each enabled link span, in text order (what Tab and screen readers reach).</summary>
    internal IReadOnlyList<CdsHitTarget> LinkTargets => [.. _targets.Select(t => t.Hit)];

    void OnFormattedTextChanged(FormattedString? oldText, FormattedString? newText)
    {
        if (oldText?.Spans is INotifyCollectionChanged oldSpans) oldSpans.CollectionChanged -= OnSpansChanged;
        _label.FormattedText = newText;
        if (newText?.Spans is INotifyCollectionChanged newSpans) newSpans.CollectionChanged += OnSpansChanged;
        RebuildLinks();
    }

    void OnSpansChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildLinks();

    void OnSpanPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Span.Text) or nameof(CdsLinkSpan.IsEnabled)) RebuildLinks();
    }

    void RebuildLinks()
    {
        foreach (var (span, hit) in _targets)
        {
            span.PropertyChanged -= OnSpanPropertyChanged;
            span.ShowsFocus = false;
            hit.Focused -= OnLinkFocused;
            hit.Unfocused -= OnLinkUnfocused;
            hit.Clicked -= OnLinkClicked;
        }
        _targets.Clear();
        _links.Clear();
        foreach (var span in FormattedText?.Spans.OfType<CdsLinkSpan>() ?? [])
        {
            span.PropertyChanged += OnSpanPropertyChanged;
            if (!span.IsEnabled || string.IsNullOrEmpty(span.Text)) continue;
            var hit = new CdsHitTarget { Role = CdsAccessibilityRole.Link };
            SemanticProperties.SetDescription(hit, span.Text.Trim());
            hit.Focused += OnLinkFocused;
            hit.Unfocused += OnLinkUnfocused;
            hit.Clicked += OnLinkClicked;
            // A 1 x 1 cell (the hit target fills its cell): out of the way of taps on the text, but a native focus target.
            var cell = new Grid { WidthRequest = 1, HeightRequest = 1 };
            cell.Add(hit);
            _links.Add(cell);
            _targets.Add((span, hit));
        }
    }

    CdsLinkSpan? SpanOf(object? hit) => _targets.FirstOrDefault(t => ReferenceEquals(t.Hit, hit)).Span;

    void OnLinkFocused(object? sender, FocusEventArgs e)
    {
        if (sender is CdsHitTarget hit && SpanOf(hit) is { } span) span.ShowsFocus = hit.IsFocusFromKeyboard;
        SetState(CdsInteractionState.Focused, true);
        CdsFocusTracker.SetFocused(this);
    }

    void OnLinkUnfocused(object? sender, FocusEventArgs e)
    {
        if (SpanOf(sender) is { } span) span.ShowsFocus = false;
        if (!_targets.Any(t => t.Hit.IsFocused))
        {
            SetState(CdsInteractionState.Focused, false);
            CdsFocusTracker.ClearFocused(this);
        }
    }

    void OnLinkClicked(object? sender, EventArgs e)
    {
        if (IsEnabled) SpanOf(sender)?.Activate();
    }
}
