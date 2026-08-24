using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Сегментный переключатель (Design / XAML / Split, Dark / Light): горизонтальный
/// ряд взаимоисключающих сегментов. Выбор — через API выделения списка.
/// </summary>
public class AxSegmentedControl : ListBox
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxSegmentItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxSegmentItem>(item, out recycleKey);
}
