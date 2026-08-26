using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Пункт меню студии: колонка иконок 16 фиксирована — пункт без иконки
/// получает пустой отступ, и текст не съезжает; шорткат прижат вправо.
/// </summary>
public class AxMenuItem : MenuItem
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxMenuItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxMenuItem>(item, out recycleKey);
}
