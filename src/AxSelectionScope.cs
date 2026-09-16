using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Область выделения: где сейчас клавиатура.
/// </summary>
/// <remarks>
/// Выделенная строка красится полным цветом, пока клавиатура внутри её списка, и приглушённым,
/// когда она ушла: отмеченное остаётся видно, но не спорит за внимание с тем полем, куда человек
/// перешёл. До этой вехи то же самое делали глобальные стили с предком —
/// <c>:is(ListBox):focus-within ax|AxListBoxItem:selected</c>, — и у них было три беды.
/// <list type="bullet">
/// <item>Вложенный список загорался от фокуса внешнего: предка в селекторе хватало любого.</item>
/// <item>Список внутри попапа поиска не загорался никогда: фокус стоит в поле над ним.</item>
/// <item>Открытое контекстное меню гасило выделение владельца: у окна попапа нет визуального
/// предка, и <c>:focus-within</c> у списка пропадал ровно в тот миг, когда человек выбирает
/// действие над выделенной строкой.</item>
/// </list>
/// <para>
/// Поэтому область объявляет себя сама — список, дерево, попап поиска, панель — и метит своё
/// поддерево наследуемым свойством. Ближайшая область побеждает: своё значение она ставит себе
/// локально, и оно сильнее унаследованного от панели сверху.
/// </para>
/// <para>
/// Фокус ищется по логической цепочке, а не по визуальной: у окна попапа логический родитель —
/// его владелец, и открытое меню остаётся «внутри» области, из которой его позвали.
/// </para>
/// </remarks>
internal static class AxSelectionScope
{
    /// <summary>Клавиатура внутри этой области.</summary>
    public static readonly AttachedProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsActive", typeof(AxSelectionScope), inherits: true);

    /// <summary>Область, отданная хозяину: значение ей приходит сверху.</summary>
    private static readonly AttachedProperty<bool> ReleasedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("Released", typeof(AxSelectionScope));

    /// <summary>Читает признак области у элемента.</summary>
    /// <param name="control">Элемент.</param>
    public static bool GetIsActive(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return control.GetValue(IsActiveProperty);
    }

    /// <summary>
    /// Объявляет контрол областью выделения.
    /// </summary>
    /// <param name="scope">Список, дерево, попап поиска или панель.</param>
    /// <remarks>
    /// Подписки маршрутизируемые и с <c>handledEventsToo</c>: фокус внутри области почти всегда
    /// помечают обработанным сами контролы, и без этого область узнавала бы только о тех переходах,
    /// до которых никому не было дела.
    /// </remarks>
    public static void Track(Control scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.AddHandler(InputElement.GotFocusEvent, (_, _) => Update(scope), handledEventsToo: true);

        // Потеря фокуса разбирается сразу, а не отложенно: Avalonia к этому мигу уже назначила
        // новый фокус, и область видит, куда он ушёл. Отложить значило бы оставить выделение
        // гореть или гаснуть до следующего оборота очереди — на экране это мигание, а в тесте
        // состояние, которого он не дождётся.
        scope.AddHandler(InputElement.LostFocusEvent, (_, _) => Update(scope), handledEventsToo: true);

        scope.AttachedToVisualTree += (_, _) => Update(scope);
        scope.DetachedFromVisualTree += (_, _) => scope.SetValue(IsActiveProperty, false);
    }

    /// <summary>
    /// Отдаёт область хозяину: список внутри чужого шаблона перестаёт быть областью сам.
    /// </summary>
    /// <param name="scope">Список, отданный тому, кто его показывает.</param>
    /// <remarks>
    /// Так устроен попап поиска: фокус там стоит в строке запроса, а выбранное — в списке под ней,
    /// и стрелками двигают именно его. Считай список своей областью — он гас бы всегда, потому что
    /// клавиатура в нём не бывает; поэтому областью остаётся весь попап, а список берёт его
    /// значение по наследству.
    /// </remarks>
    public static void Release(Control scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.SetValue(ReleasedProperty, true);
        scope.ClearValue(IsActiveProperty);
    }

    private static void Update(Control scope)
    {
        if (scope.GetValue(ReleasedProperty))
            return;

        var focused = TopLevel.GetTopLevel(scope)?.FocusManager?.GetFocusedElement() as ILogical;

        for (var node = focused; node is not null; node = node.LogicalParent)
        {
            if (ReferenceEquals(node, scope))
            {
                scope.SetValue(IsActiveProperty, true);

                return;
            }
        }

        scope.SetValue(IsActiveProperty, false);
    }
}
