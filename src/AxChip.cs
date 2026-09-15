using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>Вид чипа.</summary>
public enum AxChipKind
{
    /// <summary>Метка: тег шаблона, версия, состояние.</summary>
    Default,

    /// <summary>Выделенная метка.</summary>
    Accent,

    /// <summary>Клавиша или сочетание: «Ctrl+Shift+P».</summary>
    Key,
}

/// <summary>
/// Чип-метка: тег шаблона, версия, состояние плагина, сочетание клавиш.
/// </summary>
/// <remarks>
/// Вид — перечисление <see cref="Kind"/>, а не классы стиля: тема видит его
/// псевдоклассами <c>:accent</c> и <c>:key</c>.
/// </remarks>
[PseudoClasses(":accent", ":key")]
public class AxChip : ContentControl
{
    /// <summary>Вид чипа.</summary>
    public static readonly StyledProperty<AxChipKind> KindProperty =
        AvaloniaProperty.Register<AxChip, AxChipKind>(nameof(Kind));

    static AxChip() =>
        KindProperty.Changed.AddClassHandler<AxChip>((chip, change) =>
        {
            chip.PseudoClasses.Set(":accent", change.GetNewValue<AxChipKind>() == AxChipKind.Accent);
            chip.PseudoClasses.Set(":key", change.GetNewValue<AxChipKind>() == AxChipKind.Key);
        });

    /// <inheritdoc cref="KindProperty"/>
    public AxChipKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }
}
