using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>Строка дерева со стрелкой раскрытия и отступом по уровню.</summary>
public class AxTreeViewItem : TreeViewItem
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);
}
