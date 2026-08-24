using Avalonia;
using Avalonia.Controls;
 
using Avalonia.Styling;

namespace Controls.Gallery;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ThemeSwitch.SelectedIndex = 0;
    }

    private void OnThemeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = ThemeSwitch.SelectedIndex == 1 ? ThemeVariant.Light : ThemeVariant.Dark;
    }
}
