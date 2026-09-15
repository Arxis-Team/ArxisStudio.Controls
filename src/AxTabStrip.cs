using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>Чьи вкладки держит полоса.</summary>
public enum AxTabStripKind
{
    /// <summary>Вкладки документов: выбранная поднята фоном документа над подложкой.</summary>
    Document,

    /// <summary>Вкладки панели: без своего фона, выбор показывает одна полоса снизу.</summary>
    ToolWindow,
}

/// <summary>
/// Полоса вкладок: горизонтальный ряд <see cref="AxTabItem"/>.
/// Содержимое вкладки размещает хост — полоса отвечает только за выбор.
/// </summary>
/// <remarks>
/// Вид вкладки задаёт полоса, а не сама вкладка: тема вкладки не видит, в какой полосе
/// та стоит, и прежде вкладке панели приходилось повторять класс <c>compact</c> на
/// каждой. Полоса ставит своим вкладкам псевдокласс <c>:tool-window</c> — и тем, что
/// создала сама, и тем, что пришли готовыми.
/// </remarks>
[PseudoClasses(":tool-window")]
public class AxTabStrip : ListBox
{
    /// <summary>Чьи вкладки держит полоса.</summary>
    public static readonly StyledProperty<AxTabStripKind> KindProperty =
        AvaloniaProperty.Register<AxTabStrip, AxTabStripKind>(nameof(Kind));

    static AxTabStrip() =>
        KindProperty.Changed.AddClassHandler<AxTabStrip>((strip, change) => strip.Mark(change.GetNewValue<AxTabStripKind>()));

    /// <inheritdoc cref="KindProperty"/>
    public AxTabStripKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Место, которое полосе дал родитель: шире не бывает ни одна её вкладка.</summary>
    internal double Room { get; private set; } = double.PositiveInfinity;

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTabItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTabItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is AxTabItem tab)
            tab.MarkToolWindow(Kind == AxTabStripKind.ToolWindow);
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);

        if (container is AxTabItem tab)
            tab.MarkToolWindow(false);
    }

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

    private void Mark(AxTabStripKind kind)
    {
        PseudoClasses.Set(":tool-window", kind == AxTabStripKind.ToolWindow);

        foreach (var tab in GetRealizedContainers().OfType<AxTabItem>())
            tab.MarkToolWindow(kind == AxTabStripKind.ToolWindow);
    }
}
