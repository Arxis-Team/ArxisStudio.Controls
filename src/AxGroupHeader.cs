using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>Как выглядит заголовок группы.</summary>
public enum AxGroupHeaderAppearance
{
    /// <summary>Подпись и линия до конца строки — группа настроек.</summary>
    Rule,

    /// <summary>Полоса во всю ширину, как заголовок компонента у Unity: значок, имя, счётчик и действия справа.</summary>
    Band,
}

/// <summary>
/// Заголовок группы настроек или секции: подпись слева и линия до конца строки — или полоса компонента
/// (<see cref="Appearance"/>).
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
/// <para>
/// <b>Полоса</b> (<see cref="Appearance"/> — <see cref="AxGroupHeaderAppearance.Band"/>) — заголовок компонента у
/// Unity: полоса во всю ширину вместо линии, значок (<see cref="Icon"/>) перед именем и действия справа
/// (<see cref="Actions"/>) — кнопка ⋮ хозяина. Нажатие по действию до заголовка не доходит: группу оно не сворачивает.
/// </para>
/// </remarks>
[PseudoClasses(":collapsible", ":band")]
public class AxGroupHeader : ContentControl
{
    static AxGroupHeader()
    {
        IsCollapsibleProperty.Changed.AddClassHandler<AxGroupHeader>((header, change) => header.Mark(change.GetNewValue<bool>()));
        AppearanceProperty.Changed.AddClassHandler<AxGroupHeader>((header, change) =>
            header.PseudoClasses.Set(":band", change.GetNewValue<AxGroupHeaderAppearance>() == AxGroupHeaderAppearance.Band));
    }

    /// <summary>Показывать стрелку раскрытия.</summary>
    public static readonly StyledProperty<bool> IsCollapsibleProperty =
        AvaloniaProperty.Register<AxGroupHeader, bool>(nameof(IsCollapsible));

    /// <summary>Группа раскрыта.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AxGroupHeader, bool>(nameof(IsExpanded), true);

    /// <summary>Счётчик после подписи: сколько всего в группе.</summary>
    public static readonly StyledProperty<object?> CounterProperty =
        AvaloniaProperty.Register<AxGroupHeader, object?>(nameof(Counter));

    /// <summary>Вид: линия или полоса компонента.</summary>
    public static readonly StyledProperty<AxGroupHeaderAppearance> AppearanceProperty =
        AvaloniaProperty.Register<AxGroupHeader, AxGroupHeaderAppearance>(nameof(Appearance));

    /// <summary>Значок перед подписью — контур в клетке 16; пусто — значка нет.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AxGroupHeader, Geometry?>(nameof(Icon));

    /// <summary>Действия справа — кнопка ⋮ или несколько; пусто — их нет.</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<AxGroupHeader, object?>(nameof(Actions));

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

    /// <inheritdoc cref="AppearanceProperty"/>
    public AxGroupHeaderAppearance Appearance
    {
        get => GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <inheritdoc cref="IconProperty"/>
    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc cref="ActionsProperty"/>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
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
