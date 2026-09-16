using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Список студии с выделением строк.
/// </summary>
public class AxListBox : ListBox
{
    /// <summary>Заводит список и объявляет его областью выделения.</summary>
    /// <remarks>
    /// Полным цветом строка горит, пока клавиатура в этом списке, — а не в любом предке, как было
    /// со стилем по <c>:focus-within</c>: вложенный список загорался от фокуса внешнего.
    /// </remarks>
    public AxListBox() => AxSelectionScope.Track(this);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxListBoxItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxListBoxItem>(item, out recycleKey);
}
