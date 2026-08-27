using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ArxisStudio.Controls;

/// <summary>
/// Отступ строки дерева по её уровню вложенности: 14px на уровень, как в
/// дизайне студии. Живёт в библиотеке контролов, потому что им пользуется
/// шаблон дерева, а шаблон — это тема.
/// </summary>
public sealed class AxTreeIndentConverter : IValueConverter
{
    /// <summary>
    /// Ширина одного уровня вложенности.
    /// </summary>
    /// <remarks>
    /// 18: карточка «Дерево» проекта отбивает уровни как 8 → 26 → 44, считая
    /// от левого края строки. Шаг меньше сбивает лестницу, и на глубине третьего
    /// уровня дерево перестаёт читаться как дерево.
    /// </remarks>
    public const double LevelWidth = 18;

    /// <summary>Общий экземпляр для шаблонов.</summary>
    public static AxTreeIndentConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new Thickness(value is int level ? level * LevelWidth : 0, 0, 0, 0);

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
