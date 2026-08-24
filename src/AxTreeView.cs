using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Дерево: иерархия документа, файлы проекта, структура решения. Строка —
/// <see cref="AxTreeViewItem"/> высотой в строку списка Int UI.
/// </summary>
public class AxTreeView : TreeView
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);
}
