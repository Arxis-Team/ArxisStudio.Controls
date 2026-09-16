using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;

namespace ArxisStudio.Controls;

/// <summary>
/// Заголовок группы настроек или секции: подпись слева и линия до конца строки.
/// </summary>
/// <remarks>
/// Сворачиваемый заголовок — тот же контрол со стрелкой слева и счётчиком
/// после подписи, а не отдельный: в инспекторе группы стоят подряд, и
/// разъезжаться в разметке им нельзя.
/// <para>
/// Сворачивается он и мышью, и с клавиатуры: стрелка рисовалась, но не делала ничего — группу
/// приходилось складывать хозяину, снаружи. Цель — вся строка заголовка, а не одна стрелка в
/// двенадцать точек: попасть в неё мышью труднее, чем прочесть подпись, а промах здесь ничем не
/// грозит. Фокус контрол берёт, только когда сворачивается: у заголовка, который ничего не делает,
/// остановке Tab взяться неоткуда.
/// </para>
/// </remarks>
[PseudoClasses(":collapsible")]
public class AxGroupHeader : ContentControl
{
    static AxGroupHeader() =>
        IsCollapsibleProperty.Changed.AddClassHandler<AxGroupHeader>((header, change) => header.Mark(change.GetNewValue<bool>()));

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

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        if (!IsCollapsible || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        // Фокус переезжает на заголовок вместе с щелчком: следующая клавиша обязана достаться
        // тому, что человек только что тронул.
        Focus(NavigationMethod.Pointer);
        SetCurrentValue(IsExpandedProperty, !IsExpanded);

        e.Handled = true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Пробел и Enter делают одно: пробел — потому что это переключатель, Enter — потому что рука
    /// с него не уходит, когда идут по списку групп.
    /// </remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnKeyDown(e);

        if (e.Handled || !IsCollapsible || (e.Key != Key.Space && e.Key != Key.Enter))
            return;

        SetCurrentValue(IsExpandedProperty, !IsExpanded);

        e.Handled = true;
    }

    private void Mark(bool collapsible)
    {
        // Фокусируемость — следствие сворачиваемости, а не отдельная настройка: остановка Tab
        // нужна там, где есть что нажать.
        Focusable = collapsible;
        PseudoClasses.Set(":collapsible", collapsible);
    }
}
