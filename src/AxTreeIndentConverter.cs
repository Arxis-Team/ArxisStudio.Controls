using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ArxisStudio.Controls;

/// <summary>
/// Отступ строки дерева по её уровню вложенности. Живёт в библиотеке
/// контролов, потому что им пользуется шаблон дерева, а шаблон — это тема.
/// </summary>
public sealed class AxTreeIndentConverter : IValueConverter
{
    /// <summary>
    /// Ширина одного уровня вложенности.
    /// </summary>
    /// <remarks>
    /// 18: карточка «Дерево» проекта отбивает уровни как 8 → 26 → 44, считая
    /// от левого края строки. Шаг меньше сбивает лестницу, и на глубине третьего
    /// уровня дерево перестаёт читаться как дерево. Ровно столько же занимает
    /// шеврон с зазором (12 + 6) — потому у строки без шеврона значок и встаёт
    /// в его колонку, а не правее.
    /// </remarks>
    public const double LevelWidth = 18;

    /// <summary>
    /// Отбивка первого уровня от левого края панели.
    /// </summary>
    /// <remarks>
    /// 8 — левая часть отбивки строки из карточки (padding: 0 8). Живёт здесь,
    /// а не отдельным Margin в шаблоне: у DockPanel строки один отступ на всех,
    /// и складывать его негде.
    /// </remarks>
    public const double BaseIndent = 8;

    /// <summary>Общий экземпляр для шаблонов.</summary>
    public static AxTreeIndentConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new Thickness(BaseIndent + (value is int level ? level * LevelWidth : 0), 0, 0, 0);

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
