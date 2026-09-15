using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Строка дерева со стрелкой раскрытия, значком и отступом по уровню.
/// </summary>
/// <remarks>
/// Значок — путь в клетке 16 и его цвет, как у вкладки. Прежде значком было любое
/// содержимое, и рядом с глифом папки стояли плашки типа файла со своими размерами:
/// клетка набора ложится в пиксели, а плашка — как получится. Тип документа
/// различают цветом значка, <see cref="IconBrush"/>.
/// </remarks>
public class AxTreeViewItem : TreeViewItem
{
    /// <summary>Значок слева от подписи.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AxTreeViewItem, Geometry?>(nameof(Icon));

    /// <summary>Цвет значка; без него значок идёт цветом иконки темы.</summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AxTreeViewItem, IBrush?>(nameof(IconBrush));

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

    /// <summary>
    /// Двойной щелчок по строке раскрывает и сворачивает узел.
    /// </summary>
    /// <remarks>
    /// Попасть в стрелку шириной 12 куда труднее, чем в строку, и во всяком
    /// файловом дереве это привычный способ.
    ///
    /// Слушаем жест, а не считаем нажатия сами: у нажатия счёт щелчков ведёт
    /// платформа, и подряд идущие двойные щелчки продолжают его — третьим,
    /// четвёртым, — а жест каждый раз приходит ровно один.
    /// </remarks>
    public AxTreeViewItem() => AddHandler(DoubleTappedEvent, OnRowDoubleTapped);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (ItemCount == 0 || e.Source is not Visual source)
            return;

        // Складывается своя строка. Жест поднимается из глубины наружу, и без
        // этой проверки двойной щелчок по файлу свернул бы папку над ним, а
        // следом и всё дерево.
        if (source.FindAncestorOfType<AxTreeViewItem>(includeSelf: true) != this)
            return;

        // Щелчок по стрелке — дело самой стрелки: она уже переключилась дважды,
        // и третье переключение отсюда вернуло бы узел не туда.
        if (source.FindAncestorOfType<Button>(includeSelf: true) is not null)
            return;

        IsExpanded = !IsExpanded;
        e.Handled = true;
    }
}
