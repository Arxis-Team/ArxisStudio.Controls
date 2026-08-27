using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Заголовок группы настроек или секции: подпись слева и линия до конца строки.
/// </summary>
/// <remarks>
/// Сворачиваемый заголовок — тот же контрол со стрелкой слева и счётчиком
/// после подписи, а не отдельный: в инспекторе группы стоят подряд, и
/// разъезжаться в разметке им нельзя.
/// </remarks>
public class AxGroupHeader : ContentControl
{
    /// <summary>Показывать стрелку раскрытия.</summary>
    public static readonly StyledProperty<bool> IsCollapsibleProperty =
        AvaloniaProperty.Register<AxGroupHeader, bool>(nameof(IsCollapsible));

    /// <summary>Группа раскрыта.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AxGroupHeader, bool>(nameof(IsExpanded), true);

    /// <summary>Счётчик после подписи: сколько всего в группе.</summary>
    public static readonly StyledProperty<object?> CounterProperty =
        AvaloniaProperty.Register<AxGroupHeader, object?>(nameof(Counter));

    /// <inheritdoc cref="IsCollapsibleProperty"/>
    public bool IsCollapsible
    {
        get => GetValue(IsCollapsibleProperty);
        set => SetValue(IsCollapsibleProperty, value);
    }

    /// <inheritdoc cref="IsExpandedProperty"/>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <inheritdoc cref="CounterProperty"/>
    public object? Counter
    {
        get => GetValue(CounterProperty);
        set => SetValue(CounterProperty, value);
    }
}
