using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace ArxisStudio.Controls;

/// <summary>Чьи вкладки держит полоса.</summary>
public enum AxTabStripKind
{
    /// <summary>Вкладки документов: выбранная поднята фоном документа над подложкой.</summary>
    Document,

    /// <summary>Вкладки панели: без своего фона, выбор показывает одна полоса снизу.</summary>
    ToolWindow,
}

/// <summary>
/// Полоса вкладок: горизонтальный ряд <see cref="AxTabItem"/>.
/// Содержимое вкладки размещает хост — полоса отвечает только за выбор.
/// </summary>
/// <remarks>
/// Вид вкладки задаёт полоса, а не сама вкладка: тема вкладки не видит, в какой полосе
/// та стоит, и прежде вкладке панели приходилось повторять класс <c>compact</c> на
/// каждой. Полоса ставит своим вкладкам псевдокласс <c>:tool-window</c> — и тем, что
/// создала сама, и тем, что пришли готовыми.
/// </remarks>
[PseudoClasses(":tool-window", ":overflow")]
[TemplatePart("PART_Overflow", typeof(Button))]
[TemplatePart("PART_Scroll", typeof(ScrollViewer))]
public class AxTabStrip : ListBox
{
    private readonly List<AxTabItem> _listed = [];
    private readonly List<IDisposable> _bound = [];
    private Button? _overflow;
    private ScrollViewer? _scroll;
    private MenuFlyout? _menu;

    /// <summary>Чьи вкладки держит полоса.</summary>
    public static readonly StyledProperty<AxTabStripKind> KindProperty =
        AvaloniaProperty.Register<AxTabStrip, AxTabStripKind>(nameof(Kind));

    static AxTabStrip() =>
        KindProperty.Changed.AddClassHandler<AxTabStrip>((strip, change) => strip.Mark(change.GetNewValue<AxTabStripKind>()));

    /// <inheritdoc cref="KindProperty"/>
    public AxTabStripKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Место, которое полосе дал родитель: шире не бывает ни одна её вкладка.</summary>
    internal double Room { get; private set; } = double.PositiveInfinity;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_scroll is not null)
            _scroll.ScrollChanged -= OnScrolled;

        _overflow = e.NameScope.Find<Button>("PART_Overflow");
        _scroll = e.NameScope.Find<ScrollViewer>("PART_Scroll");
        _menu = null;
        _listed.Clear();

        if (_overflow is not null)
        {
            // Меню держит сама кнопка: она же его открывает, закрывает по Esc и щелчку в
            // стороне и метит себя, пока оно открыто.
            _menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };
            _overflow.Flyout = _menu;
        }

        if (_scroll is not null)
            _scroll.ScrollChanged += OnScrolled;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Вкладки, не поместившиеся в полосу, достаются из меню переполнения, и кнопка его появляется
    /// только тогда, когда доставать есть что. Считается это после раскладки: до неё неизвестно ни
    /// место полосы, ни ширина вкладок. Здесь же набирается и само меню — почему не перед показом,
    /// сказано у <see cref="Sync"/>.
    /// </remarks>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);

        Sync();

        return size;
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTabItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTabItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is AxTabItem tab)
            tab.MarkToolWindow(Kind == AxTabStripKind.ToolWindow);
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);

        if (container is AxTabItem tab)
            tab.MarkToolWindow(false);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Вкладка не шире полосы. Полоса прокручивается, и вкладке шире неё —
    /// одинокой, в узкой панели, при крупном кегле — было нечем уместиться: имя
    /// уходило за край вместе с крестиком. Теперь вкладка мерится не шире места
    /// полосы, и тема сокращает имя многоточием. Ряду вкладок, не влезающих
    /// вместе, предел не мешает: каждая остаётся своей ширины, и полоса
    /// прокручивается, как прокручивалась.
    /// <para>
    /// Место берётся у родителя, а не у окна прокрутки. У полосы шириной по
    /// содержимому окно мерится самими вкладками, и предел по нему замкнулся бы
    /// петлёй: окно в ноль — вкладка в ноль.
    /// </para>
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        var room = Math.Max(0, availableSize.Width - Padding.Left - Padding.Right - BorderThickness.Left - BorderThickness.Right);

        if (room != Room)
        {
            Room = room;

            // Вкладкам полоса даёт бесконечную ширину и в прошлый раз давала
            // такую же: сами они о новом месте не узнают.
            foreach (var tab in GetRealizedContainers())
                tab.InvalidateMeasure();
        }

        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Вкладки, которых на полосе сейчас не видно.
    /// </summary>
    /// <remarks>
    /// Считаются по месту в окне прокрутки: вкладка скрыта, если её край вышел за видимую часть.
    /// Половина вкладки — тоже скрытая: человек не должен гадать, та ли это, что ему нужна.
    /// </remarks>
    private List<AxTabItem> Hidden()
    {
        var hidden = new List<AxTabItem>();

        if (_scroll is null)
            return hidden;

        var viewport = _scroll.Viewport.Width;
        var offset = _scroll.Offset.X;

        foreach (var tab in GetRealizedContainers().OfType<AxTabItem>())
        {
            if (!tab.IsVisible)
                continue;

            var left = tab.Bounds.Left - offset;

            // Полпикселя на округление раскладки: край вкладки, севший ровно на границу окна,
            // скрытым не считается.
            if (left < -0.5 || left + tab.Bounds.Width > viewport + 0.5)
                hidden.Add(tab);
        }

        return hidden;
    }

    /// <summary>
    /// Сверяет меню переполнения с тем, что сейчас не видно.
    /// </summary>
    /// <remarks>
    /// Меню набирается здесь, в раскладке, а не перед самым показом. Всплывающее окно строит себе
    /// содержимое один раз, когда его открывают впервые, и вкладки, положенные в меню по событию
    /// <c>Opening</c>, в него уже не попадали: у кнопки открывалась пустая рамка. Проверено на
    /// живой студии — в меню лежали две вкладки, а на экране не было ни одной.
    /// <para>
    /// Заново меню набирается, только когда список скрытых сменился: раскладка зовёт это
    /// на каждый проход, а тянуть границу человек может секундами.
    /// </para>
    /// <para>
    /// Подпись пункта привязана к подписи вкладки, а не списана с неё: документ переименовывают и
    /// когда его вкладка не видна.
    /// </para>
    /// </remarks>
    private void Sync()
    {
        var hidden = Hidden();

        PseudoClasses.Set(":overflow", hidden.Count > 0);

        if (_menu is null || hidden.SequenceEqual(_listed))
            return;

        foreach (var link in _bound)
            link.Dispose();

        _bound.Clear();
        _menu.Items.Clear();
        _listed.Clear();
        _listed.AddRange(hidden);

        foreach (var tab in hidden)
        {
            var chosen = tab;
            var item = new AxMenuItem();

            _bound.Add(item.Bind(
                HeaderedSelectingItemsControl.HeaderProperty,
                tab.GetObservable(ContentControl.ContentProperty)));

            // Выбранная из меню вкладка становится выбранной и в полосе, и полоса прокручивается
            // к ней: иначе человек выбрал бы документ и не увидел, куда он делся.
            item.Click += (_, _) =>
            {
                SelectedItem = Chosen(chosen);
                chosen.BringIntoView();
                chosen.Focus(NavigationMethod.Tab);
            };

            _menu.Items.Add(item);
        }
    }

    /// <summary>Элемент, которому принадлежит контейнер: у полосы без источника это он сам.</summary>
    private object? Chosen(AxTabItem tab) =>
        ItemsSource is null ? tab : (ItemFromContainer(tab) ?? tab);

    // Прокрутка колесом или клавишами меняет то, что видно, — значит и меню переполнения.
    private void OnScrolled(object? sender, ScrollChangedEventArgs e) => Sync();

    private void Mark(AxTabStripKind kind)
    {
        PseudoClasses.Set(":tool-window", kind == AxTabStripKind.ToolWindow);

        foreach (var tab in GetRealizedContainers().OfType<AxTabItem>())
            tab.MarkToolWindow(kind == AxTabStripKind.ToolWindow);
    }
}
