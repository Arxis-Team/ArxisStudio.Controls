using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>Палитра и образцы выбора цвета — разделы окна Color у Unity под каналами.</summary>
public partial class AxColorPicker
{
    /// <summary>
    /// Хранилище образцов — наследуется: хозяин ставит его на поле или на панель с полями, и окно цвета берёт его у
    /// поля. Без хранилища раздела «Образцы» нет.
    /// </summary>
    public static readonly AttachedProperty<IAxColorPresetStore?> PresetStoreProperty =
        AvaloniaProperty.RegisterAttached<AxColorPicker, Control, IAxColorPresetStore?>("PresetStore", inherits: true);

    /// <summary>Раскрыт ли раздел «Палитра».</summary>
    public static readonly StyledProperty<bool> IsPaletteExpandedProperty =
        AvaloniaProperty.Register<AxColorPicker, bool>(nameof(IsPaletteExpanded), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Раскрыт ли раздел «Образцы».</summary>
    public static readonly StyledProperty<bool> ArePresetsExpandedProperty =
        AvaloniaProperty.Register<AxColorPicker, bool>(nameof(ArePresetsExpanded), true, defaultBindingMode: BindingMode.TwoWay);

    private const string PalettePart = "PART_Palette";
    private const string PresetGridPart = "PART_PresetGrid";
    private const string PresetListPart = "PART_PresetList";
    private const string PresetsMenuPart = "PART_PresetsMenu";
    private const string NamePopupPart = "PART_NamePopup";
    private const string NamePart = "PART_Name";
    private const string NameLabelPart = "PART_NameLabel";

    /// <summary>Тема ячейки образца и палитры — ключ темы.</summary>
    private const string CellTheme = "AxColorPresetCell";

    /// <summary>Тема строки образца в виде списком — ключ темы.</summary>
    private const string RowTheme = "AxColorPresetRow";

    /// <summary>Тяга образца начинается, когда указатель ушёл дальше этого, в точках: короче — это щелчок.</summary>
    private const double DragThreshold = 4;

    private Panel? _palette;
    private Panel? _presetGrid;
    private Panel? _presetList;
    private Button? _presetsMenu;
    private Popup? _namePopup;
    private TextBox? _name;
    private TextBlock? _nameLabel;
    private IAxColorPresetStore? _store;
    private AxColorPresetLibrary? _library;
    private AxColorSwatch? _addSwatch;
    private AxButton? _addCell;
    private Func<string, bool>? _naming;
    private PresetDrag? _drag;
    private AxMenuFlyout? _presetsFlyout;

    /// <summary>
    /// Встроенная палитра: двенадцать оттенков через 30° в четырёх тонах — светлом, мягком, насыщенном и тёмном, — и
    /// ряд серых от белого до чёрного. Своя сетка, а не копия Unity.
    /// </summary>
    public static IReadOnlyList<Color> Defaults { get; } = BuildDefaults();

    /// <inheritdoc cref="PresetStoreProperty"/>
    public IAxColorPresetStore? PresetStore
    {
        get => GetValue(PresetStoreProperty);
        set => SetValue(PresetStoreProperty, value);
    }

    /// <inheritdoc cref="IsPaletteExpandedProperty"/>
    public bool IsPaletteExpanded
    {
        get => GetValue(IsPaletteExpandedProperty);
        set => SetValue(IsPaletteExpandedProperty, value);
    }

    /// <inheritdoc cref="ArePresetsExpandedProperty"/>
    public bool ArePresetsExpanded
    {
        get => GetValue(ArePresetsExpandedProperty);
        set => SetValue(ArePresetsExpandedProperty, value);
    }

    /// <summary>Меню ⋮ образцов, которое открыто последним, — шов тестов.</summary>
    internal AxMenuFlyout? PresetsFlyout => _presetsFlyout;

    /// <summary>Хранилище образцов, унаследованное контролом.</summary>
    /// <param name="control">Контрол.</param>
    public static IAxColorPresetStore? GetPresetStore(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return control.GetValue(PresetStoreProperty);
    }

    /// <summary>Ставит хранилище образцов контролу и всему, что под ним.</summary>
    /// <param name="control">Контрол.</param>
    /// <param name="store">Хранилище; пусто — образцов нет.</param>
    public static void SetPresetStore(Control control, IAxColorPresetStore? store)
    {
        ArgumentNullException.ThrowIfNull(control);

        control.SetValue(PresetStoreProperty, store);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Listen(PresetStore);
        ShowPresets();
    }

    /// <inheritdoc/>
    /// <remarks>Хранилище живёт дольше окна цвета: подписка снимается, чтобы оно не держало закрытое окно.</remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Listen(null);

        base.OnDetachedFromVisualTree(e);
    }

    private static Color[] BuildDefaults()
    {
        (double Saturation, double Value)[] tones = [(0.25, 1), (0.55, 1), (0.85, 0.9), (0.9, 0.6)];
        var colors = new List<Color>();

        foreach (var (saturation, value) in tones)
        {
            for (var hue = 0; hue < 360; hue += 30)
                colors.Add(new HsvColor(1, hue, saturation, value).ToRgb());
        }

        const int Grays = 12;

        for (var step = 0; step < Grays; step++)
        {
            var level = (byte)Math.Round(byte.MaxValue * (Grays - 1 - step) / (double)(Grays - 1));

            colors.Add(Color.FromRgb(level, level, level));
        }

        return [.. colors];
    }

    /// <summary>Части палитры и образцов из шаблона.</summary>
    private void ApplyPresetsTemplate(TemplateAppliedEventArgs e)
    {
        if (_presetsMenu is not null)
            _presetsMenu.Click -= OnPresetsMenuClick;

        if (_name is not null)
            _name.RemoveHandler(KeyDownEvent, OnNameKey);

        _palette = e.NameScope.Find<Panel>(PalettePart);
        _presetGrid = e.NameScope.Find<Panel>(PresetGridPart);
        _presetList = e.NameScope.Find<Panel>(PresetListPart);
        _presetsMenu = e.NameScope.Find<Button>(PresetsMenuPart);
        _namePopup = e.NameScope.Find<Popup>(NamePopupPart);
        _name = e.NameScope.Find<TextBox>(NamePart);
        _nameLabel = e.NameScope.Find<TextBlock>(NameLabelPart);

        if (_presetsMenu is not null)
            _presetsMenu.Click += OnPresetsMenuClick;

        if (_name is not null)
            _name.AddHandler(KeyDownEvent, OnNameKey, RoutingStrategies.Tunnel);

        ShowPalette();
        ShowPresets();
    }

    /// <summary>Ячейки встроенной палитры — один раз на шаблон: палитра не меняется.</summary>
    private void ShowPalette()
    {
        if (_palette is null)
            return;

        _palette.Children.Clear();

        foreach (var color in Defaults)
        {
            var code = "#" + Hex(color);
            var cell = new AxButton
            {
                Content = new AxColorSwatch { Color = color, AlphaDisplay = AxColorAlphaDisplay.Checker },
                Tag = color,
            };

            Dress(cell, list: false);
            ToolTip.SetTip(cell, code);
            AutomationProperties.SetName(cell, code);
            cell.Click += OnPaletteClick;
            _palette.Children.Add(cell);
        }
    }

    /// <summary>Хранилище сменилось — подписка на него и на его текущую библиотеку.</summary>
    private void Listen(IAxColorPresetStore? store)
    {
        if (!ReferenceEquals(_store, store))
        {
            if (_store is not null)
                _store.Changed -= OnStoreChanged;

            _store = store;

            if (_store is not null)
                _store.Changed += OnStoreChanged;
        }

        var library = store?.Current;

        if (ReferenceEquals(_library, library))
            return;

        if (_library is not null)
            _library.Presets.CollectionChanged -= OnPresetsChanged;

        _library = library;

        if (_library is not null)
            _library.Presets.CollectionChanged += OnPresetsChanged;
    }

    private void OnStoreChanged(object? sender, EventArgs e)
    {
        Listen(PresetStore);
        ShowPresets();
    }

    private void OnPresetsChanged(object? sender, NotifyCollectionChangedEventArgs e) => ShowPresets();

    /// <summary>Ячейки образцов текущей библиотеки и последней — «сохранить текущий цвет»; сеткой или списком.</summary>
    private void ShowPresets()
    {
        var store = PresetStore;
        var list = store?.View == AxColorPresetView.List;

        PseudoClasses.Set(":presets", store is not null);
        PseudoClasses.Set(":list", list);

        // Ячейку, которую тянут, не трогают: она держит мышь, а порядок встанет при отпускании.
        if (_drag is { Moved: true })
            return;

        _presetGrid?.Children.Clear();
        _presetList?.Children.Clear();
        _addSwatch = null;
        _addCell = null;

        if (store is null || (list ? _presetList : _presetGrid) is not { } panel)
            return;

        foreach (var preset in store.Current.Presets)
            panel.Children.Add(PresetCell(preset, list));

        panel.Children.Add(AddCell(list));
    }

    private AxButton PresetCell(AxColorPreset preset, bool list)
    {
        var swatch = new AxColorSwatch { Color = preset.Color, AlphaDisplay = AxColorAlphaDisplay.Checker };
        var cell = new AxButton { Content = list ? Row(swatch, new TextBlock { Text = preset.Name }) : swatch, Tag = preset };

        Dress(cell, list);
        ToolTip.SetTip(cell, preset.Name);
        AutomationProperties.SetName(cell, preset.Name);
        cell.ContextFlyout = PresetMenu(preset);
        cell.Click += OnPresetClick;
        cell.AddHandler(PointerPressedEvent, OnPresetPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        cell.AddHandler(PointerMovedEvent, OnPresetMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        cell.AddHandler(PointerReleasedEvent, OnPresetReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        cell.AddHandler(PointerCaptureLostEvent, OnPresetCaptureLost, RoutingStrategies.Bubble, handledEventsToo: true);
        cell.AddHandler(KeyDownEvent, OnPresetKey, RoutingStrategies.Tunnel);

        return cell;
    }

    /// <summary>Последняя ячейка — текущий цвет, как у Unity: щелчок сохраняет его образцом с именем.</summary>
    private AxButton AddCell(bool list)
    {
        _addSwatch = new AxColorSwatch { Color = Color, AlphaDisplay = AxColorAlphaDisplay.Checker };

        var cell = new AxButton();

        if (list)
        {
            var text = new TextBlock();

            text[!TextBlock.TextProperty] = text.GetResourceObservable("AxTextColorPresetAdd").ToBinding();
            cell.Content = Row(_addSwatch, text);
        }
        else
        {
            cell.Content = _addSwatch;
        }

        Dress(cell, list);
        cell[!ToolTip.TipProperty] = cell.GetResourceObservable("AxTextColorPresetAdd").ToBinding();
        cell[!AutomationProperties.NameProperty] = cell.GetResourceObservable("AxTextColorPresetAdd").ToBinding();
        cell.Click += OnAddClick;
        _addCell = cell;

        return cell;
    }

    /// <summary>Строка образца в виде списком: ячейка цвета размером ячейки сетки и имя.</summary>
    private static DockPanel Row(AxColorSwatch swatch, TextBlock name)
    {
        swatch[!WidthProperty] = swatch.GetResourceObservable("AxColorPresetCellSize").ToBinding();
        swatch[!HeightProperty] = swatch.GetResourceObservable("AxColorPresetCellSize").ToBinding();
        swatch[!MarginProperty] = swatch.GetResourceObservable("AxSpaceTrailingThickness").ToBinding();
        DockPanel.SetDock(swatch, Dock.Left);
        name.VerticalAlignment = VerticalAlignment.Center;
        name.TextTrimming = TextTrimming.CharacterEllipsis;

        return new DockPanel { Children = { swatch, name } };
    }

    /// <summary>Одевает ячейку темой ячейки или строки — ключом темы.</summary>
    private void Dress(AxButton cell, bool list)
    {
        if (this.TryFindResource(list ? RowTheme : CellTheme, out var found) && found is ControlTheme theme)
            cell.Theme = theme;
    }

    /// <summary>Меню образца — правой кнопкой и с клавиатуры: заменить его текущим цветом или удалить.</summary>
    private AxMenuFlyout PresetMenu(AxColorPreset preset)
    {
        var replace = new AxMenuItem();
        var delete = new AxMenuItem { IsDestructive = true };

        replace[!HeaderedSelectingItemsControl.HeaderProperty] = replace.GetResourceObservable("AxTextColorPresetReplace").ToBinding();
        delete[!HeaderedSelectingItemsControl.HeaderProperty] = delete.GetResourceObservable("AxTextColorPresetDelete").ToBinding();
        replace.Click += (_, _) => Replace(preset);
        delete.Click += (_, _) => Delete(preset);

        return new AxMenuFlyout { Items = { replace, delete } };
    }

    /// <summary>Образец получает текущий цвет, имя остаётся.</summary>
    private void Replace(AxColorPreset preset)
    {
        if (PresetStore is not { } store)
            return;

        var at = Place(store.Current.Presets, preset);

        if (at < 0)
            return;

        store.Current.Presets[at] = preset with { Color = Color };
        store.Save(store.Current);
    }

    private void Delete(AxColorPreset preset)
    {
        if (PresetStore is not { } store || Place(store.Current.Presets, preset) is not (>= 0 and var at))
            return;

        store.Current.Presets.RemoveAt(at);
        store.Save(store.Current);
    }

    /// <summary>Место образца в библиотеке — его самого, а не равного ему.</summary>
    /// <remarks>
    /// Образец — запись, и два одинаковых равны, но для человека это два образца: поиск по равенству находил первый
    /// равный, и «Заменить», «Удалить», Alt+стрелка и тяга у второго из двух одинаковых правили первый.
    /// </remarks>
    private static int Place(IList<AxColorPreset> presets, AxColorPreset preset)
    {
        for (var at = 0; at < presets.Count; at++)
        {
            if (ReferenceEquals(presets[at], preset))
                return at;
        }

        return -1;
    }

    /// <summary>Цвет палитры — в выбор; прозрачность остаётся прежней, как у пипетки: в палитре цвета плотные.</summary>
    private void OnPaletteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: Color color })
            Choose(Color.FromArgb(Color.A, color.R, color.G, color.B));
    }

    /// <summary>Цвет образца — в выбор, с его прозрачностью; после тяги щелчок не считается.</summary>
    private void OnPresetClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: AxColorPreset preset } && _drag is not { Moved: true })
            Choose(preset.Color);
    }

    /// <summary>Выбранный цвет — одной фиксацией.</summary>
    /// <remarks>
    /// Тот же цвет ничего не фиксирует, если цвет был: без цвета в начале — «было» пусто, у кисти, которой нет, и у
    /// разных значений — показанный цвет лишь стоит на месте, и выбрать его значит выбрать. Иначе нельзя было задать
    /// кисти, которой нет, показанный белый и свести разные значения к цвету первого.
    /// </remarks>
    private void Choose(Color color)
    {
        if (!AllowsAlpha)
            color = Color.FromArgb(byte.MaxValue, color.R, color.G, color.B);

        if (color == Color && OriginalColor is not null)
            return;

        Adopt(color);
        Spread(force: true);
        RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color));
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control anchor || PresetStore is not { } store)
            return;

        var color = Color;

        AskName(anchor, "AxTextColorPresetName", "#" + Hex(color), name =>
        {
            store.Current.Presets.Add(new AxColorPreset(name, color));
            store.Save(store.Current);
            return true;
        });
    }

    /// <summary>Спрашивает имя всплывающим полем у ячейки: Enter — готово, Esc — отмена.</summary>
    private void AskName(Control anchor, string label, string initial, Func<string, bool> done)
    {
        if (_namePopup is null || _name is null)
            return;

        _naming = done;

        if (_nameLabel is not null)
            _nameLabel[!TextBlock.TextProperty] = _nameLabel.GetResourceObservable(label).ToBinding();

        _name.Text = initial;
        _namePopup.PlacementTarget = anchor;

        // Карточка стоит в попапе на поле под тень, как у меню: сдвиг возвращает её к ячейке.
        if (this.TryFindResource(AxMenuFlyout.ShadowRoomKey, ActualThemeVariant, out var found) && found is Thickness room)
        {
            _namePopup.HorizontalOffset = -room.Left;
            _namePopup.VerticalOffset = -room.Top;
        }

        _namePopup.IsOpen = true;

        Dispatcher.UIThread.Post(() =>
        {
            if (_namePopup?.IsOpen != true || _name is null)
                return;

            _name.Focus(NavigationMethod.Tab);
            _name.SelectAll();
        }, DispatcherPriority.Loaded);
    }

    /// <summary>Enter и Esc поля имени остаются в нём: до окна цвета, где Esc отменяет весь сеанс, они не доходят.</summary>
    private void OnNameKey(object? sender, KeyEventArgs e)
    {
        if (_namePopup is null || _name is null || e.KeyModifiers != KeyModifiers.None)
            return;

        if (e.Key == Key.Enter)
        {
            var name = (_name.Text ?? string.Empty).Trim();

            if (name.Length > 0 && _naming?.Invoke(name) == true)
                CloseNaming();

            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CloseNaming();
            e.Handled = true;
        }
    }

    private void CloseNaming()
    {
        _naming = null;

        if (_namePopup is not null)
        {
            var anchor = _namePopup.PlacementTarget;

            _namePopup.IsOpen = false;

            // Каретка — туда, откуда спрашивали, если ячейка ещё стоит; иначе — на новую ячейку «сохранить текущий».
            var target = anchor is { } control && control.IsAttachedToVisualTree() ? control : _addCell;

            target?.Focus(NavigationMethod.Tab);
        }
    }

    /// <summary>Меню ⋮ образцов: вид, библиотеки с отметкой текущей, новая библиотека в профиле или в решении.</summary>
    private void OnPresetsMenuClick(object? sender, RoutedEventArgs e)
    {
        if (PresetStore is not { } store || _presetsMenu is not { } anchor)
            return;

        var menu = new AxMenuFlyout();

        menu.Items.Add(Choice("AxTextColorPresetsGrid", store.View == AxColorPresetView.Grid, () => store.View = AxColorPresetView.Grid));
        menu.Items.Add(Choice("AxTextColorPresetsList", store.View == AxColorPresetView.List, () => store.View = AxColorPresetView.List));

        foreach (var location in new[] { AxColorPresetLocation.User, AxColorPresetLocation.Project })
        {
            var libraries = store.Libraries.Where(library => library.Location == location).ToList();

            if (libraries.Count == 0)
                continue;

            menu.Items.Add(new AxSeparator());

            foreach (var library in libraries)
            {
                menu.Items.Add(new AxMenuItem
                {
                    Header = library.Name,
                    ToggleType = MenuItemToggleType.Radio,
                    IsChecked = ReferenceEquals(library, store.Current),
                    Command = new PresetCommand(() => store.Select(library)),
                });
            }
        }

        menu.Items.Add(new AxSeparator());
        menu.Items.Add(Creation(store, AxColorPresetLocation.User, "AxTextColorLibraryInProfile", anchor));
        menu.Items.Add(Creation(store, AxColorPresetLocation.Project, "AxTextColorLibraryInProject", anchor));
        _presetsFlyout = menu;
        menu.ShowAt(anchor);
    }

    private static AxMenuItem Choice(string header, bool chosen, Action choose)
    {
        var item = new AxMenuItem
        {
            ToggleType = MenuItemToggleType.Radio,
            IsChecked = chosen,
            Command = new PresetCommand(choose),
        };

        item[!HeaderedSelectingItemsControl.HeaderProperty] = item.GetResourceObservable(header).ToBinding();

        return item;
    }

    private AxMenuItem Creation(IAxColorPresetStore store, AxColorPresetLocation location, string header, Control anchor)
    {
        var item = new AxMenuItem
        {
            IsEnabled = store.CanCreate(location),
            Command = new PresetCommand(() => AskName(anchor, "AxTextColorLibraryName", string.Empty, name =>
            {
                store.Create(name, location);
                return true;
            })),
        };

        item[!HeaderedSelectingItemsControl.HeaderProperty] = item.GetResourceObservable(header).ToBinding();

        return item;
    }

    /// <summary>Alt+стрелка двигает образец на место раньше или позже — тяга с клавиатуры.</summary>
    private void OnPresetKey(object? sender, KeyEventArgs e)
    {
        if (sender is not Control { Tag: AxColorPreset preset } || PresetStore is not { } store)
            return;

        if (e.Key == Key.Escape && _drag is { Moved: true })
        {
            CancelDrag();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers != KeyModifiers.Alt)
            return;

        var step = e.Key switch
        {
            Key.Left or Key.Up => -1,
            Key.Right or Key.Down => 1,
            _ => 0,
        };

        var presets = store.Current.Presets;
        var from = Place(presets, preset);
        var to = from + step;

        if (step == 0 || from < 0 || to < 0 || to >= presets.Count)
            return;

        presets.Move(from, to);
        store.Save(store.Current);
        Refocus(preset);
        e.Handled = true;
    }

    /// <summary>Каретка — на ячейку образца после перестройки.</summary>
    private void Refocus(AxColorPreset preset)
    {
        var cell = (PresetStore?.View == AxColorPresetView.List ? _presetList : _presetGrid)?.Children
            .OfType<Control>()
            .FirstOrDefault(child => ReferenceEquals(child.Tag, preset));

        cell?.Focus(NavigationMethod.Tab);
    }

    private void OnPresetPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { Tag: AxColorPreset preset } cell || !e.GetCurrentPoint(cell).Properties.IsLeftButtonPressed ||
            cell.Parent is not Panel panel)
        {
            return;
        }

        _drag = new PresetDrag(cell, preset, panel, e.GetPosition(panel));
    }

    /// <summary>Тяга образца: ячейка встаёт на место той, над которой указатель; порядок пишется при отпускании.</summary>
    private void OnPresetMoved(object? sender, PointerEventArgs e)
    {
        if (_drag is not { } drag || !ReferenceEquals(sender, drag.Cell))
            return;

        var at = e.GetPosition(drag.Panel);

        if (!drag.Moved)
        {
            if (Math.Abs(at.X - drag.Start.X) < DragThreshold && Math.Abs(at.Y - drag.Start.Y) < DragThreshold)
                return;

            drag.Moved = true;
            PseudoClasses.Set(":dragging", true);
        }

        var over = drag.Panel.Children
            .OfType<Control>()
            .FirstOrDefault(child => child.Tag is AxColorPreset && child.Bounds.Contains(at));

        if (over is null || ReferenceEquals(over, drag.Cell))
            return;

        drag.Panel.Children.Move(drag.Panel.Children.IndexOf(drag.Cell), drag.Panel.Children.IndexOf(over));
    }

    private void OnPresetReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_drag is not { } drag || !ReferenceEquals(sender, drag.Cell))
            return;

        if (!drag.Moved)
        {
            _drag = null;
            return;
        }

        // Щелчок кнопки после тяги ещё впереди: отметка тяги уходит постом, когда он уже прошёл.
        Dispatcher.UIThread.Post(() => FinishDrag(drag), DispatcherPriority.Input);
    }

    private void OnPresetCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_drag is { Moved: true } drag && ReferenceEquals(sender, drag.Cell))
            Dispatcher.UIThread.Post(() => FinishDrag(drag), DispatcherPriority.Input);
    }

    /// <summary>Порядок ячеек после тяги — в библиотеку и в хранилище.</summary>
    private void FinishDrag(PresetDrag drag)
    {
        if (!ReferenceEquals(_drag, drag))
            return;

        _drag = null;
        PseudoClasses.Set(":dragging", false);

        if (PresetStore is not { } store)
            return;

        var presets = store.Current.Presets;
        var order = drag.Panel.Children.OfType<Control>().Select(child => child.Tag).OfType<AxColorPreset>().ToList();
        var changed = false;

        for (var at = 0; at < order.Count; at++)
        {
            var from = Place(presets, order[at]);

            if (from >= 0 && from != at)
            {
                presets.Move(from, at);
                changed = true;
            }
        }

        if (changed)
            store.Save(store.Current);
        else
            ShowPresets();
    }

    /// <summary>Esc посреди тяги — порядок прежний, мышь отпущена, каретка — на образце.</summary>
    private void CancelDrag()
    {
        if (_drag is not { } drag)
            return;

        _drag = null;
        PseudoClasses.Set(":dragging", false);
        ShowPresets();
        Refocus(drag.Preset);
    }

    /// <summary>Ячейку «сохранить текущий» красит текущий цвет.</summary>
    private void ShowCurrentInPresets(Color color)
    {
        if (_addSwatch is not null)
            _addSwatch.Color = color;
    }

    /// <summary>Тяга образца: что тянут, откуда и ушла ли она дальше щелчка.</summary>
    private sealed class PresetDrag(Control cell, AxColorPreset preset, Panel panel, Point start)
    {
        public Control Cell { get; } = cell;

        public AxColorPreset Preset { get; } = preset;

        public Panel Panel { get; } = panel;

        public Point Start { get; } = start;

        public bool Moved { get; set; }
    }

    /// <summary>Пункт меню, который просто делает своё.</summary>
    private sealed class PresetCommand(Action run) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => run();
    }
}
