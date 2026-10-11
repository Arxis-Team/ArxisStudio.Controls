using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ArxisStudio.Controls;

/// <summary>
/// Сегментный переключатель (Design / XAML / Split, Dark / Light): горизонтальный
/// ряд взаимоисключающих сегментов. Выбор — через API выделения списка.
/// </summary>
/// <remarks>
/// К выбранному сегменту переключатель не прокручивает: своей прокрутки у ряда нет, и просьба показать
/// сегмент ушла бы к прокрутке вокруг — список, где переключатель стоит у нижнего края, уезжал бы от
/// каждого выбора, сделанного кодом, а не человеком. Сегмент, на который перешла каретка, показывает
/// сам фокус.
/// </remarks>
public class AxSegmentedControl : ListBox
{
    static AxSegmentedControl()
    {
        SelectingItemsControl.AutoScrollToSelectedItemProperty.OverrideDefaultValue<AxSegmentedControl>(false);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxSegmentItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxSegmentItem>(item, out recycleKey);
}
