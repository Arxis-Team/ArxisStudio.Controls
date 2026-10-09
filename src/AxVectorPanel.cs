using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Панель ячеек вектора: все в ряд, а тесно — поровну в несколько рядов.
/// </summary>
/// <remarks>
/// Ячейки стоят равными колонками. Число колонок — наибольший делитель числа ячеек, при котором каждая не уже
/// <see cref="CellMinWidth"/>: четыре стороны отступа становятся двумя рядами по две, а не рядом из трёх и
/// сиротой. Так Unity переносит поля вектора в узком инспекторе, а Figma — отступы.
/// </remarks>
public class AxVectorPanel : Panel
{
    /// <summary>Ячейка не уже этого.</summary>
    public static readonly StyledProperty<double> CellMinWidthProperty =
        AvaloniaProperty.Register<AxVectorPanel, double>(nameof(CellMinWidth));

    /// <summary>Зазор между ячейками и между рядами.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<AxVectorPanel, double>(nameof(Spacing));

    static AxVectorPanel()
    {
        AffectsMeasure<AxVectorPanel>(CellMinWidthProperty, SpacingProperty);
    }

    /// <inheritdoc cref="CellMinWidthProperty"/>
    public double CellMinWidth
    {
        get => GetValue(CellMinWidthProperty);
        set => SetValue(CellMinWidthProperty, value);
    }

    /// <inheritdoc cref="SpacingProperty"/>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>Сколько ячеек в ряду: так решил последний замер.</summary>
    public int Columns { get; private set; } = 1;

    /// <summary>Сколько ячеек в ряду уместится при такой ширине.</summary>
    /// <param name="width">Ширина панели.</param>
    /// <param name="count">Число ячеек.</param>
    public int ColumnsFor(double width, int count)
    {
        if (count <= 1 || !double.IsFinite(width))
            return Math.Max(1, count);

        for (var columns = count; columns > 1; columns--)
        {
            if (count % columns == 0 && columns * CellMinWidth + (columns - 1) * Spacing <= width)
                return columns;
        }

        return 1;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var cells = Cells();

        if (cells.Count == 0)
            return default;

        Columns = ColumnsFor(availableSize.Width, cells.Count);

        var cell = double.IsFinite(availableSize.Width)
            ? Math.Max(0, (availableSize.Width - (Columns - 1) * Spacing) / Columns)
            : double.PositiveInfinity;
        double widest = 0, tallest = 0;

        foreach (var child in cells)
        {
            child.Measure(new Size(cell, availableSize.Height));
            widest = Math.Max(widest, child.DesiredSize.Width);
            tallest = Math.Max(tallest, child.DesiredSize.Height);
        }

        var rows = (cells.Count + Columns - 1) / Columns;
        var width = double.IsFinite(availableSize.Width)
            ? availableSize.Width
            : Columns * Math.Max(widest, CellMinWidth) + (Columns - 1) * Spacing;

        return new Size(width, rows * tallest + (rows - 1) * Spacing);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var cells = Cells();

        if (cells.Count == 0)
            return finalSize;

        // Колонки — те, что решил замер: под них он и просил высоту.
        var columns = Math.Max(1, Columns);
        var rows = (cells.Count + columns - 1) / columns;
        var cell = Math.Max(0, (finalSize.Width - (columns - 1) * Spacing) / columns);
        var height = Math.Max(0, (finalSize.Height - (rows - 1) * Spacing) / rows);

        for (var at = 0; at < cells.Count; at++)
        {
            var column = at % columns;
            var row = at / columns;

            cells[at].Arrange(new Rect(column * (cell + Spacing), row * (height + Spacing), cell, height));
        }

        return finalSize;
    }

    private List<Control> Cells()
    {
        var cells = new List<Control>(Children.Count);

        foreach (var child in Children)
        {
            if (child.IsVisible)
                cells.Add(child);
        }

        return cells;
    }
}
