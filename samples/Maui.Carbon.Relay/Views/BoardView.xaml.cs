using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The board. Bindings carry the state; this code-behind adapts the clicks of controls whose items are declarative
/// (filter tags, card menu items, bulk move menu items), which hand their item over as the binding context.
/// </summary>
public partial class BoardView : ContentView
{
    public BoardView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        CdsTokens.SetColor(SelectBar, VisualElement.BackgroundColorProperty, CdsColorKeys.Layer01);
        BoardScroller.SizeChanged += (_, _) => SizeColumns();
        ColumnsStack.ChildAdded += (_, _) => SizeColumns();
    }

    /// <summary>The narrowest a column gets before the board scrolls sideways instead.</summary>
    const double MinColumnWidth = 232;

    /// <summary>Columns share the available width; below <see cref="MinColumnWidth"/> each the board scrolls horizontally.</summary>
    void SizeColumns()
    {
        var count = ColumnsStack.Children.Count;
        if (count == 0 || BoardScroller.Width <= 0) return;
        var width = Math.Max(MinColumnWidth, Math.Floor((BoardScroller.Width - ColumnsStack.Spacing * (count - 1)) / count));
        foreach (var child in ColumnsStack.Children.OfType<VisualElement>())
            if (Math.Abs(child.WidthRequest - width) > 0.5) child.WidthRequest = width;
    }

    BoardViewModel? ViewModel => BindingContext as BoardViewModel;

    void OnFilterClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: BoardFilter filter }) ViewModel?.ToggleFilterCommand.Execute(filter);
    }

    void OnCardAction(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: BoardCardAction action }) ViewModel?.RunCardActionCommand.Execute(action);
    }

    void OnBulkMove(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: BoardBulkTarget target }) ViewModel?.MoveSelectedCommand.Execute(target.Status);
    }
}
