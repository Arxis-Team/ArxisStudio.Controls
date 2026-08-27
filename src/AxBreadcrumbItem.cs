using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Звено хлебных крошек: подпись со значком и шевроном-разделителем слева.
/// Последнее звено принято помечать классом <c>current</c> — оно читается
/// основным текстом, остальные ведут себя как ссылки; класс <c>error</c>
/// красит звено с проблемой.
/// </summary>
/// <remarks>
/// Значок здесь — содержимое, а не путь, по той же причине, что и в дереве:
/// в пути к файлу рядом стоят папка, нарисованная глифом, и значок типа
/// файла — маленькая плашка с двумя буквами.
/// </remarks>
public class AxBreadcrumbItem : ContentControl
{
    /// <summary>Значок слева от подписи: глиф, плашка типа файла или своё.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<AxBreadcrumbItem, object?>(nameof(Icon));

    /// <inheritdoc cref="IconProperty"/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
