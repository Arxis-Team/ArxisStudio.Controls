using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Таблица как список: строка заголовков и выбираемые строки высотой
/// <c>AxRowHeight</c>. Колонки выравниваются шаблонами строк — обычно
/// Grid с одинаковыми ColumnDefinitions в шапке и в строке.
/// </summary>
/// <remarks>
/// Спецификация предлагала базой TreeDataGrid; таблица студии пока не
/// требует ни виртуализации колонок, ни иерархии, а зависимость легла бы
/// на каждого потребителя библиотеки — поэтому база та же, что у списка.
/// Решение записано в отчёте переноса и ждёт подтверждения владельца зоны.
/// </remarks>
public class AxDataGrid : AxListBox
{
    /// <summary>Строка заголовков колонок.</summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<AxDataGrid, object?>(nameof(Header));

    /// <inheritdoc cref="HeaderProperty"/>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
