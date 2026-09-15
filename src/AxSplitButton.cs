using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>
/// Кнопка с меню: левая часть выполняет действие, правая открывает
/// <see cref="Button.Flyout"/> со вспомогательными вариантами.
/// </summary>
/// <remarks>
/// Вид — тот же <see cref="AxButtonAppearance"/>, что у кнопки. Тема различает
/// обычную и <see cref="AxButtonAppearance.Primary"/> — основное действие; остальные
/// виды кнопке с меню не нужны и рисуются обычной.
/// </remarks>
[PseudoClasses(":primary", ":subtle", ":danger", ":toolbar")]
public class AxSplitButton : SplitButton
{
    /// <summary>Вид кнопки.</summary>
    public static readonly StyledProperty<AxButtonAppearance> AppearanceProperty =
        AxButton.AppearanceProperty.AddOwner<AxSplitButton>();

    static AxSplitButton() =>
        AppearanceProperty.Changed.AddClassHandler<AxSplitButton>((button, change) =>
            AxLook.Mark(button.PseudoClasses, change.GetNewValue<AxButtonAppearance>()));

    /// <inheritdoc cref="AppearanceProperty"/>
    public AxButtonAppearance Appearance
    {
        get => GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }
}
