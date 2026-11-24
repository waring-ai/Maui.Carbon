using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// One day cell of the calendar: 40 by 40, with hover, today (a dot), selection, range and the keyboard-cursor outline.
/// Not a tab stop: the date field keeps keyboard focus (virtual focus).
/// </summary>
internal sealed class CdsCalendarDay : CdsControl
{
    readonly Grid _root = new();
    readonly CdsText _label = new() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center };
    readonly BoxView _dot = new() { WidthRequest = 4, HeightRequest = 4, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End, Margin = new Thickness(0, 0, 0, 7), InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    CalendarDay _day;
    bool _cursor;

    public CdsCalendarDay()
    {
        _label.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _hit = CreateHitTarget();
        _hit.IsTabStop = false;
        _root.Add(_label);
        _root.Add(_dot);
        _root.Add(_ring);
        _root.Add(_hit);
        _root.HeightRequest = CdsCalendarTokens.DayHeight;
        SetChrome(_root);
    }

    /// <summary>Raised when the day is clicked or tapped.</summary>
    public event EventHandler<DateOnly>? Picked;

    public void Update(CalendarDay day, bool isCursor, System.Globalization.CultureInfo culture)
    {
        _day = day;
        _cursor = isCursor;
        _label.Text = day.Date.Day.ToString(culture);
        _hit.IsSelected = day.IsSelected;
        SemanticProperties.SetDescription(_hit, day.Date.ToString("D", culture));
        IsEnabled = !day.IsDisabled;
        RefreshVisuals();
    }

    protected override void OnActivated() => Picked?.Invoke(this, _day.Date);

    protected override void ApplyVisuals()
    {
        var v = CdsCalendarTokens.Resolve(_day.InMonth, _day.IsToday, _day.IsDisabled, _day.IsSelected, _day.InRange,
            Is(CdsInteractionState.Hover), _cursor);
        SetColor(_root, BackgroundColorProperty, v.Background);
        SetColor(_label, Label.TextColorProperty, v.Text);
        SetColor(_dot, BoxView.ColorProperty, v.Dot);
        _dot.IsVisible = v.Dot is not null;
        if (v.Ring is { } ring) _ring.SetColorKey(ring);
        _ring.IsActive = v.Ring is not null;
        var style = v.IsSemiBold ? CdsTypeStyle.HeadingCompact01 : CdsTypeStyle.BodyCompact01;
        if (_label.TypeStyle != style) _label.TypeStyle = style;
    }
}

/// <summary>A 40px month navigation button (previous or next month) of the calendar header.</summary>
internal sealed class CdsCalendarNavButton : CdsControl
{
    readonly Grid _root = new() { WidthRequest = CdsCalendarTokens.HeaderHeight, HeightRequest = CdsCalendarTokens.HeaderHeight };
    readonly CdsIcon _icon;
    readonly CdsHitTarget _hit;

    public CdsCalendarNavButton(CdsIconName icon, string description)
    {
        _icon = new CdsIcon(icon, 16);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _hit = CreateHitTarget();
        _hit.IsTabStop = false;
        SemanticProperties.SetDescription(_hit, description);
        _root.Add(_icon);
        _root.Add(_hit);
        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>Raised when the button is activated.</summary>
    public event EventHandler? Clicked;

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsCalendarTokens.ResolveNavigation(IsEnabled, Is(CdsInteractionState.Hover));
        SetColor(_root, BackgroundColorProperty, v.Background);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
    }
}

/// <summary>
/// The Carbon calendar: a 288 by 336 <c>layer-01</c> surface with a shadow, a month header (previous and next buttons, the
/// month and year), a weekday row and six weeks of 40px days. It only renders a <see cref="CalendarModel"/>; the date picker
/// owns the model and the keyboard.
/// </summary>
internal sealed class CdsCalendarView : CdsControl
{
    readonly Border _surface;
    readonly CdsCalendarNavButton _previous = new(CdsIconName.ChevronLeft, "Previous month");
    readonly CdsCalendarNavButton _next = new(CdsIconName.ChevronRight, "Next month");
    readonly CdsText _month = new(string.Empty, CdsTypeStyle.HeadingCompact01) { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
    readonly Grid _weekdays = new() { ColumnSpacing = 0 };
    readonly CdsText[] _weekdayLabels = new CdsText[7];
    readonly CdsCalendarDay[] _days = new CdsCalendarDay[CalendarModel.Weeks * 7];

    public CdsCalendarView()
    {
        var content = new Grid { RowSpacing = 0, ColumnSpacing = 0 };
        content.RowDefinitions.Add(new RowDefinition(CdsCalendarTokens.HeaderHeight));
        content.RowDefinitions.Add(new RowDefinition(CdsCalendarTokens.HeaderHeight));
        content.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        var header = new Grid { ColumnSpacing = 0 };
        header.ColumnDefinitions.Add(new ColumnDefinition(CdsCalendarTokens.HeaderHeight));
        header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        header.ColumnDefinitions.Add(new ColumnDefinition(CdsCalendarTokens.HeaderHeight));
        header.Add(_previous, 0);
        header.Add(_month, 1);
        header.Add(_next, 2);
        SemanticProperties.SetHeadingLevel(_month, SemanticHeadingLevel.Level2);
        _previous.Clicked += (_, _) => PreviousRequested?.Invoke(this, EventArgs.Empty);
        _next.Clicked += (_, _) => NextRequested?.Invoke(this, EventArgs.Empty);

        for (var i = 0; i < 7; i++)
        {
            _weekdays.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _weekdayLabels[i] = new CdsText { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center };
            AutomationProperties.SetIsInAccessibleTree(_weekdayLabels[i], false);
            _weekdays.Add(_weekdayLabels[i], i);
        }

        var grid = new Grid { RowSpacing = 0, ColumnSpacing = 0, VerticalOptions = LayoutOptions.Start };
        for (var i = 0; i < 7; i++) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (var i = 0; i < CalendarModel.Weeks; i++) grid.RowDefinitions.Add(new RowDefinition(CdsCalendarTokens.DayHeight));
        for (var i = 0; i < _days.Length; i++)
        {
            var day = new CdsCalendarDay();
            day.Picked += (_, date) => DayPicked?.Invoke(this, date);
            _days[i] = day;
            grid.Add(day, i % 7, i / 7);
        }

        content.Add(header, 0, 0);
        content.Add(_weekdays, 0, 1);
        content.Add(grid, 0, 2);

        _surface = new Border
        {
            StrokeShape = new Rectangle(),
            StrokeThickness = 0,
            Padding = new Thickness(2, 2, 2, 3),
            WidthRequest = CdsCalendarTokens.Width,
            HeightRequest = CdsCalendarTokens.Height,
            Content = content,
        };
        CdsTokens.SetColor(_surface, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(_surface, CdsColorKeys.Shadow, offsetY: 2, radius: 6);
        CdsSemantics.SetGroupName(_surface, "Choose date");
        SetColor(_month, Label.TextColorProperty, CdsColorKeys.TextPrimary);
        foreach (var label in _weekdayLabels) SetColor(label, Label.TextColorProperty, CdsColorKeys.TextPrimary);
        SetChrome(_surface);
    }

    /// <summary>Raised when a day is clicked or tapped.</summary>
    public event EventHandler<DateOnly>? DayPicked;

    /// <summary>Raised when the previous-month button is activated.</summary>
    public event EventHandler? PreviousRequested;

    /// <summary>Raised when the next-month button is activated.</summary>
    public event EventHandler? NextRequested;

    /// <summary>The visible month heading (for tests and previews).</summary>
    public string MonthText => _month.Text;

    /// <summary>Draws the model; <paramref name="showCursor"/> shows the keyboard-cursor outline.</summary>
    public void Render(CalendarModel model, bool showCursor)
    {
        _month.Text = model.MonthLabel;
        var labels = model.WeekdayLabels();
        for (var i = 0; i < 7; i++) _weekdayLabels[i].Text = labels[i];
        _previous.IsEnabled = model.CanGoPrevious;
        _next.IsEnabled = model.CanGoNext;
        var grid = model.BuildGrid();
        for (var i = 0; i < _days.Length; i++)
            _days[i].Update(grid[i], showCursor && grid[i].Date == model.Cursor, model.Culture);
    }
}
