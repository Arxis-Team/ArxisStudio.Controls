using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Хлебные крошки: путь файла или дерево контролов дизайнера одной строкой.
/// Звено — <see cref="AxBreadcrumbItem"/>; шеврон-разделитель рисует само
/// звено и прячет его у первого. Усечение длинного пути — забота приложения:
/// оно знает, какие звенья можно спрятать за «…» с выпадающим списком.
/// </summary>
public class AxBreadcrumbBar : ItemsControl
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxBreadcrumbItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxBreadcrumbItem>(item, out recycleKey);
}
