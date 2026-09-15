using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace ArxisStudio.Controls;

/// <summary>
/// Кнопка, которая остаётся нажатой: включённый инструмент, фильтр уровня, пункт
/// навигации.
/// </summary>
/// <remarks>
/// Вид и размер у неё те же, что у <see cref="AxButton"/>, а включённость — штатное
/// <see cref="ToggleButton.IsChecked"/> и псевдокласс <c>:checked</c>. Прежде студия и
/// модули держали по своему наследнику кнопки со своим свойством: псевдоклассы у
/// Avalonia защищены, и поставить состояние снаружи было нечем.
/// </remarks>
[PseudoClasses(":primary", ":subtle", ":danger", ":toolbar", ":compact")]
public class AxToggleButton : ToggleButton
{
    /// <summary>Вид кнопки.</summary>
    public static readonly StyledProperty<AxButtonAppearance> AppearanceProperty =
        AxButton.AppearanceProperty.AddOwner<AxToggleButton>();

    /// <summary>Размер: обычный или тесный.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxToggleButton>();

    static AxToggleButton()
    {
        AppearanceProperty.Changed.AddClassHandler<AxToggleButton>((button, change) =>
            AxLook.Mark(button.PseudoClasses, change.GetNewValue<AxButtonAppearance>()));
        SizeProperty.Changed.AddClassHandler<AxToggleButton>((button, change) =>
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
