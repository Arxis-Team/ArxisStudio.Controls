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
    private MenuFlyout? _menu;

    static AxBreadcrumb()
    {
        ItemsPanelProperty.OverrideDefaultValue<AxBreadcrumb>(new FuncTemplate<Panel?>(() => new AxBreadcrumbPanel()));
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<AxBreadcrumb>(KeyboardNavigationMode.Once);
    }

    /// <summary>Заводит крошки: щелчки по сегментам слушаются здесь, а не у каждого сегмента.</summary>
    public AxBreadcrumb() => AddHandler(Button.ClickEvent, OnSegmentClick);

    /// <inheritdoc cref="NavigatedEvent"/>
    public event EventHandler<AxBreadcrumbNavigatedEventArgs>? Navigated
    {
        add => AddHandler(NavigatedEvent, value);
        remove => RemoveHandler(NavigatedEvent, value);
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
            _menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedLeft };
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
