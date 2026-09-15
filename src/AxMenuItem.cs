using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>
/// Пункт меню студии: колонка иконок 16 фиксирована — пункт без иконки
/// получает пустой отступ, и текст не съезжает; шорткат прижат вправо.
/// </summary>
[PseudoClasses(":destructive")]
public class AxMenuItem : MenuItem
{
    /// <summary>Пункт необратимого действия: удалить, снять. Тема красит его цветом ошибки.</summary>
    public static readonly StyledProperty<bool> IsDestructiveProperty =
        AvaloniaProperty.Register<AxMenuItem, bool>(nameof(IsDestructive));

    static AxMenuItem() =>
        IsDestructiveProperty.Changed.AddClassHandler<AxMenuItem>((item, change) =>
            item.PseudoClasses.Set(":destructive", change.GetNewValue<bool>()));

    /// <inheritdoc cref="IsDestructiveProperty"/>
    public bool IsDestructive
    {
        get => GetValue(IsDestructiveProperty);
        set => SetValue(IsDestructiveProperty, value);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxMenuItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxMenuItem>(item, out recycleKey);
}
