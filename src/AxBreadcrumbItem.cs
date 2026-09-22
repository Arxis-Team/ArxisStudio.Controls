using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Сегмент пути в <see cref="AxBreadcrumb"/>: подпись, значок и разделитель перед ним.
/// </summary>
/// <remarks>
/// Сегмент — кнопка: щелчок и Enter по нему ведут на этот уровень пути. Разделитель рисует тема
/// перед каждым сегментом, кроме первого (<c>:first</c>), и за подложкой: наведение подсвечивает
/// подпись, а не шеврон между подписями. Последний сегмент — текущее место (<c>:current</c>): он
/// назван, читается экранным диктором, но никуда не ведёт — туда человек уже пришёл.
/// <para>
/// Значок — путь в клетке 16 и его цвет, как у строки дерева и вкладки: библиотека контролов
/// набора значков не зовёт, глиф ставит тема.
/// </para>
/// <para>
/// Сегмент бывает целью перетаскивания (<c>:drop-target</c>): уровень пути — такое же место, как
/// строка каталога, и отпущенное на нём ляжет туда. Ставит отметку хозяин крошек — только он знает,
/// что куда кладётся, — а рисует её тема.
/// </para>
/// </remarks>
[PseudoClasses(":first", ":current", ":drop-target")]
public class AxBreadcrumbItem : Button
{
    /// <summary>Значок слева от подписи.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AxBreadcrumbItem, Geometry?>(nameof(Icon));

    /// <summary>Цвет значка; без него значок идёт цветом подписи.</summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AxBreadcrumbItem, IBrush?>(nameof(IconBrush));

    /// <summary>Этот сегмент — текущее место пути, последний в ряду.</summary>
    public static readonly DirectProperty<AxBreadcrumbItem, bool> IsCurrentProperty =
        AvaloniaProperty.RegisterDirect<AxBreadcrumbItem, bool>(nameof(IsCurrent), item => item.IsCurrent);

    /// <summary>Сегмент — цель перетаскивания.</summary>
    public static readonly StyledProperty<bool> IsDropTargetProperty =
        AvaloniaProperty.Register<AxBreadcrumbItem, bool>(nameof(IsDropTarget));

    private bool _current;

    static AxBreadcrumbItem() =>
        IsDropTargetProperty.Changed.AddClassHandler<AxBreadcrumbItem>(
            (item, change) => item.PseudoClasses.Set(":drop-target", change.GetNewValue<bool>()));

    /// <summary>
    /// Сегмент — цель перетаскивания: отпущенное над крошками ляжет на этот уровень пути.
    /// </summary>
    /// <remarks>
    /// Текущий сегмент тоже бывает целью: копия, отпущенная на нём, ложится туда, где человек стоит.
    /// </remarks>
    public bool IsDropTarget
    {
        get => GetValue(IsDropTargetProperty);
        set => SetValue(IsDropTargetProperty, value);
    }

    /// <inheritdoc cref="IconProperty"/>
    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc cref="IconBrushProperty"/>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    /// <inheritdoc cref="IsCurrentProperty"/>
    public bool IsCurrent
    {
        get => _current;
        private set => SetAndRaise(IsCurrentProperty, ref _current, value);
    }

    /// <summary>Ставит сегменту место в ряду: первый он или текущий.</summary>
    /// <param name="first">Первый сегмент: разделителя перед ним нет.</param>
    /// <param name="current">Последний сегмент — то место, где человек стоит.</param>
    internal void Mark(bool first, bool current)
    {
        PseudoClasses.Set(":first", first);
        PseudoClasses.Set(":current", current);
        IsCurrent = current;
    }
}
