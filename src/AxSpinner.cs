using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace ArxisStudio.Controls;

/// <summary>Размер лоадера.</summary>
public enum AxSpinnerSize
{
    /// <summary>В клетку иконки, цветом иконки: рядом с подписью.</summary>
    Normal,

    /// <summary>Крупный, акцентом: ожидание на месте содержимого.</summary>
    Large,
}

/// <summary>
/// Лоадер: кольцо с дугой, крутящейся 0.8 с на оборот линейно, — одна из двух
/// разрешённых анимаций движения наряду с неопределённым прогрессом. Цвет
/// наследует Foreground, как иконка.
/// </summary>
[PseudoClasses(":large")]
public class AxSpinner : TemplatedControl
{
    /// <summary>Размер лоадера.</summary>
    public static readonly StyledProperty<AxSpinnerSize> SizeProperty =
        AvaloniaProperty.Register<AxSpinner, AxSpinnerSize>(nameof(Size));

    static AxSpinner() =>
        SizeProperty.Changed.AddClassHandler<AxSpinner>((spinner, change) =>
            spinner.PseudoClasses.Set(":large", change.GetNewValue<AxSpinnerSize>() == AxSpinnerSize.Large));

    /// <inheritdoc cref="SizeProperty"/>
    public AxSpinnerSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
