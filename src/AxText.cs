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

    /// <summary>
    /// Высота строки долей кегля: во столько раз строка выше своего кегля.
    /// </summary>
    /// <remarks>
    /// Абзац читается по строкам, и расстояние между ними — часть набора, а не
    /// украшение. Долей, а не числом: <c>LineHeight</c> в пикселях сломал бы и рост
    /// текста с настройкой размера, и правило «высота контрола с подписью —
    /// наименьшая» — строка осталась бы прежней, а буквы выросли.
    /// <para>
    /// Не задано — <c>NaN</c>: высоту строки считает сам шрифт по своим метрикам.
    /// Тема ставит долю переносимому тексту; однострочной подписи интервал не нужен —
    /// у неё нет второй строки, зато есть соседи, с которыми она стоит в ряд.
    /// </para>
    /// </remarks>
    public static readonly AttachedProperty<double> LineHeightRatioProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, double>("LineHeightRatio", typeof(AxText), double.NaN);

    static AxText()
    {
        RoleProperty.Changed.AddClassHandler<TextBlock>((text, change) =>
            AxLook.Mark(text.Classes, "role", change.GetOldValue<AxTextRole>(), change.GetNewValue<AxTextRole>(), AxTextRole.Body));
        ToneProperty.Changed.AddClassHandler<TextBlock>((text, change) =>
            AxLook.Mark(text.Classes, "tone", change.GetOldValue<AxTextTone>(), change.GetNewValue<AxTextTone>(), AxTextTone.Primary));

        // Доля и кегль дают высоту строки вместе, и меняться может любой из двух:
        // кегль приходит из темы и растёт с настройкой размера текста. Кегль без
        // доли не трогает ничего: высоту строки ставят и сами контролы — блок кода
        // считает её своей долей, — и сбросить чужое значение было бы правкой молча.
        LineHeightRatioProperty.Changed.AddClassHandler<TextBlock>((text, _) => Measure(text, clear: true));
        TextBlock.FontSizeProperty.Changed.AddClassHandler<TextBlock>((text, _) => Measure(text, clear: false));
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

    /// <summary>Доля кегля, которой мерится высота строки.</summary>
    /// <param name="text">Текст.</param>
    public static double GetLineHeightRatio(TextBlock text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.GetValue(LineHeightRatioProperty);
    }

    /// <summary>Задаёт долю кегля, которой мерится высота строки.</summary>
    /// <param name="text">Текст.</param>
    /// <param name="value">Доля; <c>NaN</c> возвращает высоту строки шрифту.</param>
    public static void SetLineHeightRatio(TextBlock text, double value)
    {
        ArgumentNullException.ThrowIfNull(text);

        text.SetValue(LineHeightRatioProperty, value);
    }

    /// <summary>
    /// Считает высоту строки из кегля и доли.
    /// </summary>
    /// <remarks>
    /// Целыми пикселями: дробная высота строки на дробном масштабе экрана копит
    /// ошибку от строки к строке, и в длинном абзаце последняя строка уезжает.
    /// Значение ставится как текущее — местное объявление у самого текста сильнее.
    /// </remarks>
    private static void Measure(TextBlock text, bool clear)
    {
        var ratio = text.GetValue(LineHeightRatioProperty);

        if (double.IsNaN(ratio) || ratio <= 0)
        {
            if (clear)
                text.ClearValue(TextBlock.LineHeightProperty);

            return;
        }

        text.SetCurrentValue(TextBlock.LineHeightProperty, Math.Round(text.FontSize * ratio));
    }
}
