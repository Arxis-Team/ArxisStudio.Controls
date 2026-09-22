using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>
/// Строка <see cref="AxListBox"/>.
/// </summary>
/// <remarks>
/// <para>
/// Выбранная строка говорит и о фокусе: <c>:selection-active</c>, пока клавиатура внутри её
/// списка, и ничего — когда она ушла. Признак приходит от области выделения, а не от селектора с
/// предком: предок в селекторе не различает вложенные списки и теряется, когда фокус уходит в
/// меню.
/// </para>
/// <para>
/// <c>:drop-target</c> — строка, в которую ляжет перетаскиваемое: её ставит хозяин списка, потому
/// что только он знает, что куда кладётся. Тема рисует цель поверх строки и раскладку её не трогает.
/// </para>
/// </remarks>
[PseudoClasses(":selection-active", ":drop-target")]
public class AxListBoxItem : ListBoxItem
{
    /// <summary>Строка — цель перетаскивания.</summary>
    public static readonly StyledProperty<bool> IsDropTargetProperty =
        AvaloniaProperty.Register<AxListBoxItem, bool>(nameof(IsDropTarget));

    static AxListBoxItem()
    {
        AxSelectionScope.IsActiveProperty.Changed.AddClassHandler<AxListBoxItem>(
            (item, change) => item.PseudoClasses.Set(":selection-active", change.GetNewValue<bool>()));

        IsDropTargetProperty.Changed.AddClassHandler<AxListBoxItem>(
            (item, change) => item.PseudoClasses.Set(":drop-target", change.GetNewValue<bool>()));
    }

    /// <summary>
    /// Строка — цель перетаскивания: отпущенное над списком ляжет в неё.
    /// </summary>
    /// <remarks>
    /// Цель не всегда та строка, над которой курсор: файл, брошенный на файл, ложится в его
    /// каталог, и отмечать хозяин списка будет строку каталога.
    /// </remarks>
    public bool IsDropTarget
    {
        get => GetValue(IsDropTargetProperty);
        set => SetValue(IsDropTargetProperty, value);
    }
}
