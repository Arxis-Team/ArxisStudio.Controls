using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Выпадающий список студии.
/// </summary>
public class AxComboBox : ComboBox
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxComboBoxItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxComboBoxItem>(item, out recycleKey);
}
