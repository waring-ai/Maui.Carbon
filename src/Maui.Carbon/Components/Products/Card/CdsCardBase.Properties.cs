using System.Windows.Input;

namespace Maui.Carbon;

public abstract partial class CdsCardBase
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="PrimaryButtonIcon"/>.</summary>
    public static readonly BindableProperty PrimaryButtonIconProperty = BindableProperty.Create(
        nameof(PrimaryButtonIcon), typeof(CdsIconName?), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="PrimaryButtonHref"/>.</summary>
    public static readonly BindableProperty PrimaryButtonHrefProperty = BindableProperty.Create(
        nameof(PrimaryButtonHref), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="PrimaryButtonCommand"/>.</summary>
    public static readonly BindableProperty PrimaryButtonCommandProperty = BindableProperty.Create(
        nameof(PrimaryButtonCommand), typeof(ICommand), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsPrimaryButtonEnabled"/>.</summary>
    public static readonly BindableProperty IsPrimaryButtonEnabledProperty = BindableProperty.Create(
        nameof(IsPrimaryButtonEnabled), typeof(bool), typeof(CdsCardBase), true, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonIcon"/>.</summary>
    public static readonly BindableProperty SecondaryButtonIconProperty = BindableProperty.Create(
        nameof(SecondaryButtonIcon), typeof(CdsIconName?), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonHref"/>.</summary>
    public static readonly BindableProperty SecondaryButtonHrefProperty = BindableProperty.Create(
        nameof(SecondaryButtonHref), typeof(string), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonCommand"/>.</summary>
    public static readonly BindableProperty SecondaryButtonCommandProperty = BindableProperty.Create(
        nameof(SecondaryButtonCommand), typeof(ICommand), typeof(CdsCardBase), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsSecondaryButtonEnabled"/>.</summary>
    public static readonly BindableProperty IsSecondaryButtonEnabledProperty = BindableProperty.Create(
        nameof(IsSecondaryButtonEnabled), typeof(bool), typeof(CdsCardBase), true, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsCardBase), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsCardBase), null, propertyChanged: OnCommandChanged);

    /// <summary>The title (Carbon's <c>title</c>): <c>heading-compact-02</c> on productive cards, <c>heading-03</c> on expressive ones. Sentence case.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>An optional label above the title (<c>label-01</c>), such as a category.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>An optional description or caption under the title (<c>label-01</c>).</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>An optional decorator at the top end corner, 16 from the edges. A <see cref="CdsAILabel"/> gives the card the AI treatment (gradient fill and border, soft shadow); on a clickable card the decorator does not take input.</summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>The primary button's label; the button shows when it is set. Verb first ("Read more").</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>An optional 16px icon on the primary button.</summary>
    public CdsIconName? PrimaryButtonIcon
    {
        get => (CdsIconName?)GetValue(PrimaryButtonIconProperty);
        set => SetValue(PrimaryButtonIconProperty, value);
    }

    /// <summary>An absolute URI the primary button opens: it becomes a link styled as a button.</summary>
    public string? PrimaryButtonHref
    {
        get => (string?)GetValue(PrimaryButtonHrefProperty);
        set => SetValue(PrimaryButtonHrefProperty, value);
    }

    /// <summary>Executed when the primary button is activated.</summary>
    public ICommand? PrimaryButtonCommand
    {
        get => (ICommand?)GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    /// <summary>Whether the primary button is enabled (Carbon's <c>primaryButtonDisabled</c>, inverted). Default <c>true</c>.</summary>
    public bool IsPrimaryButtonEnabled
    {
        get => (bool)GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    /// <summary>The secondary button's label; the button shows when it is set.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>An optional 16px icon on the secondary button.</summary>
    public CdsIconName? SecondaryButtonIcon
    {
        get => (CdsIconName?)GetValue(SecondaryButtonIconProperty);
        set => SetValue(SecondaryButtonIconProperty, value);
    }

    /// <summary>An absolute URI the secondary button opens.</summary>
    public string? SecondaryButtonHref
    {
        get => (string?)GetValue(SecondaryButtonHrefProperty);
        set => SetValue(SecondaryButtonHrefProperty, value);
    }

    /// <summary>Executed when the secondary button is activated.</summary>
    public ICommand? SecondaryButtonCommand
    {
        get => (ICommand?)GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    /// <summary>Whether the secondary button is enabled. Default <c>true</c>.</summary>
    public bool IsSecondaryButtonEnabled
    {
        get => (bool)GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    /// <summary>Executed when the clickable card is activated; setting it makes the card clickable. <c>CanExecute</c> drives <c>IsEnabled</c>.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => (object?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsCardBase)b).RequestRebuild();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var card = (CdsCardBase)b;
        card.SetCommand(card.Command, card.CommandParameter);
        card.RequestRebuild();
    }
}
