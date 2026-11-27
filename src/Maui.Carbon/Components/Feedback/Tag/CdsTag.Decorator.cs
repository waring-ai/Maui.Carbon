namespace Maui.Carbon;

public partial class CdsTag
{
    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsTag), null,
        propertyChanged: static (b, o, n) => ((CdsTag)b).OnDecoratorChanged((View?)o, (View?)n));

    // Carbon: margin-inline-start 1px on the inline AI label inside a tag.
    const double DecoratorGap = 1;

    // Carbon: .cds--tag--filter .cds--tag__decorator > * { min-inline-size: 32.14px }.
    const double FilterDecoratorMinWidth = 32.14;

    View? _shownDecorator;
    double _decoratorWidth;

    /// <summary>
    /// An AI label after the text, Carbon's <c>decorator</c> slot (experimental): a <see cref="CdsAILabel"/> is shown inline at size
    /// sm, in the tag's own text color, and is the trigger of the explainability callout. A read-only tag shows only an
    /// <see cref="CdsAILabel"/> (Carbon drops any other node); <see cref="CdsDismissibleTag"/> shows any view as it is, between the text and
    /// the close button. Operational and selectable tags have no decorator: the whole tag is a button, which cannot contain
    /// another, so the property is ignored there. The tag keeps its fill: Carbon gives a tag no AI surface.
    /// </summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>Whether this kind of tag can show a <see cref="Decorator"/>: not the operational and selectable tags.</summary>
    protected virtual bool SupportsDecorator => true;

    /// <summary>Whether a decorator that is not a <see cref="CdsAILabel"/> is shown: only on the dismissible tag.</summary>
    private protected virtual bool ShowsAnyDecorator => false;

    /// <summary>Puts the decorator into the layout after the label.</summary>
    private protected virtual void AttachDecorator(View decorator) =>
        Row.Insert(Math.Min(Row.Children.IndexOf(_label) + 1, Row.Children.Count), decorator);

    /// <summary>Takes the decorator out of the layout.</summary>
    private protected virtual void DetachDecorator(View decorator) => Row.Remove(decorator);

    /// <summary>The width the decorator takes beside the label, for capping the label (0 before it is laid out).</summary>
    double DecoratorSpace => _shownDecorator is { } decorator
        ? Math.Max(0, Math.Max(decorator.Width, decorator.MinimumWidthRequest)) + decorator.Margin.HorizontalThickness
        : 0;

    void OnDecoratorChanged(View? oldDecorator, View? newDecorator)
    {
        Detach();
        if (newDecorator is null || !SupportsDecorator) return;
        var isLabel = newDecorator is CdsAILabel;
        if (!isLabel && !ShowsAnyDecorator) return;
        if (newDecorator is CdsAILabel label)
        {
            // Carbon: an AI label in a tag is always size sm and inline.
            label.Size = CdsAILabelSize.Sm;
            label.Kind = CdsAILabelKind.Inline;
            newDecorator.Margin = new Thickness(DecoratorGap, 0, 0, 0);
            if (this is CdsDismissibleTag) newDecorator.MinimumWidthRequest = FilterDecoratorMinWidth;
        }
        newDecorator.VerticalOptions = LayoutOptions.Center;
        newDecorator.HorizontalOptions = LayoutOptions.Start;
        _shownDecorator = newDecorator;
        newDecorator.SizeChanged += OnDecoratorSizeChanged;
        AttachDecorator(newDecorator);
        UpdateLayout();

        void Detach()
        {
            if (_shownDecorator is null) return;
            _shownDecorator.SizeChanged -= OnDecoratorSizeChanged;
            DetachDecorator(_shownDecorator);
            _shownDecorator = null;
            _decoratorWidth = 0;
            UpdateLayout();
        }
    }

    // The row measures its children unconstrained, so the label's cap has to leave room for the decorator once its width is known.
    void OnDecoratorSizeChanged(object? sender, EventArgs e)
    {
        var width = DecoratorSpace;
        if (Math.Abs(width - _decoratorWidth) < 0.5) return;
        _decoratorWidth = width;
        UpdateLayout();
    }

    /// <summary>Gives a decorator that is an AI label the tag's text color (Carbon: <c>currentColor</c>).</summary>
    private protected void ApplyDecoratorColor(string foregroundKey)
    {
        if (_shownDecorator is CdsAILabel label) label.ForegroundKey = foregroundKey;
    }
}
