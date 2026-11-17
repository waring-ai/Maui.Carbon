namespace Maui.Carbon;

/// <summary>
/// The toolbar docked to the bottom edge of an <see cref="CdsAILabelContent"/> callout: ghost icon buttons and at most one primary
/// button, right-aligned with no gaps (the primary button last). Put it in <see cref="CdsAILabelContent.Actions"/>.
/// </summary>
public class CdsAILabelActions : HorizontalStackLayout
{
    /// <summary>Creates an empty toolbar.</summary>
    public CdsAILabelActions()
    {
        Spacing = 0;
        HorizontalOptions = LayoutOptions.End;
    }
}

/// <summary>
/// The explainability callout of an <see cref="CdsAILabel"/>: structure it as an overview, supporting details, sources and then
/// actions. The <see cref="CdsContainer.Body"/> sits in 24 of padding with 80 at the bottom (320 wide at most) so the
/// <see cref="Actions"/> toolbar can dock to the bottom edge. The surface (gradient, border, caret) is drawn by the label.
/// </summary>
public class CdsAILabelContent : CdsContainer
{
    /// <summary>Bindable property for <see cref="Actions"/>.</summary>
    public static readonly BindableProperty ActionsProperty = BindableProperty.Create(
        nameof(Actions), typeof(View), typeof(CdsAILabelContent), null,
        propertyChanged: static (b, o, n) =>
        {
            var content = (CdsAILabelContent)b;
            if (o is View old) content._root.Remove(old);
            if (n is View view)
            {
                view.VerticalOptions = LayoutOptions.End;
                view.HorizontalOptions = LayoutOptions.End;
                content._root.Add(view);
            }
        });

    /// <summary>Width limits of the callout: 280 minimum, 320 maximum content width.</summary>
    internal const double MinimumWidth = 280;

    readonly Grid _root = new() { MinimumWidthRequest = MinimumWidth };
    readonly ContentView _bodyHost = new() { Padding = new Thickness(CdsSpacing.Spacing06, CdsSpacing.Spacing06, CdsSpacing.Spacing06, 80), MaximumWidthRequest = 320 + 2 * CdsSpacing.Spacing06 };

    /// <summary>Creates empty callout content.</summary>
    public CdsAILabelContent()
    {
        HorizontalOptions = LayoutOptions.Start;
        _root.Add(_bodyHost);
        SetChrome(_root);
    }

    /// <summary>The bottom toolbar (usually a <see cref="CdsAILabelActions"/>).</summary>
    public View? Actions
    {
        get => (View?)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _bodyHost.Content = newBody;
}
