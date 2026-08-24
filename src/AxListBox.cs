using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Список студии с выделением строк.
/// </summary>
public class AxListBox : ListBox
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxListBoxItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxListBoxItem>(item, out recycleKey);
}
