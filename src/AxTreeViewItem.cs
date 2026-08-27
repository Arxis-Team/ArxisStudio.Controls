using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Строка дерева со стрелкой раскрытия, значком и отступом по уровню.
/// </summary>
/// <remarks>
/// Значок здесь — содержимое, а не путь: в дереве проекта рядом стоят папка,
/// нарисованная глифом, и значок типа файла — маленькая плашка с двумя буквами.
/// Свести их к одной геометрии нельзя, а разводить на два свойства значило бы
/// заставить каждого потребителя выбирать, каким из них он сегодня пользуется.
/// </remarks>
public class AxTreeViewItem : TreeViewItem
{
    /// <summary>Значок слева от подписи: глиф, плашка типа файла или своё.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<AxTreeViewItem, object?>(nameof(Icon));

    /// <inheritdoc cref="IconProperty"/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);
}
