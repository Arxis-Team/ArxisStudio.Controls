using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>Вид кнопки: насколько она заметна и что обещает нажатие.</summary>
public enum AxButtonAppearance
{
    /// <summary>Контурная: обычное действие.</summary>
    Default,

    /// <summary>Залитая акцентом: основное действие диалога или экрана.</summary>
    Primary,

    /// <summary>Без рамки, пока на неё не навели: действие второго плана рядом с текстом.</summary>
    Subtle,

    /// <summary>Деструктивная: удалить, закрыть без сохранения.</summary>
    Danger,

    /// <summary>Кнопка полосы и шапки панели: без рамки, высотой в тесную строку, под одну иконку — квадрат.</summary>
    Toolbar,
}

/// <summary>
/// Кнопка студии.
/// </summary>
/// <remarks>
/// Вид и размер — свойства, а не классы стиля. Класс с опечаткой молча давал кнопку
/// по умолчанию, и заметить это можно было только глазами; имя, которого нет в
/// перечислении, не пропустит компилятор разметки. Тема видит их псевдоклассами:
/// значение по умолчанию псевдокласса не несёт.
/// </remarks>
[PseudoClasses(":primary", ":subtle", ":danger", ":toolbar", ":compact")]
public class AxButton : Button
{
    /// <summary>Вид кнопки.</summary>
    public static readonly StyledProperty<AxButtonAppearance> AppearanceProperty =
        AvaloniaProperty.Register<AxButton, AxButtonAppearance>(nameof(Appearance));

    /// <summary>Размер: обычный или тесный — для полосы, шапки и подвала панели.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AvaloniaProperty.Register<AxButton, AxControlSize>(nameof(Size));

    static AxButton()
    {
        AppearanceProperty.Changed.AddClassHandler<AxButton>((button, change) =>
            AxLook.Mark(button.PseudoClasses, change.GetNewValue<AxButtonAppearance>()));
        SizeProperty.Changed.AddClassHandler<AxButton>((button, change) =>
            AxLook.Mark(button.PseudoClasses, change.GetNewValue<AxControlSize>()));
    }

    /// <inheritdoc cref="AppearanceProperty"/>
    public AxButtonAppearance Appearance
    {
        get => GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <inheritdoc cref="SizeProperty"/>
    public AxControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
