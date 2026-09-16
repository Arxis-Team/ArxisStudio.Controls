using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Дерево: иерархия документа, файлы проекта, структура решения. Строка —
/// <see cref="AxTreeViewItem"/> высотой в строку списка.
/// </summary>
public class AxTreeView : TreeView
{
    /// <summary>Заводит дерево и объявляет его областью выделения.</summary>
    /// <remarks>
    /// Выбранная строка горит полным цветом, пока клавиатура в этом дереве. Вложенных деревьев у
    /// студии нет, но есть контекстное меню над выбранной строкой: со стилем по
    /// <c>:focus-within</c> выделение гасло ровно в тот миг, когда человек выбирает над ним
    /// действие.
    /// </remarks>
    public AxTreeView() => AxSelectionScope.Track(this);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);
}
