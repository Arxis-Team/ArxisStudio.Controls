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

        LightHalf.IsVisible = PanelsSwitch.SelectedIndex != 2;
        DarkHalf.IsVisible = PanelsSwitch.SelectedIndex != 1;
    }
}
