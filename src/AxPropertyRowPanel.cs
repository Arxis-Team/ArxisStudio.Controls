using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace ArxisStudio.Controls;

/// <summary>Что за часть строки свойства стоит в её панели.</summary>
public enum AxPropertyRowPart
{
    /// <summary>Не часть строки: панель её не ставит.</summary>
    None,

    /// <summary>Полоса у края — место для отметки, если тема её ставит; нет её — подпись начинается от края.</summary>
    Marker,

    /// <summary>Подпись.</summary>
    Label,

    /// <summary>Поле.</summary>
    Value,

    /// <summary>Кнопки действий.</summary>
    Actions,

    /// <summary>Заметка под полем.</summary>
    Note,
}

/// <summary>
/// Панель строки свойства: ставит полосу, если она есть, подпись, поле, кнопки и заметку — в ряд или поле под подписью.
/// </summary>
/// <remarks>
/// <para>
/// Колонка подписи — наследуемое <see cref="AxPropertyRow.LabelWidthProperty"/>, а без него —
/// <c>max(round(w · 0,45) − LabelInset, LabelMinWidth)</c> от своей ширины, как у инспектора Unity. Отступ
/// вложенности (<see cref="Indent"/> ступеней по <see cref="IndentStep"/>) сдвигает подпись внутри колонки, а
/// поле остаётся на месте: поля всех уровней стоят одной колонкой.
/// </para>
/// <para>
/// Решение «в ряд или под подписью» принимается при замере, по ширине, которая достанется полю: меньше
/// <see cref="ValueMinWidth"/> или наименьшей ширины самого поля — поле уходит под подпись. Так строка не
/// переставляет свои части стилем посреди прохода раскладки. Числа — у темы: сама панель знает только долю
/// подписи — правило, а не размер.
/// </para>
/// </remarks>
public class AxPropertyRowPanel : Panel
{
    /// <summary>Доля ширины строки, отданная подписи, — как у инспектора Unity.</summary>
    public const double LabelShare = 0.45;

    /// <summary>Какая часть строки — этот ребёнок.</summary>
    public static readonly AttachedProperty<AxPropertyRowPart> PartProperty =
        AvaloniaProperty.RegisterAttached<AxPropertyRowPanel, Control, AxPropertyRowPart>("Part");

    /// <summary>Сколько вычесть из доли ширины — место, которое у Unity забирает отступ инспектора.</summary>
    public static readonly StyledProperty<double> LabelInsetProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, double>(nameof(LabelInset));

    /// <summary>Подпись не уже этого.</summary>
    public static readonly StyledProperty<double> LabelMinWidthProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, double>(nameof(LabelMinWidth));

    /// <summary>Поле не уже этого — иначе оно уходит под подпись.</summary>
    public static readonly StyledProperty<double> ValueMinWidthProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, double>(nameof(ValueMinWidth));

    /// <summary>Зазор между подписью, полем и кнопками.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, double>(nameof(Spacing));

    /// <summary>Ступень вложенности: на столько каждый уровень сдвигает подпись.</summary>
    public static readonly StyledProperty<double> IndentStepProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, double>(nameof(IndentStep));

    /// <summary>Уровень вложенности строки.</summary>
    public static readonly StyledProperty<int> IndentProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, int>(nameof(Indent));

    /// <summary>Как поле стоит относительно подписи.</summary>
    public static readonly StyledProperty<AxPropertyRowLayout> LayoutProperty =
        AvaloniaProperty.Register<AxPropertyRowPanel, AxPropertyRowLayout>(nameof(Layout));

    private bool _stacked;

    static AxPropertyRowPanel()
    {
        AffectsMeasure<AxPropertyRowPanel>(
            AxPropertyRow.LabelWidthProperty, LabelInsetProperty, LabelMinWidthProperty, ValueMinWidthProperty,
            SpacingProperty, IndentStepProperty, IndentProperty, LayoutProperty);
    }

    /// <inheritdoc cref="LabelInsetProperty"/>
    public double LabelInset
    {
        get => GetValue(LabelInsetProperty);
        set => SetValue(LabelInsetProperty, value);
    }

    /// <inheritdoc cref="LabelMinWidthProperty"/>
    public double LabelMinWidth
    {
        get => GetValue(LabelMinWidthProperty);
        set => SetValue(LabelMinWidthProperty, value);
    }

    /// <inheritdoc cref="ValueMinWidthProperty"/>
    public double ValueMinWidth
    {
        get => GetValue(ValueMinWidthProperty);
        set => SetValue(ValueMinWidthProperty, value);
    }

    /// <inheritdoc cref="SpacingProperty"/>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <inheritdoc cref="IndentStepProperty"/>
    public double IndentStep
    {
        get => GetValue(IndentStepProperty);
        set => SetValue(IndentStepProperty, value);
    }

    /// <inheritdoc cref="IndentProperty"/>
    public int Indent
    {
        get => GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    /// <inheritdoc cref="LayoutProperty"/>
    public AxPropertyRowLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>Поле стоит под подписью: так решил последний замер.</summary>
    public bool IsStacked => _stacked;

    /// <summary>Какая часть строки — этот ребёнок.</summary>
    /// <param name="element">Ребёнок панели.</param>
    public static AxPropertyRowPart GetPart(Control element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return element.GetValue(PartProperty);
    }

    /// <summary>Называет ребёнка частью строки.</summary>
    /// <param name="element">Ребёнок панели.</param>
    /// <param name="value">Часть.</param>
    public static void SetPart(Control element, AxPropertyRowPart value)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.SetValue(PartProperty, value);
    }

    /// <summary>Ширина колонки подписи при такой ширине строки.</summary>
    /// <param name="width">Ширина строки.</param>
    public double LabelColumn(double width)
    {
        var shared = GetValue(AxPropertyRow.LabelWidthProperty);

        if (double.IsFinite(shared) && shared >= 0)
            return shared;

        if (!double.IsFinite(width))
            return LabelMinWidth;

        return Math.Max(Math.Round(width * LabelShare) - LabelInset, LabelMinWidth);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width;
        var high = new Size(double.PositiveInfinity, availableSize.Height);
        var marker = Measure(AxPropertyRowPart.Marker, high);
        var actions = Measure(AxPropertyRowPart.Actions, high);
        var actionsSpan = actions.Width > 0 ? actions.Width + Spacing : 0;
        var left = marker.Width + Math.Max(0, Indent) * IndentStep;

        if (Layout == AxPropertyRowLayout.Full)
        {
            _stacked = false;

            var field = Measure(AxPropertyRowPart.Value, new Size(Rest(width, left + actionsSpan), availableSize.Height));
            var note = Measure(AxPropertyRowPart.Note, new Size(Rest(width, left), double.PositiveInfinity));

            return new Size(
                double.IsFinite(width) ? width : left + field.Width + actionsSpan,
                Math.Max(field.Height, actions.Height) + note.Height);
        }

        var column = LabelColumn(width);
        var valueWidth = width - marker.Width - column - Spacing - actionsSpan;

        // Поле, которому мало и своего наименьшего, — вектор в две ячейки, — уходит под подпись так же.
        _stacked = Layout == AxPropertyRowLayout.Below ||
                   valueWidth < Math.Max(ValueMinWidth, Floor(Child(AxPropertyRowPart.Value)));

        if (_stacked)
        {
            var label = Measure(AxPropertyRowPart.Label, new Size(Rest(width, left + actionsSpan), availableSize.Height));
            var field = Measure(AxPropertyRowPart.Value, new Size(Rest(width, left), availableSize.Height));
            var note = Measure(AxPropertyRowPart.Note, new Size(Rest(width, left), double.PositiveInfinity));

            return new Size(
                double.IsFinite(width) ? width : left + Math.Max(label.Width + actionsSpan, field.Width),
                Math.Max(label.Height, actions.Height) + field.Height + note.Height);
        }

        var inner = Measure(AxPropertyRowPart.Label, new Size(Math.Max(0, column - (left - marker.Width)), availableSize.Height));
        var value = Measure(AxPropertyRowPart.Value, new Size(Math.Max(0, valueWidth), availableSize.Height));
        var under = Measure(AxPropertyRowPart.Note, new Size(Math.Max(0, valueWidth + actionsSpan), double.PositiveInfinity));

        return new Size(
            double.IsFinite(width) ? width : marker.Width + column + Spacing + value.Width + actionsSpan,
            Math.Max(Math.Max(inner.Height, value.Height), actions.Height) + under.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var marker = Child(AxPropertyRowPart.Marker);
        var label = Child(AxPropertyRowPart.Label);
        var value = Child(AxPropertyRowPart.Value);
        var actions = Child(AxPropertyRowPart.Actions);
        var note = Child(AxPropertyRowPart.Note);

        var width = finalSize.Width;
        var markerWidth = marker?.DesiredSize.Width ?? 0;
        var actionsWidth = actions?.DesiredSize.Width ?? 0;
        var actionsSpan = actionsWidth > 0 ? actionsWidth + Spacing : 0;
        var left = markerWidth + Math.Max(0, Indent) * IndentStep;
        var noteHeight = note?.DesiredSize.Height ?? 0;

        marker?.Arrange(new Rect(0, 0, markerWidth, finalSize.Height));

        if (Layout == AxPropertyRowLayout.Full)
        {
            var line = Math.Max(0, finalSize.Height - noteHeight);

            value?.Arrange(new Rect(left, 0, Rest(width, left + actionsSpan), line));
            actions?.Arrange(new Rect(width - actionsWidth, 0, actionsWidth, line));
            note?.Arrange(new Rect(left, line, Rest(width, left), noteHeight));

            return finalSize;
        }

        if (_stacked)
        {
            var head = Math.Max(label?.DesiredSize.Height ?? 0, actions?.DesiredSize.Height ?? 0);
            var fieldHeight = value?.DesiredSize.Height ?? 0;

            label?.Arrange(new Rect(left, 0, Rest(width, left + actionsSpan), head));
            actions?.Arrange(new Rect(width - actionsWidth, 0, actionsWidth, head));
            value?.Arrange(new Rect(left, head, Rest(width, left), fieldHeight));
            note?.Arrange(new Rect(left, head + fieldHeight, Rest(width, left), noteHeight));

            return finalSize;
        }

        var column = LabelColumn(width);
        var top = Math.Max(0, finalSize.Height - noteHeight);
        var valueLeft = markerWidth + column + Spacing;

        label?.Arrange(new Rect(left, 0, Math.Max(0, markerWidth + column - left), top));
        value?.Arrange(new Rect(valueLeft, 0, Rest(width, valueLeft + actionsSpan), top));
        actions?.Arrange(new Rect(width - actionsWidth, 0, actionsWidth, top));
        note?.Arrange(new Rect(valueLeft, top, Rest(width, valueLeft), noteHeight));

        return finalSize;
    }

    /// <summary>
    /// Наименьшая ширина поля — его собственная, а не презентера, в котором оно стоит в шаблоне, и не обёрток, в
    /// которые оно завёрнуто по одному.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Презентер строит поле при первом замере, а решение о переносе принимается до него: поле строится здесь,
    /// иначе первый замер не знал бы его наименьшей ширины и поставил бы вектор рядом с подписью.
    /// </para>
    /// <para>
    /// Обёртка с одним ребёнком — рамка, барьер, за которым студия ставит построенное плагином, — своего
    /// наименьшего не знает: поле за ней стояло бы рядом с подписью там, где ему не встать. Поэтому наименьшее
    /// берётся по всей цепочке таких обёрток.
    /// </para>
    /// </remarks>
    private static double Floor(Control? value)
    {
        var floor = 0d;

        for (var at = value; at is not null; at = Wrapped(at))
        {
            at.ApplyTemplate();
            floor = Math.Max(floor, at.MinWidth);
        }

        return floor;
    }

    /// <summary>Единственный ребёнок обёртки; пусто — это уже само поле.</summary>
    private static Control? Wrapped(Control wrapper) => wrapper switch
    {
        ContentPresenter presenter => presenter.Child,
        Decorator decorator => decorator.Child,
        _ => null,
    };

    /// <summary>Сколько остаётся от ширины за вычетом занятого; бесконечность так и остаётся.</summary>
    private static double Rest(double width, double taken) =>
        double.IsFinite(width) ? Math.Max(0, width - taken) : double.PositiveInfinity;

    private Size Measure(AxPropertyRowPart part, Size available)
    {
        if (Child(part) is not { } child)
            return default;

        child.Measure(available);

        return child.DesiredSize;
    }

    private Control? Child(AxPropertyRowPart part)
    {
        foreach (var child in Children)
        {
            if (child.IsVisible && GetPart(child) == part)
                return child;
        }

        return null;
    }
}
