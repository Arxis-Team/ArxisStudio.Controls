using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Полоса вкладок документов: горизонтальный ряд <see cref="AxTabItem"/>.
/// Содержимое вкладки размещает хост — полоса отвечает только за выбор.
/// </summary>
public class AxTabStrip : ListBox
{
    /// <summary>Место, которое полосе дал родитель: шире не бывает ни одна её вкладка.</summary>
    internal double Room { get; private set; } = double.PositiveInfinity;

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTabItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTabItem>(item, out recycleKey);

    /// <inheritdoc/>
    /// <remarks>
    /// Вкладка не шире полосы. Полоса прокручивается, и вкладке шире неё —
    /// одинокой, в узкой панели, при крупном кегле — было нечем уместиться: имя
    /// уходило за край вместе с крестиком. Теперь вкладка мерится не шире места
    /// полосы, и тема сокращает имя многоточием. Ряду вкладок, не влезающих
    /// вместе, предел не мешает: каждая остаётся своей ширины, и полоса
    /// прокручивается, как прокручивалась.
    /// <para>
    /// Место берётся у родителя, а не у окна прокрутки. У полосы шириной по
    /// содержимому окно мерится самими вкладками, и предел по нему замкнулся бы
    /// петлёй: окно в ноль — вкладка в ноль.
    /// </para>
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        var room = Math.Max(0, availableSize.Width - Padding.Left - Padding.Right - BorderThickness.Left - BorderThickness.Right);

        if (room != Room)
        {
            Room = room;

            // Вкладкам полоса даёт бесконечную ширину и в прошлый раз давала
            // такую же: сами они о новом месте не узнают.
            foreach (var tab in GetRealizedContainers())
                tab.InvalidateMeasure();
        }

        return base.MeasureOverride(availableSize);
    }
}
