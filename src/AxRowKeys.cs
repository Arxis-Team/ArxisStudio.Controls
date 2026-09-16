using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Клавиатура строки: поиск набором и клавиша контекстного меню.
/// </summary>
/// <remarks>
/// Два жеста, которые в списке и в дереве обязаны быть и которых у Avalonia нет.
/// <list type="bullet">
/// <item>Набор букв ведёт выбор к строке, которая с них начинается: список в сотню строк не листают
/// стрелкой по одной, и мышь ради этого не берут.</item>
/// <item>Клавиша меню и Shift+F10 открывают то же меню, что правая кнопка. Avalonia поднимает
/// <c>ContextRequested</c> только от указателя, и всё, что живёт в контекстном меню, с клавиатуры
/// было недостижимо.</item>
/// </list>
/// <para>
/// Помощник общий: список и дерево спрашивают его об одном и том же, а строки свои перечисляют
/// сами — у дерева это раскрытая часть иерархии, у списка её нет вовсе.
/// </para>
/// </remarks>
internal sealed class AxRowKeys
{
    /// <summary>
    /// Сколько миллисекунд набор остаётся одним словом.
    /// </summary>
    /// <remarks>
    /// Пауза длиннее — новое слово. Такой же порядок держат проводник Windows и списки Rider:
    /// меньше — и «пр» распадается на «п» и «р» у того, кто печатает неспешно; больше — и
    /// вернувшийся к списку через секунду человек продолжает чужой набор.
    /// </remarks>
    private const long Pause = 900;

    private string _typed = string.Empty;
    private long _at;

    /// <summary>Просят ли этой клавишей контекстное меню.</summary>
    /// <param name="e">Нажатие.</param>
    public static bool AsksForMenu(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return e.Key == Key.Apps || (e.Key == Key.F10 && e.KeyModifiers == KeyModifiers.Shift);
    }

    /// <summary>
    /// Просит меню там, где стоит клавиатура.
    /// </summary>
    /// <param name="owner">Список или дерево.</param>
    /// <remarks>
    /// Событие поднимается на сфокусированной строке, а не на самом списке: меню бывает и у
    /// строки, а всплытие всё равно доведёт просьбу до владельца. Места у просьбы нет — меню
    /// встанет у контрола, а не под курсором, которого в этом жесте не было.
    /// </remarks>
    public static void AskForMenu(Control owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var focused = TopLevel.GetTopLevel(owner)?.FocusManager?.GetFocusedElement() as Control;
        var target = focused is not null && (ReferenceEquals(focused, owner) || owner.IsVisualAncestorOf(focused))
            ? focused
            : owner;

        target.RaiseEvent(new ContextRequestedEventArgs());
    }

    /// <summary>
    /// Подпись строки: то, что человек на ней читает.
    /// </summary>
    /// <param name="row">Контейнер строки.</param>
    /// <remarks>
    /// Первая надпись в строке и есть её имя: значок стоит перед ней, но букв не несёт, а
    /// счётчики и жесты идут после. У строки дерева так же: свои дочерние строки она держит ниже
    /// собственной шапки, и первой надписью остаётся её собственная.
    /// </remarks>
    public static string? Label(Control row)
    {
        ArgumentNullException.ThrowIfNull(row);

        foreach (var text in row.GetVisualDescendants().OfType<TextBlock>())
        {
            if (!string.IsNullOrWhiteSpace(text.Text))
                return text.Text;
        }

        return null;
    }

    /// <summary>
    /// Куда ведёт набор.
    /// </summary>
    /// <param name="typed">Что набрали сейчас.</param>
    /// <param name="label">Подпись строки по её номеру.</param>
    /// <param name="count">Сколько строк.</param>
    /// <param name="current">Где выбор стоит; −1 — нигде.</param>
    /// <returns>Номер строки или −1, если такой нет.</returns>
    /// <remarks>
    /// Одна и та же буква подряд ведёт к следующей строке на эту букву — так ищут, не помня
    /// названия целиком. Разные буквы уточняют набранное, и поиск начинается с нынешней строки:
    /// «п», «пр», «про» не должны уводить с «Программы», пока она подходит.
    /// </remarks>
    public int Match(string typed, Func<int, string?> label, int count, int current)
    {
        ArgumentNullException.ThrowIfNull(label);

        if (string.IsNullOrEmpty(typed) || count == 0)
            return -1;

        var now = Environment.TickCount64;

        _typed = now - _at > Pause ? typed : _typed + typed;
        _at = now;

        // Повтор одной буквы — это «следующая на ту же букву», а не «слово из одинаковых букв».
        var repeat = _typed.Length > 1 && _typed.All(letter => letter == _typed[0]);
        var needle = repeat ? _typed[..1] : _typed;
        var from = repeat || _typed.Length == 1 ? current + 1 : Math.Max(current, 0);

        for (var step = 0; step < count; step++)
        {
            var at = (((from + step) % count) + count) % count;

            if (label(at) is { } row && row.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase))
                return at;
        }

        return -1;
    }
}
