using ArxisStudio.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Controls.Gallery;

/// <summary>
/// Содержимое одной половины галереи. Всё, что нельзя показать разметкой, —
/// текст блока кода, диалоги и меню по щелчку — живёт здесь.
/// </summary>
public partial class GalleryCards : UserControl
{
    /// <summary>Создаёт карточки и наполняет блок кода примером разметки.</summary>
    public GalleryCards()
    {
        InitializeComponent();

        QuickSearch.ItemsSource = new[]
        {
            "ChatView.axaml · Views",
            "ChatViewModel.cs · ViewModels",
            "ChatService.cs · Services",
        };

        Code.Text = """
            <!-- ChatView.axaml -->
            <StackPanel Spacing="8">
              <TextBox Text="{Binding Message}" />
              <Button Classes="accent" Content="Отправить" />
            </StackPanel>
            """;
    }

    private async void OnOpenDialog(object? sender, RoutedEventArgs e)
    {
        var dialog = new AxDialog
        {
            Title = "Сохранить изменения?",
            Content = new TextBlock
            {
                Text = "MainWindow.axaml изменён. Сохранить файл перед закрытием редактора?",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 360,
            },
            FooterContent = new AxCheckBox { Content = "Больше не спрашивать" },
        };

        var buttons = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        var discard = new AxButton { Content = "Не сохранять" };
        var save = new AxButton { Content = "Сохранить", Classes = { "accent" } };

        discard.Click += (_, _) => dialog.Close();
        save.Click += (_, _) => dialog.Close();
        buttons.Children.Add(discard);
        buttons.Children.Add(save);
        dialog.Buttons = buttons;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);
    }

    private async void OnOpenDangerDialog(object? sender, RoutedEventArgs e)
    {
        var dialog = new AxDialog
        {
            Title = "Удалить форму LoginView.axaml?",
            Content = new TextBlock
            {
                Text = "Форма и её привязки будут удалены из проекта. Действие нельзя отменить.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 340,
            },
        };

        var buttons = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        var cancel = new AxButton { Content = "Отмена" };
        var delete = new AxButton { Content = "Удалить", Classes = { "danger" } };

        cancel.Click += (_, _) => dialog.Close();
        delete.Click += (_, _) => dialog.Close();
        buttons.Children.Add(cancel);
        buttons.Children.Add(delete);
        dialog.Buttons = buttons;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);
    }

    private void OnOpenMenu(object? sender, RoutedEventArgs e)
    {
        var menu = new AxMenuFlyout();

        menu.Items.Add(new MenuItem
        {
            Header = "Добавить контрол",
            Icon = new AxIcon { Data = AxIcons.Plus },
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control),
        });
        menu.Items.Add(new MenuItem
        {
            Header = "Переименовать",
            Icon = new AxIcon { Data = AxIcons.Edit },
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F2),
        });

        var wrap = new MenuItem { Header = "Обернуть в панель" };
        wrap.Items.Add(new MenuItem { Header = "StackPanel" });
        wrap.Items.Add(new MenuItem { Header = "Grid" });
        menu.Items.Add(wrap);

        menu.Items.Add(new Separator());
        menu.Items.Add(new MenuItem
        {
            Header = "Заблокировать слой",
            IsEnabled = false,
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.L, Avalonia.Input.KeyModifiers.Control),
        });
        menu.Items.Add(new Separator());

        var delete = new MenuItem
        {
            Header = "Удалить",
            Icon = new AxIcon { Data = AxIcons.Trash },
        };
        delete.Classes.Add("danger");
        menu.Items.Add(delete);

        menu.ShowAt(MenuAnchor);
    }
}
