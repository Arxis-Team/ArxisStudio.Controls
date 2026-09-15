using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Перечисления вида — псевдоклассами, одним правилом на все контролы.
/// </summary>
/// <remarks>
/// Кнопка и переключатель наследуют разные базы Avalonia, а тема должна видеть их вид
/// одинаково. Имя псевдокласса — имя значения строчными с дефисами; значение по
/// умолчанию псевдокласса не несёт: его вид — вид контрола без уточнений.
/// </remarks>
internal static class AxLook
{
    public static void Mark(IPseudoClasses classes, AxButtonAppearance appearance)
    {
        classes.Set(":primary", appearance == AxButtonAppearance.Primary);
        classes.Set(":subtle", appearance == AxButtonAppearance.Subtle);
        classes.Set(":danger", appearance == AxButtonAppearance.Danger);
        classes.Set(":toolbar", appearance == AxButtonAppearance.Toolbar);
    }

    public static void Mark(IPseudoClasses classes, AxControlSize size) =>
        classes.Set(":compact", size == AxControlSize.Compact);

    /// <summary>Ставит псевдокласс оси по значению перечисления, снимая прежний.</summary>
    /// <param name="classes">Псевдоклассы элемента.</param>
    /// <param name="axis">Приставка оси: <c>role</c>, <c>tone</c>.</param>
    /// <param name="old">Прежнее значение.</param>
    /// <param name="now">Новое значение.</param>
    /// <param name="none">Значение, у которого псевдокласса нет.</param>
    public static void Mark<T>(IPseudoClasses classes, string axis, T old, T now, T none)
        where T : struct, Enum
    {
        if (!EqualityComparer<T>.Default.Equals(old, none))
            classes.Remove(Name(axis, old));

        if (!EqualityComparer<T>.Default.Equals(now, none))
            classes.Add(Name(axis, now));
    }

    private static string Name<T>(string axis, T value)
        where T : struct, Enum =>
        $":{axis}-{Kebab(value.ToString())}";

    private static string Kebab(string name) =>
        string.Concat(name.Select((letter, index) =>
            char.IsUpper(letter) && index > 0 ? "-" + char.ToLowerInvariant(letter) : char.ToLowerInvariant(letter).ToString()));
}
