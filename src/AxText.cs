using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>Роль текста: какой это текст по смыслу, а с ней — кегль, начертание и шрифт.</summary>
public enum AxTextRole
{
    /// <summary>Основной текст.</summary>
    Body,

    /// <summary>Мелкий текст: пояснение под подписью, счётчик, путь.</summary>
    Small,

    /// <summary>Подпись: самый мелкий кегль темы.</summary>
    Caption,

    /// <summary>Заголовок экрана или карточки.</summary>
    Title,

    /// <summary>Заголовок секции: мелкий кегль с разрядкой.</summary>
    Section,

    /// <summary>Код, путь, значение: моноширинный шрифт темы.</summary>
    Code,
}

/// <summary>Тон текста: насколько он заметен и что сообщает цветом.</summary>
public enum AxTextTone
{
    /// <summary>Цвет, который текст получает от места: основной.</summary>
    Primary,

    /// <summary>Второстепенный.</summary>
    Secondary,

    /// <summary>Третий план: подсказка, счётчик, неважное.</summary>
    Tertiary,

    /// <summary>Недоступное.</summary>
    Disabled,

    /// <summary>Акцент: то, что выделяет текст из соседей.</summary>
    Accent,

    /// <summary>Ошибка.</summary>
    Error,

    /// <summary>Предупреждение.</summary>
    Warning,

    /// <summary>Успех.</summary>
    Success,
}

/// <summary>
/// Роль и тон текста — присоединёнными свойствами у <see cref="TextBlock"/>.
/// </summary>
/// <remarks>
/// Прежде роль и цвет текста задавались классами стиля — <c>dim</c>, <c>dimmer</c>,
/// <c>mono</c>, <c>small</c>, <c>bad</c>, — и опечатка в классе молча оставляла текст
/// обычным. Перечисление компилятор разметки проверяет, а тема видит значение
/// псевдоклассом <c>:role-small</c>, <c>:tone-secondary</c>. Роль и тон независимы:
/// мелкий текст бывает и второстепенным, и цветом ошибки.
/// <para>
/// Значение по умолчанию — основной текст основного тона — псевдокласса не несёт: его
/// вид — вид текста, которому ничего не сказали, и цвет он берёт от места, где стоит,
/// например от выбранной строки.
/// </para>
/// </remarks>
public static class AxText
{
    /// <summary>Роль текста.</summary>
    public static readonly AttachedProperty<AxTextRole> RoleProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, AxTextRole>("Role", typeof(AxText));

    /// <summary>Тон текста.</summary>
    public static readonly AttachedProperty<AxTextTone> ToneProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, AxTextTone>("Tone", typeof(AxText));

    static AxText()
    {
        RoleProperty.Changed.AddClassHandler<TextBlock>((text, change) =>
            AxLook.Mark(text.Classes, "role", change.GetOldValue<AxTextRole>(), change.GetNewValue<AxTextRole>(), AxTextRole.Body));
        ToneProperty.Changed.AddClassHandler<TextBlock>((text, change) =>
            AxLook.Mark(text.Classes, "tone", change.GetOldValue<AxTextTone>(), change.GetNewValue<AxTextTone>(), AxTextTone.Primary));
    }

    /// <summary>Роль текста.</summary>
    /// <param name="text">Текст.</param>
    public static AxTextRole GetRole(TextBlock text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.GetValue(RoleProperty);
    }

    /// <summary>Задаёт роль текста.</summary>
    /// <param name="text">Текст.</param>
    /// <param name="value">Роль.</param>
    public static void SetRole(TextBlock text, AxTextRole value)
    {
        ArgumentNullException.ThrowIfNull(text);

        text.SetValue(RoleProperty, value);
    }

    /// <summary>Тон текста.</summary>
    /// <param name="text">Текст.</param>
    public static AxTextTone GetTone(TextBlock text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.GetValue(ToneProperty);
    }

    /// <summary>Задаёт тон текста.</summary>
    /// <param name="text">Текст.</param>
    /// <param name="value">Тон.</param>
    public static void SetTone(TextBlock text, AxTextTone value)
    {
        ArgumentNullException.ThrowIfNull(text);

        text.SetValue(ToneProperty, value);
    }
}
