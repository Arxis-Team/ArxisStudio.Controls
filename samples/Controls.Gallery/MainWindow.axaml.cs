using Avalonia;
using Avalonia.Controls;

namespace Controls.Gallery;

/// <summary>
/// Окно галереи: светлая и тёмная половины рядом, как на странице
/// «20 Контролы» дизайн-проекта; переключатель прячет одну из них.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>Создаёт окно с обеими половинами.</summary>
    public MainWindow()
    {
        InitializeComponent();
        PanelsSwitch.SelectedIndex = 0;
    }

    private void OnPanelsChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LightHalf is null || DarkHalf is null)
            return;

        Show(LightHalf, Halves.ColumnDefinitions[0], PanelsSwitch.SelectedIndex != 2);
        Show(DarkHalf, Halves.ColumnDefinitions[1], PanelsSwitch.SelectedIndex != 1);
    }

    /// <summary>
    /// Показывает или убирает половину вместе с её колонкой.
    /// </summary>
    /// <remarks>
    /// Обязательно вместе. Скрытая половина сама по себе перестаёт рисоваться,
    /// но её звёздочка продолжает делить окно пополам — оставшаяся тема так и
    /// осталась бы на половине, а рядом была бы пустота. Нулевая колонка сама
    /// по себе тоже не годится: половина осталась бы живой и мерилась бы в
    /// нулевой ширине.
    /// </remarks>
    private static void Show(Control half, ColumnDefinition column, bool visible)
    {
        half.IsVisible = visible;
        column.Width = visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }
}
