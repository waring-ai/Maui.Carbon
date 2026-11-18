using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// The container of a fluid form: fields stack edge to edge with no gaps (each keeps its own 1px
/// <c>border-strong-01</c> bottom rule), and every Carbon input inside renders its fluid layout (label inside a 64px
/// field, no helper text, messages inside the field). It publishes the <c>Cds.IsFluid</c> context key the way
/// <see cref="CdsLayer"/> publishes the layer level, so plain inputs and <c>CdsFluid*</c> inputs look alike inside it.
/// Put <see cref="CdsFluidFormRow"/>s around side-by-side fields and full-bleed buttons last. Never mix fluid and
/// default inputs in one form, and do not use it for long or complex forms (use <see cref="CdsForm"/>).
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsFluidForm : ContentView
{
    /// <summary>Bindable property for <see cref="SubmitCommand"/>.</summary>
    public static readonly BindableProperty SubmitCommandProperty = BindableProperty.Create(
        nameof(SubmitCommand), typeof(ICommand), typeof(CdsFluidForm), null);

    readonly CdsScopeView _scope = new();
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };

    /// <summary>Creates a fluid form.</summary>
    public CdsFluidForm()
    {
        _scope.Resources = new ResourceDictionary { [CdsContextKeys.IsFluid] = true };
        _scope.Content = _stack;
        Content = _scope;
    }

    /// <summary>Raised by <see cref="Submit"/> (call it from the primary button).</summary>
    public event EventHandler? Submitted;

    /// <summary>The rows and fields of the form (the content property).</summary>
    public IList<IView> Items => _stack.Children;

    /// <summary>Command run by <see cref="Submit"/> after <see cref="Submitted"/>.</summary>
    public ICommand? SubmitCommand
    {
        get => (ICommand?)GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    /// <summary>Submits the form: raises <see cref="Submitted"/> and runs <see cref="SubmitCommand"/>.</summary>
    public void Submit()
    {
        Submitted?.Invoke(this, EventArgs.Empty);
        if (SubmitCommand?.CanExecute(null) == true) SubmitCommand.Execute(null);
    }

    /// <summary>Whether <paramref name="element"/> sits inside a fluid form (Carbon's fluid context).</summary>
    public static bool GetIsFluid(Element element) => CdsContext.Find(element, CdsContextKeys.IsFluid) is true;
}

/// <summary>
/// A row of two or three fluid fields side by side: equal columns separated by a 1px <c>border-strong-01</c> line instead
/// of a gutter. Keep validation messages to one line so the fields in a row share one height.
/// </summary>
public class CdsFluidFormRow : Grid
{
    /// <summary>Creates a row.</summary>
    public CdsFluidFormRow()
    {
        ColumnSpacing = 1;
        RowSpacing = 0;
        CdsTokens.SetColor(this, BackgroundColorProperty, CdsColorKeys.BorderStrong01);
        ChildAdded += (_, _) => Distribute();
        ChildRemoved += (_, _) => Distribute();
    }

    void Distribute()
    {
        ColumnDefinitions.Clear();
        var index = 0;
        foreach (var child in Children)
        {
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            SetColumn((BindableObject)child, index++);
        }
    }
}
