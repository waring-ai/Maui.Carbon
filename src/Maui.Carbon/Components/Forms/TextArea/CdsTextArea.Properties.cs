namespace Maui.Carbon;

public partial class CdsTextArea
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTextArea), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTextArea)b).OnTextPropertyChanged((string?)n));

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsTextArea), null, propertyChanged: OnAreaChanged);

    /// <summary>Bindable property for <see cref="Rows"/>.</summary>
    public static readonly BindableProperty RowsProperty = BindableProperty.Create(
        nameof(Rows), typeof(int), typeof(CdsTextArea), 4, propertyChanged: OnAreaChanged,
        coerceValue: static (_, v) => Math.Max(1, (int)v));

    /// <summary>Bindable property for <see cref="EnableCounter"/>.</summary>
    public static readonly BindableProperty EnableCounterProperty = BindableProperty.Create(
        nameof(EnableCounter), typeof(bool), typeof(CdsTextArea), false, propertyChanged: OnAreaChanged);

    /// <summary>Bindable property for <see cref="MaxCount"/>.</summary>
    public static readonly BindableProperty MaxCountProperty = BindableProperty.Create(
        nameof(MaxCount), typeof(int), typeof(CdsTextArea), 0, propertyChanged: OnAreaChanged);

    /// <summary>Bindable property for <see cref="CounterMode"/>.</summary>
    public static readonly BindableProperty CounterModeProperty = BindableProperty.Create(
        nameof(CounterMode), typeof(CdsCounterMode), typeof(CdsTextArea), CdsCounterMode.Character, propertyChanged: OnAreaChanged);

    /// <summary>The value (two-way).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A hint about what a good answer contains; never the label.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Visible lines (each <c>body-01</c> line is 20 high). Default 4.</summary>
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>Shows "n/max" on the label row and stops input at <see cref="MaxCount"/>.</summary>
    public bool EnableCounter
    {
        get => (bool)GetValue(EnableCounterProperty);
        set => SetValue(EnableCounterProperty, value);
    }

    /// <summary>The limit when <see cref="EnableCounter"/> is on (pair it with a limit people can act on).</summary>
    public int MaxCount
    {
        get => (int)GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>Whether the counter counts characters (default) or words.</summary>
    public CdsCounterMode CounterMode
    {
        get => (CdsCounterMode)GetValue(CounterModeProperty);
        set => SetValue(CounterModeProperty, value);
    }

    static void OnAreaChanged(BindableObject b, object? o, object? n) => ((CdsTextArea)b).UpdateChrome();
}
