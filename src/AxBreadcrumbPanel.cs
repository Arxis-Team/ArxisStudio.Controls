using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Ряд сегментов пути: последний виден всегда, ведущие уходят, когда места не хватает.
/// </summary>
/// <remarks>
/// Раскладывается с конца: текущее место дороже корня, и узкая колонка должна показать, где человек
/// стоит, а не откуда путь начался. Сегмент, который не поместился, встаёт за левый край ряда, а не
/// получает <c>IsVisible = false</c>: невидимый контрол Avalonia не меряет, и ширину спрятанного
/// сегмента нечем было бы сравнить с местом — он показался бы снова на следующем же проходе, и ряд
/// качался бы.
/// <para>
/// Когда спрятан хоть один сегмент, слева оставляется <see cref="Reserve"/> — место под кнопку
/// переполнения. Кнопку кладёт шаблон поверх ряда, а не рядом с ним: так её появление не меняет
/// места самого ряда, и решение «помещается или нет» от кнопки не зависит.
/// </para>
/// </remarks>
internal sealed class AxBreadcrumbPanel : Panel
{
    private double _reserve;

    /// <summary>Ширина кнопки переполнения: её меряет шаблон, а место под неё держит ряд.</summary>
    /// <remarks>
    /// Ширина приходит, когда кнопка впервые встала на место, — после того как ряд что-то спрятал,
    /// — а размер самого ряда от неё не меняется, и Avalonia его раскладку пропустила бы: сегменты
    /// остались бы под кнопкой. Поэтому новая ширина раскладку сбрасывает сама.
    /// </remarks>
    internal double Reserve
    {
        get => _reserve;
        set
        {
            if (value == _reserve)
                return;

            _reserve = value;
            InvalidateArrange();
        }
    }

    /// <summary>Номер первого видимого сегмента; всё, что до него, лежит в меню переполнения.</summary>
    internal int FirstShown { get; private set; }

    /// <summary>
    /// Зовётся после каждой раскладки ряда.
    /// </summary>
    /// <remarks>
    /// Ряд раскладывается и сам по себе — когда пришла ширина кнопки переполнения, — и владелец
    /// тогда не раскладывается вовсе. Сверить пометки сегментов и меню с новой раскладкой может
    /// только тот, кто её сделал.
    /// </remarks>
    internal Action? Arranged { get; set; }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0, height = 0;

        foreach (var child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }

        return new Size(Math.Min(width, availableSize.Width), height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var total = 0.0;

        foreach (var child in Children)
            total += child.DesiredSize.Width;

        var fits = total <= finalSize.Width + 0.5;
        var start = fits ? 0 : Reserve;
        var room = finalSize.Width - start;
        var first = Children.Count;
        var used = 0.0;

        // С конца: последний сегмент стоит всегда, даже шире места, — тогда он сокращается.
        for (var at = Children.Count - 1; at >= 0; at--)
        {
            var width = Children[at].DesiredSize.Width;

            if (at < Children.Count - 1 && used + width > room + 0.5)
                break;

            used += width;
            first = at;
        }

        FirstShown = fits ? 0 : first;

        var x = start;

        for (var at = 0; at < Children.Count; at++)
        {
            var child = Children[at];

            if (at < FirstShown)
            {
                // За левым краем ряда, в своём размере: ряд обрезается по границам и такой сегмент
                // прячет. Рамка нулевого размера не прятала: значок у сегмента фиксированного
                // размера и рисовался из неё поверх кнопки переполнения.
                var hidden = child.DesiredSize.Width;

                child.Arrange(new Rect(-hidden, 0, hidden, finalSize.Height));
                continue;
            }

            var width = child.DesiredSize.Width;

            if (at == Children.Count - 1)
                width = Math.Max(0, Math.Min(width, finalSize.Width - x));

            child.Arrange(new Rect(x, 0, width, finalSize.Height));
            x += width;
        }

        Arranged?.Invoke();

        return finalSize;
    }
}
