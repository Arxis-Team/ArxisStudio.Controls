using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Полоса вкладок документов: горизонтальный ряд <see cref="AxTabItem"/>.
/// Содержимое вкладки размещает хост — полоса отвечает только за выбор.
/// </summary>
public class AxTabStrip : ListBox
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTabItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTabItem>(item, out recycleKey);
}
