using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>
/// Поле поиска: <see cref="AxTextBox"/> со значком поиска слева и крестиком
/// очистки справа.
/// </summary>
/// <remarks>
/// Крестик — не украшение, а единственный способ отменить поиск, не выделяя
/// набранное. Появляется он от текста и вместе с текстом исчезает: пустое поле
/// с крестиком предлагает стереть то, чего нет. Показом занимается тема,
/// поведением — контрол.
/// </remarks>
public class AxSearchField : AxTextBox
{
    /// <summary>Имя кнопки очистки в теме.</summary>
    private const string ClearPart = "PART_Clear";

    /// <summary>
    /// Слушает нажатие по маршруту события, а не ищет кнопку в шаблоне.
    /// </summary>
    /// <remarks>
    /// Кнопка живёт в <c>InnerRightContent</c>, а это своя область имён:
    /// <c>OnApplyTemplate</c> её не видит. Нажатие же всплывает до поля в любом
    /// случае — по нему и работаем.
    /// </remarks>
    public AxSearchField() =>
        AddHandler(Button.ClickEvent, OnClick, RoutingStrategies.Bubble);

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button { Name: ClearPart })
        {
            return;
        }

        Clear();

        // Курсор остаётся в поле: человек стёр запрос, чтобы набрать другой,
        // а не чтобы уйти отсюда.
        Focus();
        e.Handled = true;
    }
}
