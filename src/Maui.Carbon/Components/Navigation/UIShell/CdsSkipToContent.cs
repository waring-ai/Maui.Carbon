using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The "Skip to main content" link: the first focusable element of the header. It is invisible (1 x 1) until it gets
/// keyboard focus, then shows as a 48 high bar over the header (<c>background</c>, <c>text-secondary</c>, 4px <c>focus</c>
/// border). Activating it moves focus to <see cref="Target"/> (the page body or its first control). It needs a hardware
/// keyboard: on touch-only devices it is never reached (best effort on desktop).
/// </summary>
public class CdsSkipToContent : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSkipToContent), "Skip to main content",
        propertyChanged: static (b, _, n) => ((CdsSkipToContent)b).Apply((string?)n));

    /// <summary>Bindable property for <see cref="Target"/>.</summary>
    public static readonly BindableProperty TargetProperty = BindableProperty.Create(
        nameof(Target), typeof(VisualElement), typeof(CdsSkipToContent), null);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsSkipToContent), null,
        propertyChanged: static (b, _, _) => ((CdsSkipToContent)b).SetCommand(((CdsSkipToContent)b).Command, null));

    readonly Grid _root = new() { MinimumWidthRequest = 1, MinimumHeightRequest = 1 };
    readonly Border _bar = new() { StrokeThickness = 4, StrokeShape = new Rectangle(), IsVisible = false, HeightRequest = UIShellModel.HeaderHeight, InputTransparent = true };
    readonly Label _label = new() { VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
    readonly CdsHitTarget _hit;

    /// <summary>Creates the link.</summary>
    public CdsSkipToContent()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        CdsTypography.Apply(_label, CdsTypeStyle.BodyCompact01);
        _label.SetDynamicResource(Label.TextColorProperty, CdsColorKeys.TextSecondary);
        _bar.Content = _label;
        AutomationProperties.SetExcludedWithChildren(_bar, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Link);
        _root.Add(_bar);
        _root.Add(_hit);
        SetChrome(_root);
        Apply(Text);
    }

    /// <summary>Raised on activation, before focus moves to <see cref="Target"/>.</summary>
    public event EventHandler? Clicked;

    /// <summary>The link text. Default "Skip to main content".</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The element that receives focus (the <c>href="#main-content"</c> target).</summary>
    public VisualElement? Target
    {
        get => (VisualElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>Executed on activation as well.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        Target?.Focus();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var shown = Is(CdsInteractionState.FocusVisible);
        _bar.IsVisible = shown;
        var v = CdsUIShellTokens.SkipToContent();
        SetColor(_bar, BackgroundColorProperty, v.Background);
        SetBrush(_bar, Border.StrokeProperty, shown ? CdsColorKeys.Focus : null);
    }

    void Apply(string? text)
    {
        _label.Text = text;
        SemanticProperties.SetDescription(_hit, text);
    }
}
