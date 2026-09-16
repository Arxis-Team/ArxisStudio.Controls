using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>
/// Строка <see cref="AxListBox"/>.
/// </summary>
/// <remarks>
/// Выбранная строка говорит и о фокусе: <c>:selection-active</c>, пока клавиатура внутри её
/// списка, и ничего — когда она ушла. Признак приходит от области выделения, а не от селектора с
/// предком: предок в селекторе не различает вложенные списки и теряется, когда фокус уходит в
/// меню.
/// </remarks>
[PseudoClasses(":selection-active")]
public class AxListBoxItem : ListBoxItem
{
    static AxListBoxItem() =>
        AxSelectionScope.IsActiveProperty.Changed.AddClassHandler<AxListBoxItem>(
            (item, change) => item.PseudoClasses.Set(":selection-active", change.GetNewValue<bool>()));
}
