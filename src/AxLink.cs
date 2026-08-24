using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Ссылка: текст, по которому кликают. Внешне не кнопка, но ведёт себя как
/// кнопка — есть наведение, нажатие, фокус с клавиатуры и состояние
/// «посещённая», которое хост выставляет сам.
/// </summary>
public class AxLink : Button
{
    /// <summary>Ссылку уже открывали.</summary>
    public static readonly StyledProperty<bool> IsVisitedProperty =
        AvaloniaProperty.Register<AxLink, bool>(nameof(IsVisited));

    /// <inheritdoc cref="IsVisitedProperty"/>
    public bool IsVisited
    {
        get => GetValue(IsVisitedProperty);
        set => SetValue(IsVisitedProperty, value);
    }
}
