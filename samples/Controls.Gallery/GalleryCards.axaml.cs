using ArxisStudio.Controls;
using ArxisStudio.Icons;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

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

        // Дерево и таблица: выделение показано в полную силу — AxSel живёт под
        // фокусом списка, а в витрине фокус один на всех, и без метки карточки
        // показывали бы погашенное AxSelInactive, которого в макете нет.
        Pin(ProjectTree, ":focus-within");
        Pin(FilesTable, ":focus-within");

        // Кнопки карточки «Button»: два вида по пяти состояниям.
        foreach (var hovered in new[] { HoveredPrimary, HoveredSecondary, HoveredSlim })
            Pin(hovered, ":pointerover");

        foreach (var pressed in new[] { PressedPrimary, PressedSecondary, PressedSlim })
            Pin(pressed, ":pressed");

        // Фокус кнопки рисуется по :focus-visible — тем же псевдоклассом, по
        // которому его поднимает обход с клавиатуры.
        foreach (var focused in new[] { FocusedPrimary, FocusedSecondary, FocusedSlim })
            Pin(focused, ":focus-visible");

        // Флажки карточки «Checkbox»: три типа по пяти состояниям.
        foreach (var hovered in new[] { HoveredBox, HoveredChecked, HoveredMixed })
            Pin(hovered, ":pointerover");

        foreach (var focused in new[] { FocusedBox, FocusedChecked, FocusedMixed })
            Pin(focused, ":focus-visible");

        // Развёрнутый скроллбар второй области: разворот поднимает сама
        // область прокрутки под курсором, а карточка показывает оба состояния
        // сразу — курсор в живом окне один.
        // Полосы ищем на каждом пересчёте разметки: на подключении к дереву
        // шаблон области ещё не развёрнут, и искать в ней нечего, а сама
        // область поднимает и опускает разворот, пока курсор ходит по окну.
        // Повторная установка того же значения ничего не делает.
        ExpandedScroll.LayoutUpdated += (_, _) =>
        {
            foreach (var bar in ExpandedScroll.GetVisualDescendants()
                         .OfType<Avalonia.Controls.Primitives.ScrollBar>())
                ((Avalonia.Controls.IPseudoClasses)bar.Classes).Set(":expanded", true);
        };

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
        // Ширину задаёт содержимое: диалог растёт под текст (SizeToContent), и
        // число на окне он бы просто не заметил. Карточка даёт 400 на всю
        // карточку — за вычетом отбивки 16 с боков это 368 на текст.
        var dialog = new AxDialog
        {
            Title = "Сохранить изменения?",
            Content = new TextBlock
            {
                Text = "MainWindow.axaml изменён. Сохранить файл перед закрытием редактора?",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Width = 368,
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

    /// <summary>
    /// Знак алерта: треугольник предупреждения на жёлтом.
    /// </summary>
    /// <remarks>
    /// Кисть берётся ресурсом темы, а не разово через Application: у варианта
    /// свои значения, и снятое заранее не переключилось бы вместе с темой — а
    /// без кисти путь не рисуется вовсе.
    ///
    /// Двадцать восемь — размер из карточки. Это не глиф в ряду иконок, а знак
    /// на весь диалог, и клетка набора ему не указ.
    /// </remarks>
    private static AxIcon Sign()
    {
        var sign = new AxIcon { Width = 28, Height = 28, Data = AxIcons.WarningTriangle };

        sign[!AxIcon.ForegroundProperty] = new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("AxYelBrush");

        return sign;
    }

    private async void OnOpenDangerDialog(object? sender, RoutedEventArgs e)
    {
        // Алерт: значок вместо шапки. Треугольник крупнее клетки набора —
        // это не глиф в ряду иконок, а знак на весь диалог, и карточка даёт
        // ему 28. Размер ставит потребитель: тема о нём ничего не знает.
        var dialog = new AxDialog
        {
            Title = "Удалить форму LoginView.axaml?",
            AlertIcon = Sign(),
            Content = new TextBlock
            {
                Text = "Форма и её привязки будут удалены из проекта. Действие нельзя отменить.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                // 360 карточки минус отбивка 16 с боков, значок 28 и зазор 12.
                Width = 288,
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
