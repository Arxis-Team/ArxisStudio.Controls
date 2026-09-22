using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Крошки навигации: путь от корня до текущего места, где каждый сегмент ведёт на свой уровень.
/// </summary>
/// <remarks>
/// Сегменты — <see cref="AxBreadcrumbItem"/>; источником служат и готовые сегменты, и любые данные
/// с шаблоном. Выбор сегмента — щелчком, Enter или пунктом меню переполнения — поднимает
/// <see cref="Navigated"/> с его данными и номером; последний сегмент — текущее место и события
/// не поднимает. Что значит «перейти», решает владелец: крошки пути не меняют.
/// <para>
/// Места не хватает — ведущие сегменты уходят в меню переполнения у левого края, последний
/// остаётся всегда. Меню набирается при раскладке, как у <see cref="AxTabStrip"/>: всплывающее окно
/// строит содержимое при первом открытии, и набранное по событию открытия туда уже не попадало бы.
/// </para>
/// <para>
/// На клавиатуре путь — одна остановка Tab; по сегментам ходят Left и Right, Home и End ведут к
/// крайним видимым, а шаг влево с первого видимого сегмента — на кнопку переполнения, когда она есть.
/// </para>
/// <para>
/// Крошки бывают целью перетаскивания, и спрятанные уровни — тоже: хозяин, над «…» которого держат
/// перетаскиваемое, раскрывает меню, не забирая ни клавиатуры, ни окна (<see cref="OpenOverflow"/>),
/// и спрашивает, какой сегмент под курсором — показанный или спрятанный в меню
/// (<see cref="SegmentAt"/>). Цель, поставленная спрятанному сегменту, видна на его пункте. Тяга из
/// проводника над меню приходит к крошкам их собственными событиями: у всплывающего окна свой корень,
/// и до крошек его события иначе не доходят.
/// </para>
/// </remarks>
[PseudoClasses(":overflow")]
[TemplatePart("PART_Overflow", typeof(Button))]
public class AxBreadcrumb : ItemsControl
{
    /// <summary>Выбран сегмент пути, кроме текущего.</summary>
    public static readonly RoutedEvent<AxBreadcrumbNavigatedEventArgs> NavigatedEvent =
        RoutedEvent.Register<AxBreadcrumb, AxBreadcrumbNavigatedEventArgs>(nameof(Navigated), RoutingStrategies.Bubble);

    private readonly List<AxBreadcrumbItem> _listed = [];
    private Button? _overflow;
    private OverflowMenu? _menu;

    static AxBreadcrumb()
    {
        ItemsPanelProperty.OverrideDefaultValue<AxBreadcrumb>(new FuncTemplate<Panel?>(() => new AxBreadcrumbPanel()));
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<AxBreadcrumb>(KeyboardNavigationMode.Once);
        AxBreadcrumbItem.IsDropTargetProperty.Changed.AddClassHandler<AxBreadcrumbItem>(
            (segment, _) => (ItemsControlFromItemContainer(segment) as AxBreadcrumb)?.Reflect(segment));
    }

    /// <summary>Заводит крошки: щелчки по сегментам слушаются здесь, а не у каждого сегмента.</summary>
    public AxBreadcrumb() => AddHandler(Button.ClickEvent, OnSegmentClick);

    /// <inheritdoc cref="NavigatedEvent"/>
    public event EventHandler<AxBreadcrumbNavigatedEventArgs>? Navigated
    {
        add => AddHandler(NavigatedEvent, value);
        remove => RemoveHandler(NavigatedEvent, value);
    }

    /// <summary>Меню спрятанных уровней открыто.</summary>
    public bool IsOverflowOpen => _menu?.IsOpen == true;

    /// <summary>
    /// Раскрывает меню спрятанных уровней, не забирая клавиатуры и не закрывая окна: так его
    /// раскрывает тяга, задержавшаяся над «…».
    /// </summary>
    /// <returns>Открыто ли меню: спрятанных уровней может и не быть.</returns>
    /// <remarks>
    /// Щелчок по «…» раскрывает меню по-прежнему — с клавиатурой в нём. Тяга же держит клавиатуру там,
    /// где начата: Esc и Ctrl, нажатые посреди неё, должны прийти туда.
    /// <para>
    /// Под всякое всплывающее с лёгким закрытием Avalonia стелет поверх окна прозрачный слой, которым
    /// ловит щелчок мимо, — и в нём тонет всякое попадание в окно, включая попадание системной тяги:
    /// раскрытое меню сделало бы недосягаемым всё, над чем несут. Меню, раскрытое тягой, поэтому
    /// пропускает ввод к окну насквозь, и цель под ним находится как обычно; закрывшись, оно
    /// возвращает и слой, и клавиатуру.
    /// </para>
    /// </remarks>
    public bool OpenOverflow()
    {
        if (_menu is null || _overflow is not { IsEffectivelyVisible: true } overflow || _listed.Count == 0)
            return false;

        if (!_menu.IsOpen)
        {
            _menu.ShowMode = FlyoutShowMode.Transient;
            _menu.OverlayInputPassThroughElement = TopLevel.GetTopLevel(this);
            _menu.ShowAt(overflow);
        }

        return _menu.IsOpen;
    }

    /// <summary>Закрывает меню спрятанных уровней.</summary>
    public void CloseOverflow() => _menu?.Hide();

    /// <summary>Лежит ли точка экрана на «…» или на карточке открытого меню спрятанных уровней.</summary>
    /// <param name="screen">Точка экрана.</param>
    /// <remarks>
    /// Карточка, а не полотно: вокруг карточки тема оставляет поле под тень, сквозь которое видно то,
    /// что лежит под меню, — и несут туда, к нему, а не к меню.
    /// </remarks>
    public bool IsOverflowAt(PixelPoint screen) =>
        (_overflow is { IsEffectivelyVisible: true } overflow && Holds(overflow, screen))
        || (IsOverflowOpen && _menu!.Card is { } card && Holds(card, screen));

    /// <summary>
    /// Сегмент под точкой экрана: показанный в ряду — или спрятанный, чей пункт открытого меню под ней.
    /// </summary>
    /// <param name="screen">Точка экрана.</param>
    /// <returns>Сегмент; пусто — под точкой нет уровня пути.</returns>
    /// <remarks>
    /// Точка экрана, а не окна: меню — отдельное окно поверх того, что под крошками, и хозяин, который
    /// несёт на захвате указателя, видит курсор в координатах своего окна.
    /// </remarks>
    public AxBreadcrumbItem? SegmentAt(PixelPoint screen)
    {
        if (IsOverflowOpen)
        {
            for (var at = 0; at < _listed.Count && at < _menu!.Items.Count; at++)
            {
                if (_menu.Items[at] is Control item && Holds(item, screen))
                    return _listed[at];
            }
        }

        return Shown().FirstOrDefault(segment => Holds(segment, screen));
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_overflow is not null)
            _overflow.SizeChanged -= OnOverflowSized;

        _overflow = e.NameScope.Find<Button>("PART_Overflow");
        _menu = null;
        _listed.Clear();

        if (_overflow is not null)
        {
            _menu = new OverflowMenu(this) { Placement = PlacementMode.BottomEdgeAlignedLeft };
            _overflow.Flyout = _menu;
            _overflow.SizeChanged += OnOverflowSized;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Ряд сверяется с меню сам, после каждой своей раскладки: раскладываться ему случается и без
    /// владельца — когда пришла ширина кнопки переполнения.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);

        if (ItemsPanelRoot is AxBreadcrumbPanel panel)
            panel.Arranged = Sync;

        return size;
    }

    /// <summary>
    /// Кнопка переполнения встала на место: ряд держит под неё столько, сколько она заняла.
    /// </summary>
    /// <remarks>
    /// Ширину нельзя взять при замере крошек: кнопка появляется, когда ряд впервые что-то спрятал,
    /// а размер крошек от неё не меняется, и замерять их заново Avalonia не станет. Спрятанная кнопка
    /// размера не меняет, и ряд помнит прежнюю ширину до следующего переполнения.
    /// </remarks>
    private void OnOverflowSized(object? sender, SizeChangedEventArgs e)
    {
        if (ItemsPanelRoot is AxBreadcrumbPanel panel && e.NewSize.Width > 0)
            panel.Reserve = e.NewSize.Width;
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxBreadcrumbItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxBreadcrumbItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnKeyDown(e);

        if (e.Handled || e.KeyModifiers != KeyModifiers.None)
            return;

        var shown = Shown();

        if (shown.Count == 0)
            return;

        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
        var onOverflow = focused is not null && _overflow is not null && focused.FindAncestorOfType<Button>(includeSelf: true) == _overflow;
        var at = focused?.FindAncestorOfType<AxBreadcrumbItem>(includeSelf: true) is { } item ? shown.IndexOf(item) : -1;

        Control? next = e.Key switch
        {
            Key.Right when onOverflow => shown[0],
            Key.Right when at >= 0 && at + 1 < shown.Count => shown[at + 1],
            Key.Left when at == 0 && _overflow is { IsVisible: true } => _overflow,
            Key.Left when at > 0 => shown[at - 1],
            Key.Home => shown[0],
            Key.End => shown[^1],
            _ => null,
        };

        if (next is null)
            return;

        next.Focus(NavigationMethod.Directional);
        e.Handled = true;
    }

    /// <summary>Сегменты ряда по порядку — и видимые, и ушедшие в меню.</summary>
    private List<AxBreadcrumbItem> Segments() =>
        ItemsPanelRoot is { } panel ? [.. panel.Children.OfType<AxBreadcrumbItem>()] : [];

    /// <summary>Сегменты, которые сейчас видны в ряду.</summary>
    private List<AxBreadcrumbItem> Shown()
    {
        var first = (ItemsPanelRoot as AxBreadcrumbPanel)?.FirstShown ?? 0;

        return [.. Segments().Skip(first)];
    }

    /// <summary>
    /// Сверяет сегменты и меню переполнения с тем, что показала раскладка.
    /// </summary>
    /// <remarks>
    /// Место сегмента — первый он или текущий — ставится здесь, а не при создании: путь растёт и
    /// укорачивается на ходу, и сегмент, бывший текущим, после шага вглубь им быть перестаёт.
    /// Спрятанный сегмент теряет и фокус клавиатуры: рамка у него нулевая, и фокус на нём был бы
    /// невидим. Меню набирается заново, только когда список спрятанных сменился: раскладка зовёт
    /// это на каждом проходе.
    /// </remarks>
    private void Sync()
    {
        var segments = Segments();
        var first = (ItemsPanelRoot as AxBreadcrumbPanel)?.FirstShown ?? 0;

        for (var at = 0; at < segments.Count; at++)
        {
            var segment = segments[at];
            var shown = at >= first;

            segment.Mark(first: at == 0, current: at == segments.Count - 1);
            segment.Focusable = shown;
            segment.IsTabStop = shown;
        }

        var hidden = segments.Take(first).ToList();

        PseudoClasses.Set(":overflow", hidden.Count > 0);

        if (_menu is null || hidden.SequenceEqual(_listed))
            return;

        _menu.Items.Clear();
        _listed.Clear();
        _listed.AddRange(hidden);

        foreach (var segment in hidden)
        {
            var chosen = segment;

            // Данные сегмента идут в меню теми же, с тем же шаблоном. Контрол так не отдать — он
            // уже стоит в ряду, и второй родитель ему не положен: пункт берёт тогда имя сегмента.
            var item = new AxMenuItem
            {
                Header = segment.Content is Control ? AutomationProperties.GetName(segment) : segment.Content,
                HeaderTemplate = segment.Content is Control ? null : segment.ContentTemplate,
                IsDropTarget = segment.IsDropTarget,
            };

            item.Click += (_, _) => Raise(chosen);
            _menu.Items.Add(item);
        }
    }

    private void OnSegmentClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not AxBreadcrumbItem segment || IndexFromContainer(segment) < 0)
            return;

        e.Handled = true;

        if (!segment.IsCurrent)
            Raise(segment);
    }

    private void Raise(AxBreadcrumbItem segment)
    {
        var index = IndexFromContainer(segment);

        if (index < 0)
            return;

        RaiseEvent(new AxBreadcrumbNavigatedEventArgs(NavigatedEvent, ItemFromContainer(segment), index));
    }

    /// <summary>Цель у спрятанного сегмента — цель и у его пункта в меню: сам сегмент в ряду не виден.</summary>
    private void Reflect(AxBreadcrumbItem segment)
    {
        var at = _listed.IndexOf(segment);

        if (_menu is not null && at >= 0 && at < _menu.Items.Count && _menu.Items[at] is AxMenuItem item)
            item.IsDropTarget = segment.IsDropTarget;
    }

    /// <summary>
    /// Тяга из проводника над меню спрятанных уровней — тяга над крошками: то же событие поднимается у
    /// них, с курсором в их координатах — за краем ряда, там, где меню, — и ответ хозяина уходит назад.
    /// </summary>
    private void OnMenuDrag(object? sender, DragEventArgs e)
    {
        if (sender is not Visual menu || e.RoutedEvent is not RoutedEvent<DragEventArgs> routed)
            return;

        var relayed = new DragEventArgs(routed, e.DataTransfer, this, this.PointToClient(menu.PointToScreen(e.GetPosition(menu))), e.KeyModifiers)
        {
            DragEffects = e.DragEffects,
        };

        RaiseEvent(relayed);

        e.DragEffects = relayed.DragEffects;
        e.Handled = relayed.Handled;
    }

    /// <summary>Лежит ли точка экрана в границах элемента, показанного в каком-нибудь окне.</summary>
    private static bool Holds(Visual visual, PixelPoint screen) =>
        visual.IsEffectivelyVisible && TopLevel.GetTopLevel(visual) is not null
        && new Rect(visual.Bounds.Size).Contains(visual.PointToClient(screen));

    /// <summary>
    /// Меню спрятанных уровней: знает свою карточку и передаёт тягу над ней крошкам.
    /// </summary>
    /// <remarks>
    /// Целью сброса бывает полотно, а не окно всплывающего: окно шире карточки на поле под тень, и
    /// сброс, пришедший на поле, разобрать некому — источник счёл бы его принятым и мог бы убрать у
    /// себя отпущенное. Наследование поэтому обрывается на попапе, а полотно берёт разрешение прямо
    /// у крошек: спрятанные уровни — цель ровно тогда, когда цель сами крошки.
    /// <para>
    /// Открытое тягой — без клавиатуры и со слоем лёгкого закрытия, пропускающим ввод к окну;
    /// закрывшись, меню возвращает и то и другое, и следующий щелчок по «…» раскрывает его как всегда.
    /// </para>
    /// </remarks>
    /// <param name="owner">Крошки.</param>
    private sealed class OverflowMenu(AxBreadcrumb owner) : AxMenuFlyout
    {
        /// <summary>Полотно меню; пусто — меню ещё не открывали.</summary>
        public Control? Presenter { get; private set; }

        /// <summary>Видимая карточка меню — полотно без поля под тень; пусто — меню ещё не открывали.</summary>
        public Visual? Card => Presenter?.GetVisualChildren().FirstOrDefault() ?? Presenter;

        protected override Control CreatePresenter()
        {
            var presenter = base.CreatePresenter();

            DragDrop.SetAllowDrop(Popup, false);
            _ = presenter.Bind(DragDrop.AllowDropProperty, owner.GetObservable(DragDrop.AllowDropProperty));

            presenter.AddHandler(DragDrop.DragEnterEvent, owner.OnMenuDrag);
            presenter.AddHandler(DragDrop.DragOverEvent, owner.OnMenuDrag);
            presenter.AddHandler(DragDrop.DragLeaveEvent, owner.OnMenuDrag);
            presenter.AddHandler(DragDrop.DropEvent, owner.OnMenuDrag);
            Presenter = presenter;

            return presenter;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            ShowMode = FlyoutShowMode.Standard;
            OverlayInputPassThroughElement = null;
        }
    }
}

/// <summary>Сегмент, выбранный в <see cref="AxBreadcrumb"/>.</summary>
public sealed class AxBreadcrumbNavigatedEventArgs : RoutedEventArgs
{
    /// <summary>Заводит событие выбора сегмента.</summary>
    /// <param name="routedEvent">Событие крошек.</param>
    /// <param name="item">Данные сегмента — элемент источника или сам сегмент, если источника нет.</param>
    /// <param name="index">Номер сегмента от корня пути.</param>
    public AxBreadcrumbNavigatedEventArgs(RoutedEvent routedEvent, object? item, int index)
        : base(routedEvent)
    {
        Item = item;
        Index = index;
    }

    /// <summary>Данные выбранного сегмента.</summary>
    public object? Item { get; }

    /// <summary>Номер выбранного сегмента от корня пути.</summary>
    public int Index { get; }
}
