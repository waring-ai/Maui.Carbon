using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>One drawn step: connector line, 16px icon, label and secondary label, with a hit target when clickable.</summary>
internal sealed class CdsProgressStepView : CdsControl
{
    readonly Grid _root = new();
    readonly BoxView _line = new() { InputTransparent = true };
    readonly CdsIcon _icon = new() { InputTransparent = true };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, InputTransparent = true };
    readonly CdsText _secondary = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, InputTransparent = true };
    readonly Border _focus = new() { StrokeThickness = 1, BackgroundColor = Colors.Transparent, StrokeShape = new Rectangle(), InputTransparent = true, IsVisible = false };
    readonly VerticalStackLayout _text = new() { Spacing = 0 };
    readonly Grid _layout = new();
    readonly CdsHitTarget _hit;
    CdsProgressStepState _state;
    bool _reached;
    bool _clickable;

    public CdsProgressStepView()
    {
        _hit = CreateHitTarget();
        _hit.IsVisible = false;
        _text.Add(_label);
        _text.Add(_secondary);
        AutomationProperties.SetExcludedWithChildren(_layout, true);
        _root.Add(_layout);
        _root.Add(_focus);
        _root.Add(_hit);
        SetChrome(_root);
    }

    public event EventHandler? Clicked;

    public bool IsClickableStep => _clickable;

    public void Update(CdsProgressStep step, CdsProgressStepState state, bool reached, bool vertical, bool spaceEqually, bool clickable)
    {
        _state = state;
        _reached = reached;
        _clickable = clickable;
        var visuals = CdsProgressStepTokens.Resolve(state);

        _icon.Icon = visuals.Icon;
        _icon.Description = step.Description;
        _label.Text = step.Label;
        _label.LineBreakMode = vertical ? LineBreakMode.WordWrap : LineBreakMode.TailTruncation;
        _label.MaxLines = vertical ? 0 : 1;
        _label.MaximumWidthRequest = ProgressIndicatorModel.LabelMaxWidth(vertical, spaceEqually);
        // Start, not Fill: a filling label with a maximum width is centered once the step is wider than the maximum.
        _label.HorizontalOptions = LayoutOptions.Start;
        _secondary.Text = step.SecondaryLabel;
        _secondary.IsVisible = !string.IsNullOrEmpty(step.SecondaryLabel);
        ToolTipProperties.SetText(_layout, step.Label ?? "");

        _layout.Children.Clear();
        _layout.RowDefinitions.Clear();
        _layout.ColumnDefinitions.Clear();
        _layout.ColumnSpacing = 0;
        _layout.RowSpacing = 0;
        // The label stack is built once: a new stack per update would add the labels while the old stack still holds them.
        var text = _text;
        _icon.VerticalOptions = LayoutOptions.Start;
        _icon.HorizontalOptions = LayoutOptions.Start;
        if (vertical)
        {
            // 1px connector along the start edge; icon 8px in; label up to 160 wide; at least 58 high.
            _layout.MinimumHeightRequest = 58;
            _layout.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1)));
            _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _line.WidthRequest = 1;
            _line.HeightRequest = -1;
            _line.HorizontalOptions = LayoutOptions.Fill;
            _line.VerticalOptions = LayoutOptions.Fill;
            _icon.Margin = new Thickness(CdsSpacing.Spacing03, 1, CdsSpacing.Spacing03, 0);
            text.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03);
            _layout.Add(_line, 0);
            _layout.Add(_icon, 1);
            _layout.Add(text, 2);
        }
        else
        {
            // 2px connector along the top edge; icon 10px below it; label uses the width the step has left of the icon.
            _layout.MinimumHeightRequest = -1;
            _layout.RowDefinitions.Add(new RowDefinition(new GridLength(2)));
            _layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _line.WidthRequest = -1;
            _line.HeightRequest = 2;
            _line.HorizontalOptions = LayoutOptions.Fill;
            _line.VerticalOptions = LayoutOptions.Fill;
            _icon.Margin = new Thickness(0, 10, CdsSpacing.Spacing03, 0);
            text.Margin = new Thickness(0, CdsSpacing.Spacing03, CdsSpacing.Spacing04, 0);
            _layout.Add(_line, 0, 0);
            Grid.SetColumnSpan(_line, 2);
            _layout.Add(_icon, 0, 1);
            _layout.Add(text, 1, 1);
        }

        _hit.IsVisible = clickable;
        SemanticProperties.SetDescription(_hit, $"{step.Label}, {CdsProgressStepTokens.StatusWord(state)}");
        // Non-clickable steps are read as a whole; clickable ones are named by the hit target.
        SemanticProperties.SetDescription(_root, clickable ? null : $"{step.Label}, {CdsProgressStepTokens.StatusWord(state)}{(string.IsNullOrEmpty(step.SecondaryLabel) ? "" : ", " + step.SecondaryLabel)}");
        AutomationProperties.SetIsInAccessibleTree(_root, true);
        RefreshVisuals();
    }

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsProgressStepTokens.Resolve(_state);
        var hover = _clickable && (Is(CdsInteractionState.Hover) || Is(CdsInteractionState.Pressed));
        SetColor(_line, BoxView.ColorProperty, CdsProgressStepTokens.LineColor(_reached));
        SetColor(_icon, CdsIcon.ColorProperty, v.IconColor);
        SetColor(_label, Label.TextColorProperty, hover ? CdsColorKeys.LinkPrimaryHover : v.LabelColor);
        _label.TextDecorations = hover ? TextDecorations.Underline : TextDecorations.None;
        SetBrush(_focus, Border.StrokeProperty, CdsColorKeys.Focus);
        _focus.IsVisible = _clickable && Is(CdsInteractionState.FocusVisible);
    }
}
