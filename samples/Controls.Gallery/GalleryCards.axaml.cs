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
    /// <summary>
    /// Показывает поле в фокусе, не отбирая фокус у окна.
    /// </summary>
    /// <remarks>
    /// Псевдокласс тот же, по которому тема рисует контур: галерея ничего не
    /// подменяет собой, она лишь ставит контрол в состояние, которое обязана
    /// показать. Иначе одна из двух половин и одна из двух колонок остались бы
    /// пустыми — фокус в окне один.
    /// </remarks>
    private static void Show(AxTextBox field) =>
        // При появлении, а не в конструкторе: поставленный слишком рано
        // псевдокласс у поля поиска не удерживался — его внутренняя кнопка
        // очистки при подключении пересчитывает состояние фокуса.
        field.AttachedToVisualTree += (_, _) =>
            ((Avalonia.Controls.IPseudoClasses)field.Classes).Set(":focus", true);

    /// <summary>
    /// Показывает звено под курсором, не двигая настоящий курсор.
    /// </summary>
    /// <summary>
    /// Ставит контролу состояние тем же псевдоклассом, каким его включает тема.
    /// </summary>
    /// <remarks>
    /// Метка ставится отложенно и возвращается после каждого события указателя.
    /// Наведение и нажатие контрол считает сам: на подключении к дереву он
    /// пересчитывает их и стирает чужую метку, а проведя над карточкой курсором,
    /// стирает её насовсем — половина витрины так и осталась бы без состояния.
    /// </remarks>
    private static void Pin(Control control, string state)
    {
        void Set() => Avalonia.Threading.Dispatcher.UIThread.Post(
            () => ((Avalonia.Controls.IPseudoClasses)control.Classes).Set(state, true),
            Avalonia.Threading.DispatcherPriority.Loaded);

        control.AttachedToVisualTree += (_, _) => Set();
        control.PointerExited += (_, _) => Set();
        control.PointerReleased += (_, _) => Set();
    }

    /// <summary>Создаёт карточки и наполняет блок кода примером разметки.</summary>
    public GalleryCards()
    {
        InitializeComponent();

        // Две колонки карточки показывают поле в фокусе, а живое окно даёт один
        // фокус на всех — и половин галереи две. Ставим состояние прямо, тем же
        // псевдоклассом, которым его включает тема: рисует по-прежнему тема, и
        // видно ровно то, что она делает на :focus.
        Show(FocusedField);
        Show(InvalidFocusedField);
        Show(FocusedSearch);
        Pin(HoveredCrumb, ":pointerover");

        // Ряд состояний иконочной кнопки в тулбаре: живое окно показало бы
        // одно состояние за раз, а карточка требует все пять сразу.
        Pin(HoveredTool, ":pointerover");
        Pin(PressedTool, ":pressed");
        Pin(SelectedTool, ":selected");

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

        menu.Items.Add(new AxMenuItem
        {
            Header = "Добавить контрол",
            Icon = new AxIcon { Data = AxIcons.Plus },
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control),
        });
        menu.Items.Add(new AxMenuItem
        {
            Header = "Переименовать",
            Icon = new AxIcon { Data = AxIcons.Edit },
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F2),
        });

        var wrap = new AxMenuItem { Header = "Обернуть в панель" };
        wrap.Items.Add(new AxMenuItem { Header = "StackPanel" });
        wrap.Items.Add(new AxMenuItem { Header = "Grid" });
        menu.Items.Add(wrap);

        menu.Items.Add(new Separator());
        menu.Items.Add(new AxMenuItem
        {
            Header = "Заблокировать слой",
            IsEnabled = false,
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.L, Avalonia.Input.KeyModifiers.Control),
        });
        menu.Items.Add(new Separator());

        var delete = new AxMenuItem
        {
            Header = "Удалить",
            Icon = new AxIcon { Data = AxIcons.Trash },
        };
        delete.Classes.Add("danger");
        menu.Items.Add(delete);

        menu.ShowAt(MenuAnchor);
    }
}
