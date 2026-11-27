using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The "Skip to main content" link: the first focusable element of the header. It is invisible (1 x 1) until it gets
/// keyboard focus, then shows as a 48 high bar over the header (<c>background</c>, <c>text-secondary</c>, 4px <c>focus</c>
/// border). Activating it moves focus to <see cref="Target"/>, by default the first focusable element of the enclosing
/// <see cref="CdsUIShell"/>'s <see cref="CdsContainer.Body"/> (Carbon's <c>#main-content</c>). It needs a hardware
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

    /// <summary>
    /// The element that receives focus (the <c>href="#main-content"</c> target). Default <c>null</c>: the first focusable element
    /// of the enclosing shell's body. A layout or control given here passes focus to its first focusable element.
    /// </summary>
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

    /// <summary>
    /// The element activation focuses: <see cref="Target"/> when it can take focus itself, else the first focusable element inside
    /// it; without a target, the first focusable element of the enclosing <see cref="CdsUIShell"/>'s body. <c>null</c> when
    /// there is none.
    /// </summary>
    internal VisualElement? ResolveTarget()
    {
        if (Target is { } target) return IsFocusable(target) ? target : FirstFocusable(target);
        return CdsShellContext.FindShell(this)?.Body is { } body ? FirstFocusable(body) : null;
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        ResolveTarget()?.Focus();
    }

    /// <summary>
    /// The first element under <paramref name="root"/> (itself included) that keyboard focus can land on, in tree order: a
    /// visible, enabled hit target that is a tab stop, or a native input or control. Hidden and disabled branches are skipped.
    /// </summary>
    internal static VisualElement? FirstFocusable(Element root)
    {
        if (CdsDeck.IsHidden(root) || root is VisualElement { IsEnabled: false }) return null;
        if (root is VisualElement element && IsFocusable(element)) return element;
        if (root is not IVisualTreeElement tree) return null;
        foreach (var child in tree.GetVisualChildren())
            if (child is Element e && FirstFocusable(e) is { } found) return found;
        return null;
    }

    static bool IsFocusable(VisualElement element) =>
        element is { IsVisible: true, IsEnabled: true } && element switch
        {
            CdsHitTarget hit => hit.IsTabStop,
            InputView or Picker or DatePicker or TimePicker or Button or CheckBox or Switch or Slider or Stepper or RadioButton => true,
            _ => false,
        };

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var shown = Is(CdsInteractionState.FocusVisible);
        _bar.IsVisible = shown;
        var v = CdsUIShellTokens.SkipToContent();
        SetBrush(_bar, BackgroundProperty, v.Background);
        SetBrush(_bar, Border.StrokeProperty, shown ? CdsColorKeys.Focus : null);
    }

    void Apply(string? text)
    {
        _label.Text = text;
        SemanticProperties.SetDescription(_hit, text);
    }
}
