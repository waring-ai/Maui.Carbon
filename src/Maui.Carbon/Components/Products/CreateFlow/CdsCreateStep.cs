namespace Maui.Carbon;

/// <summary>
/// One step of a <see cref="CdsCreateTearsheet"/> or <see cref="CdsCreateFullPage"/> (Carbon for IBM Products'
/// <c>CreateTearsheetStep</c> and <c>CreateFullPageStep</c>): a heading, an optional subtitle and description, and the
/// step's fields, in a fieldset by default. Only the current step shows. Put the fields in a <see cref="CdsStack"/> with
/// 16 between them (<c>Gap="5"</c>). Use <see cref="NextAsync"/> to validate before moving on and
/// <see cref="IsSubmitDisabled"/> to hold the Next button until the step is complete.
/// </summary>
public partial class CdsCreateStep : CdsContainer
{
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Heading04, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
    readonly CdsText _subtitle = new() { TypeStyle = CdsTypeStyle.HeadingCompact01, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly CdsText _description = new() { TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing06) };
    readonly CdsFormGroup _fieldset = new();
    readonly ContentView _plain = new();

    /// <summary>Creates a step.</summary>
    public CdsCreateStep()
    {
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level3);
        SemanticProperties.SetHeadingLevel(_subtitle, SemanticHeadingLevel.Level4);
        _root.Add(_title);
        _root.Add(_subtitle);
        _root.Add(_description);
        _root.Add(_fieldset);
        _root.Add(_plain);
        SetChrome(_root, fillsWidth: true);
        Sync();
    }

    /// <summary>Raised when the step becomes the current one (Carbon: <c>onMount</c>).</summary>
    public event EventHandler? Shown;

    /// <summary>Raised when a property that the flow reads (inclusion, labels, enabled buttons) changes.</summary>
    internal event EventHandler? FlowChanged;

    internal void RaiseShown() => Shown?.Invoke(this, EventArgs.Empty);

    internal CreateStepInfo Info => new(IsIncluded, IsIntro);

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => Sync();

    void NotifyFlow() => FlowChanged?.Invoke(this, EventArgs.Empty);

    void Sync()
    {
        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _subtitle.Text = Subtitle;
        _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
        _description.Text = Description;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        if (HasFieldset)
        {
            _plain.Content = null;
            _fieldset.Legend = FieldsetLegend;
            _fieldset.Body = Body;
        }
        else
        {
            _fieldset.Body = null;
            _plain.Content = Body;
        }
        _fieldset.IsVisible = HasFieldset;
        _plain.IsVisible = !HasFieldset;
        NotifyFlow();
    }
}

/// <summary>
/// A horizontal rule between groups of fields inside a <see cref="CdsCreateStep"/> (Carbon for IBM Products'
/// <c>CreateTearsheetDivider</c>): 1px <c>layer-accent-01</c>, 24 above and 32 below.
/// </summary>
public class CdsCreateTearsheetDivider : BoxView
{
    /// <summary>Creates a divider.</summary>
    public CdsCreateTearsheetDivider()
    {
        HeightRequest = 1;
        Margin = new Thickness(0, CdsSpacing.Spacing06, 0, CdsSpacing.Spacing07);
        HorizontalOptions = LayoutOptions.Fill;
        CdsTokens.SetColor(this, ColorProperty, CdsColorKeys.LayerAccent01);
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }
}
